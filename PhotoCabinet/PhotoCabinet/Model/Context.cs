using System.Collections.Generic;

namespace PhotoCabinet.Model
{
    public class Context
    {
        public Configuration Configuration { get; set; }

        public Library Library { get; set; }

        public Dictionary<string, Metadata> FilePathToMetadataMap { get; set; }

        public IEnumerable<MoveAction> MoveActions { get; set; }

        public IEnumerable<CopyAction> CopyActions { get; set; }
    }
}
