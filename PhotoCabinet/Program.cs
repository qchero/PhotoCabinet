using Microsoft.Extensions.Logging;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using System;
using System.Collections.Generic;
using System.IO;

namespace PhotoCabinet
{
    class Program
    {
        static readonly List<IProcessor> Processors = new List<IProcessor>
        {
            new FileDiscoverProcessor(),
            new FileRenameProcessor()
        };

        static void Main(string[] _)
        {
            var context = new Context();

            var logFilePath = Path.Combine(context.Configuration.LibraryDirectory, "log-{Date}.log");
            var loggerFactory = new LoggerFactory()
                .AddFile(pathFormat: logFilePath,
                    outputTemplate: "{Timestamp:s} [{Level:u3}] ({EventId:x8}) {Message}{NewLine}{Exception}");

            foreach (var processor in Processors)
            {
                try
                {
                    var log = loggerFactory.CreateLogger(processor.GetType());

                    log.LogInformation("=====================================================");
                    log.LogInformation($"Starting {processor.GetType()}");
                    if (!processor.PrepareContext(context, log))
                    {
                        throw new Exception($"Failed to prepare context for {processor.GetType()}");
                    }

                    Console.WriteLine($"Please check the log file for pending operations in: {context.Configuration.LibraryDirectory}");
                    Console.Write("Confirm if you want to proceed (Y/N):");
                    var input = Console.ReadLine();
                    if (!input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Exit as user chose not to proceed");
                        log.LogWarning("Exit as user chose not to proceed");
                        return;
                    }

                    if (!processor.ProcessContext(context, log))
                    {
                        throw new Exception($"Failed to process context for {processor.GetType()}");
                    }
                }
                catch (Exception exception)
                {
                    loggerFactory.CreateLogger("Program").LogError(exception, exception.Message);
                }
            }

            return;
        }
    }
}
