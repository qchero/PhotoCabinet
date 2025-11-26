using Microsoft.Extensions.Logging;
using PhotoCabinet.Analyzer;
using PhotoCabinet.Database;
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
        private Md5Cache m_md5Cache;

        public FileDiscoverProcessor(Md5Cache md5Cache)
        {
            m_md5Cache = md5Cache ?? throw new ArgumentNullException();
        }

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

            // Discover library
            DiscoverLibraryFiles(context, log);

            // Discover no date files
            DiscoverNoDateFiles(context, log);

            // Discover pending processing files
            DiscoverPendingProcessingFiles(context, log);

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
                // Don't use Md5 cache when analyzing pending files
                var metadata = MetadataAnalyzer.Extract(path);
                if (metadata.MediaType == MediaType.Unknown)
                {
                    log.LogWarning($"Skipping file since it's not a image or video: {path}");
                    continue;
                }

                context.AddPendingProcessingFiles(path, metadata);
            }

            log.LogInformation($"{context.PendingProcessingFiles.Count} files pending processing");
        }

        private void DiscoverNoDateFiles(Context context, ILogger log)
        {
            foreach (var path in GetAllFilesInDirectory(Path.Combine(context.Configuration.LibraryDirectory.ToFullPath(), context.Configuration.NoDateDirectoryName)))
            {
                var metadata = MetadataAnalyzer.Extract(path);
                if (metadata.MediaType == MediaType.Unknown)
                {
                    log.LogError($"Skipping file since it's not a image or video: {path}");
                    continue;
                }
                context.AddNoDateFiles(path, metadata);
            }

            log.LogInformation($"{context.Configuration.NoDateDirectoryName} contains {context.NoDateFiles.Count} files");
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

                // If the directory is an ignored directory, ignore it
                var yearDirName = yearDir.LastPart();
                if (context.Configuration.IgnoredDirectories.Any(d => d.Equals(yearDirName, StringComparison.OrdinalIgnoreCase)))
                {
                    log.LogDebug($"Directory ignored since it's an ignored directory: {yearDir}");
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
                    int groupCount = 0;
                    foreach (var path in GetAllFilesInDirectory(monthDir))
                    {
                        var metadata = MetadataAnalyzer.Extract(path, m_md5Cache);
                        if (metadata.MediaType == MediaType.Unknown)
                        {
                            log.LogError($"Skipping file since it's not a image or video: {path}");
                            continue;
                        }

                        // Check for duplicate in library by name and by MD5
                        if (context.LibraryFileNameSet.Contains(metadata.FileName))
                        {
                            log.LogError($"Files with duplicated names in library: {path} == {metadata.FileName}");
                            continue;
                        }

                        if (context.LibraryMd5ToFileMap.ContainsKey(metadata.Md5.Value))
                        {
                            log.LogError($"Files with duplicated MD5 in library: {path} == {context.LibraryMd5ToFileMap[metadata.Md5.Value]}");
                            continue;
                        }

                        // Passed all checks, add to library
                        metadata.Group = group;
                        context.AddLibraryFiles(path, group, metadata);
                        groupCount++;
                    }

                    log.LogInformation($"Group {group} contains {groupCount} files");
                }
            }
        }
    }
}
