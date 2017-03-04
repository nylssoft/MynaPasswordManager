using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    public sealed class ThumbnailCache
    {
        private Dictionary<string, string> mappings;
        private readonly string cacheDirectory;
        private const string IMAGE_SUFFIX = "png";

        public ThumbnailCache(string cacheDirectory)
        {
            mappings = new Dictionary<string, string>();
            this.cacheDirectory = cacheDirectory;
        }

        public void Load()
        {
            var mappingFile = CacheMappingFile;
            if (File.Exists(mappingFile))
            {
                IFormatter formatter = new BinaryFormatter();
                List<Tuple<string, string>> list;
                using (var fs = new FileStream(mappingFile, FileMode.Open))
                {
                    list = (List<Tuple<string, string>>)formatter.Deserialize(fs);
                }
                lock (mappings)
                {
                    foreach (var item in list)
                    {
                        mappings.Add(item.Item1, item.Item2);
                    }
                }
            }
        }

        public void Clean()
        {
            var usedFileNames = new HashSet<string>();
            var deletedKeys = new HashSet<string>();
            lock (mappings)
            {
                foreach (var item in mappings)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        if (!File.Exists(item.Value))
                        {
                            deletedKeys.Add(item.Key);
                        }
                        else
                        {
                            FileInfo fi = new FileInfo(item.Value);
                            usedFileNames.Add(fi.Name);
                        }
                    }
                }
                foreach (var key in deletedKeys)
                {
                    mappings.Remove(key);
                }
            }
            foreach (var direntry in Directory.EnumerateFiles(cacheDirectory, $"*.{IMAGE_SUFFIX}"))
            {
                FileInfo fi = new FileInfo(direntry);
                if (!usedFileNames.Contains(fi.Name) && File.Exists(direntry))
                {
                    File.Delete(direntry);
                }
            }
        }

        public void Save()
        {
            IFormatter formatter = new BinaryFormatter();
            using (var fs = new FileStream(CacheMappingFile, FileMode.Create))
            {
                var list = new List<Tuple<string, string>>();
                lock (mappings)
                {
                    foreach (var entry in mappings)
                    {
                        if (!string.IsNullOrEmpty(entry.Value))
                        {
                            list.Add(Tuple.Create(entry.Key, entry.Value));
                        }
                    }
                }
                formatter.Serialize(fs, list);
            }
        }

        public bool Remove(string url)
        {
            string host = GetHostFromUrl(url);
            if (!string.IsNullOrEmpty(host))
            {
                lock (mappings)
                {
                    return mappings.Remove(host);
                }
            }
            return false;
        }

        public async Task<string> GetImageFileNameAsync(string url)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return GetImageFileName(url);
            });
        }

        public string GetImageFileName(string url1)
        {
            string host = GetHostFromUrl(url1);
            string filename = null;
            if (!string.IsNullOrEmpty(host))
            {
                lock (mappings)
                {
                    if (mappings.TryGetValue(host, out filename))
                    {
                        return filename;
                    }
                }
                WebClient webclient = new WebClient();
                try
                {
                    filename = $"{cacheDirectory}\\{Guid.NewGuid().ToString()}.{IMAGE_SUFFIX}";
                    webclient.DownloadFile($"http://www.google.com/s2/favicons?domain={host}", filename);
                }
                catch (Exception)
                {
                    // ignored
                }
                lock (mappings)
                {
                    mappings.Add(host, filename);
                }
            }
            return filename;
        }

        private string CacheMappingFile
        {
            get
            {
                return $"{cacheDirectory}\\mapping.bin";
            }
        }

        private static string GetHostFromUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                url = url.ToLowerInvariant();
                if (!url.StartsWith("http://") &&
                    !url.StartsWith("https://"))
                {
                    url = $"https://{url}";
                }
                try
                {
                    return new Uri(url).Host;
                }
                catch (Exception)
                {
                    // ignored
                }
            }
            return null;
        }
    }
}
