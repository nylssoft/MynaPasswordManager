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

        public static async Task<(string,bool)> Authenticate(string username, string password)
        {
            var client = GetHttpClient();
            client.DefaultRequestHeaders.Remove("token");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/auth");
            var authentication = JsonSerializer.Serialize(new { Username = username, Password = password });
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
            return JsonSerializer.Deserialize<string>(res);
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
    }
}
