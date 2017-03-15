/*
    PasswordManager
    Copyright (C) 2017 Niels Stockfleth

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
using PasswordManager.Repository;
using System;
using System.Security;
using System.Windows;

namespace PasswordManager
{
    public partial class ChangeMasterPasswordWindow : Window
    {
        public SecureString SecurePassword { get; private set; }

        public ChangeMasterPasswordWindow(Window owner, string title, SecureString securePassword)
        {
            Owner = owner;
            Title = title;
            SecurePassword = securePassword;
            InitializeComponent();
            passwordBoxOld.Focus();
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled =
                passwordBoxOld.SecurePassword.Length > 0 &&
                passwordBoxNew.SecurePassword.Length > 0;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!SecurePassword.IsEqualTo(passwordBoxOld.SecurePassword))
                {
                    MessageBox.Show(Properties.Resources.ERROR_WRONG_PASSWORD, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (!passwordBoxNew.SecurePassword.IsEqualTo(passwordBoxNewConfirm.SecurePassword))
                {
                    MessageBox.Show($"{Properties.Resources.ERROR_PASSWORD_DOES_NOT_MATCH}", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (passwordBoxNew.SecurePassword.Length < 4)
                {
                    MessageBox.Show($"{Properties.Resources.ERROR_PASSWORD_MIN_CHARS}", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                SecurePassword = passwordBoxNew.SecurePassword;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBoxNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            passwordBoxNewConfirm.Password = string.Empty;
            UpdateControls();
        }

        private void PasswordBoxNewConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }
        private void PasswordBoxOld_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }
    }
}
