using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Vele.Scrappy
{
    public class Scrappy
    {
        private readonly string _outputFolder;
        private readonly Uri _websiteUrl;
        private readonly bool _downloadExternal;
        private readonly int _parallelExecutionCount;
        private readonly ILogger _logger;

        public Scrappy(string outputFolder, string websiteUrl, bool downloadExternal, int parallelExecutionCount, ILogger outputLogger)
        {
            if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder))
                throw new ArgumentException($"Output folder '{outputFolder}' is invalid or doesn't exist.", nameof(outputFolder));

            if (string.IsNullOrEmpty(websiteUrl) || !Uri.TryCreate(websiteUrl, UriKind.Absolute, out Uri parsedUri))
                throw new ArgumentException($"Website URI '{websiteUrl}' is invalid.", nameof(websiteUrl));

            if (parallelExecutionCount <= 0)
                throw new ArgumentException("The parallel execution count has to be greater than 0", nameof(parallelExecutionCount));

            _outputFolder = outputFolder;
            _websiteUrl = parsedUri;
            _downloadExternal = downloadExternal;
            _parallelExecutionCount = parallelExecutionCount;
            _logger = outputLogger ?? Logger.None;

            ThreadPool.SetMaxThreads(_parallelExecutionCount, _parallelExecutionCount);
        }

        public async Task ScrapeWebsite()
        {
            // TODO: perform scraping
        }
    }
}
