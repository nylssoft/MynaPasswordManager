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
            Title = Properties.Resources.CMD_GENERATE_PASSWORD;
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
