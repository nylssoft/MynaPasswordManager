/*
    Myna Password Manager
    Copyright (C) 2017 Niels Stockfleth

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
