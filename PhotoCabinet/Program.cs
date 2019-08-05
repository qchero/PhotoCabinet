using Microsoft.Extensions.Logging;
using PhotoCabinet.Database;
using PhotoCabinet.FileOperation;
using PhotoCabinet.Model;
using PhotoCabinet.Processor;
using System;
using System.Collections.Generic;
using System.IO;

namespace PhotoCabinet
{
    class Program
    {
        private static List<IProcessor> Processors(Context context)
        {
            return new List<IProcessor>
            {
                new FileDiscoverProcessor(new Md5Cache(Path.Combine(context.Configuration.LibraryDirectory, "Data", "Md5Cache.db"))),
                new FileGroupingProcessor(new FileNameTransformer(), new FileMover())
            };
        }

        static void Main(string[] _)
        {
            var context = new Context();

            var logFilePath = Path.Combine(context.Configuration.LibraryDirectory, @"ProcessingLogs\log-{Date}.log");
            var loggerFactory = new LoggerFactory()
                .AddFile(pathFormat: logFilePath,
                    outputTemplate: "{Timestamp:s} [{Level:u3}] ({EventId:x8}) {Message}{NewLine}{Exception}");

            var log = loggerFactory.CreateLogger(context.GetType());
            log.LogInformation("=====================================================");
            log.LogInformation("=====================================================");
            log.LogInformation($"Running Photo Cabinet at {DateTime.Now.ToString()}");
            log.LogInformation("=====================================================");
            foreach (var processor in Processors(context))
            {
                try
                {
                    log.LogInformation("=====================================================");
                    log.LogInformation($"Starting {processor.GetType()}");
                    if (!processor.PrepareContext(context, log))
                    {
                        throw new Exception($"Failed to prepare context for {processor.GetType()}");
                    }

                    log.LogInformation("===========================");

                    Console.WriteLine($"Please check the log file for pending operations in: {context.Configuration.LibraryDirectory}");
                    Console.Write("Confirm if you want to proceed (Y/N):");
                    var input = Console.ReadLine();
                    if (!input.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Exit as user chose not to proceed");
                        log.LogWarning("Exit as user chose not to proceed");
                        log.LogInformation("=====================================================");
                        return;
                    }

                    if (!processor.ProcessContext(context, log))
                    {
                        throw new Exception($"Failed to process context for {processor.GetType()}");
                    }

                    log.LogInformation("=====================================================");
                }
                catch (Exception exception)
                {
                    log.LogCritical(exception.Message);
                    log.LogInformation("=====================================================");
                    return;
                }
            }

            log.LogInformation("=====================================================");
            log.LogInformation($"Successfully running Photo Cabinet at {DateTime.Now.ToString()}");
            log.LogInformation("=====================================================");
            log.LogInformation("=====================================================");

            return;
        }
    }
}
