using System;
using System.Net;
using System.Threading.Tasks;

namespace PasswordManager
{
    public sealed class ThumbnailCache : StringCache
    {
        private readonly string cacheDirectory;
        private const string IMAGE_SUFFIX = "png";

        public ThumbnailCache(string cacheDirectory)
        {
            this.cacheDirectory = cacheDirectory;
        }

        protected override string MappingFile => $"{cacheDirectory}\\mapping.bin";

        public async Task<string> GetImageFileNameAsync(string url)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return GetImageFileName(url);
            });
        }

        public string GetImageFileName(string url1)
        {
            var host = GetHostFromUrl(url1);
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
                var webclient = new WebClient();
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
