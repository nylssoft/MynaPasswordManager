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
using System.Windows;
using System.Windows.Input;

namespace PasswordManager
{
    public partial class CloudRegisterWindow : Window
    {
        private bool uploading = false;
        private bool init = true;

        public CloudRegisterWindow(Window owner, string title)
        {
            Owner = owner;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = Settings.Default.Topmost;
            InitializeComponent();
            init = false;
            textBoxUsername.Focus();
            UpdateControls();
        }

        private void UpdateControls()
        {
            if (uploading)
            {
                textBoxUsername.IsEnabled = false;
                passwordBoxUser.IsEnabled = false;
                passwordBoxConfirm.IsEnabled = false;
                checkBoxRequires2FA.IsEnabled = false;
                textBoxEmail.IsEnabled = false;
                textBoxConfirmEmail.IsEnabled = false;
                buttonCancel.IsEnabled = false;
                buttonRegister.IsEnabled = false;
            }
            else
            {
                textBoxUsername.IsEnabled = true;
                passwordBoxUser.IsEnabled = true;
                passwordBoxConfirm.IsEnabled = true;
                checkBoxRequires2FA.IsEnabled = true;
                textBoxEmail.IsEnabled = checkBoxRequires2FA.IsChecked == true;
                textBoxConfirmEmail.IsEnabled = checkBoxRequires2FA.IsChecked == true;
                buttonCancel.IsEnabled = true;
                buttonRegister.IsEnabled =
                    textBoxUsername.Text.Length > 0 &&
                    passwordBoxUser.SecurePassword.Length > 0 &&
                    passwordBoxConfirm.Password == passwordBoxUser.Password &&
                    (checkBoxRequires2FA.IsChecked == false ||
                    textBoxEmail.Text.Length > 0 &&
                    textBoxConfirmEmail.Text == textBoxEmail.Text);
            }
        }

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            var old = Cursor;
            try
            {
                Cursor = Cursors.Wait;
                uploading = true;
                UpdateControls();
                string email = checkBoxRequires2FA.IsChecked == true ? textBoxEmail.Text : "";
                await RestClient.RegisterUser(textBoxUsername.Text, passwordBoxUser.Password, checkBoxRequires2FA.IsChecked == true, textBoxEmail.Text);
                Cursor = old;
                uploading = false;
                MessageBox.Show(Properties.Resources.CLOUD_REGISTER_SUCCEEDED, Title, MessageBoxButton.OK, MessageBoxImage.Information);
                Settings.Default.CloudUsername = textBoxUsername.Text;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Cursor = old;
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
                uploading = false;
                UpdateControls();
            }
        }

        private void OnChanged(object sender, RoutedEventArgs e)
        {
            if (init) return;
            if (checkBoxRequires2FA.IsChecked == false)
            {
                textBoxEmail.Text = "";
                textBoxConfirmEmail.Text = "";
            }
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
