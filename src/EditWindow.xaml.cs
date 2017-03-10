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
using System.Windows;
using System.Windows.Media;

namespace PasswordManager
{
    public partial class EditWindow : Window
    {
        public Password Password { get; private set; }

        private bool pwdChanged;

        public EditWindow(Window owner, string title, ImageSource icon, Password password = null)
        {
            Owner = owner;
            InitializeComponent();
            Title = title;
            Icon = icon;
            Password = new Password();
            if (password != null)
            {
                Password = password;
                textBoxName.Text = password.Name;
                textBoxLogin.Text = password.Login;
                passwordBox.Password = "********";
                passwordBoxConfirmed.Password = "********";
                textBoxUrl.Text = password.Url;
                textBoxDescription.Text = password.Description;
            }
            pwdChanged = false;
            textBoxName.Focus();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (!passwordBoxConfirmed.SecurePassword.IsEqualTo(passwordBox.SecurePassword))
            {
                MessageBox.Show(Properties.Resources.ERROR_PASSWORD_DOES_NOT_MATCH);
                return;
            }
            Password.Name = textBoxName.Text;
            Password.Login = textBoxLogin.Text;
            Password.Url = textBoxUrl.Text;
            Password.Description = textBoxDescription.Text;
            if (pwdChanged)
            {
                Password.SecurePassword = passwordBox.SecurePassword;
            }
            DialogResult = true;
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            pwdChanged = true;
            passwordBoxConfirmed.Password = string.Empty;
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new GeneratePasswordWindow(this);
                if (dlg.ShowDialog() == true)
                {
                    passwordBox.Password = dlg.Password.GetAsString();
                    passwordBoxConfirmed.Password = passwordBox.Password;
                    pwdChanged = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
