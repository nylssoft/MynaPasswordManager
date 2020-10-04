/*
    Myna Password Manager
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
        private bool init;

        public SettingsWindow(Window owner)
        {
            Owner = owner;
            Title = Properties.Resources.TITLE_SETTINGS;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            init = true;
            textBoxCloudUrl.Text = Properties.Settings.Default.CloudUrl;
            checkBoxTopmost.IsChecked = Properties.Settings.Default.Topmost;
            textBoxAutoClearClipboard.Text = Convert.ToString(Properties.Settings.Default.AutoClearClipboard);
            textBoxAutoHidePassword.Text = Convert.ToString(Properties.Settings.Default.AutoHidePassword);
            textBoxReenterPassword.Text = Convert.ToString(Properties.Settings.Default.ReenterPassword);
            checkBoxLock.IsChecked = Properties.Settings.Default.AutoLockWindow;
            checkBoxMinimize.IsChecked = Properties.Settings.Default.AutoMinimizeWindow;
            UpdateControls();
            textBoxCloudUrl.Focus();
            init = false;
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
            if (ok)
            {
                ok = textBoxCloudUrl.Text.Length == 0 || textBoxCloudUrl.Text.StartsWith("https://");
            }
            checkBoxMinimize.IsEnabled = checkBoxLock.IsChecked == true;
            if (!checkBoxMinimize.IsEnabled && checkBoxMinimize.IsChecked == true)
            {
                init = true;
                checkBoxMinimize.IsChecked = false;
                init = false;
            }
            buttonOK.IsEnabled = ok && changed;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            int val = -1;
            Properties.Settings.Default.CloudUrl = textBoxCloudUrl.Text;
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
            Properties.Settings.Default.Topmost = Topmost;
            Properties.Settings.Default.AutoLockWindow = checkBoxLock.IsChecked == true;
            Properties.Settings.Default.AutoMinimizeWindow = checkBoxMinimize.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (init) return;
            changed = true;
            UpdateControls();
        }

        private void checkBoxTopmost_Checked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Topmost = true;
            Owner.Topmost = true;
            changed = true;
            UpdateControls();
        }

        private void checkBoxTopmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            Topmost = false;
            Owner.Topmost = false;
            changed = true;
            UpdateControls();
        }

        private void CheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            if (init) return;
            changed = true;
            UpdateControls();
        }
    }
}
