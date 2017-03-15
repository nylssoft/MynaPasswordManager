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
using System.Reflection;
using System.Windows;

namespace PasswordManager
{
    public partial class AboutWindow : Window
    {
        public AboutWindow(Window owner)
        {
            Owner = owner;
            var assembly = Assembly.GetExecutingAssembly();
            var productAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true);
            var versionAttribute = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true);
            if (productAttribute.Length > 0 && versionAttribute.Length > 0)
            {
                var p = productAttribute[0] as AssemblyProductAttribute;
                var v = versionAttribute[0] as AssemblyFileVersionAttribute;
                if (p != null && v != null)
                {
                    Title = $"{p.Product} Version {v.Version}";
                }
            }
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
