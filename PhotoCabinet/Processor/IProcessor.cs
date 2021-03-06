﻿using Microsoft.Extensions.Logging;
using PhotoCabinet.Model;

namespace PhotoCabinet.Processor
{
    public interface IProcessor
    {
        bool PrepareContext(Context context, ILogger log);

        bool ProcessContext(Context context, ILogger log);
    }
}
