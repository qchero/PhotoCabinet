using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet.Model
{
    public class CopyAction
    {
        bool IsPerformed { get; set; }

        string CurrentPath { get; set; }

        string NewPath { get; set; }
    }
}
