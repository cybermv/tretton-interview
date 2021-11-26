using System;
using System.Collections.Generic;
using System.Linq;

namespace Vele.Scrappy.Storage
{
    public class ScrapedItemStorage
    {
        private readonly Dictionary<string, ScrapedItem> _storage;
        private readonly object _lock;

        public ScrapedItemStorage()
        {
            _storage = new Dictionary<string, ScrapedItem>();
            _lock = new object();
        }

        public bool StoreAndGet(string key, out ScrapedItem stored)
        {
            lock (_lock)
            {
                if (_storage.TryGetValue(key, out stored))
                {
                    // item already exists in storage, return false
                    return false;
                }
                else
                {
                    ScrapedItem newItem = new ScrapedItem(key);
                    if (_storage.TryAdd(key, newItem))
                    {
                        // item added to storage, return true
                        stored = newItem;
                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException("bad");
                    }
                }
            }
        }

        public IList<ScrapedItem> GetInStatus(ScrapedItemStatus status)
        {
            lock (_lock)
            {
                return _storage
                    .Where(kv => kv.Value.Status == status)
                    .Select(kv => kv.Value)
                    .ToList();
            }
        }

        public int Count => _storage.Count;
    }
}
