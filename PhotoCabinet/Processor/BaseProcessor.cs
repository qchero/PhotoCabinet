using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using PhotoCabinet.Model;

namespace PhotoCabinet.Processor
{
    public abstract class BaseProcessor : IProcessor
    {
        public IProcessor NextProcessor { get; set; }

        BaseProcessor(IProcessor nextProcessor)
        {
            this.NextProcessor = nextProcessor;
        }

        public abstract bool PrepareContext(Context context, ILogger log);

        public abstract bool ProcessContext(Context context, ILogger log);
    }
}
