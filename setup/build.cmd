echo off

setlocal
rem Edit and uncomment the following line (or set a global environment variable) if node is installed in a non-default location
rem set GPII_INST_NODE=C:\Program Files\nodejs\node.exe

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

endlocal

rem Copy files to staging\windows
if exist "..\staging\windows" (
	echo Cleaning up staging\windows...
	rmdir /s /q ..\staging\windows
)
robocopy /job:staging.rcj

if not exist "..\staging\windows" (
	echo ERROR - You need to edit staging.rcj before proceeding
	exit /b -1
)

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