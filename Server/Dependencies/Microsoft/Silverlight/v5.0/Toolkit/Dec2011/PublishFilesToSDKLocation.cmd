@ECHO OFF

SET SILVERLIGHT_TOOLKIT_PATH=C:\Program Files (x86)\Microsoft SDKs\Silverlight\v5.0\Toolkit\dec11
SET SILVERLIGHT_TOOLKIT_BIN_PATH=%SILVERLIGHT_TOOLKIT_PATH%\Bin
SET SILVERLIGHT_TOOLKIT_THEMES_PATH=%SILVERLIGHT_TOOLKIT_PATH%\Themes

SET SILVERLIGHT_TOOLKIT_BIN_BACKUP_PATH=%SILVERLIGHT_TOOLKIT_BIN_PATH%Backup
SET SILVERLIGHT_TOOLKIT_THEMES_BACKUP_PATH=%SILVERLIGHT_TOOLKIT_THEMES_PATH%Backup

IF NOT EXIST "%SILVERLIGHT_TOOLKIT_BIN_BACKUP_PATH%" (	
	ECHO ***** Backing up Silverlight Bin path *****	
	MKDIR "%SILVERLIGHT_TOOLKIT_BIN_BACKUP_PATH%"
	XCOPY "%SILVERLIGHT_TOOLKIT_BIN_PATH%" "%SILVERLIGHT_TOOLKIT_BIN_BACKUP_PATH%" /E /F
	IF NOT errorlevel 0 GOTO toolkitbackupfailed	
)

IF NOT EXIST "%SILVERLIGHT_TOOLKIT_THEMES_BACKUP_PATH%" (	
	ECHO ***** Backing up Silverlight Themes path *****	
	MKDIR "%SILVERLIGHT_TOOLKIT_THEMES_BACKUP_PATH%"
	XCOPY "%SILVERLIGHT_TOOLKIT_THEMES_PATH%" "%SILVERLIGHT_TOOLKIT_THEMES_BACKUP_PATH%" /E /F
	IF NOT errorlevel 0 GOTO toolkitbackupfailed	
)

ECHO ***** Publishing Silverlight toolkit files *****
FOR /F "tokens=1,2 delims=|" %%i in (SilverlightToolkitFiles.txt) DO (
	ECHO Publishing file from "%%i\%%j" to "%SILVERLIGHT_TOOLKIT_PATH%\%%i"
	IF NOT EXIST "%%i\%%j" (
		ECHO Source file "%%i\%%j" does not exist.
		GOTO failed 	
	)
	COPY "%%i\%%j" "%SILVERLIGHT_TOOLKIT_PATH%\%%i" /Y
	IF NOT EXIST "%SILVERLIGHT_TOOLKIT_PATH%\%%i" (
		ECHO Failed to copy file "%%i" to "%SILVERLIGHT_TOOLKIT_PATH%\%%i".
		GOTO failed 	
	)
)

GOTO exitcompleted

:toolkitbackupfailed
ECHO ***** Failed *****
ECHO ***** Failed to backup the Silverlight toolkit files. *****
GOTO exit

:failed
ECHO ***** Failed *****
ECHO ***** One or more Silverlight toolkit files failed to publish. *****
GOTO exit

:exitcompleted
ECHO ***** Completed *****
GOTO exit

:exit