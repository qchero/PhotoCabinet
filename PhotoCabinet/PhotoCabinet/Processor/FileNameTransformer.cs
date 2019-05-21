using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoCabinet
{
    public static class FileNameTransformer
    {
        public static string Transform(Metadata metadata, string format)
        {
            var fileExtension = Path.GetExtension(metadata.FilePath);

            if (metadata?.TimeTaken == null)
            {
                throw new MetadataMissingException();
            }

            var newName = format
                .Replace("[date]", metadata.TimeTaken.ToString("yyyyMMdd"))
                .Replace("[time]", metadata.TimeTaken.ToString("HHmmss"))
                .Replace(".[ext]", "[ext]")
                .Replace("[ext]", fileExtension);

            return newName;
        }
    }
}
