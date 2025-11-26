using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PhotoCabinet.FileOperation;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using PhotoCabinet.Utility;

namespace PhotoCabinet
{
    /// <summary>
    /// Place all PendingProcessingFiles to the target group directory
    /// [Context] Update library to include PendingProcessingFiles
    /// [Context] Add MoveActions for moving PendingProcessingFiles to target group directory
    /// </summary>
    public class FileGroupingProcessor : IProcessor
    {
        public FileGroupingProcessor(IFileNameTransformer fileNameTransformer, IFileMover fileMover)
        {
            m_fileNameTransformer = fileNameTransformer ?? throw new ArgumentNullException();
            m_fileMover = fileMover ?? throw new ArgumentNullException();
        }

        private readonly IFileNameTransformer m_fileNameTransformer;
        private readonly IFileMover m_fileMover;

        private Dictionary<string, int> m_groupToFileCount;

        public bool PrepareContext(Context context, ILogger log)
        {
            // Take a snapshot before grouping
            TakeGroupCountSnapshot(context, log);

            // First, group all the pending processing files
            var noDateFiles = new List<string>();
            var pendingProcessingGroupToFileMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in context.PendingProcessingFiles)
            {
                var metadata = context.FileToMetadataMap[path];
                var preferredTime = metadata.GetPreferredTime();
                if (preferredTime <= DateTime.MinValue)
                {
                    noDateFiles.Add(path);
                    //log.LogWarning($"Skipping processing since no preferred time is found: {path}");
                    continue;
                }

                var group = new DateTime(preferredTime.Year, preferredTime.Month, 1).ToDirectory();
                if (!pendingProcessingGroupToFileMap.ContainsKey(group))
                {
                    pendingProcessingGroupToFileMap.Add(group, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                }

                pendingProcessingGroupToFileMap[group].Add(path);
            }

            // Then, Move no date files to no date directory
            var noDateFileMoveLogs = new List<string>();
            foreach (var path in noDateFiles)
            {
                var metadata = context.FileToMetadataMap[path];
                var currentFileName = metadata.FileName;
                var newFileName = m_fileNameTransformer.TransformSuffix(Path.GetFileNameWithoutExtension(path), Path.GetExtension(path), context.Configuration.DedupSuffixFormat, context.NoDateFileNameSet);
                var newPath = Path.Combine(context.Configuration.LibraryDirectory, context.Configuration.NoDateDirectoryName, newFileName);
                metadata.FilePath = newPath;
                context.AddNoDateFiles(newFileName, metadata);
                context.MoveActions.Add(new MoveAction
                {
                    CurrentPath = path,
                    NewPath = newPath
                });

                // Log
                var renamePath = newFileName == currentFileName
                        ? string.Empty
                        : $" => {newFileName}";
                noDateFileMoveLogs.Add($"{path}{renamePath}");
            }

            if (noDateFileMoveLogs.Count > 0)
            {
                log.LogInformationWithPaths($"{noDateFileMoveLogs.Count} no-date files will be moved to {context.Configuration.NoDateDirectoryName}", noDateFileMoveLogs);
            }

            // Then, for each group, process the files in order
            foreach (var group in pendingProcessingGroupToFileMap)
            {
                var groupDirectory = Path.Combine(context.Configuration.LibraryDirectory, group.Key);

                // Process each file in the group in the order of file name without extension
                var filesToMove = new List<string>();
                var filesDuplicated = new List<string>();
                var paths = group.Value.OrderBy(p => Path.GetFileNameWithoutExtension(p));
                foreach (var path in paths)
                {
                    // De-duplicate within the current latest library
                    var metadata = context.FileToMetadataMap[path];
                    if (context.LibraryMd5ToFileMap.ContainsKey(metadata.Md5.Value))
                    {
                        filesDuplicated.Add($"{path} == {context.LibraryMd5ToFileMap[metadata.Md5.Value]}   MD5:{metadata.Md5.Value}");
                        continue;
                    }

                    // Try finding a proper name
                    string newFileName = m_fileNameTransformer.Transform(metadata, context.Configuration.Format, context.Configuration.DedupSuffixFormat, context.LibraryFileNameSet);

                    // Update context and Add move action
                    var currentFileName = metadata.FileName;
                    var newPath = Path.Combine(groupDirectory, newFileName);
                    metadata.FilePath = newPath;
                    context.AddLibraryFiles(metadata.FilePath, group.Key, metadata);
                    context.MoveActions.Add(new MoveAction
                    {
                        CurrentPath = path,
                        NewPath = newPath
                    });

                    // Add to logs
                    var renamePath = newFileName == currentFileName
                        ? string.Empty
                        : $" => {newFileName}";
                    filesToMove.Add($"{path}{renamePath}");
                }

                // Log a moving group
                if (filesToMove.Count > 0)
                {
                    log.LogInformationWithPaths($"{filesToMove.Count} files will be moved to group {group.Key} at: {groupDirectory}", filesToMove);
                }

                if (filesDuplicated.Count > 0)
                {
                    log.LogInformationWithPaths($"{filesDuplicated.Count} files are skipped because of duplication:", filesDuplicated);
                }
            }

            return true;
        }

        public bool ProcessContext(Context context, ILogger log)
        {
            foreach (var moveAction in context.MoveActions)
            {
                m_fileMover.Move(moveAction.CurrentPath, moveAction.NewPath);
                log.LogInformation($"Moved file {moveAction.CurrentPath} => {moveAction.NewPath}");
            }

            DiffGroupCountSnapshot(context, log);

            return true;
        }

        private void TakeGroupCountSnapshot(Context context, ILogger log)
        {
            m_groupToFileCount = context.LibraryGroupToFileMap
                .ToDictionary(p => p.Key, p => p.Value.Count);
        }

        private void DiffGroupCountSnapshot(Context context, ILogger log)
        {
            foreach (var p in context.LibraryGroupToFileMap)
            {
                var oldCount = m_groupToFileCount.ContainsKey(p.Key)
                    ? m_groupToFileCount[p.Key]
                    : 0;
                var newCount = p.Value.Count;
                if (newCount == oldCount)
                {
                    continue;
                }

                log.LogInformation($"Group {p.Key} has changes in files: {oldCount} => {newCount}");
            }
        }
    }
}
