/*
    Myna Password Manager
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
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PasswordManager
{
    public static class Extensions
    {
        #region Window extensions

        public static void RestorePosition(this Window window, double left, double top, double width, double height)
        {
            var virtualWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var virtualHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            height = Math.Min(height, virtualHeight);
            width = Math.Min(width, virtualWidth);
            if (width >= window.MinWidth && height >= window.MinHeight)
            {
                if (top + height / 2 > virtualHeight)
                {
                    top = virtualHeight - height;
                }
                if (left + width / 2 > virtualWidth)
                {
                    left = virtualWidth - width;
                }
                window.Left = left;
                window.Top = top;
                window.Width = width;
                window.Height = height;
            }
        }

        #endregion

        #region ListViewItem extensions

        public static ListViewItem GetItemAt(this ListView listView, Point clientRelativePosition)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listView, clientRelativePosition);
            var selectedItem = hitTestResult.VisualHit;
            while (selectedItem != null)
            {
                if (selectedItem is ListViewItem)
                {
                    break;
                }
                selectedItem = VisualTreeHelper.GetParent(selectedItem);
            }
            return selectedItem != null ? ((ListViewItem)selectedItem) : null;
        }

        #endregion

        #region SecureString extensions

        public static string GetAsString(this SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
        }

        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 != length2)
                {
                    return false;
                }
                for (int x = 0; x < length1; ++x)
                {
                    byte b1 = Marshal.ReadByte(bstr1, x);
                    byte b2 = Marshal.ReadByte(bstr2, x);
                    if (b1 != b2)
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr2);
                }
                if (bstr1 != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(bstr1);
                }
            }
        }

        #endregion

        #region string extensions

        public static string ReplaceSpecialFolder(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("%MyDocuments%"))
                {
                    str = str.Replace("%MyDocuments%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }
                if (str.Contains("%ProgramData%"))
                {
                    str = str.Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                }
                if (str.Contains("%Module%"))
                {
                    string moddir = AppDomain.CurrentDomain.BaseDirectory;
                    if (moddir.EndsWith("\\"))
                    {
                        moddir = moddir.Substring(0, moddir.Length - 1);
                    }
                    str = str.Replace("%Module%", moddir);
                }
            }
            return str;
        }

        #endregion
    }
}
