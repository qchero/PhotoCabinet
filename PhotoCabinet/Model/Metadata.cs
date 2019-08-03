using Microsoft.Extensions.Logging;
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

        public string FileName { get; set; }

        /// <summary>
        /// The year-month group the photo belongs to, e.g. "2019/09"
        /// </summary>
        public string Group { get; set; }

        public DateTime? TimeInferredFromFileName { get; set; }

        public DateTime? TimeTaken { get; set; }

        public DateTime TimeFileCreated { get; set; }

        public DateTime TimeFileModified { get; set; }

        public string Md5 => this.Md5Func();

        public Func<string> Md5Func { get; set; }

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

            if (TimeTaken.HasValue)
            {
                return TimeTaken.Value;
            }

            return DateTime.MinValue;
        }

        public bool IsSameMedia(Metadata otherMetadata)
        {
            return this.Md5 == otherMetadata.Md5;
        }
    }
}
