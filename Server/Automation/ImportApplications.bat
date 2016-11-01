@ECHO OFF
@REM Imports the installed applications into the application library

FOR /f "delims=" %%a IN ('dir /b /s "C:\Program Files\ReadiNow\SoftwarePlatform\Applications\*.db"') DO (
	ECHO %date% %time%   - Importing %%~na
	"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -ia -package "%%a" > nul
)