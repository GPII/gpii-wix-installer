echo off

rem Do not override global variables when using set
setlocal

rem Edit and uncomment the following line (or set a global environment variable) if node is installed in a non-default location
rem set GPII_INST_NODE=C:\Program Files\nodejs\node.exe

rem Edit the following line (or set a global environment variable) with the folder of the GPII/windows repository (do no include a trailing backslash)
if not defined GPII_INST_WINDOWS set GPII_INST_WINDOWS=C:\projects\gpii\windows

if defined ProgramFiles(x86) set GPII_INST_X64=1

rem Bundle local copy of node
if defined GPII_INST_NODE (

    rem Using custom node location
    if exist "%GPII_INST_NODE%" (
        echo Copying "%GPII_INST_NODE%" to staging...
        copy "%GPII_INST_NODE%" ..\staging /y
    ) else (
        rem Node not found, exit
        echo ERROR - "%GPII_INST_NODE%" is missing
        exit /b 1
    )

) else (

    rem Autodetect x86 node location
    if defined GPII_INST_X64 (

        rem 64bit OS
        if exist "%ProgramFiles(x86)%\nodejs\node.exe" (
            echo Copying "%ProgramFiles(x86)%\nodejs\node.exe" to staging...
            copy "%ProgramFiles(x86)%\nodejs\node.exe" ..\staging /y
        ) else (
            rem Node not found, exit
            echo ERROR - "%ProgramFiles(x86)%\nodejs\node.exe" is missing
            exit /b 1
        )

    ) else (

        rem 32bit OS
        if exist "%ProgramFiles%\nodejs\node.exe" (
            echo Copying "%ProgramFiles%\nodejs\node.exe" to staging...
            copy "%ProgramFiles%\nodejs\node.exe" ..\staging /y
        ) else (
            rem Node not found, exit
            echo ERROR - "%ProgramFiles%\nodejs\node.exe" is missing
            exit /b -1
        )
    )
)

rem Cleaning up
if exist "..\staging\windows" (
	echo Cleaning up staging\windows...
	rmdir /s /q ..\staging\windows
)

mkdir ..\staging\windows

rem Copy files to staging\windows
robocopy "%GPII_INST_WINDOWS%\gpii"         ..\staging\windows\gpii         /job:staging.rcj
robocopy "%GPII_INST_WINDOWS%\listeners"    ..\staging\windows\listeners    /job:staging.rcj
robocopy "%GPII_INST_WINDOWS%\node_modules" ..\staging\windows\node_modules /job:staging.rcj
robocopy "%GPII_INST_WINDOWS%\tests"        ..\staging\windows\tests        /job:staging.rcj

robocopy "%GPII_INST_WINDOWS%" ..\staging\windows gpii.js index.js package.json README.md LICENSE.txt /NFL /NDL

if not exist "..\staging\windows\gpii" (
	echo ERROR - You need to set GPII_INST_WINDOWS before proceeding
	exit /b -1
)

endlocal

rem Remove development/testing modules
pushd ..\staging\windows
	call npm prune --production
popd
		
pushd ..\
	if not exist output\\. mkdir output
	if not exist temp\\. mkdir temp
popd

rem Build the installer
msbuild setup.msbuild