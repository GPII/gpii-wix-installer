pushd ..\staging

    if not exist windows\\. git clone https://github.com/gpii/windows.git	
	pushd .\windows

		git reset --hard
		git clean -fd
		git pull
			
		call npm install
		
		pushd .\listeners
			msbuild /p:Configuration=Release
		popd
		
	popd
	
	if not exist node_modules\\. mkdir node_modules
	pushd .\node_modules
	
		if not exist universal\\. git clone https://github.com/GPII/universal.git
		pushd .\universal
		
			git reset --hard
			git clean -fd
			git pull
				
			rmdir /s /q node_modules
			
			call npm install
			call npm install dedupe-infusion
			node -e "require('dedupe-infusion')()"			
		
		popd
	
	popd
popd

pushd ..\
	if not exist output\\. mkdir output
	if not exist temp\\. mkdir temp
popd

msbuild setup.msbuild
