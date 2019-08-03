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
    /// <summary>
    /// Discover all photos and videos in PendingProcessingDirectory
    /// [Context] Set PendingProcessingFiles to the list of files to be processed
    /// [Context] Initialize LibraryFiles to the list of files in library
    /// [Context] Build LibraryGroupToFileMap for indexing the library by year and month
    /// [Context] Update FileToMetadataMap to include the metadata of all PendingProcessingFiles and LibraryFiles
    /// </summary>
    public class FileDiscoverProcessor : IProcessor
    {
        /// <summary>
        /// Validate the context
        /// </summary>
        private static void ValidateContext(Context context, ILogger log)
        {
            if (context.Configuration.LibraryDirectory.IsSubPathOf(context.Configuration.PendingProcessingDirectory))
            {
                log.LogError("Library directory shouldn't be sub directory of pending processing directory");
                throw new Exception("Library directory shouldn't be sub directory of pending processing directory");
            }
        }

        /// <summary>
        /// Get all the files under a certain directory
        /// </summary>
        private static List<string> GetAllFilesInDirectory(string directoryPath)
        {
            IEnumerable<string> subDirectoryFiles = Directory.GetDirectories(directoryPath)
                .SelectMany(f => GetAllFilesInDirectory(f));

            return Directory.GetFiles(directoryPath)
                .Concat(subDirectoryFiles)
                .ToList();
        }

        /// <summary>
        /// Discover all the files in library, pending processing directory and failed processing directory
        /// And retrive the metadata
        /// </summary>
        public bool PrepareContext(Context context, ILogger log)
        {
            ValidateContext(context, log);

            DiscoverPendingProcessingFiles(context, log);

            DiscoverLibraryFiles(context, log);

            return true;
        }

        /// <summary>
        /// By design this does nothing
        /// </summary>
        public bool ProcessContext(Context context, ILogger log)
        {
            return true;
        }

        private void DiscoverPendingProcessingFiles(Context context, ILogger log)
        {
            foreach (var path in GetAllFilesInDirectory(context.Configuration.PendingProcessingDirectory.ToFullPath()))
            {
                var metadata = MetadataAnalyzer.Extract(path);
                context.AddPendingProcessingFiles(path, metadata);
            }

            log.LogInformationWithPaths($"{context.FileToMetadataMap.Count} files pending processing", context.PendingProcessingFiles);
        }

        private void DiscoverLibraryFiles(Context context, ILogger log)
        {
            // Enumerate year directory
            foreach (var yearDir in Directory.GetDirectories(context.Configuration.LibraryDirectory.ToFullPath()))
            {
                // If the directory is under pending processing directory, ignore it
                if (yearDir.IsSubPathOf(context.Configuration.PendingProcessingDirectory.ToFullPath()))
                {
                    log.LogInformation($"Directory ignored since it's under pending processing directory: {yearDir}");
                    continue;
                }

                // If the directory is other known directory, ignore it
                var yearDirName = yearDir.LastPart();
                if (yearDirName.Equals("ProcessingLogs", StringComparison.OrdinalIgnoreCase))
                {
                    log.LogInformation($"Directory ignored since it's logging directory: {yearDir}");
                    continue;
                }

                // Check for invalid year
                if (!int.TryParse(yearDirName, out var year))
                {
                    log.LogWarning($"The name of year directory is not valid: {yearDir.LastPart()}");
                    log.LogWarning($"Photos in {yearDir} will be ignored");
                    continue;
                }

                // Files should not be placed at the year directory level
                var yearDirFiles = Directory.GetFiles(yearDir);
                if (yearDirFiles.Any())
                {
                    log.LogWarning($"Unexpected files in year dir, e.g. {yearDirFiles.First()}");
                }

                // Enumerate month directory
                foreach (var monthDir in Directory.GetDirectories(yearDir))
                {
                    // If the directory is under pending processing directory, ignore it
                    if (yearDir.IsSubPathOf(context.Configuration.PendingProcessingDirectory.ToFullPath()))
                    {
                        log.LogInformation($"Directory ignored since it's under pending processing directory: {yearDir}");
                        continue;
                    }

                    // Check for invalid month
                    if (monthDir.LastPart().Length != 2 || !int.TryParse(monthDir.LastPart(), out var month))
                    {
                        log.LogWarning($"The name of month directory is not valid: {monthDir.LastPart()}");
                        continue;
                    }

                    DateTime date;
                    try
                    {
                        date = new DateTime(year, month, 1);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        log.LogWarning($"The year or month of the directory is not valid: {monthDir}");
                        continue;
                    }

                    // Start evaluating each file
                    var group = date.ToDirectory();
                    int count = 0;
                    foreach (var path in GetAllFilesInDirectory(monthDir))
                    {
                        count++;

                        var metadata = MetadataAnalyzer.Extract(path);
                        metadata.Group = group;
                        context.AddLibraryFiles(path, group, metadata);
                    }

                    log.LogInformation($"Group {group} contains {count} files");
                }
            }
        }
    }
}
