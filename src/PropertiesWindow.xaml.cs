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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PasswordManager
{
    /// <summary>
    /// Interaktionslogik für PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        private KeyDirectoryCache keyDirCache;
        private PasswordRepository repository;

        public PropertiesWindow(string title, KeyDirectoryCache keyDirCache, PasswordRepository repository, string filename)
        {
            Title = title;
            this.keyDirCache = keyDirCache;
            this.repository = repository;
            InitializeComponent();
            textBoxName.Text = repository.Name;
            textBoxDescription.Text = repository.Description;
            textBoxPasswordFile.Text = filename;
            textBoxKeyDirectory.Text = keyDirCache.Get(repository.Id);
            textBoxKey.Text = repository.Id;
            textBoxName.Focus();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled = !string.Equals(textBoxName.Text, repository.Name) ||
                !string.Equals(textBoxDescription.Text, repository.Description);
        }
 
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            repository.Name = textBoxName.Text;
            repository.Description = textBoxDescription.Text;
            DialogResult = true;
            Close();
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }
    }
}
