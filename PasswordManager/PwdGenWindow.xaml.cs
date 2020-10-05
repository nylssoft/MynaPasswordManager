/*
    Myna Password Manager
    Copyright (C) 2017-2020 Niels Stockfleth

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
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace PasswordManager
{
    public partial class PwdGenWindow : Window
    {
        private int autoClearClipboardAfterSec = 30; // clear clipboard after 30 seconds, 0 to disable

        private DateTime copiedToClipboardSince;

        private bool copiedToClipboard = false;

        private DispatcherTimer timer;

        private PwdGen generator = new PwdGen();

        public PwdGenWindow()
        {
            Title = Properties.Resources.TITLE_GENERATE_PASSWORD;
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && string.Equals(args[1], "topmost"))
            {
                Topmost = true;
            }
            InitializeComponent();
            autoClearClipboardAfterSec = Properties.Settings.Default.AutoClearClipboard;
            textBoxDigits.Text = Properties.Settings.Default.PasswordGeneratorDigits;
            textBoxSymbols.Text = Properties.Settings.Default.PasswordGeneratorSymbols;
            textBoxUpperChars.Text = Properties.Settings.Default.PasswordGeneratorUpperCharacters;
            textBoxLowerChars.Text = Properties.Settings.Default.PasswordGeneratorLowerCharacters;
            textBoxLength.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorLength);
            textBoxMinDigits.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinDigits);
            textBoxMinSymbols.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinSymbols);
            textBoxMinUpperChars.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinUpperCharacters);
            textBoxMinLowerChars.Text = Convert.ToString(Properties.Settings.Default.PasswordGeneratorMinLowerCharacters);
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(OnTimer);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            ButtonGenerate_Click(null, null);
        }

        private void OnTimer(object obj, EventArgs args)
        {
            try
            {
                UpdateStatus();
                if (copiedToClipboard && autoClearClipboardAfterSec > 0)
                {
                    var tscopied = DateTime.Now - copiedToClipboardSince;
                    if (tscopied.TotalSeconds > autoClearClipboardAfterSec)
                    {
                        copiedToClipboard = false;
                        Clipboard.Clear();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public bool IsClosed { get; set; } = false;


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            timer.Stop();
            timer.IsEnabled = false;
            IsClosed = true;
            if (copiedToClipboard)
            {
                copiedToClipboard = false;
                Clipboard.Clear();
            }
        }

        private void UpdateStatus()
        {
            var status = string.Empty;
            if (copiedToClipboard)
            {
                TimeSpan ts = DateTime.Now - copiedToClipboardSince;
                int sec = Math.Max(0, autoClearClipboardAfterSec - (int)ts.TotalSeconds);
                if (sec > 0)
                {
                    string hidestr;
                    if (sec == 1)
                    {
                        hidestr = Properties.Resources.AUTO_CLEAR_CLIPBOARD_IN_ONE;
                    }
                    else
                    {
                        hidestr = string.Format(Properties.Resources.AUTO_CLEAR_CLIPBOARD_IN_0, sec);
                    }
                    status += " " + hidestr;
                }
            }
            textBlockStatus.Text = status;
        }

        private bool Validate(PwdGen gen)
        {
            if (gen.Length < 4 || gen.Length > 40 ||
                gen.MinDigits + gen.MinSymbols + gen.MinLowerCharacters + gen.MinUpperCharacters > gen.Length ||
                gen.MinDigits < 0 || gen.MinSymbols < 0 || gen.MinLowerCharacters < 0 || gen.MinUpperCharacters < 0 ||
                gen.Symbols.Length == 0 && gen.Digits.Length == 0 && gen.UpperCharacters.Length == 0 && gen.LowerCharacters.Length == 0 ||
                gen.MinDigits == 0 && gen.MinSymbols == 0 && gen.MinLowerCharacters == 0 && gen.MinUpperCharacters == 0 ||
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

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(textBlockPassword.Text);
                copiedToClipboardSince = DateTime.Now;
                copiedToClipboard = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                    textBlockPassword.Text = generator.Generate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
