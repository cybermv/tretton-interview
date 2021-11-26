using CommandLine;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using Vele.Scrappy.ConsoleApp.Options;

namespace Vele.Scrappy.ConsoleApp
{
    public static class Program
    {
        private static Logger _logger;

        public static int Main(string[] args)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff}] {Message:lj}{NewLine}")
                .CreateLogger();

            int returnCode = Parser.Default.ParseArguments<ConsoleOptions>(args)
                .MapResult(
                    (ConsoleOptions opts) => Run(opts),
                    (IEnumerable<Error> errs) => HandleErrors(errs));

            _logger.Dispose();

            return returnCode;
        }

        private static int HandleErrors(IEnumerable<Error> errs)
        {
            return -1;
        }

        private static int Run(ConsoleOptions opts)
        {
            _logger.Information($"Starting scraping process for '{opts.WebsiteUrl}'...");

            try
            {
                // TODO: invoke scraping process here!

                _logger.Information("Scraping completed!");
                return 0;
            }
            catch (AggregateException aggrEx)
            {
                _logger.Error($"Exception occured: {aggrEx.InnerException ?? aggrEx}");
                return -1;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception occured: {ex}");
                return -1;
            }
        }
    }
}
