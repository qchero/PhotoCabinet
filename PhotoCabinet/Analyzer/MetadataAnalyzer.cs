using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using PhotoCabinet.Database;
using PhotoCabinet.Utility;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PhotoCabinet.Analyzer
{
    public static class MetadataAnalyzer
    {
        public static Metadata Extract(string filePath, Md5Cache md5Cache = null)
        {
            // Get file info
            string fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath);
            var mediaType = MediaTypeUtils.GetMediaType(fileExtension);

            // Infer time from file name
            var timeInferredFromFileName = InferDateTimeFromFileName(new Configuration().KnownDateFormats, fileName);

            // Lazy retrival of created and modified time
            var timeFileCreatedLazy = new Lazy<DateTime>(() =>
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.CreationTime;
            });

            var timeFileModifiedLazy = new Lazy<DateTime>(() =>
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.LastWriteTime;
            });

            // Lazy retrival of time taken
            var timeTakenLazy = new Lazy<DateTime?>(() =>
            {
                DateTime? timeTaken = null;
                var metadataDirectories = ImageMetadataReader.ReadMetadata(filePath);
                if (mediaType.IsImage())
                {
                    // Extract metadata from EXIF data
                    var exifSubIfdDirectories = metadataDirectories.OfType<ExifSubIfdDirectory>();
                    var timeTakenStr =
                        exifSubIfdDirectories
                        .Select(d => d.GetDescription(ExifDirectoryBase.TagDateTimeOriginal))
                        .Where(d => !string.IsNullOrEmpty(d))
                        .FirstOrDefault()
                        ??
                        exifSubIfdDirectories
                        .Select(d => d.GetDescription(ExifDirectoryBase.TagDateTime))
                        .Where(d => !string.IsNullOrEmpty(d))
                        .FirstOrDefault();
                    timeTaken = timeTakenStr != null ?
                        DateTime.ParseExact(timeTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture) :
                        new DateTime?();
                }
                else if (mediaType.IsVideo())
                {
                    // Extract metadata for video - format: "Sun Oct 23 00:46:50 2016"
                    var quickTimeMovieHeaderDirectory = metadataDirectories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                    var timeTakenStr = quickTimeMovieHeaderDirectory?.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated);
                    timeTaken = timeTakenStr != null ?
                        DateTime.ParseExact(timeTakenStr, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture) :
                        new DateTime?();
                }

                return timeTaken;
            });

            // Lazy retrival of MD5
            var md5Lazy = new Lazy<string>(() =>
            {
                if (md5Cache == null)
                {
                    return GetMd5(filePath);
                }

                // Md5 cache is keyed by file name
                return md5Cache.CachedFunc(fileName, () =>
                {
                    return GetMd5(filePath);
                });
            });

            return new Metadata
            {
                FilePath = filePath,
                MediaType = mediaType,
                TimeInferredFromFileName = timeInferredFromFileName,
                TimeTaken = timeTakenLazy,
                TimeFileCreated = timeFileCreatedLazy,
                TimeFileModified = timeFileModifiedLazy,
                Md5 = md5Lazy
            };
        }

        public static DateTime? InferDateTimeFromFileName(string[] knownDateFormats, string fileName)
        {
            foreach (var format in knownDateFormats)
            {
                var regexText = format
                    .Replace("d", @"\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("y", @"\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("m", @"\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("h", @"\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("s", @"\d", StringComparison.InvariantCultureIgnoreCase);

                var match = Regex.Match(fileName, regexText);
                if (!match.Success)
                {
                    continue;
                }

                string dateTimeText = match.Groups[0].Value;
                if (!DateTime.TryParseExact(dateTimeText, format, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var dateTime))
                {
                    continue;
                }

                return dateTime;
            }

            return null;
        }

        // Extract file info and calculate MD5
        public static string GetMd5(string filePath)
        {
            // Extract file info and calculate MD5
            FileInfo fileInfo = new FileInfo(filePath);
            using (var fileStream = fileInfo.OpenRead())
            {
                var md5Algorithm = MD5.Create();
                return Md5ToString(md5Algorithm.ComputeHash(fileStream));
            }
        }

        private static string Md5ToString(byte[] md5)
        {
            // Loop through each byte of the hashed data and format each one as a hexadecimal string.
            var sBuilder = new StringBuilder();
            foreach (var t in md5)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}
