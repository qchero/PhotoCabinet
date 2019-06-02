using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoCabinet.Utility
{
    public static class LoggerExtension
    {
        public static void LogInformationWithPaths(this ILogger log, string message, IEnumerable<string> paths)
        {
            var stringBuilder = new StringBuilder(message);
            foreach (var path in paths.OrderBy(p => p))
            {
                stringBuilder.Append($"\n    {path}");
            }

            log.LogInformation(stringBuilder.ToString());
        }
    }
}
