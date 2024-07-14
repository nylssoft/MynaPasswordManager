/*
    Myna Password Manager
    Copyright (C) 2017-2024 Niels Stockfleth

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
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using PasswordManager.Properties;
using PasswordManager.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordManager
{
    public class RestClient
    {
        private static HttpClient httpClient = null;

        private class ProblemDetails
        {
            public string title { get; set; }
            public int status { get; set; }
        }

        private class AuthenticationResult
        {
            public string token { get; set; }
            public bool requiresPass2 { get; set; }
        }

        private class UserSaltResult
        {
            public string passwordManagerSalt { get; set; }
        }

        private class PasswordItem
        {
            public string Name { get; set; }
            public string Url { get; set; }            
            public string Login { get; set; }            
            public string Description { get; set; }            
            public string Password { get; set; }
        }

        public static async Task<(string,bool)> Authenticate(string username, string password, ClientInfo clientInfo)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/auth");
            var authentication = JsonSerializer.Serialize(new
            {
                Username = username,
                Password = password,
                ClientUUID = clientInfo?.UUID,
                ClientName = clientInfo?.Name
            });
            request.Content = new StringContent(authentication, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            await EnsureSuccess(response);
            var res = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthenticationResult>(res);
            return (authResult.token, authResult.requiresPass2);
        }

        public static async Task<string> AuthenticatePass2(string token, string totp)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("token", token);
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/auth2");
            var totpjson = JsonSerializer.Serialize(totp);
            request.Content = new StringContent(totpjson, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            await EnsureSuccess(response);
            var res = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthenticationResult>(res);
            return authResult.token;
        }

        public static async Task<string> GetUserSalt(string token)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("token", token);
            var request = new HttpRequestMessage(HttpMethod.Get, "api/pwdman/user");
            var response = await client.SendAsync(request);
            await EnsureSuccess(response);
            var res = await response.Content.ReadAsStringAsync();
            var userSaltResult = JsonSerializer.Deserialize<UserSaltResult>(res);
            return userSaltResult.passwordManagerSalt;
        }

        public static async Task UploadPasswords(string token, string userSalt, string secretKey, List<Password> passwords)
        {
            var pwdItems = new List<PasswordItem>();
            foreach (var password in passwords)
            {
                var pwdItem = new PasswordItem
                {
                    Name = password.Name,
                    Url = password.Url,
                    Login = password.Login,
                    Description = password.Description,
                    Password = password.SecurePassword.GetAsString()
                };
                pwdItems.Add(pwdItem);
            }
            var encoded = EncodePasswords(userSalt, secretKey, pwdItems);
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("token", token);
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/file");
            var json = JsonSerializer.Serialize(ConvertToHexString(encoded));
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            await EnsureSuccess(response);
        }

        private static HttpClient GetHttpClient()
        {
            if (httpClient == null || httpClient.BaseAddress != new Uri(Settings.Default.CloudUrl))
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(Settings.Default.CloudUrl)
                };
            }
            return httpClient;
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(json);
                throw new ArgumentException(problemDetails.title);
            }
        }

        private static string ConvertToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        private static byte[] EncodeSecret(byte[] salt, string password, byte[] secret)
        {
            var iv = new byte[12];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }
            var key = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, 1000, 256 / 8);
            var encoded = new byte[secret.Length];
            var tag = new byte[16];
            using (var cipher = new AesGcm(key, tag.Length))
            {
                cipher.Encrypt(iv, secret, encoded, tag);
            }
            var ret = new byte[iv.Length + encoded.Length + tag.Length];
            iv.CopyTo(ret, 0);
            encoded.CopyTo(ret, iv.Length);
            tag.CopyTo(ret, iv.Length + encoded.Length);
            return ret;
        }

        private static bool VerifyPasswordStrength(string pwd)
        {
            var pwdGen = new PwdGen();
            if (pwd.Length >= 8)
            {
                var cntSymbols = pwd.Count((c) => pwdGen.Symbols.Contains(c));
                var cntUpper = pwd.Count((c) => pwdGen.UpperCharacters.Contains(c));
                var cntLower = pwd.Count((c) => pwdGen.LowerCharacters.Contains(c));
                var cntDigits = pwd.Count((c) => pwdGen.Digits.Contains(c));
                if (cntSymbols >= 1 && cntUpper >= 1 && cntLower >= 1 && cntDigits >= 1)
                {
                    return true;
                }
            }
            return false;
        }

        private static byte[] EncodePasswords(string userSalt, string secretKey, List<PasswordItem> pwds)
        {
            if (!VerifyPasswordStrength(secretKey))
            {
                throw new ArgumentException("Secret key not strong enough.");
            }
            var salt = Encoding.UTF8.GetBytes(userSalt);
            foreach (var item in pwds)
            {
                item.Password = ConvertToHexString(
                    EncodeSecret(salt, secretKey, Encoding.UTF8.GetBytes(item.Password)));
            }
            var passwords = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pwds));
            var encoded = EncodeSecret(salt, secretKey, passwords);
            return encoded;
        }
    }
}
