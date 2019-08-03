using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoCabinet.Utility
{
    public static class Helpers
    {
        public static string ToDirectory(this DateTime dateTime)
        {
            return dateTime.ToString(@"yyyy\\MM");
        }
    }
}
