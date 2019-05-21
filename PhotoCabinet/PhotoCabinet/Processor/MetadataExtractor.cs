using MetadataExtractor;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PhotoCabinet
{
    public static class MetadataExtractor
    {
        public static Metadata Extract(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            FileInfo fileInfo = new FileInfo(filePath);
            
            // Extract metadata from EXIF data
            var metadataDirectories = ImageMetadataReader.ReadMetadata(filePath);

            string timeTakenStr =
                metadataDirectories
                .Where(d => d.Name == "Exif SubIFD")
                .FirstOrDefault()
                ?.Tags
                ?.Where(t => t.Name == "Date/Time Original")
                ?.FirstOrDefault()
                ?.Description
                ?? throw new MetadataMissingException();
            var timeTaken = DateTime.ParseExact(timeTakenStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

            var timeInferredFromFileName = InferDateTimeFromFileName(Configuration.KnownDateFormats, fileName);

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

        private static string InferDateTimeFromFileName(string[] knownDateFormats, string fileName)
        {
            foreach (var format in knownDateFormats)
            {

            }

            return null;
        }
    }
}
