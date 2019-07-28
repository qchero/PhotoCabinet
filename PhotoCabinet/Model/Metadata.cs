using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        public string Md5 { get; set; }

        /// <summary>
        /// Choose the preferred datetime in the order of:
        /// TimeInferredFromFileName => TimeTaken => TimeFileModified
        /// </summary>
        public DateTime PreferredTime(ILogger log)
        {
            if (TimeInferredFromFileName.HasValue)
            {
                return TimeInferredFromFileName.Value;
            }

            if (TimeTaken.HasValue)
            {
                return TimeInferredFromFileName.Value;
            }

            log.LogWarning($"No time found for {FilePath}, fallback to file modified time");

            return TimeFileModified;
        }

        public bool IsSameMedia(Metadata otherMetadata)
        {
            return this.Md5 == otherMetadata.Md5;
        }
    }
}
