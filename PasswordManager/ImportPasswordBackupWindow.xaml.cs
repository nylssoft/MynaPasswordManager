/*
    Myna Password Manager
    Copyright (C) 2026 Niels Stockfleth

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
using System.Security;
using System.Windows;

namespace PasswordManager
{
    public partial class ImportPasswordBackupWindow : Window
    {
        public SecureString Passphrase { get; private set; }

        public ImportPasswordBackupWindow(Window owner)
        {
            Owner = owner;
            Title = Properties.Resources.TITLE_IMPORT_PASSWORD_BACKUP;
            Topmost = Properties.Settings.Default.Topmost;
            InitializeComponent();
            passwordBox.Focus();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            buttonOK.IsEnabled = passwordBox.SecurePassword.Length >= 12;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Passphrase = passwordBox.SecurePassword.Copy();
            Passphrase.MakeReadOnly();
            passwordBox.Clear();
            DialogResult = true;
            Close();
        }
    }
}
