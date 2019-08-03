using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PhotoCabinet.FileOperation
{
    public interface IFileMover
    {
        void Move(string oldPath, string newPath);
    }

    public class FileMover : IFileMover
    {
        public void Move(string oldPath, string newPath)
        {
            if (!File.Exists(oldPath))
            {
                throw new Exception("File doesn't exist");
            }

            if (File.Exists(newPath))
            {
                throw new Exception("File exist in the new path");
            }

            var directory = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Move(oldPath, newPath);
        }
    }
}
