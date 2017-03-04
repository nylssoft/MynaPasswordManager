# PasswordManager
A simple password manager to learn how to use GitHub and a learning project how to build WPF desktop applications.

The passwords for each item are encrypted with the Rijndael algorithm using an encrypted key file (also with Rijndael).
Preferable the key file should be stored on a different device than the password file (e.g. a USB stick).
The key file is encrypted using the SHA-256 hash of the master password.
The process itself tries to keep the plain text passwords in memory as short as possible. The SecureString class is used to store passwords in memory for the life time of the process.

Learning tasks includes

- How to use CustomCommands in XAML,
- How to build menus, context menues and tool bars,
- How to work with a list view,
- Thumbnails for list view items,
- Hide or show columns in a list view,
- Usage of RijndaelManaged and SHA256Managed for encryption and hashing,
- Read and write of XML files,
- Static localization within XAML (currently English and German).

TODOs:

- Dynamic localization using a MarkupExtension
- A settings dialog, e.g. for the time the password is shown in plain text (30 seconds) or the master password has to be reentered (5 minutes)
- Learn how to provide "skins" using XAML styles.
- Add license information for the images that are taken from the Open Icon Library (might be necessary to mention several licenses as each image might have a different license)
- Add license information for the PasswordManager, will likely by the MIT license if this is in accordance with the image licenses of Open Icon Library
- A new project name...
- Provide a WiX installer for windows using the WiX Toolset, not yet available for VS 2017.

Installation:
Use the ZIP file in the install directory by now. Requires at least Windows 7.
