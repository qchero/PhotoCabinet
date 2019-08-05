using System;
using System.Collections.Generic;

namespace PhotoCabinet.Model
{
    public class Context
    {
        public Context()
        {
            Configuration = new Configuration();
            LibraryFiles = new List<string>();
            LibraryFileNameSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            LibraryGroupToFileMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            LibraryMd5ToFileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            PendingProcessingFiles = new List<string>();
            FileToMetadataMap = new Dictionary<string, Metadata>(StringComparer.OrdinalIgnoreCase);
            MoveActions = new List<MoveAction>();
        }

        public Configuration Configuration { get; set; }

        public List<string> LibraryFiles { get; private set; }

        public HashSet<string> LibraryFileNameSet { get; private set; }

        public Dictionary<string, HashSet<string>> LibraryGroupToFileMap { get; private set; }

        /// <summary>
        /// Library MD5 to file path map
        /// Used for global deduplication
        /// </summary>
        public Dictionary<string, string> LibraryMd5ToFileMap { get; private set; }

        /// <summary>
        /// List of files to be processed
        /// </summary>
        public List<string> PendingProcessingFiles { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Metadata> FileToMetadataMap { get; private set; }

        public List<MoveAction> MoveActions { get; set; }

        public void AddPendingProcessingFiles(string path, Metadata metadata)
        {
            PendingProcessingFiles.Add(path);
            FileToMetadataMap[path] = metadata;
        }

        public void AddLibraryFiles(string path, string group, Metadata metadata)
        {
            LibraryFiles.Add(path);
            FileToMetadataMap[path] = metadata;

            // Add to LibraryGroupToFileMap
            if (!LibraryGroupToFileMap.ContainsKey(group))
            {
                LibraryGroupToFileMap.Add(group, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }

            LibraryGroupToFileMap[group].Add(path);

            // Add to LibraryFileNameSet
            LibraryFileNameSet.Add(metadata.FileName);

            // Add to LibraryMd5ToFileMap
            LibraryMd5ToFileMap.Add(metadata.Md5.Value, path);
        }
    }
}
