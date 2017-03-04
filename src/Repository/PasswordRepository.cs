using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PasswordManager.Repository
{
    public class PasswordRepository
    {
        public static readonly Version FORMAT_VERSION = new Version(0, 5, 2);

        public static readonly Version FORMAT_VER050 = new Version(0, 5, 0);
   
        private enum SecretType { Key, IV };
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
            Version = FORMAT_VERSION;
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

        public void Save(string repositoryFile, string keyDirectory, SecureString securePassword)
        {
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                // read or create key and vector for the ID, mix password in and prepare encryptor
                if (!ExistKey(keyDirectory, Id))
                {
                    WriteNewKey(keyDirectory, securePassword);
                }
                byte[] iv = ReadSecret(keyDirectory, Id, SecretType.IV);
                byte[] encryptedKey = ReadSecret(keyDirectory, Id, SecretType.Key);
                // key file is encrypted with password hash
                rijAlg.Key = DecryptKey(encryptedKey, iv, securePassword);
                rijAlg.IV = iv;
                var cryptoTransform = rijAlg.CreateEncryptor();
                // create XML document
                XmlDocument doc = new XmlDocument();
                XmlElement rootElem = doc.CreateElement("PasswordRepository");
                doc.AppendChild(rootElem);
                AddElement(doc, rootElem, "Version", FORMAT_VERSION.ToString());
                // AddElement(doc, rootElem, "Id", Id); // @TODO: Duplicated in unencrypted header
                AddElement(doc, rootElem, "Name", Name);
                AddElement(doc, rootElem, "Description", Description);
                XmlElement passwordsElem = doc.CreateElement("Passwords");
                foreach (var pwd in passwordDict.Values)
                {
                    XmlElement passwordElem = doc.CreateElement("Password");
                    AddElement(doc, passwordElem, "Id", pwd.Id);
                    AddElement(doc, passwordElem, "Name", pwd.Name);
                    AddElement(doc, passwordElem, "Description", pwd.Description);
                    AddElement(doc, passwordElem, "Url", pwd.Url);
                    byte[] cipherLogin = Encrypt(cryptoTransform, pwd.Login);
                    AddElement(doc, passwordElem, "CipherLogin", Convert.ToBase64String(cipherLogin));
                    string pwd_str = pwd.SecurePassword.GetAsString();
                    byte[] cipherPassword = Encrypt(cryptoTransform, pwd_str);
                    pwd_str = string.Empty;
                    AddElement(doc, passwordElem, "CipherPassword", Convert.ToBase64String(cipherPassword));
                    passwordsElem.AppendChild(passwordElem);
                }
                rootElem.AppendChild(passwordsElem);
                using (MemoryStream ms = new MemoryStream())
                {
                    // write XML to memory
                    doc.Save(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    using (FileStream fs = new FileStream(repositoryFile, FileMode.Create))
                    {
                        // write header and ID
                        byte[] header = new byte[6] { 23, 9, 78, 121, 108, 115 };
                        fs.Write(header, 0, 6);
                        Guid guid = new Guid(Id);
                        fs.Write(guid.ToByteArray(), 0, 16);
                        Encrypt(cryptoTransform, ms, fs);
                    }
                }
            }
            Version = FORMAT_VERSION;
            Changed = false;
        }

        public static PasswordRepository Read(string repositoryFile, string keyDirectory, SecureString securePassword, bool oldFormat)
        {
            PasswordRepository repository = new PasswordRepository();
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ICryptoTransform cryptoTransform;
                    using (FileStream fs = new FileStream(repositoryFile, FileMode.Open))
                    {
                        // read header part of the file
                        repository.Id = ReadId(fs);
                        // read encrypted key and vector for the ID
                        byte[] iv = ReadSecret(keyDirectory, repository.Id, SecretType.IV);
                        if (oldFormat)
                        {
                            byte[] key = Read($"{keyDirectory}\\{repository.Id}.kv");
                            Mix(key, securePassword);
                            rijAlg.Key = key;
                        }
                        else
                        {
                            byte[] encryptedKey = ReadSecret(keyDirectory, repository.Id, SecretType.Key);
                            rijAlg.Key = DecryptKey(encryptedKey, iv, securePassword);
                        }
                        rijAlg.IV = iv;
                        cryptoTransform = rijAlg.CreateDecryptor();
                        // decrypt XML part of the file
                        Decrypt(cryptoTransform, fs, ms);
                    }
                    string data = Encoding.UTF8.GetString(ms.ToArray());
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(data);
                    XmlElement rootElem = xmldoc.DocumentElement;
                    repository.Name = rootElem["Name"].InnerText;
                    repository.Description = rootElem["Description"].InnerText;
                    repository.Version = new Version(rootElem["Version"].InnerText); // @TODO: write in unencrypted header
                    // if (repository.Version > FORMAT_VER050)
                    // {
                        // @TODO: remove, duplicated
                    //    repository.Id = rootElem["Id"].InnerText;
                    // }
                    XmlElement entriesElem = rootElem["Passwords"];
                    if (entriesElem.HasChildNodes)
                    {
                        foreach (XmlNode node in entriesElem.ChildNodes)
                        {
                            if (node.Name == "Password")
                            {
                                XmlElement passwordElem = node as XmlElement;
                                Password pwd = new Password()
                                {
                                    Name = passwordElem["Name"].InnerText,
                                    Description = passwordElem["Description"].InnerText,
                                    Url = passwordElem["Url"].InnerText
                                };
                                if (repository.Version > FORMAT_VER050)
                                {
                                    pwd.Id = passwordElem["Id"].InnerText;
                                }
                                // decrypt login and password
                                byte[] cipherLogin = Convert.FromBase64String(passwordElem["CipherLogin"].InnerText);
                                pwd.Login = Decrypt(cryptoTransform, cipherLogin);
                                byte[] cipherPwd = Convert.FromBase64String(passwordElem["CipherPassword"].InnerText);
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
                byte[] iv = ReadSecret(keyDirectory, id, SecretType.IV);
                byte [] key = Read($"{keyDirectory}\\{id}.kv");
                byte [] encryptedKey = EncryptKey(key, iv, securePassword);
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

        private static string ReadId(Stream stream)
        {
            byte[] required = new byte[6] { 23, 9, 78, 121, 108, 115 };
            byte[] header = new byte[6];
            stream.Read(header, 0, 6);
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
            byte[] guid = new byte[16];
            stream.Read(guid, 0, 16);
            return new Guid(guid).ToString();
        }

        private static void Encrypt(ICryptoTransform cryptoTransform, Stream input, Stream output)
        {
            using (CryptoStream cs = new CryptoStream(output, cryptoTransform, CryptoStreamMode.Write))
            {
                input.CopyTo(cs);
            }
        }

        private static byte[] Encrypt(ICryptoTransform cryptoTransform, string plainText)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return ms.ToArray();
                }
            }
        }

        private static void Decrypt(ICryptoTransform cryptoTransform, Stream input, Stream output)
        {
            using (CryptoStream cs = new CryptoStream(input, cryptoTransform, CryptoStreamMode.Read))
            {
                cs.CopyTo(output);
            }
        }

        private static string Decrypt(ICryptoTransform cryptoTransform, byte[] cipherText)
        {
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        private static void Mix(byte[] key, SecureString securePassword)
        {
            string str_pwd = securePassword.GetAsString();
            if (str_pwd.Length > 0)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str_pwd);
                str_pwd = string.Empty;
                byte[] passwordHash;
                using (var sha265 = new SHA256Managed())
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
        }

        private static byte [] EncryptKey(byte [] key, byte [] iv, SecureString securePassword)
        {
            using (var sha265 = new SHA256Managed())
            {
                using (var rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = 256; // same size as password hash (SHA-256)
                    string str_pwd = securePassword.GetAsString(); // GetAsUTF8
                    byte[] bytes = Encoding.UTF8.GetBytes(str_pwd);
                    str_pwd = string.Empty;
                    rijAlg.Key = sha265.ComputeHash(bytes);
                    Array.Clear(bytes, 0, bytes.Length);
                    rijAlg.IV = iv;
                    using (var destStream = new MemoryStream())
                    {
                        using (var sourceStream = new MemoryStream(key))
                        {
                            Encrypt(rijAlg.CreateEncryptor(), sourceStream, destStream);
                        }
                        return destStream.ToArray();
                    }
                }
            }
        }

        private static byte [] DecryptKey(byte[] encryptedKey, byte[] iv, SecureString securePassword)
        {
            using (var sha265 = new SHA256Managed())
            {
                using (var rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = 256; // same size as password hash
                    string str_pwd = securePassword.GetAsString(); // GetAsUTF8
                    byte[] bytes = Encoding.UTF8.GetBytes(str_pwd);
                    str_pwd = string.Empty;
                    rijAlg.Key = sha265.ComputeHash(bytes);
                    Array.Clear(bytes, 0, bytes.Length);
                    rijAlg.IV = iv;
                    using (var destStream = new MemoryStream())
                    {
                        using (var sourceStream = new MemoryStream(encryptedKey))
                        {
                            Decrypt(rijAlg.CreateDecryptor(), sourceStream, destStream);
                        }
                        return destStream.ToArray();
                    }
                }
            }
        }

        private void WriteNewKey(string keyDirectory, SecureString securePassword)
        {
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.KeySize = 256; // same size as SHA-256 hash
                // create and write initialization vector file
                rijAlg.GenerateIV();
                using (var fsiv = new FileStream($"{keyDirectory}\\{Id}.iv", FileMode.CreateNew))
                {
                    fsiv.Write(rijAlg.IV, 0, rijAlg.IV.Length);
                }
                // create key and encrypt key with password hash
                rijAlg.GenerateKey();
                byte[] encryptedKey = EncryptKey(rijAlg.Key, rijAlg.IV, securePassword);
                // write key file
                using (var fskv = new FileStream($"{keyDirectory}\\{Id}.kv2", FileMode.CreateNew))
                {
                    fskv.Write(encryptedKey, 0, encryptedKey.Length);
                }
                Array.Clear(encryptedKey, 0, encryptedKey.Length);
            }
        }

        private static byte[] ReadSecret(string keyDirectory, string id, SecretType st)
        {
            string keyfile = keyDirectory + "\\";
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
            MemoryStream ms = new MemoryStream();
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

        private static void AddElement(XmlDocument xmldoc, XmlElement parentelem, string elemname, string elemvalue)
        {
            XmlElement childelem = xmldoc.CreateElement(elemname);
            childelem.InnerText = elemvalue;
            parentelem.AppendChild(childelem);
        }

    }
}
