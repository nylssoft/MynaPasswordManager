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
using System;
using System.Security;
using System.Windows;

namespace PasswordManager
{
    public partial class GeneratePasswordWindow : Window
    {
        private PasswordGenerator generator = new PasswordGenerator();

        public SecureString Password { get; private set; }

        public GeneratePasswordWindow()
        {
            Title = Properties.Resources.TITLE_GENERATE_PASSWORD;
            InitializeComponent();
            ButtonGenerate_Click(null, null);
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Password = generator.Generate();
                textBoxPassword.Text = Password.GetAsString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled = textBoxPassword.Text.Length > 0;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
