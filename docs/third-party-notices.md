# Third-Party Notices

This document lists the principal third-party software, assets, and services used by Myna Password Manager. Check the original licenses and notices, including those of transitive dependencies, when redistributing the application. Direct NuGet dependencies and their versions are listed in [PasswordManager.csproj](../PasswordManager/PasswordManager.csproj).

## Software

| Component | Use | License |
| --- | --- | --- |
| [.NET](https://github.com/dotnet/runtime) and [WPF](https://github.com/dotnet/wpf) | Runtime and desktop UI | [MIT License](https://licenses.nuget.org/MIT) |
| [Microsoft.AspNetCore.Cryptography.KeyDerivation](https://www.nuget.org/packages/Microsoft.AspNetCore.Cryptography.KeyDerivation/) | Password-based key derivation for cloud upload | [MIT License](https://licenses.nuget.org/MIT) |
| [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/) | .NET asynchronous interfaces | [MIT License](https://licenses.nuget.org/MIT) |

## Icons

The project includes icons from the [Open Icon Library](https://sourceforge.net/projects/openiconlibrary/). The original project README recorded the following icon-set and license information. Some original filenames differ from the PNG filenames in this repository; consult the original icon's attribution and license before reuse or redistribution.

| Original listed icon | Icon set | License as previously recorded |
| --- | --- | --- |
| application-exit-5.png | nuovext2 | LGPL-2.1 |
| document-decrypt-3.png | oxygen | CC-BY-SA 3.0 or LGPL |
| document-encrypt-3.png | oxygen | CC-BY-SA 3.0 or LGPL |
| document-new-6.ico | oxygen | CC-BY-SA 3.0 or LGPL |
| document-open-2.png | echo | CC-BY-SA-3.0 |
| document-properties-2.png | gnome | GPLv2 |
| document-save-7.png | nuovext2 | LGPL-2.1 |
| document-save-as-6.png | nuovext2 | LGPL-2.1 |
| edit.png | gimp | GPLv2 |
| edit-copy-7.png | famfamfam-silk | CC-BY-2.5 or CC-BY-3.0 |
| homepage.png | nuvola | LGPL-2.1 |
| key.png | famfamfam-silk | CC-BY-2.5 or CC-BY-3.0 |
| kgpg_info.png | nuvola | LGPL-2.1 |
| list-add-4.ico | oxygen | CC-BY-SA 3.0 or LGPL |
| list-remove-4.ico | oxygen | CC-BY-SA 3.0 or LGPL |

## External services

- Google's favicon service is used to retrieve website thumbnails. Requests include the website's domain and are currently made over HTTP. Google's service terms and privacy policies apply.
- Cloud upload uses the configured MynaAPIServer instance; its operator's terms and privacy policies apply.

For the application's own license, see [LICENSE.txt](../LICENSE.txt).
