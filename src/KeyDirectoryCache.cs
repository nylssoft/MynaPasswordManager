namespace PasswordManager
{
    public sealed class KeyDirectoryCache : StringCache
    {
        private string mappingFile;

        public KeyDirectoryCache(string cacheDirectory)
        {
            mappingFile = $"{cacheDirectory}\\keydirectory.bin";
        }

        protected override string MappingFile => mappingFile;

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
    }
}
