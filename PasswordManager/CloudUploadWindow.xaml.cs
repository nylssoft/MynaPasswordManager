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
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace PasswordManager
{
    public partial class CloudUploadWindow : Window
    {
        private List<Password> passwords;

        private static HttpClient httpClient = null;

        private static string httpClientBaseUrl = null;

        private bool uploading = false;

        public CloudUploadWindow(Window owner, string title, List<Password> passwords)
        {
            Owner = owner;
            Title = title;
            this.passwords = passwords;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = Settings.Default.Topmost;
            InitializeComponent();
            textBoxUrl.Text = Settings.Default.CloudUrl;
            textBoxUsername.Text = Settings.Default.CloudUsername;
            if (textBoxUrl.Text.Length == 0)
            {
                textBoxUrl.Focus();
            }
            else if (textBoxUsername.Text.Length == 0)
            {
                textBoxUsername.Focus();
            }
            else
            {
                passwordBoxUser.Focus();
            }
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (uploading)
            {
                textBoxUrl.IsEnabled = false;
                textBoxUsername.IsEnabled = false;
                passwordBoxUser.IsEnabled = false;
                passwordBoxSecretKey.IsEnabled = false;
                buttonCancel.IsEnabled = false;
                buttonUpload.IsEnabled = false;
            }
            else
            {
                textBoxUrl.IsEnabled = true;
                textBoxUsername.IsEnabled = true;
                passwordBoxUser.IsEnabled = true;
                passwordBoxSecretKey.IsEnabled = true;
                buttonCancel.IsEnabled = true;                
                buttonUpload.IsEnabled =
                    textBoxUrl.Text.Length > 0 &&
                    textBoxUsername.Text.Length > 0 &&
                    passwordBoxUser.SecurePassword.Length > 0 &&
                    passwordBoxSecretKey.SecurePassword.Length > 0;
            }
        }

        private static HttpClient GetHttpClient(string baseUrl)
        {
            if (httpClient == null || httpClientBaseUrl != baseUrl)
            {
                httpClient = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl)
                };
                httpClientBaseUrl = baseUrl;
            }
            return httpClient;
        }

        private async void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            var old = Cursor;
            try
            {
                uploading = true;
                UpdateControls();
                Cursor = Cursors.Wait;
                var client = GetHttpClient(textBoxUrl.Text);
                client.DefaultRequestHeaders.Remove("token");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authentication = JsonSerializer.Serialize(new
                {
                    Username = textBoxUsername.Text,
                    passwordBoxUser.Password
                });
                var request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/auth");
                request.Content = new StringContent(authentication, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var res = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<string>(res);
                if (token == null || token.Length == 0)
                {
                    throw new ArgumentException(Properties.Resources.ERROR_CLOUD_LOGIN_FAILED);
                }
                client.DefaultRequestHeaders.Add("token", token);
                request = new HttpRequestMessage(HttpMethod.Post, "api/pwdman/file");
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
                var passwordFile = JsonSerializer.Serialize(new {
                    SecretKey = passwordBoxSecretKey.Password,
                    Passwords = pwdItems
                });
                request.Content = new StringContent(passwordFile, Encoding.UTF8, "application/json");
                response = await client.SendAsync(request);
                res = await response.Content.ReadAsStringAsync();
                if (bool.Parse(res) != true)
                {
                    throw new ArgumentException(Properties.Resources.ERROR_CLOUD_UPLOAD_FAILED);
                }
                Cursor = old;
                uploading = false;
                UpdateControls();
                MessageBox.Show(Properties.Resources.CLOUD_UPLOAD_SUCCEEDED, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.CloudUrl = textBoxUrl.Text;
                Settings.Default.CloudUsername = textBoxUsername.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Cursor = old;
                uploading = false;
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateControls();
            }
        }

        private void OnChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!uploading)
            {
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (uploading)
            {
                e.Cancel = true;
            }
        }

    }
}
