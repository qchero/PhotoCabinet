using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using PhotoCabinet.Utility;
using System;
using System.Collections.Generic;
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
        public static Metadata Extract(string filePath)
        {
            // Get file info
            string fileName = Path.GetFileName(filePath);
            FileInfo fileInfo = new FileInfo(filePath);
            var fileExtension = fileInfo.Extension;

            DateTime? timeTaken = null;
            var metadataDirectories = ImageMetadataReader.ReadMetadata(filePath);
            if (MediaTypeUtils.IsImage(fileExtension))
            {
                // Extract metadata from EXIF data
                var exifSubIfdDirectory = metadataDirectories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                var timeTakenStr = exifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal)
                    ?? exifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTime);
                timeTaken = timeTakenStr != null ?
                    DateTime.ParseExact(timeTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture) :
                    new DateTime?();
            }
            else if (MediaTypeUtils.IsMovVideo(fileExtension))
            {
                // Extract metadata for video - format: "Sun Oct 23 00:46:50 2016"
                var quickTimeMovieHeaderDirectory = metadataDirectories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                var timeTakenStr = quickTimeMovieHeaderDirectory?.GetDescription(QuickTimeMovieHeaderDirectory.TagCreated);
                timeTaken = timeTakenStr != null ?
                    DateTime.ParseExact(timeTakenStr, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture) :
                    new DateTime?();
            }

            // Infer time from file name
            var timeInferredFromFileName = InferDateTimeFromFileName(new Configuration().KnownDateFormats, fileName);

            return new Metadata
            {
                FilePath = filePath,
                FileName = fileName,
                TimeInferredFromFileName = timeInferredFromFileName,
                TimeTaken = timeTaken,
                TimeFileCreated = fileInfo.CreationTime,
                TimeFileModified = fileInfo.LastWriteTime,
                Md5Func = new Func<string>(() =>
                {
                    return GetMd5(filePath);
                })
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
