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
    public class PageLinkExtractor : IResourceExtractor
    {
        private readonly Uri _baseUri;
        private readonly ILogger _logger;

        public PageLinkExtractor(Uri baseUri, ILogger logger)
        {
            _baseUri = baseUri;
            _logger = logger;
        }

        public async Task<IList<string>> Extract(HtmlDocument parsedDocument)
        {
            List<string> links = parsedDocument
                .DocumentNode
                .Descendants()
                .Where(n => n.Name.Equals("a", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrEmpty(n.Attributes["href"]?.Value))
                .Select(n => n.Attributes["href"].Value)
                .ToList();

            List<string> validLinks = new List<string>();
            foreach (string link in links)
            {
                Uri parsedLink;
                if (!Uri.TryCreate(link, UriKind.RelativeOrAbsolute, out parsedLink))
                {
                    _logger.Error($"Bad link found in page; '{link}'; ignoring");
                    continue;
                }

                if (!parsedLink.IsAbsoluteUri)
                {
                    // make the uri absolute using the website base
                    string absoluteUriString = UriExtensions.Combine(_baseUri.ToString(), parsedLink.ToString());
                    if (!Uri.TryCreate(absoluteUriString, UriKind.Absolute, out parsedLink))
                    {
                        _logger.Error($"Couldn't make absolute link; '{absoluteUriString}'; ignoring");
                        continue;
                    }
                }
                else
                {
                    if (parsedLink.Host != _baseUri.Host)
                    {
                        // it's an external link - ignore it
                        continue;
                    }
                    if (parsedLink == _baseUri)
                    {
                        // it's linking to itsels - ignore it
                        continue;
                    }
                    if (parsedLink.Scheme != _baseUri.Scheme)
                    {
                        // the link is not matching the base website scheme - ignore it
                        // TODO: figure out a way what to do with http/https links
                        continue;
                    }
                }

                string validLink = parsedLink.GetLeftPart(UriPartial.Path);
                if (!validLinks.Contains(validLink))
                {
                    validLinks.Add(validLink);
                }
            }

            return validLinks;
        }
    }
}
