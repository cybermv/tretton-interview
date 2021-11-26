using System;

namespace Vele.Scrappy.Storage
{
    public class ScrapedItem
    {
        public ScrapedItemStatus Status { get; private set; }

        public string AbsoluteWebPath { get; private set; }

        public string Content { get; private set; }

        public string FilePath { get; set; }

        public ScrapedItem(string absoluteWebPath)
        {
            AbsoluteWebPath = absoluteWebPath;
            Status = ScrapedItemStatus.Discovered;
        }

        public void SetContent(string content)
        {
            if (Status != ScrapedItemStatus.Discovered)
            {
                throw new InvalidOperationException($"Item with absolute web path '{AbsoluteWebPath}' has already been scraped");
            }

            Content = content;
            Status = ScrapedItemStatus.Scraped;
        }

        public void SetFilePath(string filePath)
        {
            if (Status != ScrapedItemStatus.Scraped)
            {
                throw new InvalidOperationException($"Item with absolute web path '{AbsoluteWebPath}' hasn't been scraped yet");
            }

            FilePath = filePath;
            Status = ScrapedItemStatus.Persisted;
        }

        public void SetFaulted()
        {
            Status = ScrapedItemStatus.Faulted;
        }
    }
}
