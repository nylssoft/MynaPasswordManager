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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Repository
{
    public class Password
    {
        public Password()
        {
            Id = Guid.NewGuid().ToString();
            SecurePassword = new SecureString();
            Name = string.Empty;
            Login = string.Empty;
            Url = string.Empty;
            Description = string.Empty;
        }

        public Password(Password pwd)
        {
            Id = pwd.Id;
            Name = pwd.Name;
            Login = pwd.Login;
            Url = pwd.Url;
            Description = pwd.Description;
            SecurePassword = pwd.SecurePassword;
            SecurePassword.MakeReadOnly();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Login { get; set; }

        public SecureString SecurePassword { get; set; }

        public string Url { get; set; }

        public string Description { get; set; }
    }
}
