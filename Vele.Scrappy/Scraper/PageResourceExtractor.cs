using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vele.Scrappy.Extensions;
using Vele.Scrappy.Scraper.Interfaces;

namespace Vele.Scrappy.Scraper
{
    public class PageResourceExtractor : IResourceExtractor
    {
        private readonly Uri _baseUri;
        private readonly bool _shouldDownloadExternal;
        private readonly ILogger _logger;

        public PageResourceExtractor(Uri baseUri, bool shouldDownloadExternal, ILogger logger)
        {
            _baseUri = baseUri;
            _shouldDownloadExternal = shouldDownloadExternal;
            _logger = logger;
        }

        public async Task<IList<string>> Extract(HtmlDocument document)
        {
            List<string> resources = document
                .DocumentNode
                .Descendants()
                .Where(n => !n.Name.Equals("a", StringComparison.OrdinalIgnoreCase) &&
                            (!string.IsNullOrEmpty(n.Attributes["href"]?.Value) ||
                            !string.IsNullOrEmpty(n.Attributes["src"]?.Value)))
                .Select(n => (n.Attributes["href"] ?? n.Attributes["src"]).Value)
                .ToList();

            List<string> validResources = new List<string>();
            foreach (string resource in resources)
            {
                Uri parsedResourceLink;
                if (!Uri.TryCreate(resource, UriKind.RelativeOrAbsolute, out parsedResourceLink))
                {
                    _logger.Warning($"Bad resource link found in page; '{resource}'; ignoring");
                    continue;
                }

                if (!parsedResourceLink.IsAbsoluteUri)
                {
                    // make the uri absolute using the website base
                    string absoluteUriString = UriExtensions.Combine(_baseUri.ToString(), parsedResourceLink.ToString());
                    if (!Uri.TryCreate(absoluteUriString, UriKind.Absolute, out parsedResourceLink))
                    {
                        _logger.Error($"Couldn't make absolute resource link; '{absoluteUriString}'; ignoring");
                        continue;
                    }
                }
                else
                {
                    if (parsedResourceLink.Host != _baseUri.Host)
                    {
                        // it's an external link - ignore it if user specified not to scrape externals
                        if (_shouldDownloadExternal == false)
                        {
                            continue;
                        }
                    }
                    if (parsedResourceLink.Scheme != _baseUri.Scheme)
                    {
                        // the link is not matching the base website scheme - ignore it
                        // TODO: figure out a way what to do with http/https links
                        continue;
                    }
                }

                string validResourceLink = parsedResourceLink.GetLeftPart(UriPartial.Path);
                if (!validResources.Contains(validResourceLink))
                {
                    validResources.Add(validResourceLink);
                }
            }

            return validResources;
        }

    }
}
