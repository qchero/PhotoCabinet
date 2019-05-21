using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoCabinet
{
    public static class FileDiscoverProcessor
    {
        public static IEnumerable<string> GetAllFilesInDirectory(string directoryPath)
        {
            IEnumerable<string> subDirectoryFiles = Directory.GetDirectories(directoryPath)
                .SelectMany(f => GetAllFilesInDirectory(f));

            return Directory.GetFiles(directoryPath)
                .Concat(subDirectoryFiles)
                .ToList();
        }
    }
}
