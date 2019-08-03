using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Configuration
    {
        public string LibraryDirectory = @"F:\OneDrive\Photo Library";

        public string PendingProcessingDirectory = @"F:\OneDrive\Photo Library\PendingProcessing";

        public string Format = @"[type]_[date]_[time]";

        public string DedupSuffixFormat = @"_[num]";

        public string[] KnownDateFormats =
        {
            "yyyyMMdd_HHmmss"
        };
    }
}
