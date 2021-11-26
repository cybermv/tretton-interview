using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vele.Scrappy.Scraper;
using Vele.Scrappy.Scraper.Interfaces;
using Vele.Scrappy.Storage;

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
            ScrapedItemStorage storage = new ScrapedItemStorage();

            IResourceExtractor pageLinkExtractor = new PageLinkExtractor(_websiteUrl, _logger);
            IResourceExtractor pageResourcesExtractor = new PageResourceExtractor(_websiteUrl, _downloadExternal, _logger);

            WebsiteScraper scraper = new WebsiteScraper(
                _websiteUrl.ToString(),
                storage,
                pageLinkExtractor,
                pageResourcesExtractor,
                _logger);

            _logger.Information("Initiating scraping task");
            await scraper.Scrape();

            // TODO: persist files on filesystem
            // TODO: handle errors
        }
    }
}
