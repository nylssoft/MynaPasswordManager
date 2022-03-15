/*
    Myna Password Manager
    Copyright (C) 2017-2022 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Diagnostics;
using System.IO;
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

        protected override string MappingFile => $"{cacheDirectory}\\mapping.json";

        public async Task<string> GetImageFileNameAsync(string url)
        {
            return await Task<string>.Factory.StartNew(() =>
            {
                return GetImageFileName(url);
            });
        }

        public string GetImageFileName(string url)
        {
            string fn = null;
            var domainName = GetDomainNameFromUrl(url);
            if (!string.IsNullOrEmpty(domainName))
            {
                lock (mappings)
                {
                    if (mappings.TryGetValue(domainName, out var filename))
                    {
                        if (filename == null || File.Exists(filename))
                        {
                            return filename;
                        }
                    }
                }
                try
                {
                    fn = $"{cacheDirectory}\\{domainName}.{IMAGE_SUFFIX}";
                    if (!File.Exists(fn))
                    {
                        var webclient = new WebClient();
                        Debug.WriteLine($"Download favicon for {domainName} to file {fn}.");
                        webclient.DownloadFile($"http://www.google.com/s2/favicons?domain={domainName}", fn);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to download favicon for domain {domainName}. {ex.Message}");
                    fn = null;
                }
                lock (mappings)
                {
                    mappings[domainName] = fn;
                }
            }
            return fn;
        }

        private static string GetDomainNameFromUrl(string url)
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
                    var domainName = new Uri(url).Host;
                    var arr = domainName.Split(".");
                    if (arr.Length > 2)
                    {
                        domainName = $"{arr[^2]}.{arr[^1]}";
                    }
                    return domainName.ToLowerInvariant();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Cannot create URL for '{url}'. {ex.Message}");
                }
            }
            return null;
        }
    }
}
