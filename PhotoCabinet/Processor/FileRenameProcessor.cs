using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using PhotoCabinet.FileOperation;
using PhotoCabinet.Model;
using PhotoCabinet.Utility;

namespace PhotoCabinet.Processor
{
    public class FileRenameProcessor : IProcessor
    {
        public FileRenameProcessor(IFileMover fileMover)
        {
            m_fileMover = fileMover ?? throw new ArgumentNullException();
        }

        private readonly IFileMover m_fileMover;

        public bool PrepareContext(Context context, ILogger log)
        {
            foreach (var pair in context.LibraryGroupToFileMap)
            {
                if (!pair.Key.StartsWith("2017") && !pair.Key.StartsWith("2018"))
                {
                    continue;
                }

                var group = pair.Key;
                var files = pair.Value;

                var renameLogs = new List<string>();
                foreach (var path in files)
                {
                    var metadata = context.FileToMetadataMap[path];
                    var oldName = metadata.FileName;
                    var newName = RenameWithoutConflict(oldName);
                    if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var newPath = Path.Combine(Path.GetDirectoryName(path), newName);
                    renameLogs.Add($"{metadata.FilePath} => {newPath}");
                    context.MoveActions.Add(new MoveAction
                    {
                        CurrentPath = path,
                        NewPath = newPath
                    });
                }

                log.LogInformationWithPaths($"{renameLogs.Count} files will be renamed:", renameLogs);
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

        private static string RenameWithoutConflict(string oldName)
        {
            return oldName
                .Replace("IMG_", "", StringComparison.OrdinalIgnoreCase)
                .Replace("VID_", "", StringComparison.OrdinalIgnoreCase);
        }
    }
}
