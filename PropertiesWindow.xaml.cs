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
