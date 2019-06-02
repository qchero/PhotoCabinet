using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Configuration
    {
        public string LibraryDirectory = @"F:\TestTestPhotoCabinet";

        public string PendingProcessingDirectory = @"F:\TestTestPhotoCabinet\PendingProcessing";

        public string FailedProcessingDirectory = @"F:\TestTestPhotoCabinet\FailedProcessing";

        public string[] KnownDateFormats =
        {
            "yyyyMMdd_HHmmss"
        };
    }
}
