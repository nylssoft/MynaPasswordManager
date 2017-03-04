using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    public sealed class KeyDirectoryCache
    {
        private Dictionary<string, string> mappings;
        private readonly string cacheDirectory;

        public KeyDirectoryCache(string cacheDirectory)
        {
            mappings = new Dictionary<string, string>();
            this.cacheDirectory = cacheDirectory;
        }

        public string GetLastUsed()
        {
            return Properties.Settings.Default.KeyDirectory.ReplaceSpecialFolder();
        }

        public string Get(string key)
        {
            string val;
            if (mappings.TryGetValue(key, out val))
            {
                return val;
            }
            return GetLastUsed();
        }

        public void Set(string key, string val)
        {
            mappings[key] = val;
            Properties.Settings.Default.KeyDirectory = val;
        }

        public void Load()
        {
            var mappingFile = CacheKeyDirectoryFile;
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

        public void Save()
        {
            IFormatter formatter = new BinaryFormatter();
            using (var fs = new FileStream(CacheKeyDirectoryFile, FileMode.Create))
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

        private string CacheKeyDirectoryFile
        {
            get
            {
                return $"{cacheDirectory}\\keydirectory.bin";
            }
        }

    }
}
