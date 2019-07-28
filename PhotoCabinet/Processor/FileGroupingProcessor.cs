using Microsoft.Extensions.Logging;
using PhotoCabinet.Analyzer;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;

namespace PhotoCabinet
{
    public class FileGroupingProcessor : IProcessor
    {
        /// <summary>
        /// Discover all the files in library, pending processing directory and failed processing directory
        /// And retrive the metadata
        /// </summary>
        public bool PrepareContext(Context context, ILogger log)
        {
            ValidateContext(context);

            

            return true;
        }

        /// <summary>
        /// By design this does nothing
        /// </summary>
        public bool ProcessContext(Context context, ILogger log)
        {
            return true;
        }

        private static void ValidateContext(Context context)
        {
            
        }
    }
}
