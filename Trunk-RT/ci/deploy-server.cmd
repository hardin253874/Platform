@echo off

rem deploy the specified RN Server
rem either do it remotely or locally based on the -remote switch

set PWD=%CD%
set SCRIPT=%~n0
set SCRIPTDIR=%~dp0
set roboopts=/NP /NJH /NJS /R:2 /XD .svn .git
set scriptsDir=.\ci

echo SCRIPTDIR=%SCRIPTDIR%
echo PWD=%PWD%

rem **************** process command line args ********************

rem ### set defaults for optional parameters
set remote=
set proj=
set label=latest
set releasesShare=\\SPDEVNAS01.SP.LOCAL\Development\BuildArchives\Releases
set tenant=

:parseargs
if "%~1" == "" goto endparseargs
if "%~1" == "-h" goto usagehelp
if "%~1" == "-remote" set remote=%~2
if "%~1" == "-proj" set proj=%~2
if "%~1" == "-label" set label=%~2
if "%~1" == "-tenant" set tenant=%~2
if "%~1" == "-releasesShare" set releasesShare=%~2
shift
goto parseargs
:endparseargs

if "%proj%" == "" goto usageerr

if %releasesShare:~-1%==\ set releasesShare=%releasesShare:~0,-1%

echo %time% %SCRIPT%: Parameters are:
echo .      -remote="%remote%"
echo .      -releasesShare="%releasesShare%"
echo .      -proj="%proj%"
echo .      -label="%label%"
echo .      -tenant="%tenant%"

if "%remote%" == "" goto runLocal

rem **************** deploy to target and run with psexec ********************

set psexec=C:\Program Files\SysinternalsSuite\PSExec.exe
if not exist "%psexec%" set psexec=C:\Sysinternals\PSExec.exe
if not exist "%psexec%" set psexec=C:\Tools\Sysinternals\PSExec.exe

set destPath=Deployment\%proj%
set destDir=\\%remote%\%destPath%
set destDirLocal=c:\%destPath%
set remoteCmd=%destDirLocal%\ci\%SCRIPT%.cmd

echo Deploying client remotely: %destDir%

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr

echo robocopy .\ci "%destDir%\ci" %roboopts%
robocopy .\ci "%destDir%\ci" %roboopts%

set STATUS=0

echo %date% %time% - Deploying RN client on %remote%
set REMOTEEXEC="%psexec%" \\%remote% -w "%destDirLocal%" -s "%remoteCmd%" -releasesShare="%releasesShare%" -proj="%proj%" -label="%label%"
if not "%tenant%" == "" set REMOTEEXEC=%REMOTEEXEC% -tenant="%tenant"
echo %REMOTEEXEC%
%REMOTEEXEC%  ^>"psexec-%SCRIPT%.log" >nul 2>&1
if errorlevel 1 (set STATUS=%ERRORLEVEL% & echo remote %SCRIPT% failed)

echo Copy back results

robocopy "%destDir%" . psexec*.log %roboopts%

popd
exit /b %STATUS%

rem **************** running locally... ********************

:runLocal

echo %time% %SCRIPT%: Installing or restoring the RN Server

rem authenticate on the releases share.. assuming it is on SP domain
net use r: "%releasesShare%" /user:sp\svc-build 35Cat247

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr
if exist ".\psexec-%SCRIPT%.log" del /Q ".\psexec-%SCRIPT%.log"

if "%label%"=="latest" (
	echo finding latest version of %releasesShare%\%proj%
	dir /o-n /b "%releasesShare%\%proj%" > rs-versions.txt
	set /p label=<rs-versions.txt
)
if "%label%" == "" goto findlatesterr

set relDir=%releasesShare%\%proj%\%label%
if not exist "%relDir%" goto noreldir

rem All client ci configs use the same location for the server install
rem Stick with this so we re-use the server that the client configs install
set serverProjLocalDir=..\server4client\%proj%
set serverLocalDir=%serverProjLocalDir%\%label%
set platformConfigure=C:\Program Files\EDC\SoftwarePlatform\Bin\PlatformConfigure.exe

echo %time% %SCRIPT%: Using server from "%relDir%"

:installOrRestoreServer
echo %time% %SCRIPT%: Determining whether to install or restore the server
if not exist %serverLocalDir% goto installServer
if not exist %serverLocalDir%\SoftwarePlatform.bak goto installServer

set ServerVerKey=HKLM\Software\EDC\ReadiNow\Server
set ServerVerValue=CurrentVersion
reg query %ServerVerKey% /v %ServerVerValue% 2>nul || (echo no currentVersion regkey & goto installServer)
set currentVersion=
for /f "tokens=2,*" %%a in ('reg query %ServerVerKey% /v %ServerVerValue% ^| findstr %ServerVerValue%') do set currentVersion=%%b
echo %time% %SCRIPT%: Current version of the server is "%currentVersion%". We want "%label%"

