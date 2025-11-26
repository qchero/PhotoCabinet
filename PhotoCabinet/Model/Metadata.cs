using Microsoft.Extensions.Logging;
using PhotoCabinet.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PhotoCabinet
{
    public class Metadata
    {
        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public MediaType MediaType { get; set; }

        /// <summary>
        /// The year-month group the photo belongs to, e.g. "2019/09"
        /// </summary>
        public string Group { get; set; }

        public DateTime? TimeInferredFromFileName { get; set; }

        public Lazy<DateTime?> TimeTaken { get; set; }

        public Lazy<DateTime> TimeFileCreated { get; set; }

        public Lazy<DateTime> TimeFileModified { get; set; }

        public Lazy<string> Md5;

        /// <summary>
        /// Choose the preferred datetime in the order of:
        /// TimeInferredFromFileName => TimeTaken //=> TimeFileModified
        /// </summary>
        public DateTime GetPreferredTime()
        {
            if (TimeInferredFromFileName.HasValue)
            {
                return TimeInferredFromFileName.Value;
            }

            if (TimeTaken.Value.HasValue)
            {
                return TimeTaken.Value.Value;
            }

            return DateTime.MinValue;
        }

        public bool IsSameMedia(Metadata otherMetadata)
        {
            return this.Md5 == otherMetadata.Md5;
        }
    }
}
