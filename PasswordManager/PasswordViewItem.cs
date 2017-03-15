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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PasswordManager
{
    public class PasswordViewItem : INotifyPropertyChanged
    {
        private BitmapImage image;

        public PasswordViewItem(Password password, BitmapImage image)
        {
            Password = password;
            Login = password.Login;
            Name = password.Name;
            Image = image;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update(Password password)
        {
            Password = password;
            Name = password.Name;
            Login = password.Login;
        }

        public string Name { get; set; }

        public string Login { get; set; }

        public string PasswordString

        {
            get
            {
                if (HidePassword)
                {
                    return "********";
                }
                return Password.SecurePassword.GetAsString();
            }
        }

        public bool HidePassword { get; set; } = true;

        public BitmapImage Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }

        public Password Password { get; set; }
    }
}
