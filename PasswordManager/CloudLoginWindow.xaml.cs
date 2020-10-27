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
using System;
using System.ComponentModel;
using System.Security;
using System.Windows;
using System.Windows.Input;

namespace PasswordManager
{
    public partial class CloudLoginWindow : Window
    {
        private bool requiresPass2 = false;

        private bool waiting = false;

        public SecureString CloudToken { get; set; }

        public CloudLoginWindow(Window owner, string title)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = Settings.Default.Topmost;
            InitializeComponent();
            textBoxUsername.Text = Settings.Default.CloudUsername;
            if (textBoxUsername.Text.Length == 0)
            {
                textBoxUsername.Focus();
            }
            else
            {
                passwordBoxUser.Focus();
            }
            textBoxCode.IsEnabled = false;
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (requiresPass2)
            {
                textBoxUsername.IsEnabled = false;
                passwordBoxUser.IsEnabled = false;
                labelPass1.Visibility = Visibility.Visible;
                labelCode.Visibility = Visibility.Visible;
                textBoxCode.Visibility = Visibility.Visible;
                textBoxCode.IsEnabled = true;
                buttonCancel.IsEnabled = true;
                buttonLogin.IsEnabled = true;
                buttonLogin.IsEnabled = textBoxCode.Text.Length > 0;
            }
            else
            {
                textBoxUsername.IsEnabled = true;
                passwordBoxUser.IsEnabled = true;
                labelPass1.Visibility = Visibility.Hidden;
                labelCode.Visibility = Visibility.Hidden;
                textBoxCode.Visibility = Visibility.Hidden;
                textBoxCode.IsEnabled = false;
                buttonCancel.IsEnabled = true;
                buttonLogin.IsEnabled =
                    textBoxUsername.Text.Length > 0 &&
                    passwordBoxUser.SecurePassword.Length > 0;
            }
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            var old = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                waiting = true;
                UpdateControls();
                var pass1token = CloudToken?.GetAsString();
                if (requiresPass2)
                {
                    var token = await RestClient.AuthenticatePass2(pass1token, textBoxCode.Text.Trim());
                    CloudToken.Clear();
                    foreach (var c in token)
                    {
                        CloudToken.AppendChar(c);
                    }
                    requiresPass2 = false;
                }
                else if (string.IsNullOrEmpty(pass1token))
                {
                    var authResult = await RestClient.Authenticate(textBoxUsername.Text, passwordBoxUser.Password);
                    CloudToken = new SecureString();
                    foreach (var c in authResult.Item1)
                    {
                        CloudToken.AppendChar(c);
                    }
                    requiresPass2 = authResult.Item2;
                    if (requiresPass2)
                    {
                        Cursor = old;
                        UpdateControls();
                        textBoxCode.Focus();
                        waiting = false;
                        return;
                    }
                }
                waiting = false;
                Cursor = old;
                MessageBox.Show(Properties.Resources.CLOUD_LOGIN_SUCCEEDED, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.CloudUsername = textBoxUsername.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                requiresPass2 = false;
                CloudToken = null;
                waiting = false;
                Cursor = old;
                textBoxCode.Text = "";
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateControls();
            }
        }

        private void OnChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!waiting)
            {
                Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (waiting)
            {
                e.Cancel = true;
            }
        }
    }
}
