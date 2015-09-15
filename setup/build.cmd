pushd ..\staging

	rmdir /s /q node_modules
    if not exist windows\\. git clone https://github.com/GPII/windows.git
	
	pushd .\windows

		git reset --hard
		git clean -fd
		git pull
			
		call npm install --ignore-scripts=true
		
		pushd .\listeners
			msbuild /p:Configuration=Release
		popd
		
		call grunt build	
	popd
popd

pushd ..\
	if not exist output\\. mkdir output
	if not exist temp\\. mkdir temp
popd

msbuild setup.msbuild
