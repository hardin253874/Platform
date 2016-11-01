@ECHO OFF

SET SOURCEBINPATH=C:\Development\ThirdParty\Microsoft\Silverlight\v5.0\Toolkit\Dec2011\Source\Binaries

IF NOT EXIST .\Bin (	
	MKDIR .\Bin	
)
IF NOT EXIST .\Bin\Design (	
	MKDIR .\Bin\Design	
)
IF NOT EXIST .\Themes (	
	MKDIR .\Themes	
)

ECHO ***** Copying binaries from custom build location *****
FOR /F "tokens=1,2 delims=|" %%i in (SilverlightToolkitFiles.txt) DO (
	ECHO Copying file from "%SOURCEBINPATH%\%%j" to "%%i\%%j"
	IF NOT EXIST "%SOURCEBINPATH%\%%j" (
		ECHO Source file "%SOURCEBINPATH%\%%j" does not exist.
		GOTO failed 	
	)
	COPY "%SOURCEBINPATH%\%%j" "%%i\%%j" /Y
	IF NOT EXIST ".\%%i\%%j" (
		ECHO Failed to copy file "%SOURCEBINPATH%\%%j" to "%%i\%%j".
		GOTO failed 	
	)
)

GOTO exitcompleted

:failed
ECHO ***** Failed *****
ECHO ***** One or more Silverlight toolkit files failed to copy. *****
GOTO exit

:exitcompleted
ECHO ***** Completed *****
GOTO exit

:exit