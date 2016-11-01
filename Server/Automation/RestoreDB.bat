@echo off
setlocal enabledelayedexpansion
REM RESTORE the database

SET DBBCK=%~dp1%~n1%~x1
echo Restoring from "%DBBCK%"
if "%DBBCK%" == "" goto nodb
if not exist "%DBBCK%" goto nodbexist

SET FILEREPOBACKUPDIR=%~dp1%~n1_FileRepo
echo Restoring file repositories from "%FILEREPOBACKUPDIR%"

iisreset /stop
NET STOP SchedulerService

ECHO %date% %time% - Restore the database from "%DBBCK%"
sqlcmd -E -S localhost -b -Q "ALTER DATABASE SoftwarePlatform SET SINGLE_USER WITH ROLLBACK IMMEDIATE RESTORE DATABASE SoftwarePlatform FROM DISK = '%DBBCK%' WITH REPLACE ALTER DATABASE SoftwarePlatform SET MULTI_USER"
if !errorlevel! neq 0 exit /B !errorlevel!

if exist "%FILEREPOBACKUPDIR%" (
	ECHO %date% %time% - Restore file repository from "%FILEREPOBACKUPDIR%"

	mkdir C:\PlatformFileRepos
	xcopy %FILEREPOBACKUPDIR% C:\PlatformFileRepos /E /Q /Y /V
	if !errorlevel! neq 0 exit /B !errorlevel!
)

iisreset /start
NET START SchedulerService

ECHO %date% %time% - restore done
exit /B 0

:nodb
echo Please specify full path to db bak file name
exit /b 1

:nodbexist
echo Cannot find the given database backup file
exit /b 1
