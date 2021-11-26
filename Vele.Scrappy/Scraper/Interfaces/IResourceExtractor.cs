using HtmlAgilityPack;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Vele.Scrappy.Scraper.Interfaces
{
    public interface IResourceExtractor
    {
        public Task<IList<string>> Extract(HtmlDocument document);
    }
}
