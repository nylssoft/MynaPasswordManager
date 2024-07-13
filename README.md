# Myna Password Manager

## Overview

A password manager for Windows using WPF.

![screenshot](Screenshots/mynapasswordmanager.png)

## Installation

The program is published as ZIP file. Unpack the ZIP file and start MynaPasswordManager.exe.

The program requires .NET 8.

## Encryption

The passwords for each item are encrypted with the Rijndael algorithm using an encrypted key file.
Preferable the key file should be stored on a different device than the password file.
The key file is encrypted using the SHA-256 hash of the master password.
The process itself tries to keep the plain text passwords in memory as short as possible.

## Features

* Random password generator
* Copy login and password to clipboard and clear clipboard after 30 seconds
* Open website for login
* Shortcuts for most commands
* Thumbnails for login websites
* Secure password storage in encrypted files
* Change of master password and key file
* Available for German and English
* Cloud upload for websites based on (https://github.com/nylssoft/MynaAPIServer)

## Screenshots

### Create New Repository

![Create New Repository Screenshot](Screenshots/mynapasswordmanager_create.png)

### Menu Items

#### File Menu Item

![File Menu Items Screenshot](Screenshots/mynapasswordmanager_file.png)

#### Edit Menu Item

![Edit Menu Items Screenshot](Screenshots/mynapasswordmanager_edit.png)

#### View Menu Item

![View Menu Items Screenshot](Screenshots/mynapasswordmanager_view.png)

#### Context Menu Item and Search Filter

![Filter Context Menu Screenshot](Screenshots/mynapasswordmanager_filterandcontext.png)

### Add Password Entry

![Add Password Entry](Screenshots/mynapasswordmanager_add.png)

### Password Generator

![Password Generator](Screenshots/mynapasswordmanager_pwdgen.png)

### Settings

![Settings](Screenshots/mynapasswordmanager_settings.png)

## Build

- Build with VS 2022

## Licenses

The following icons are used from the **Open Icon Library** (https://sourceforge.net/projects/openiconlibrary):

application-exit-5.png / nuovext2 / LGPL-2.1<br>
document-decrypt-3.png / oxygen / CC-BY-SA 3.0 or LGPL<br>
document-encrypt-3.png / oxygen / CC-BY-SA 3.0 or LGPL<br>
document-new-6.ico / oxygen / CC-BY-SA 3.0 or LGPL<br>
document-open-2.png / echo / CC-BY-SA-3.0<br>
document-properties-2.png / gnome / GPLv2<br>
document-save-7.png / nuovext2 / LGPL-2.1<br>
document-save-as-6.png / nuovext2 / LGPL-2.1<br>
edit.png / gimp / GPLv2<br>
edit-copy-7.png / famfamfam-silk / CC-BY-2.5 or CC-BY-3.0<br>
homepage.png / nuvola / LGPL-2.1<br>
key.png / famfamfam-silk / CC-BY-2.5 or CC-BY-3.0<br>
kgpg_info.png / nuvola / LGPL-2.1<br>
list-add-4.ico / oxygen / CC-BY-SA 3.0 or LGPL<br>
list-remove-4.ico / oxygen / CC-BY-SA 3.0 or LGPL

