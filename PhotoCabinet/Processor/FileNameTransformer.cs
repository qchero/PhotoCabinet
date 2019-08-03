using PhotoCabinet.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoCabinet
{
    public interface IFileNameTransformer
    {
        string Transform(Metadata metadata, string format, string dedupFormat, HashSet<string> dedupSet);
    }

    public class FileNameTransformer : IFileNameTransformer
    {
        public string Transform(Metadata metadata, string format, string dedupFormat, HashSet<string> dedupSet)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(dedupFormat))
            {
                throw new Exception("Invalid format input");
            }

            // Find out extension and base name
            var fileExtension = Path.GetExtension(metadata.FilePath);
            var type = MediaTypeUtils.IsImage(fileExtension) ? "IMG" : "VID";

            var time = metadata?.GetPreferredTime();
            if (time == null)
            {
                throw new Exception("Preferred time is missing in metadata");
            }

            var newNameWithoutExtension = format
                .Replace("[type]", type)
                .Replace("[date]", time.Value.ToString("yyyyMMdd"))
                .Replace("[time]", time.Value.ToString("HHmmss"));

            // Enumerate names for deduping
            int dedupNum = 0;
            while (true)
            {
                if (dedupNum > 100)
                {
                    throw new Exception("Attempted 100 dedup number but couldn't find a new name");
                }

                // If it's 0 don't add suffix
                var dedupStr = dedupNum == 0
                    ? string.Empty
                    : dedupFormat.Replace("[num]", $"{dedupNum:D2}");
                var attemptedName = newNameWithoutExtension + dedupStr + fileExtension;

                if (!dedupSet.Contains(attemptedName))
                {
                    return attemptedName;
                }

                dedupNum++;
            }
        }
    }
}
