using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using System;
using System.Collections.Generic;
using System.IO;

namespace PhotoCabinet
{
    public class FileBackupProcessor
    {
        public string PrepareContext(Context context)
        {
            throw new NotImplementedException();
        }

        public bool ProcessContext(Context context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return a map of file path to new file names
        /// </summary>
        public static void BackupFiles(string workDirectoryPath, string backupDirectoryName)
        {
            var backupDirectory = Path.Combine(workDirectoryPath, backupDirectoryName);
            Console.WriteLine($"[Backup] {workDirectoryPath} to {backupDirectory}");

            if (Directory.Exists(backupDirectory))
            {
                throw new BackupDirectoryAlreadyExistException();
            }

            int count = 0;
            var filePaths = new List<string> { };// FileDiscoverProcessor.GetAllFilesInDirectory(workDirectoryPath);
            foreach (var filePath in filePaths)
            {
                var relativePath = Path.GetRelativePath(workDirectoryPath, filePath);
                var backupPath = Path.Combine(backupDirectory, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
                File.Copy(filePath, backupPath);
                count++;
            }

            Console.WriteLine($"[Backup] {count} files backed up");
        }

        
    }
}
