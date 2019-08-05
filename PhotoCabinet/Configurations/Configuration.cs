using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Configuration
    {
        public string LibraryDirectory = @"F:\OneDrive\Photo Library";

        public string PendingProcessingDirectory = @"F:\Photo_PendingProcessing";
            //@"F:\OneDrive\Photo Library\PendingProcessing";

        public string[] IgnoredDirectories =
        {
            @"Data",
            @"ProcessingLogs",
            @"NoDate",
            @"Special",
            @"Duplicates",
            @"PendingProcessing"
        };

        public string Format = @"[type]_[date]_[time]";

        public string DedupSuffixFormat = @"_[num]";

        public string[] KnownDateFormats =
        {
            "yyyyMMdd_HHmmss",
            "yyyy_MM_dd_HH_mm_ss",
        };
    }
}
