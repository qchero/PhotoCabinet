﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PhotoCabinet.Analyzer;
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

        private IFileNameTransformer m_fileNameTransformer;
        private IFileMover m_fileMover;

        public bool PrepareContext(Context context, ILogger log)
        {
            // First, group all the pending processing files
            Dictionary<string, HashSet<string>> pendingProcessingGroupToFileMap = new Dictionary<string, HashSet<string>>();
            foreach (var path in context.PendingProcessingFiles)
            {
                var metadata = context.FileToMetadataMap[path];
                var preferredTime = metadata.GetPreferredTime();
                if (preferredTime <= DateTime.MinValue)
                {
                    log.LogWarning($"Skipping processing since no preferred time is found: {path}");
                    continue;
                }

                var group = new DateTime(preferredTime.Year, preferredTime.Month, 1).ToDirectory();
                if (!pendingProcessingGroupToFileMap.ContainsKey(group))
                {
                    pendingProcessingGroupToFileMap.Add(group, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                }

                pendingProcessingGroupToFileMap[group].Add(path);
            }

            // Then, for each group, process the files in order
            foreach (var group in pendingProcessingGroupToFileMap)
            {
                var groupDirectory = Path.Combine(context.Configuration.LibraryDirectory, group.Key);

                // Build a map for dedup
                var groupMd5ToFileNameMap = context.LibraryGroupToFileMap.ContainsKey(group.Key)
                    ? context.LibraryGroupToFileMap[group.Key]
                        .Select(p => context.FileToMetadataMap[p])
                        .ToDictionary(m => m.Md5, m => Path.GetFileName(m.FilePath))
                    : new Dictionary<string, string>();
                var groupFileNameSet = groupMd5ToFileNameMap.Values
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Process each file in the group in the order of file name without extension
                var filesToMove = new List<string>();
                var filesDuplicated = new List<string>();
                var paths = group.Value.OrderBy(p => Path.GetFileNameWithoutExtension(p));
                foreach (var path in paths)
                {
                    // Check for duplicates
                    var metadata = context.FileToMetadataMap[path];
                    if (groupMd5ToFileNameMap.ContainsKey(metadata.Md5))
                    {
                        filesDuplicated.Add($"{path} == {groupMd5ToFileNameMap[metadata.Md5]}");
                        continue;
                    }

                    // Try finding a proper name
                    string newFileName = m_fileNameTransformer.Transform(metadata, context.Configuration.Format, context.Configuration.DedupSuffixFormat, groupFileNameSet);

                    // Update context and Add move action
                    var newPath = Path.Combine(groupDirectory, newFileName);
                    metadata.FilePath = newPath;
                    context.AddLibraryFiles(metadata.FilePath, group.Key, metadata);
                    context.MoveActions.Add(new MoveAction
                    {
                        CurrentPath = path,
                        NewPath = newPath
                    });

                    // Update local map and set
                    groupMd5ToFileNameMap.Add(metadata.Md5, newFileName);
                    groupFileNameSet.Add(newFileName);

                    // Add to logs
                    var renamePath = newFileName == metadata.FileName
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

            return true;
        }
    }
}
