using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet
{
    public class Configuration
    {
        public string WorkDirectory = "./";

        public string PendingProcessingDirectory = "PendingProcessing/";

        public string[] KnownDateFormats =
        {
            "yyyyMMddHHmmss"
        };
    }
}