if not "%currentVersion%"=="%label%" (echo %time% %SCRIPT%: Wrong version installed & goto installServer)

rem if we get to here we are good to restore

:restoreServer
echo %time% %SCRIPT%: Restoring the server database
cd %serverLocalDir%
call .\RestoreDB.bat SoftwarePlatform.bak
if errorlevel 1 (echo %time% %SCRIPT%: Restore db failed  & exit /b %ERRORLEVEL%)
goto installOrRestoreDone

:installServer
rem echo %time% %SCRIPT%: NOT installing a server... to be completed
rem goto installOrRestoreDone

echo %time% %SCRIPT%: Download and install the server
if exist "%serverProjLocalDir%" rmdir /s /q "%serverProjLocalDir%"
robocopy "%relDir%" %serverLocalDir% /E /NP /NJH /NJS
cd %serverLocalDir%
call .\Install.bat EDC
if errorlevel 1 (echo install.bat failed  & exit /b %ERRORLEVEL%)
goto installOrRestoreDone

:installOrRestoreDone

if "%tenant%" == "" goto skipConfigureTenant
if "%tenant%" == "EDC" goto skipConfigureTenant

echo %time% %SCRIPT%: Configuring the tenant: "%tenant%"
if "%tenant%" == "" (echo Missing tenant specifier & goto configtenanterr)
if not exist "%platformConfigure%" (echo Cannot find %platformConfigure% & goto configtenanterr)

ECHO %date% %time%   - Recreating tenant "%tenant%"
"%platformConfigure%" -dt -tenant "%tenant%" > nul
"%platformConfigure%" -ct -tenant "%tenant%" > nul
ECHO %date% %time%   - Deploying 'ReadiNow Core'...
"%platformConfigure%" -da -tenant "%tenant%" -app "ReadiNow Core" -ver 1.0 > nul
ECHO %date% %time%   - Deploying 'ReadiNow Console'...
"%platformConfigure%" -da -tenant "%tenant%" -app "ReadiNow Console" -ver 1.0 > nul
ECHO %date% %time%   - Deploying 'Core Data'...
"%platformConfigure%" -da -tenant "%tenant%" -app "ReadiNow Core Data" -ver 1.0 > nul
ECHO %date% %time%   - Deploying 'Shared'...
"%platformConfigure%" -da -tenant "%tenant%" -app "Shared Resources" -ver 1.0 > nul
ECHO %date% %time%   - Deploying 'Test Solution'...
"%platformConfigure%" -da -tenant "%tenant%" -app "Test Solution" -ver 1.0 > nul

rem # the following script expects the "tenant" environment variable to exist
sqlcmd -d SoftwarePlatform -i copyAdminUser.sql -I
if errorlevel 1 (echo tenant/user provisioning failed & exit /b %ERRORLEVEL%)

:skipConfigureTenant

cd %SCRIPTDIR%
set PLATFORMCONFIGURE="%programfiles%\edc\softwareplatform\bin\platformconfigure"
ECHO %date% %time% - Import any tenants
for %%f in (..\*_tenant*.db) do %PLATFORMCONFIGURE% -importTenant -package "%%f"
ECHO %date% %time% - Import any apps
for %%f in (..\*_app*.db) do %PLATFORMCONFIGURE% -importApp -package "%%f"

ECHO %date% %time% - Deploy the Performance Test app
%PLATFORMCONFIGURE% -deployApp -tenant EDC -app "Performance Test"

rem seem to need to reset iis to get the app visible in the console
ECHO %date% %time% - Killing off the IIS worker processes
taskkill -f -im w3wp.exe >nul 2>&1

echo %time% %SCRIPT%: All done
popd
net use r: /delete
exit /b 0

rem **************** error handling and usage **********************************

:noreldir
echo %time% %SCRIPT%: Cannot find "%relDir%"
cd %PWD%
exit /b 1

:locationerr
echo "Cannot find ci folder in %CD%"
cd %PWD%
exit /b 1

:findlatesterr
echo %time% %SCRIPT%: Error finding latest for client or server
cd %PWD%
exit /b 1

:usageerr
echo %time% %SCRIPT%: Missing required arguments. Try -h for usage.
cd %PWD%
exit /b 1

:usagehelp
echo .
echo -remote        (default to none, runs locally)
echo -proj          (no default - you need this, e.g. "Trunk-Server")
echo -label         (default to %label%)
echo -tenant        (default to %tenant%)
echo -releasesShare (default to %releasesShare%)
cd %PWD%
exit /b 1
