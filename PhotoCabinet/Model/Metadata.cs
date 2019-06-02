using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Metadata
    { 
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public DateTime? TimeInferredFromFileName { get; set; }

        public DateTime? TimeTaken { get; set; }

        public DateTime TimeFileCreated { get; set; }

        public DateTime TimeFileModified { get; set; }
    }
}
