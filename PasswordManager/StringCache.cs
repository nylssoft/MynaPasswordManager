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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PasswordManager
{
    public abstract class StringCache
    {
        protected Dictionary<string, string> mappings = new Dictionary<string, string>();

        protected abstract string MappingFile { get; }

        public void Load()
        {
            if (File.Exists(MappingFile))
            {
                var formatter = new BinaryFormatter();
                List<Tuple<string, string>> list;
                using (var fs = new FileStream(MappingFile, FileMode.Open))
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
            var formatter = new BinaryFormatter();
            using (var fs = new FileStream(MappingFile, FileMode.Create))
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
    }
}
