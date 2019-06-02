using MetadataExtractor;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PhotoCabinet.Analyzer
{
    public static class MetadataAnalyzer
    {
        public static Metadata Extract(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            FileInfo fileInfo = new FileInfo(filePath);
            
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
                TimeFileModified = fileInfo.LastWriteTime
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
    }
}
