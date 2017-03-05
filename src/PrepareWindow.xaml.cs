using PasswordManager.Repository;
using System;
using System.IO;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PasswordManager
{
    public partial class PrepareWindow : Window
    {
        private KeyDirectoryCache keyDirCache;

        public SecureString SecurePassword { get; private set; }

        public PasswordRepository PasswordRepository { get; private set; }
        
        public PrepareWindow(string title, KeyDirectoryCache keyDirCache)
        {
            Title = title;
            this.keyDirCache = keyDirCache;
            InitializeComponent();
            textBoxKeyDirectory.Text = keyDirCache.GetLastUsed();
            textBoxName.Focus();
            UpdateControls();
        }
 
        private void UpdateControls()
        {
            buttonOK.IsEnabled =
                passwordBox.SecurePassword.Length > 0 &&
                Directory.Exists(textBoxKeyDirectory.Text) &&
                !string.IsNullOrEmpty(textBoxName.Text);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!passwordBox.SecurePassword.IsEqualTo(passwordBoxConfirm.SecurePassword))
                {
                    MessageBox.Show($"{Properties.Resources.ERROR_PASSWORD_DOES_NOT_MATCH}", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (passwordBox.SecurePassword.Length < 4)
                {
                    MessageBox.Show($"{Properties.Resources.ERROR_PASSWORD_MIN_CHARS}", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                SecurePassword = passwordBox.SecurePassword;
                PasswordRepository = new PasswordRepository()
                {
                    Name = textBoxName.Text,
                    Description = textBoxDescription.Text
                };
                keyDirCache.Set(PasswordRepository.Id, textBoxKeyDirectory.Text);
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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            passwordBoxConfirm.Password = string.Empty;
            UpdateControls();
        }

        private void PasswordBoxConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateControls();
        }

        private void TextBoxKeyDirectory_TextChanged(object sender, TextChangedEventArgs e)
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
