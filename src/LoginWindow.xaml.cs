using PasswordManager.Repository;
using System;
using System.IO;
using System.Security;
using System.Text;
using System.Windows;

namespace PasswordManager
{
    public partial class LoginWindow : Window
    {
        private KeyDirectoryCache keyDirCache;
        private string repositoryFile;

        public SecureString SecurePassword { get; set; }

        public PasswordRepository PasswordRepository { get; private set; }

        public LoginWindow(string title, KeyDirectoryCache keyDirCache, string repositoryFile)
        {
            Title = title;
            this.keyDirCache = keyDirCache;
            this.repositoryFile = repositoryFile;
            InitializeComponent();
            labelDescription.Content = string.Format(
                Properties.Resources.LABEL_ENTER_MASTER_PASSWORD_FOR_0,
                new FileInfo(repositoryFile).Name);
            var id = PasswordRepository.GetIdFromFile(repositoryFile);
            textBoxKey.Text = id;
            textBoxKeyDirectory.Text = keyDirCache.Get(id);
            passwordBox.Focus();
            UpdateControls();
        }

        private void UpdateControls()
        {
            buttonOK.IsEnabled =
                passwordBox.SecurePassword.Length > 0 &&
                Directory.Exists(textBoxKeyDirectory.Text);
        }
 
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SecurePassword != null)
                {
                    if (!SecurePassword.IsEqualTo(passwordBox.SecurePassword))
                    {
                        MessageBox.Show(Properties.Resources.ERROR_WRONG_PASSWORD, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    var keydir = textBoxKeyDirectory.Text;
                    var id = textBoxKey.Text;
                    bool oldFormat = false;
                    if (!PasswordRepository.ExistKey(keydir, id))
                    {
                        if (!PasswordRepository.MigrateKey(keydir, id, passwordBox.SecurePassword))
                        {
                            MessageBox.Show(
                                string.Format(Properties.Resources.ERROR_KEY_NOT_FOUND_0_1,
                                id,
                                keydir),
                                Title,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                            return;
                        }
                        oldFormat = true;
                    }
                    PasswordRepository = PasswordRepository.Read(repositoryFile, keydir, passwordBox.SecurePassword, oldFormat); // throws exception for wrong password (encoding fails)
                    SecurePassword = passwordBox.SecurePassword;
                    keyDirCache.Set(id, keydir);
                }
                DialogResult = true;
                Close();
            }
            catch (ArgumentException)
            {
                MessageBox.Show(Properties.Resources.ERROR_INVALID_FORMAT, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(Properties.Resources.ERROR_WRONG_PASSWORD, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBoxKeyDirectory_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateControls();
        }

        private void ButtonSelectKeyDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new System.Windows.Forms.FolderBrowserDialog()
                {
                    Description = Properties.Resources.LABEL_SELECT_KEY_DIRECTORY
                };
                if (Directory.Exists(textBoxKeyDirectory.Text))
                {
                    dlg.SelectedPath = textBoxKeyDirectory.Text;
                }
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBoxKeyDirectory.Text = dlg.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.ERROR_OCCURRED_0, ex.Message), Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
