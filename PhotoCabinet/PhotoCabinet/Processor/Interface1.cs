using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet.Processor
{
    interface IProcessor
    {
        string Preview();

        bool Process();
    }
}
