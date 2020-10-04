﻿/*
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
using System.Windows;
using System.Windows.Input;

namespace PasswordManager
{
    public partial class CloudUploadWindow : Window
    {
        private List<Password> passwords;

        private bool uploading = false;

        public CloudUploadWindow(Window owner, string title, List<Password> passwords)
        {
            Owner = owner;
            Title = title;
            this.passwords = passwords;
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
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (uploading)
            {
                textBoxUsername.IsEnabled = false;
                passwordBoxUser.IsEnabled = false;
                passwordBoxSecretKey.IsEnabled = false;
                buttonCancel.IsEnabled = false;
                buttonUpload.IsEnabled = false;
            }
            else
            {
                textBoxUsername.IsEnabled = true;
                passwordBoxUser.IsEnabled = true;
                passwordBoxSecretKey.IsEnabled = true;
                buttonCancel.IsEnabled = true;                
                buttonUpload.IsEnabled =
                    textBoxUsername.Text.Length > 0 &&
                    passwordBoxUser.SecurePassword.Length > 0 &&
                    passwordBoxSecretKey.SecurePassword.Length > 0;
            }
        }

        private async void ButtonUpload_Click(object sender, RoutedEventArgs e)
        {
            var old = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                uploading = true;
                UpdateControls();
                var token = await RestClient.Authenticate(textBoxUsername.Text, passwordBoxUser.Password);
                await RestClient.UploadPasswords(token, passwordBoxSecretKey.Password, passwords);
                Cursor = old;
                uploading = false;
                MessageBox.Show(Properties.Resources.CLOUD_UPLOAD_SUCCEEDED, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.CloudUsername = textBoxUsername.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Cursor = old;
                uploading = false;
                UpdateControls();
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
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
