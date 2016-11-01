@echo off
ECHO %date% %time% - Killing IIS worker processes...
taskkill -f -im w3wp.exe

REM delete the database
ECHO %date% %time% - Setting 'SoftwarePlatform' database to single user mode...
sqlcmd -E -S localhost -Q "ALTER DATABASE SoftwarePlatform SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Dropping 'SoftwarePlatform' database...
sqlcmd -E -S localhost -Q "DROP DATABASE SoftwarePlatform"
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Copying the SQL assembly
COPY "..\Deployment\SoftwarePlatformSetup\Dependencies\EDC.SoftwarePlatform.SQL.*" "C:\Program Files\ReadiNow\SoftwarePlatform\Bin"

ECHO %date% %time% - Create the new database 
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -cdb -database SoftwarePlatform -datadirectory "C:\PlatformDatabase\" -streamdirectory "C:\PlatformFilestream\"
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Create the user in the database
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -dbu  -account "ENTDATA\svc-swp-iis" -database SoftwarePlatform -server localhost
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Copying the Platform entity files
COPY "..\EDC.ReadiNow.Common\Config\Schema\*.xml"    "C:\Program Files\ReadiNow\SoftwarePlatform\Solutions\Core"
COPY "..\EDC.ReadiNow.Common\Config\Data\*.xml"      "C:\Program Files\ReadiNow\SoftwarePlatform\Solutions\Core"
COPY "..\EDC.ReadiNow.Common\Config\Database\*.sql"  "C:\Program Files\ReadiNow\SoftwarePlatform\Solutions\Core"
COPY "..\EDC.ReadiNow.Common\Config\*.xml"           "C:\Program Files\ReadiNow\SoftwarePlatform\Solutions\Core"
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Installing bootstrap solutions
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -is -solution "C:\Program Files\ReadiNow\SoftwarePlatform\Solutions\Core\Solution.xml" -database SoftwarePlatform -server localhost
IF %ERRORLEVEL% NEQ 0 GOTO End

ECHO %date% %time% - Copying Applications
COPY "..\ApplicationCache\*.db"	"C:\Program Files\ReadiNow\SoftwarePlatform\Applications"
IF %ERRORLEVEL% NEQ 0 GOTO End

call ImportApplications.bat
IF %ERRORLEVEL% NEQ 0 GOTO End

REM Install the default EDC solution
ECHO %date% %time% - Activating the EDC Tenant...
call ConfigureNewTenant.bat EDC

iisreset

:End