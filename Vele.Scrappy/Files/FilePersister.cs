using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vele.Scrappy.Storage;

namespace Vele.Scrappy.Files
{
    public class FilePersister
    {
        private readonly string _outputFolder;
        private readonly Uri _websiteUri;
        private readonly ScrapedItemStorage _storage;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cts;

        public FilePersister(
            string outputFolder,
            Uri websiteUri,
            ScrapedItemStorage storage,
            ILogger logger)
        {
            _outputFolder = outputFolder;
            _websiteUri = websiteUri;
            _storage = storage;
            _logger = logger;
            _cts = new CancellationTokenSource();
        }

        public Task Persist()
        {
            return Task.Run(() => PersistLoop(_cts.Token), _cts.Token);
        }

        public void Complete()
        {
            _cts.Cancel();
        }

        private async Task PersistLoop(CancellationToken ct)
        {
            while (true)
            {
                _logger.Information("Looking for files to persist");

                IList<ScrapedItem> itemsToPersist = _storage.GetInStatus(ScrapedItemStatus.Scraped);

                if (!itemsToPersist.Any())
                {
                    if (ct.IsCancellationRequested)
                    {
                        _logger.Information("Completed persisting files");
                        break;
                    }
                    else
                    {
                        _logger.Information("No files to persist, waiting ...");
                        await Task.Delay(250, CancellationToken.None);
                        continue;
                    }
                }

                _logger.Information($"Persisting {itemsToPersist.Count} files");

                foreach (ScrapedItem item in itemsToPersist)
                {
                    await PersistSingleItem(item);
                }
            }
        }

        private async Task PersistSingleItem(ScrapedItem item)
        {
            string fullFilePath = DetermineFilePath(item.AbsoluteWebPath);
            string directoryPath = Path.GetDirectoryName(fullFilePath);

            try
            {
                _logger.Information($"Writing file '{fullFilePath}'");
                Directory.CreateDirectory(directoryPath);

                if (File.Exists(fullFilePath))
                {
                    _logger.Warning($"Duplicate file occurred on {fullFilePath}");
                    item.SetFilePath(fullFilePath);
                    return;
                }

                await File.WriteAllTextAsync(fullFilePath, item.Content);
                item.SetFilePath(fullFilePath);
                _logger.Information($"Completed writing file '{fullFilePath}'");
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, $"Exception occurred while writing file {fullFilePath}");
                item.SetFaulted();
            }
        }

        private string DetermineFilePath(string absoluteWebPath)
        {
            Uri absoluteWebPathUri = new Uri(absoluteWebPath);

            if (absoluteWebPathUri.Scheme != _websiteUri.Scheme)
            {
                // HACK: in case there's been an http/https mismatch,
                // let's just replace the current path scheme with the one from the website base
                absoluteWebPathUri = new Uri(absoluteWebPath.Replace($"{absoluteWebPathUri.Scheme}:", $"{absoluteWebPathUri.Scheme}:"));
            }

            string relativeUri = _websiteUri.MakeRelativeUri(absoluteWebPathUri).ToString();
            if (string.IsNullOrEmpty(relativeUri))
            {
                // HACK: in case the relative path is empty, we're probably looking at the website base,
                // and this we can name as index
                relativeUri = "index";
            }

            string filePath = Path.Combine(_outputFolder, $"{_websiteUri.Host}", relativeUri.ToString());
            filePath = filePath.TrimEnd('/');           // remove trailing slash
            filePath = filePath.Replace('/', '\\');     // unify slashes

            string fileExtension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(fileExtension))
            {
                // HACK: in case the extension is not present, we're probably looking at a HTML file,
                // and we can append an .html extension in order to save it as such
                // maybe we could also check if the content starts with '<!doctype html>'
                filePath += ".html";
            }

            return filePath;
        }
    }
}
