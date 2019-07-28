using System.Collections.Generic;

namespace PhotoCabinet.Model
{
    public class Context
    {
        public Context()
        {
            Configuration = new Configuration();
            LibraryFiles = new List<string>();
            LibraryGroupToFileMap = new Dictionary<string, HashSet<string>>();
            PendingProcessingFiles = new List<string>();
            FileToMetadataMap = new Dictionary<string, Metadata>();
        }

        public Configuration Configuration { get; set; }

        public List<string> LibraryFiles { get; private set; }

        public Dictionary<string, HashSet<string>> LibraryGroupToFileMap { get; private set; }

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

            if (!LibraryGroupToFileMap.ContainsKey(group))
            {
                LibraryGroupToFileMap.Add(group, new HashSet<string>());
            }

            LibraryGroupToFileMap[group].Add(path);
        }
    }
}
