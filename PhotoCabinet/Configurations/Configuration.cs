using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Configuration
    {
        public string LibraryDirectory = @"E:\OneDrive\Photo Library";

        //public string PendingProcessingDirectory = @"E:\Photo_PendingProcessing";
        public string PendingProcessingDirectory = @"E:\OneDrive\Photo Library\PendingProcessing";
        //@"F:\OneDrive\Photo Library\PendingProcessing";

        public string NoDateDirectoryName = @"NoDate";

        public string[] IgnoredDirectories =
        {
            @"ProcessingData",
            @"NoDate",
            @"Special",
            @"Duplicates",
            @"PendingProcessing"
        };

        /// <summary>
        /// Available attributes:
        /// [type]: IMG or VID
        /// [date]: date in yyyyMMdd format
        /// [time]: time in HHmmss format
        /// </summary>
        public string Format = @"[date]_[time]";

        public string DedupSuffixFormat = @"_[num]";

        public string[] KnownDateFormats =
        {
            "yyyyMMdd_HHmmss",
            "yyyy_MM_dd_HH_mm_ss",
            "yyyy-MM-dd HHmmss",
            "yyyyMMddHHmmss"
        };
    }
}
