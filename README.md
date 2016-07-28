# GPII Windows installer
[WiX](http://wixtoolset.org/) based Windows installer for GPII

## Building
### Requirements
- A local copy of the [GPII/windows](https://github.com/GPII/windows) repository. You should be able to build the repository, so all requirements of building GPII/windows apply here as well.
- [WiX Toolset v3.10](http://wixtoolset.org/)
- [MSBuild Extension Pack v4](http://www.msbuildextensionpack.com/)

Make sure MSBuild and the WiX folder are in the path.

### Staging
- Build GPII/windows (including the listeners - see https://github.com/GPII/windows/blob/master/provisioning/build.bat)
- Edit [`setup/staging.rcj`](https://github.com/GPII/gpii-wix-installer/blob/master/setup/staging.rcj) and replace `C:\projects\gpii\windows\` with the path of your local GPII/windows repository
- Edit [`setup/build.cmd`](https://github.com/GPII/gpii-wix-installer/blob/master/setup/build.cmd) and uncomment lines 4-5 and 8-9.
- Run [`setup/build.cmd`](https://github.com/GPII/gpii-wix-installer/blob/master/setup/build.cmd)

Staging makes a private copy of the GPII/windows repository inside the `staging` folder and uses that for building the installer. Any files not needed(eg Git and Vagrant related folders) are excluded. 

After running `build.cmd`, an MSI file can be found in the `output` folder.  
Whenever there are changes to the local GPII/windows repository just rerun `build.cmd` to build a new installer.
