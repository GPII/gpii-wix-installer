# GPII Windows installer
[WiX](http://wixtoolset.org/) based Windows installer for GPII

## Building
### Requirements
- A local copy of the [GPII/windows](https://github.com/GPII/windows) repository. You should be able to build the repository, so all requirements of building GPII/windows apply here as well.
- [WiX Toolset v3.10](http://wixtoolset.org/)
- [MSBuild Extension Pack v4](http://www.msbuildextensionpack.com/)

Make sure MSBuild and the WiX folder are in the path.

### Staging
- Build GPII/windows (including the listeners - see https://github.com/GPII/windows/tree/master/provisioning)
- Edit line 10 of [`setup/build.cmd`](https://github.com/GPII/gpii-wix-installer/blob/master/setup/build.cmd) and replace `C:\projects\gpii\windows` with the path of your local GPII/windows repository (**no** trailing backslash)
- Run [`setup/build.cmd`](https://github.com/GPII/gpii-wix-installer/blob/master/setup/build.cmd)

Staging makes a private copy of the GPII/windows repository inside the `staging` folder and uses that for building the installer. Any files not needed (eg Git and Vagrant related folders) are excluded. 

After running `build.cmd`, an MSI file can be found in the `output` folder.  
Whenever there are changes to the local GPII/windows repository, rerun `build.cmd` to make a new installer.

### Signing
The MSI file can be optionally signed in the last step of the build. 

You will need a code signing certificate in the PKCS#12/PFX format.
- Set the `GPII_SIGNING_CERT` environment variable with the location of the certificate (eg `C:\gpii\my_certificate.pfx`)
- Set the `GPII_SIGNING_CERT_PASSWORD` environment variable with the PFX password (or `""` if none)
- Set the `GPII_SIGNING_DESCRIPTION` environment variable with a description (eg `"GPII Installer"`)

Use double quotes if there are spaces in any of the values. 

## Unattended installation
The installer is a standard MSI file and as such it can be also executed using `msiexec.exe` and supports all the [command-line options](https://msdn.microsoft.com/en-us/library/windows/desktop/aa367988(v=vs.85).aspx) available to `msiexec.exe`.

To perform an unattended installation (requires administrative rights):
```
msiexec /qn /i GPII.msi
```
To perform an unattended installation and keep a verbose log to `log.txt`:
```
msiexec /qn /lv log.txt /i GPII.msi
```
To silently uninstall:
```
msiexec /qn /x GPII.msi
```
### Customizing
You can pass a number of property key/values to the installer to customize its behavior.  
For example, to perform an unattended installation to `C:\GPII`:
```
msiexec /qn /i GPII.msi INSTALLFOLDER=C:\GPII
```
To perform an unattended installation to `C:\GPII` that **does not** include the listeners:
```
msiexec /qn /i GPII.msi INSTALLFOLDER=C:\GPII ADDLOCAL=GPIIFeature,VCRedist
```
To perform an unattended installation to `C:\GPII` that **does not** include the listeners and creates no desktop shortcuts:
```
msiexec /qn /i GPII.msi INSTALLFOLDER=C:\GPII ADDLOCAL=GPIIFeature,VCRedist DESKTOP_SHORTCUTS=0
```
The list of possible properties include:

| Name | Value | Description |
| ---- | ------ | ----------- |
| INSTALLFOLDER | A folder name<br /><br />**Default:** <code>C:\Program&nbsp;Files\GPII</code> or <code>C:\Program&nbsp;Files&nbsp;(x86)\GPII</code> | The installation folder |
| ADDLOCAL | Comma (,) separated list<br/></br/>Possible values:<br/><ul><li>`GPIIFeature`</li><li>`VCRedist`</li><li>`USBListenerFeature`</li><li>`RFIDListenerFeature`</li><li>`ALL` **(Default)**</li></ul> | The GPII features that will get installed.<br /><br/>It is advisable to always include `GPIIFeature` (the main GPII platform) and `VCRedist` (the Visual C++ Redistributable Package) and only use this property to control installation (or not) of the listeners. |
| SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any desktop and start menu shortcuts |
| DESKTOP_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any desktop shortcuts |
| START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any start menu shortcuts |
| GPII_SHORTCUTS<br/>GPII_DESKTOP_SHORTCUTS<br/>GPII_START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create shortcuts for the main GPII platform |
| USB_LISTENER_SHORTCUTS<br/>USB_LISTENER_DESKTOP_SHORTCUTS<br/>USB_LISTENER_START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create shortcuts for the USB Listener |
| RFID_LISTENER_SHORTCUTS<br/>RFID_LISTENER_DESKTOP_SHORTCUTS<br/>RFID_LISTENER_START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create shortcuts for the RFID Listener |
| GPII_AUTOSTART<br/>RFID_LISTENER_AUTOSTART<br/>USB_LISTENER_AUTOSTART | `0` **(Default)** or `1` | Whether or not to autostart the respective component on Windows sign in |

An exhaustive list of other properties can be found at the [Windows Installer Guide](https://msdn.microsoft.com/en-us/library/windows/desktop/aa370905(v=vs.85).aspx).