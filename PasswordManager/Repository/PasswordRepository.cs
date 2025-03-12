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
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace PasswordManager.Repository
{
    public class PasswordRepository
    {
        public static readonly Version FILE_FORMAT = new Version(0, 5, 2);

        public static readonly Version FILE_FORMAT_050 = new Version(0, 5, 0);
   
        private enum SecretType { Key, IV };

        private enum TransformType { Encrypt, Decrypt };

        private Dictionary<string, Password> passwordDict;
        private string name;
        private string description;

        public bool Changed { get; private set; }

        public string Id { get; private set; }

        public Version Version { get; private set; }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                Changed = true;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                Changed = true;
            }
        }
        
        public PasswordRepository()
        {
            passwordDict = new Dictionary<string, Password>();
            Version = FILE_FORMAT;
            Id = Guid.NewGuid().ToString();
            name = string.Empty;
            description = string.Empty;
            Changed = false;
        }

        public List<Password> Passwords
        {
            get
            {
                List<Password> ret = new List<Password>();
                foreach (var p in passwordDict.Values)
                {
                    ret.Add(new Password(p));
                }
                return ret;
            }
        }

        public void Add(Password pwd)
        {
            if (passwordDict.ContainsKey(pwd.Id))
            {
                throw new ArgumentException($"Password ID '{pwd.Id}' already exists in repository.");
            }
            passwordDict[pwd.Id] = new Password(pwd);
            Changed = true;
        }

        public void Update(Password pwd)
        {
            if (!passwordDict.ContainsKey(pwd.Id))
            {
                throw new ArgumentException($"Password ID '{pwd.Id}' not found in repository.");
            }
            passwordDict[pwd.Id] = new Password(pwd);
            Changed = true;
        }

        public void Remove(Password pwd)
        {
            if (!passwordDict.ContainsKey(pwd.Id))
            {
                throw new ArgumentException($"Password ID '{pwd.Id}' not found in repository.");
            }
            passwordDict.Remove(pwd.Id);
            Changed = true;
        }

        public void ChangeMasterPassword(
            string          repositoryFile,
            string          keyDirectory,
            SecureString    newSecurePassword)
        {
            var ok = false;
            var keyFile = $"{keyDirectory}\\{Id}.kv2";
            var ivFile = $"{keyDirectory}\\{Id}.iv";
            var backupKeyFile = Backup(keyFile);
            var backupIVFile = Backup(ivFile);
            try
            {
                Save(repositoryFile, keyDirectory, newSecurePassword, true /* overwrite key */);
                ok = true;
            }
            finally
            {
                HandleBackup(ok, keyFile, backupKeyFile);
                HandleBackup(ok, ivFile, backupIVFile);
            }
        }

        public void Save(
            string          repositoryFile,
            string          keyDirectory,
            SecureString    securePassword,
            bool            overwriteKey = false)
        {
            bool ok = false;
            var backupFile = Backup(repositoryFile);
            try
            {
                using (var rijAlg = Aes.Create())
                {
                    // create new key if it does not exist
                    if (overwriteKey || !ExistKey(keyDirectory, Id))
                    {
                        WriteNewKey(keyDirectory, Id, securePassword);
                    }
                    // read initialization vector and encrypted key
                    var iv = ReadSecret(keyDirectory, Id, SecretType.IV);
                    var encryptedKey = ReadSecret(keyDirectory, Id, SecretType.Key);
                    // decrypt key file with password hash
                    rijAlg.Key = TransformKey(encryptedKey, iv, securePassword, TransformType.Decrypt);
                    rijAlg.IV = iv;
                    var cryptoTransform = rijAlg.CreateEncryptor();
                    // create XML document
                    var doc = new XmlDocument();
                    var rootElem = doc.CreateElement("PasswordRepository");
                    doc.AppendChild(rootElem);
                    AddElement(doc, rootElem, "Version", FILE_FORMAT.ToString());
                    AddElement(doc, rootElem, "Name", Name);
                    AddElement(doc, rootElem, "Description", Description);
                    var passwordsElem = doc.CreateElement("Passwords");
                    foreach (var pwd in passwordDict.Values)
                    {
                        var passwordElem = doc.CreateElement("Password");
                        AddElement(doc, passwordElem, "Id", pwd.Id);
                        AddElement(doc, passwordElem, "Name", pwd.Name);
                        AddElement(doc, passwordElem, "Description", pwd.Description);
                        AddElement(doc, passwordElem, "Url", pwd.Url);
                        var cipherLogin = Encrypt(cryptoTransform, pwd.Login);
                        AddElement(doc, passwordElem, "CipherLogin", Convert.ToBase64String(cipherLogin));
                        var pwd_str = pwd.SecurePassword.GetAsString();
                        var cipherPassword = Encrypt(cryptoTransform, pwd_str);
                        pwd_str = string.Empty;
                        AddElement(doc, passwordElem, "CipherPassword", Convert.ToBase64String(cipherPassword));
                        passwordsElem.AppendChild(passwordElem);
                    }
                    rootElem.AppendChild(passwordsElem);
                    using (var ms = new MemoryStream())
                    {
                        // write XML to memory
                        doc.Save(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var fs = new FileStream(repositoryFile, FileMode.Create))
                        {
                            // write header and ID
                            var header = new byte[6] { 23, 9, 78, 121, 108, 115 };
                            fs.Write(header, 0, 6);
                            var guid = new Guid(Id);
                            fs.Write(guid.ToByteArray(), 0, 16);
                            Encrypt(cryptoTransform, ms, fs);
                        }
                    }
                }
                Version = FILE_FORMAT;
                Changed = false;
                ok = true;
            }
            finally
            {
                HandleBackup(ok, repositoryFile, backupFile);
            }
        }

        public static PasswordRepository Read(
            string          repositoryFile,
            string          keyDirectory,
            SecureString    securePassword,
            bool            oldFormat)
        {
            var repository = new PasswordRepository();
            using (var rijAlg = Aes.Create())
            {
                using (var ms = new MemoryStream())
                {
                    ICryptoTransform cryptoTransform;
                    using (var fs = new FileStream(repositoryFile, FileMode.Open))
                    {
                        // read header part of the file
                        repository.Id = ReadId(fs);
                        // read encrypted key and vector for the ID
                        var iv = ReadSecret(keyDirectory, repository.Id, SecretType.IV);
                        if (oldFormat)
                        {
                            var key = Read($"{keyDirectory}\\{repository.Id}.kv");
                            Mix(key, securePassword);
                            rijAlg.Key = key;
                        }
                        else
                        {
                            var encryptedKey = ReadSecret(keyDirectory, repository.Id, SecretType.Key);
                            rijAlg.Key = TransformKey(encryptedKey, iv, securePassword, TransformType.Decrypt);
                        }
                        rijAlg.IV = iv;
                        cryptoTransform = rijAlg.CreateDecryptor();
                        // decrypt XML part of the file
                        Decrypt(cryptoTransform, fs, ms);
                    }
                    var xmldoc = new XmlDocument();
                    xmldoc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
                    var rootElem = xmldoc.DocumentElement;
                    repository.Name = rootElem["Name"].InnerText;
                    repository.Description = rootElem["Description"].InnerText;
                    repository.Version = new Version(rootElem["Version"].InnerText);
                    var entriesElem = rootElem["Passwords"];
                    if (entriesElem.HasChildNodes)
                    {
                        foreach (XmlNode node in entriesElem.ChildNodes)
                        {
                            if (node.Name == "Password")
                            {
                                var passwordElem = node as XmlElement;
                                var pwd = new Password()
                                {
                                    Name = passwordElem["Name"].InnerText,
                                    Description = passwordElem["Description"].InnerText,
                                    Url = passwordElem["Url"].InnerText
                                };
                                if (repository.Version > FILE_FORMAT_050)
                                {
                                    pwd.Id = passwordElem["Id"].InnerText;
                                }
                                // decrypt login and password
                                var cipherLogin = Convert.FromBase64String(passwordElem["CipherLogin"].InnerText);
                                pwd.Login = Decrypt(cryptoTransform, cipherLogin);
                                var cipherPwd = Convert.FromBase64String(passwordElem["CipherPassword"].InnerText);
                                foreach (var c in Decrypt(cryptoTransform, cipherPwd))
                                {
                                    pwd.SecurePassword.AppendChar(c);
                                }
                                Array.Clear(cipherPwd, 0, cipherPwd.Length);
                                pwd.SecurePassword.MakeReadOnly();
                                repository.passwordDict[pwd.Id] = pwd;
                            }
                        }
                    }
                }
            }
            repository.Changed = false;
            return repository;
        }

        public static string GetIdFromFile(string repositoryFile)
        {
            using (var fs = new FileStream(repositoryFile, FileMode.Open))
            {
                return ReadId(fs);
            }
        }

        public static bool ExistKey(string keyDirectory, string id)
        {
            return
                File.Exists($"{keyDirectory}\\{id}.kv2") &&
                File.Exists($"{keyDirectory}\\{id}.iv");
        }

        public static bool MigrateKey(string keyDirectory, string id, SecureString securePassword)
        {
            if (File.Exists($"{keyDirectory}\\{id}.kv"))
            {
                var iv = ReadSecret(keyDirectory, id, SecretType.IV);
                var key = Read($"{keyDirectory}\\{id}.kv");
                var encryptedKey = TransformKey(key, iv, securePassword, TransformType.Encrypt);
                using (var fs = new FileStream($"{keyDirectory}\\{id}.kv2", FileMode.CreateNew))
                {
                    fs.Write(encryptedKey, 0, encryptedKey.Length);
                }
                return true;
            }
            return false;
        }
 
        public void MoveKey(string sourceKeyDirectory, string destinationKeyDirectory)
        {
            if (ExistKey(sourceKeyDirectory, Id))
            {
                File.Move($"{sourceKeyDirectory}\\{Id}.kv2", $"{destinationKeyDirectory}\\{Id}.kv2");
                File.Move($"{sourceKeyDirectory}\\{Id}.iv", $"{destinationKeyDirectory}\\{Id}.iv");
            }
        }

        #region static helper methods

        private static string Backup(string originalFilename)
        {
            if (File.Exists(originalFilename))
            {
                var backupFile = $"{originalFilename}.bak";
                File.Copy(originalFilename, backupFile, true);
                return backupFile;
            }
            return string.Empty;
        }

        private static void HandleBackup(bool ok, string originalFilename, string backupFilename)
        {
            if (!string.IsNullOrEmpty(backupFilename))
            {
                if (ok)
                {
                    File.Delete(backupFilename);
                }
                else
                {
                    if (File.Exists(originalFilename))
                    {
                        File.Delete(originalFilename);
                    }
                    File.Move(backupFilename, originalFilename);
                }
            }
        }

        private static void AddElement(XmlDocument xmldoc, XmlElement parentelem, string elemname, string elemvalue)
        {
            var childelem = xmldoc.CreateElement(elemname);
            childelem.InnerText = elemvalue;
            parentelem.AppendChild(childelem);
        }

        private static string ReadId(Stream stream)
        {
            var required = new byte[6] { 23, 9, 78, 121, 108, 115 };
            var header = new byte[6];
            stream.ReadExactly(header, 0, 6);
            bool invalid = true;
            if (header.Length == required.Length)
            {
                invalid = false;
                for (int idx = 0; idx < required.Length; idx++)
                {
                    if (header[idx] != required[idx])
                    {
                        invalid = true;
                        break;
                    }
                }
            }
            if (invalid)
            {
                throw new ArgumentException("Invalid format for password file.");
            }
            var guid = new byte[16];
            stream.ReadExactly(guid, 0, 16);
            return new Guid(guid).ToString();
        }

        private static void Encrypt(ICryptoTransform cryptoTransform, Stream input, Stream output)
        {
            using (var cs = new CryptoStream(output, cryptoTransform, CryptoStreamMode.Write))
            {
                input.CopyTo(cs);
            }
        }

        private static byte[] Encrypt(ICryptoTransform cryptoTransform, string plainText)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static void Decrypt(ICryptoTransform cryptoTransform, Stream input, Stream output)
        {
            using (var cs = new CryptoStream(input, cryptoTransform, CryptoStreamMode.Read))
            {
                cs.CopyTo(output);
            }
        }

        private static string Decrypt(ICryptoTransform cryptoTransform, byte[] cipherText)
        {
            using (var ms = new MemoryStream(cipherText))
            {
                using (var cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        private static void Mix(byte[] key, SecureString securePassword)
        {
            var str_pwd = securePassword.GetAsString();
            var bytes = Encoding.UTF8.GetBytes(str_pwd);
            str_pwd = string.Empty;
            byte[] passwordHash;
            using (var sha265 = SHA256.Create())
            {
                passwordHash = sha265.ComputeHash(bytes);
            }
            Array.Clear(bytes, 0, bytes.Length);
            var maxlen = Math.Min(passwordHash.Length, key.Length);
            for (int idx = 0; idx < maxlen; idx++)
            {
                key[idx] = passwordHash[idx];
            }
            Array.Clear(passwordHash, 0, passwordHash.Length);
        }

        private static byte [] TransformKey(byte [] key, byte [] iv, SecureString securePassword, TransformType t)
        {
            using (var sha265 = SHA256.Create())
            {
                using (var rijAlg = Aes.Create())
                {
                    rijAlg.KeySize = 256; // same size as password hash (SHA-256)
                    var str_pwd = securePassword.GetAsString();
                    var bytes = Encoding.UTF8.GetBytes(str_pwd);
                    str_pwd = string.Empty;
                    rijAlg.Key = sha265.ComputeHash(bytes);
                    Array.Clear(bytes, 0, bytes.Length);
                    rijAlg.IV = iv;
                    using (var destStream = new MemoryStream())
                    {
                        using (var sourceStream = new MemoryStream(key))
                        {
                            switch (t)
                            {
                                case TransformType.Encrypt:
                                    Encrypt(rijAlg.CreateEncryptor(), sourceStream, destStream);
                                    break;
                                case TransformType.Decrypt:
                                    Decrypt(rijAlg.CreateDecryptor(), sourceStream, destStream);
                                    break;
                                default:
                                    break;
                            }
                        }
                        return destStream.ToArray();
                    }
                }
            }
        }

        private static void WriteNewKey(string keyDirectory, string id, SecureString securePassword)
        {
            using (var rijAlg = Aes.Create())
            {
                rijAlg.KeySize = 256; // same size as SHA-256 hash
                // create and write initialization vector file
                rijAlg.GenerateIV();
                using (var fsiv = new FileStream($"{keyDirectory}\\{id}.iv", FileMode.Create))
                {
                    fsiv.Write(rijAlg.IV, 0, rijAlg.IV.Length);
                }
                // create key and encrypt key with password hash
                rijAlg.GenerateKey();
                var encryptedKey = TransformKey(rijAlg.Key, rijAlg.IV, securePassword, TransformType.Encrypt);
                // write key file
                using (var fskv = new FileStream($"{keyDirectory}\\{id}.kv2", FileMode.Create))
                {
                    fskv.Write(encryptedKey, 0, encryptedKey.Length);
                }
                Array.Clear(encryptedKey, 0, encryptedKey.Length);
            }
        }

        private static byte[] ReadSecret(string keyDirectory, string id, SecretType st)
        {
            var keyfile = keyDirectory + "\\";
            keyfile += st == SecretType.Key ? $"{id}.kv2" : $"{id}.iv";
            if (!File.Exists(keyfile))
            {
                throw new FileNotFoundException($"Key file '{keyfile}' not found.");
            }
            return Read(keyfile);
        }

        private static byte[] Read(string filename)
        {
            const int CHUNK_SIZE = 8192;
            using (var ms = new MemoryStream())
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    int readCount;
                    var buffer = new byte[CHUNK_SIZE];
                    while ((readCount = fs.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        ms.Write(buffer, 0, readCount);
                    }
                }
                return ms.ToArray();
            }
        }

        #endregion
    }
}
