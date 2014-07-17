path %path%;C:\Users\nminogiannis\Apps\MinGW\bin;C:\Users\nminogiannis\Apps\Wix;C:\Program Files (x86)\MSBuild\12.0\Bin

pushd ..\dist\windows
	call clean.cmd
	
	git reset --hard
	git clean -fd
	git pull

	pushd .\UsbUserListener
		mkdir bin
		copy lib\libcurl.dll bin
		call build.cmd
	popd
popd

pushd ..\dist\node_modules\universal
	git reset --hard
	git clean -fd
	git pull

	call npm install
popd

msbuild setup.msbuild
