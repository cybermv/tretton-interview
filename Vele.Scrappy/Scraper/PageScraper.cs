using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Vele.Scrappy.Scraper.Interfaces;
using Vele.Scrappy.Storage;

namespace Vele.Scrappy.Scraper
{
    public class WebsiteScraper
    {
        private readonly HttpClient _client;
        private readonly string _websiteAbsolutePath;
        private readonly ScrapedItemStorage _storage;
        private readonly IResourceExtractor _pageLinkExtractor;
        private readonly IResourceExtractor _pageResourcesExtractor;
        private readonly ILogger _logger;

        public WebsiteScraper(
            string websiteAbsolutePath,
            ScrapedItemStorage storage,
            IResourceExtractor pageLinkExtractor,
            IResourceExtractor pageResourcesExtractor, 
            ILogger logger)
        {
            _client = new HttpClient();
            _websiteAbsolutePath = websiteAbsolutePath;
            _storage = storage;
            _pageLinkExtractor = pageLinkExtractor;
            _pageResourcesExtractor = pageResourcesExtractor;
            _logger = logger;
        }

        public Task Scrape()
        {
            return ScrapePageRecurse(_websiteAbsolutePath);
        }

        private async Task ScrapePageRecurse(string absolutePath)
        {
            ScrapedItem scrapedItem;
            bool shouldContinue = _storage.StoreAndGet(absolutePath, out scrapedItem);
            if (!shouldContinue)
            {
                return;
            }

            try
            {
                _logger.Information($"GET {absolutePath}");
                scrapedItem.SetContent(await _client.GetStringAsync(absolutePath));
                _logger.Information($"Completed GET {absolutePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occurred while performing GET {absolutePath}");
                throw;
            }

            HtmlDocument currentPageHtml = new HtmlDocument();
            currentPageHtml.LoadHtml(scrapedItem.Content);

            IList<string> validLinks = await _pageLinkExtractor.Extract(currentPageHtml);
            IList<string> validResources = await _pageResourcesExtractor.Extract(currentPageHtml);

            List<Task> childScrapingTasks = new List<Task>();
            foreach (string link in validLinks)
            {
                Task scrapeTask = ScrapePageRecurse(link);
                childScrapingTasks.Add(scrapeTask);
            }

            foreach (string resource in validResources)
            {
                Task scrapeTask = ScrapeResource(resource);
                childScrapingTasks.Add(scrapeTask);
            }

            await Task.WhenAll(childScrapingTasks);
        }

        private async Task ScrapeResource(string absolutePath)
        {
            ScrapedItem scrapedItem;
            bool shouldContinue = _storage.StoreAndGet(absolutePath, out scrapedItem);
            if (!shouldContinue)
            {
                return;
            }

            try
            {
                _logger.Information($"GET {absolutePath}");
                scrapedItem.SetContent(await _client.GetStringAsync(absolutePath));
                _logger.Information($"Completed GET {absolutePath}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error occurred while performing GET {absolutePath}");
                throw;
            }
        }
    }
}
