using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoCabinet
{
    public class FileDiscoverProcessor : IProcessor
    {
        public bool PrepareContext(Context context)
        {
            var allFilesInDirectory = FileDiscoveryHelper.GetAllFilesInDirectory(
                context.Configuration.PendingProcessingDirectory.ToFullPath());
            context.FilePathToMetadataMap = allFilesInDirectory
                .ToDictionary(
                    path => path,
                    path => MetadataExtractor.Extract(path));

            return true;
        }

        public bool ProcessContext(Context context)
        {
            throw new System.NotImplementedException();
        }
    }
}
