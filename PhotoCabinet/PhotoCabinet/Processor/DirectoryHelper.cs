using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoCabinet.Processor
{
    public static class DirectoryHelper
    {
        public static string ToFullPath(this string path)
        {
            var currentDirectoryPath = Directory.GetCurrentDirectory();
            if (Path.IsPathRooted(currentDirectoryPath))
            {
                return path;
            }

            return Path.Combine(currentDirectoryPath, path);
        }
    }
}
