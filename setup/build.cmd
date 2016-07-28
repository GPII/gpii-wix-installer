rem Staging
rem Edit staging.rcj and then uncomment the following lines	as needed
	
rem rmdir /s /q ..\staging\windows
rem robocopy /job:staging.rcj
	
pushd ..\staging\windows
	rem call npm prune --production
	rem call npm dedupe	
popd
		
pushd ..\
	if not exist output\\. mkdir output
	if not exist temp\\. mkdir temp
popd

rem Build the installer
msbuild setup.msbuild