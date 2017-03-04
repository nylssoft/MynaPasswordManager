using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static readonly RoutedUICommand Add =
            new RoutedUICommand(
            Properties.Resources.CMD_ADD,
            "Add",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Remove =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE,
            "Remove",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Edit =
            new RoutedUICommand(
            Properties.Resources.CMD_EDIT,
            "Edit",
            typeof(CustomCommands));

        public static readonly RoutedUICommand OpenURL =
            new RoutedUICommand(
            Properties.Resources.CMD_OPEN_URL,
            "OpenURL",
            typeof(CustomCommands));

        public static readonly RoutedUICommand TogglePassword =
            new RoutedUICommand(
            Properties.Resources.CMD_SHOW_PASSWORD,
            "TogglePassword",
            typeof(CustomCommands));

        public static readonly RoutedUICommand CopyLogin =
            new RoutedUICommand(
            Properties.Resources.CMD_COPY_LOGIN,
            "CopyLogin",
            typeof(CustomCommands));

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
    }
}
