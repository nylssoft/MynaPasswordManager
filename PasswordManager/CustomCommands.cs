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
using System.Windows.Input;

namespace PasswordManager
{
    public class CustomCommands
    {
        public static readonly RoutedUICommand New =
            new RoutedUICommand(
            Properties.Resources.CMD_NEW,
            "New",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.N, ModifierKeys.Control) });

        public static readonly RoutedUICommand Open =
            new RoutedUICommand(
            Properties.Resources.CMD_OPEN,
            "Open",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.O, ModifierKeys.Control) });

        public static readonly RoutedUICommand Close =
            new RoutedUICommand(
            Properties.Resources.CMD_CLOSE,
            "Close",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Save =
            new RoutedUICommand(
            Properties.Resources.CMD_SAVE,
            "Save",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.S, ModifierKeys.Control) });

        public static readonly RoutedUICommand SaveAs =
            new RoutedUICommand(
            Properties.Resources.CMD_SAVE_AS,
            "SaveAs",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Props =
            new RoutedUICommand(
            Properties.Resources.CMD_PROPERTIES,
            "Properties",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Return, ModifierKeys.Alt) });

        public static readonly RoutedUICommand Exit =
            new RoutedUICommand(
            Properties.Resources.CMD_EXIT,
            "Exit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });

        public static readonly RoutedUICommand ChangeKeyDirectory =
            new RoutedUICommand(
            Properties.Resources.CMD_CHANGE_KEY_DIRECTORY,
            "ChangeKeyDirectory",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ChangeMasterPassword =
            new RoutedUICommand(
            Properties.Resources.CMD_CHANGE_MASTER_PASSWORD,
            "ChangeMasterPassword",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Add =
            new RoutedUICommand(
            Properties.Resources.CMD_ADD,
            "Add",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) });

        public static readonly RoutedUICommand Remove =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE,
            "Remove",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Delete)});

        public static readonly RoutedUICommand Edit =
            new RoutedUICommand(
            Properties.Resources.CMD_EDIT,
            "Edit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Enter)});

        public static readonly RoutedUICommand OpenURL =
            new RoutedUICommand(
            Properties.Resources.CMD_OPEN_URL,
            "OpenURL",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.G, ModifierKeys.Control) });

        public static readonly RoutedUICommand TogglePassword =
            new RoutedUICommand(
            Properties.Resources.CMD_SHOW_PASSWORD,
            "TogglePassword",
            typeof(CustomCommands));

        public static readonly RoutedUICommand CopyLogin =
            new RoutedUICommand(
            Properties.Resources.CMD_COPY_LOGIN,
            "CopyLogin",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.L, ModifierKeys.Control) });

        public static readonly RoutedUICommand CopyPassword =
            new RoutedUICommand(
            Properties.Resources.CMD_COPY_PASSWORD,
            "CopyPassword",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.P, ModifierKeys.Control) });

        public static readonly RoutedUICommand About =
            new RoutedUICommand(
            Properties.Resources.CMD_ABOUT,
            "About",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ShowLoginColumn =
            new RoutedUICommand(
            Properties.Resources.CMD_SHOW_LOGIN_COLUMN,
            "ShowLoginColumn",
            typeof(CustomCommands));

        public static readonly RoutedUICommand ShowPasswordColumn =
            new RoutedUICommand(
            Properties.Resources.CMD_SHOW_PASSWORD_COLUMN,
            "ShowPasswordColumn",
            typeof(CustomCommands));

        public static readonly RoutedUICommand GeneratePassword =
            new RoutedUICommand(
            Properties.Resources.CMD_GENERATE_PASSWORD,
            "GeneratePassword",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.T, ModifierKeys.Control) });

        public static readonly RoutedUICommand ShowSettings =
            new RoutedUICommand(
            Properties.Resources.CMD_SETTINGS,
            "ShowSettings",
            typeof(CustomCommands));
    }
}
