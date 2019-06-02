using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PhotoCabinet.Analyzer;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;

namespace PhotoCabinet
{
    public class FileRenameProcessor : IProcessor
    {
        public bool PrepareContext(Context context, ILogger log)
        {
            throw new NotImplementedException();
        }

        public bool ProcessContext(Context context, ILogger log)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rename the files based on the results of PreviewRename
        /// Before each rename, back up the file to /_Rename/directory
        /// Stop if any Rename operation fails
        /// </summary>
        public bool RenameFiles(string workDirectoryPath, string format)
        {
            var filePathToNewNameMap = DiscoverFilesToRename(workDirectoryPath, format)
                .Where(pair => Path.GetFileName(pair.Key) != pair.Value);

            foreach (var pair in filePathToNewNameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            return true;
        }

        /// <summary>
        /// Return a map of file path to new file names
        /// </summary>
        private Dictionary<string, string> DiscoverFilesToRename(string workDirectoryPath, string format)
        {
            var filePaths = new string[] { };//FileDiscoverProcessor.GetAllFilesInDirectory(workDirectoryPath);
            return filePaths
                .Select(f =>
                {
                    var metadata = MetadataAnalyzer.Extract(f);
                    string newFileName = FileNameTransformer.Transform(metadata, format);
                    return KeyValuePair.Create(f, newFileName);
                })
                .ToDictionary(p => p.Key, p => p.Value);
        }

        private void Rename(string filePath, string newName)
        {
            Console.WriteLine($"[Rename] {filePath} to {newName}");

            var directory = Path.GetDirectoryName(filePath);
            var newPath = Path.Combine(directory, newName);
            if (filePath != newPath)
            {
                File.Move(filePath, newPath);
            }
        }
    }
}
