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
