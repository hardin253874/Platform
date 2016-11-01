@echo off
setlocal enabledelayedexpansion
REM backup the database

SET DBBCK=%~dp1%~n1%~x1
echo backing up to "%DBBCK%"
if "%DBBCK%" == "" goto nodb

SET FILEREPOBACKUPDIR=%~dp1%~n1_FileRepo
echo backing up file repositories to "%FILEREPOBACKUPDIR%"
if "%FILEREPOBACKUPDIR%" == "" goto nodb

ECHO %date% %time% - Backup the database... to %DBBCK%
if exist "%DBBCK%" del "%DBBCK%"

sqlcmd -E -S localhost -b -Q "BACKUP DATABASE SoftwarePlatform TO DISK = '%DBBCK%'"
if !errorlevel! neq 0 exit /B !errorlevel!

ECHO %date% %time% - Backup the file repositories... to %FILEREPOBACKUPDIR%

if exist "%FILEREPOBACKUPDIR%" rmdir /S /Q %FILEREPOBACKUPDIR%
mkdir %FILEREPOBACKUPDIR%
xcopy C:\PlatformFileRepos %FILEREPOBACKUPDIR% /E /Q /Y /V
if !errorlevel! neq 0 exit /B !errorlevel!

ECHO %date% %time% - backup done
exit /B 0

:nodb
echo Please specify full path to db bak file name
exit /b 1