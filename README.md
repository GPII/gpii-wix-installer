# GPII Windows installer
[WiX](http://wixtoolset.org/) based Windows installer for GPII

## Building

This code is pulled and executed from [GPII/gpii-app's Installer.ps1 provisioning script](https://github.com/GPII/gpii-app/blob/master/provisioning/Installer.ps1) and can be called manually, but the latter is only for people that are familiar to this code.

### Requirements
- [WiX Toolset v3.11.2](http://wixtoolset.org/)
- [MSBuild Extension Pack v4](http://www.msbuildextensionpack.com/)

Make sure MSBuild and the WiX folder are in the path.

### Staging
The `staging` folder is where the content is going to be placed before building the installer.

After running `setup.msbuild`, an MSI file can be found in the `output` folder.

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
To perform an unattended installation to `C:\GPII` that creates no desktop shortcuts:
```
msiexec /qn /i GPII.msi INSTALLFOLDER=C:\GPII ADDLOCAL=GPIIFeature,VCRedist DESKTOP_SHORTCUTS=0
```
The list of possible properties include:

| Name | Value | Description |
| ---- | ------ | ----------- |
| INSTALLFOLDER | A folder name<br /><br />**Default:** <code>C:\Program&nbsp;Files\GPII</code> or <code>C:\Program&nbsp;Files&nbsp;(x86)\GPII</code> | The installation folder |
| ADDLOCAL | Comma (,) separated list<br/></br/>Possible values:<br/><ul><li>`GPIIFeature`</li><li>`VCRedist`</li><li>`ALL` **(Default)**</li></ul> | The GPII features that will get installed.<br /><br/>It is advisable to always include `GPIIFeature` (the main GPII platform) and `VCRedist` (the Visual C++ Redistributable Package). |
| SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any desktop and start menu shortcuts |
| DESKTOP_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any desktop shortcuts |
| START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create any start menu shortcuts |
| GPII_SHORTCUTS<br/>GPII_DESKTOP_SHORTCUTS<br/>GPII_START_MENU_SHORTCUTS | `0` or `1` **(Default)** | Whether or not to create shortcuts for the main GPII platform |

An exhaustive list of other properties can be found at the [Windows Installer Guide](https://msdn.microsoft.com/en-us/library/windows/desktop/aa370905(v=vs.85).aspx).
