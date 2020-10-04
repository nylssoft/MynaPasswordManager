/*
    Myna Password Manager
    Copyright (C) 2017-2020 Niels Stockfleth

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
using PasswordManager.Properties;
using PasswordManager.Repository;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PasswordManager
{
    public class RestClient
    {
        private static HttpClient httpClient = null;

        public static async Task<string> Authenticate(string username, string password)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/auth");
            var authentication = JsonSerializer.Serialize(new { Username = username, Password = password });
            request.Content = new StringContent(authentication, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<string>(res);
            if (token == null || token.Length == 0)
            {
                throw new ArgumentException(Resources.ERROR_CLOUD_LOGIN_FAILED);
            }
            return token;
        }

        public static async Task UploadPasswords(string token, string secretKey, List<Password> passwords)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("token", token);
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/file");
            var pwdItems = new List<object>();
            foreach (var password in passwords)
            {
                pwdItems.Add(new
                {
                    password.Name,
                    password.Url,
                    password.Login,
                    password.Description,
                    Password = password.SecurePassword.GetAsString()
                });
            }
            var passwordFile = JsonSerializer.Serialize(new { SecretKey = secretKey, Passwords = pwdItems });
            request.Content = new StringContent(passwordFile, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            if (bool.Parse(res) != true)
            {
                throw new ArgumentException(Properties.Resources.ERROR_CLOUD_UPLOAD_FAILED);
            }
        }

        public static async Task RegisterUser(string username, string password)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var authentication = JsonSerializer.Serialize(new { Username = username, Password = password });
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/user");
            request.Content = new StringContent(authentication, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            var res = await response.Content.ReadAsStringAsync();
            if (bool.Parse(res) != true)
            {
                throw new ArgumentException(Properties.Resources.ERROR_CLOUD_REGISTER_FAILED);
            }
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

    }
}
