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

        public GeneratePasswordWindow(Window owner)
        {
            Owner = owner;
            Title = Properties.Resources.TITLE_GENERATE_PASSWORD;
            InitializeComponent();
            textBoxDigits.Text = Properties.Settings.Default.PasswordGeneratorDigits;
            textBoxSymbols.Text = Properties.Settings.Default.PasswordGeneratorSymbols;
            textBoxUpperChars.Text = Properties.Settings.Default.PasswordGeneratorUpperCharacters;
            textBoxLowerChars.Text = Properties.Settings.Default.PasswordGeneratorLowerCharacters;
            textBoxLength.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorLength);
            textBoxMinDigits.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinDigits);
            textBoxMinSymbols.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinSymbols);
            textBoxMinUpperChars.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinUpperCharacters);
            textBoxMinLowerChars.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinLowerCharacters);
            ButtonGenerate_Click(null, null);
        }

        private bool Validate(PasswordGenerator gen)
        {
            if (gen.Length < 4 || gen.Length > 40 ||
                gen.MinDigits + gen.MinSymbols + gen.MinLowerCharacters + gen.MinUpperCharacters > gen.Length ||
                gen.MinDigits < 0 || gen.MinSymbols < 0 || gen.MinLowerCharacters < 0 || gen.MinUpperCharacters < 0 ||
                gen.Symbols.Length == 0 && gen.Digits.Length == 0 && gen.UpperCharacters.Length == 0 && gen.LowerCharacters.Length == 0 ||
                gen.MinDigits > 0 && gen.Digits.Length == 0 ||
                gen.MinSymbols > 0 && gen.Symbols.Length == 0 ||
                gen.MinUpperCharacters > 0 && gen.UpperCharacters.Length == 0 ||
                gen.MinLowerCharacters > 0 && gen.LowerCharacters.Length == 0)
            {
                MessageBox.Show(Properties.Resources.ERROR_PWDGEN_INVALID_INPUT, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private int ToInt(string txt)
        {
            int val = -1;
            Int32.TryParse(txt, out val);
            return val;
        }

        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                generator.Digits = textBoxDigits.Text;
                generator.Symbols = textBoxSymbols.Text;
                generator.UpperCharacters = textBoxUpperChars.Text;
                generator.LowerCharacters = textBoxLowerChars.Text;
                generator.Length = ToInt(textBoxLength.Text);
                generator.MinDigits = ToInt(textBoxMinDigits.Text);
                generator.MinSymbols = ToInt(textBoxMinSymbols.Text);
                generator.MinUpperCharacters = ToInt(textBoxMinUpperChars.Text);
                generator.MinLowerCharacters = ToInt(textBoxMinLowerChars.Text);
                if (Validate(generator))
                {
                    Password = generator.Generate();
                    textBoxPassword.Text = Password.GetAsString();
                }
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
