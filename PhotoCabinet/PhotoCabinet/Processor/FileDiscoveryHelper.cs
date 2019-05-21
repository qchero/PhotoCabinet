using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoCabinet.Processor
{
    public static class FileDiscoveryHelper
    {
        /// <summary>
        /// File all file paths in a directory
        /// </summary>
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
