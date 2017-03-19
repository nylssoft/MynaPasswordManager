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
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
    public partial class SettingsWindow : Window
    {
        private bool changed;

        public SettingsWindow()
        {
            Title = Properties.Resources.TITLE_SETTINGS;
            InitializeComponent();
            textBoxAutoClearClipboard.Text = Convert.ToString(Properties.Settings.Default.AutoClearClipboard);
            textBoxAutoHidePassword.Text = Convert.ToString(Properties.Settings.Default.AutoHidePassword);
            textBoxReenterPassword.Text = Convert.ToString(Properties.Settings.Default.ReenterPassword);
            changed = false;
            UpdateControls();
            textBoxAutoClearClipboard.Focus();
        }

        private void UpdateControls()
        {
            int val = -1;
            bool ok = Int32.TryParse(textBoxAutoClearClipboard.Text, out val) && val >= 0;
            if (ok)
            {
                ok = Int32.TryParse(textBoxAutoHidePassword.Text, out val) && val >= 0;
            }
            if (ok)
            {
                ok = Int32.TryParse(textBoxReenterPassword.Text, out val) && val >= 0;
            }
            buttonOK.IsEnabled = ok && changed;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            int val = -1;
            if (Int32.TryParse(textBoxAutoClearClipboard.Text, out val))
            {
                Properties.Settings.Default.AutoClearClipboard = val;
            }
            if (Int32.TryParse(textBoxAutoHidePassword.Text, out val))
            {
                Properties.Settings.Default.AutoHidePassword = val;
            }
            if (Int32.TryParse(textBoxReenterPassword.Text, out val))
            {
                Properties.Settings.Default.ReenterPassword = val;
            }
            DialogResult = true;
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            changed = true;
            UpdateControls();
        }
    }
}
