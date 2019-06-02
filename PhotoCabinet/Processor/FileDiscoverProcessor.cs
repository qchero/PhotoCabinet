using Microsoft.Extensions.Logging;
using PhotoCabinet.Analyzer;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using PhotoCabinet.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoCabinet
{
    public class FileDiscoverProcessor : IProcessor
    {
        /// <summary>
        /// Discover all the files in library, pending processing directory and failed processing directory
        /// And retrive the metadata
        /// </summary>
        public bool PrepareContext(Context context, ILogger log)
        {
            ValidateContext(context);

            var allFilesInDirectory = GetAllFilesInDirectory(context.Configuration.PendingProcessingDirectory.ToFullPath());
            context.PendingProcessingFileToMetadataMap = allFilesInDirectory
                .ToDictionary(
                    path => path,
                    path => MetadataAnalyzer.Extract(path));

            log.LogInformationWithPaths($"{context.PendingProcessingFileToMetadataMap.Count} files pending processing", allFilesInDirectory);

            return true;
        }

        /// <summary>
        /// By design this does nothing
        /// </summary>
        public bool ProcessContext(Context context, ILogger log)
        {
            return true;
        }

        private static void ValidateContext(Context context)
        {
            if (context.Configuration.LibraryDirectory.IsSubPathOf(context.Configuration.PendingProcessingDirectory))
            {
                throw new Exception("Library directory shouldn't be sub directory of pending processing directory");
            }

            if (context.Configuration.LibraryDirectory.IsSubPathOf(context.Configuration.FailedProcessingDirectory))
            {
                throw new Exception("Library directory shouldn't be sub directory of failed processing directory");
            }
        }

        /// <summary>
        /// Get all the files under a certain directory
        /// </summary>
        private static IEnumerable<string> GetAllFilesInDirectory(string directoryPath)
        {
            IEnumerable<string> subDirectoryFiles = Directory.GetDirectories(directoryPath)
                .SelectMany(f => GetAllFilesInDirectory(f));

            return Directory.GetFiles(directoryPath)
                .Concat(subDirectoryFiles)
                .ToList();
        }
    }
}
