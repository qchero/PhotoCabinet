using MetadataExtractor;
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
        public static Metadata Extract(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            // Extract file info and calculate MD5
            FileInfo fileInfo = new FileInfo(filePath);
            string md5;
            using (var fileStream = fileInfo.OpenRead())
            {
                var md5Algorithm = MD5.Create();
                md5 = Md5ToString(md5Algorithm.ComputeHash(fileStream));
            }

            // Extract metadata from EXIF data
            var metadataDirectories = ImageMetadataReader.ReadMetadata(filePath);

            var timeTakenStr =
                metadataDirectories
                .Where(d => d.Name == "Exif SubIFD")
                .FirstOrDefault()
                ?.Tags
                ?.Where(t => t.Name == "Date/Time Original")
                ?.FirstOrDefault()
                ?.Description;
            var timeTaken = timeTakenStr != null ?
                DateTime.ParseExact(timeTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture) :
                new DateTime?();

            var timeInferredFromFileName = InferDateTimeFromFileName(new Configuration().KnownDateFormats, fileName);

            return new Metadata
            {
                FilePath = filePath,
                FileName = fileName,
                TimeInferredFromFileName = timeInferredFromFileName,
                TimeTaken = timeTaken,
                TimeFileCreated = fileInfo.CreationTime,
                TimeFileModified = fileInfo.LastWriteTime,
                Md5 = md5
            };
        }

        public static DateTime? InferDateTimeFromFileName(string[] knownDateFormats, string fileName)
        {
            foreach (var format in knownDateFormats)
            {
                var regexText = format
                    .Replace("y", "\\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("m", "\\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("d", "\\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("h", "\\d", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("s", "\\d", StringComparison.InvariantCultureIgnoreCase);

                var match = Regex.Match(fileName, regexText);
                if (!match.Success)
                {
                    continue;
                }

                string dateTimeText = match.Groups[1].Value;
                if (!DateTime.TryParseExact(dateTimeText, format, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var dateTime))
                {
                    continue;
                }

                return dateTime;
            }

            return null;
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
