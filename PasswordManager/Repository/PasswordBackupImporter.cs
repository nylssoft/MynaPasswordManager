/*
    Myna Password Manager
    Copyright (C) 2026 Niels Stockfleth

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
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordManager.Repository
{
    public static class PasswordBackupImporter
    {
        private const int MaxArchiveBytes = 16 * 1024 * 1024;
        private const int MaxPasswordItems = 10000;
        private const int Pbkdf2Iterations = 600000;
        private const string BackupEntryName = "passwords/passwords.encrypted.json";

        public static List<Password> Read(string archivePath, SecureString passphrase)
        {
            if (string.IsNullOrEmpty(archivePath) || passphrase == null || passphrase.Length < 12)
            {
                throw new InvalidDataException("Invalid backup or passphrase.");
            }

            var archiveInfo = new FileInfo(archivePath);
            if (!archiveInfo.Exists || archiveInfo.Length <= 0 || archiveInfo.Length > MaxArchiveBytes)
            {
                throw new InvalidDataException("The password backup is empty or exceeds the supported size.");
            }

            byte[] envelopeBytes;
            using (var file = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var archive = new ZipArchive(file, ZipArchiveMode.Read))
            {
                if (archive.Entries.Count is < 1 or > 2)
                {
                    throw new InvalidDataException("The archive does not have the expected password backup structure.");
                }

                ZipArchiveEntry payloadEntry = null;
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName == BackupEntryName && payloadEntry == null)
                    {
                        payloadEntry = entry;
                    }
                    else if (entry.FullName != "passwords/" || entry.Length != 0)
                    {
                        throw new InvalidDataException("The archive contains unexpected files.");
                    }
                }

                if (payloadEntry == null || payloadEntry.Length <= 0 || payloadEntry.Length > MaxArchiveBytes)
                {
                    throw new InvalidDataException("The encrypted password data is missing or too large.");
                }

                using var input = payloadEntry.Open();
                using var output = new MemoryStream((int)payloadEntry.Length);
                input.CopyTo(output);
                if (output.Length != payloadEntry.Length || output.Length > MaxArchiveBytes)
                {
                    throw new InvalidDataException("The encrypted password data has an invalid size.");
                }
                envelopeBytes = output.ToArray();
            }

            byte[] passphraseBytes = null;
            byte[] salt = null;
            byte[] iv = null;
            byte[] ciphertextAndTag = null;
            byte[] key = null;
            byte[] plaintext = null;
            try
            {
                passphraseBytes = GetUtf8Passphrase(passphrase);
                using var envelopeDocument = JsonDocument.Parse(envelopeBytes);
                var envelope = envelopeDocument.RootElement;
                if (envelope.ValueKind != JsonValueKind.Object ||
                    GetRequiredString(envelope, "format") != "myna-passwords" ||
                    GetRequiredInt32(envelope, "version") != 1 ||
                    GetRequiredString(envelope, "cipher") != "AES-256-GCM" ||
                    GetRequiredString(envelope, "kdf") != "PBKDF2-SHA256" ||
                    GetRequiredInt32(envelope, "iterations") != Pbkdf2Iterations)
                {
                    throw new InvalidDataException("The password backup format is not supported.");
                }

                salt = DecodeHex(GetRequiredString(envelope, "salt"), 16);
                iv = DecodeHex(GetRequiredString(envelope, "iv"), 12);
                ciphertextAndTag = DecodeHex(GetRequiredString(envelope, "ciphertext"), -1);
                if (ciphertextAndTag.Length < 16)
                {
                    throw new InvalidDataException("The encrypted password data is incomplete.");
                }

                key = Rfc2898DeriveBytes.Pbkdf2(passphraseBytes, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, 32);
                plaintext = new byte[ciphertextAndTag.Length - 16];
                using (var aes = new AesGcm(key, 16))
                {
                    aes.Decrypt(
                        iv,
                        ciphertextAndTag.AsSpan(0, plaintext.Length),
                        ciphertextAndTag.AsSpan(plaintext.Length, 16),
                        plaintext);
                }

                return ReadPasswordItems(plaintext);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("The password backup contains invalid JSON.", ex);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(envelopeBytes);
                if (passphraseBytes != null) CryptographicOperations.ZeroMemory(passphraseBytes);
                if (salt != null) CryptographicOperations.ZeroMemory(salt);
                if (iv != null) CryptographicOperations.ZeroMemory(iv);
                if (ciphertextAndTag != null) CryptographicOperations.ZeroMemory(ciphertextAndTag);
                if (key != null) CryptographicOperations.ZeroMemory(key);
                if (plaintext != null) CryptographicOperations.ZeroMemory(plaintext);
            }
        }

        private static List<Password> ReadPasswordItems(byte[] plaintext)
        {
            var passwords = new List<Password>();
            bool completed = false;
            try
            {
                using var document = JsonDocument.Parse(plaintext);
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object ||
                    GetRequiredInt32(root, "version") != 1 ||
                    !root.TryGetProperty("items", out var items) ||
                    items.ValueKind != JsonValueKind.Array ||
                    items.GetArrayLength() is < 1 or > MaxPasswordItems)
                {
                    throw new InvalidDataException("The password backup contains an invalid item list.");
                }

                foreach (var item in items.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidDataException("The password backup contains an invalid item.");
                    }

                    var password = new Password
                    {
                        Name = GetRequiredString(item, "Name"),
                        Login = GetRequiredString(item, "Login"),
                        Url = GetRequiredString(item, "Url"),
                        Description = GetRequiredString(item, "Description")
                    };
                    var passwordText = GetRequiredString(item, "Password");
                    foreach (var character in passwordText)
                    {
                        password.SecurePassword.AppendChar(character);
                    }
                    password.SecurePassword.MakeReadOnly();
                    passwords.Add(password);
                }
                completed = true;
                return passwords;
            }
            finally
            {
                if (!completed)
                {
                    foreach (var password in passwords)
                    {
                        password.SecurePassword.Dispose();
                    }
                }
            }
        }

        private static string GetRequiredString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                throw new InvalidDataException($"The password backup is missing '{propertyName}'.");
            }
            return property.GetString();
        }

        private static int GetRequiredInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind != JsonValueKind.Number ||
                !property.TryGetInt32(out int value))
            {
                throw new InvalidDataException($"The password backup has an invalid '{propertyName}' value.");
            }
            return value;
        }

        private static byte[] DecodeHex(string value, int expectedLength)
        {
            if (value.Length == 0 || value.Length % 2 != 0 ||
                expectedLength >= 0 && value.Length != expectedLength * 2)
            {
                throw new InvalidDataException("The password backup contains invalid cryptographic data.");
            }

            var bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (!byte.TryParse(value.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out bytes[i]))
                {
                    throw new InvalidDataException("The password backup contains invalid cryptographic data.");
                }
            }
            return bytes;
        }

        private static byte[] GetUtf8Passphrase(SecureString passphrase)
        {
            IntPtr passphrasePointer = IntPtr.Zero;
            var characters = new char[passphrase.Length];
            try
            {
                passphrasePointer = Marshal.SecureStringToGlobalAllocUnicode(passphrase);
                for (int i = 0; i < characters.Length; i++)
                {
                    characters[i] = (char)Marshal.ReadInt16(passphrasePointer, i * sizeof(char));
                }
                return Encoding.UTF8.GetBytes(characters);
            }
            finally
            {
                Array.Clear(characters);
                if (passphrasePointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(passphrasePointer);
                }
            }
        }
    }
}
