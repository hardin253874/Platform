@echo off

rem deploy the specified RN client
rem either do it remotely or locally based on the -remote switch

set PWD=%CD%
set SCRIPT=%~n0
set SCRIPTDIR=%~dp0
set roboopts=/NP /NJH /NJS /R:2 /XD .svn .git
set scriptsDir=.\ci

rem **************** process command line args ********************

rem the following are mandatory
set proj=

rem the following are optional and these are the defaults
set remote=
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

echo %time% %SCRIPT%: Installing the client

rem authenticate on the releases share.. assuming it is on SP domain
net use r: "%releasesShare%" /user:sp\svc-build 35Cat247

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr
if exist ".\psexec-%SCRIPT%.log" del /Q ".\psexec-%SCRIPT%.log"

if "%label%"=="latest" (
	echo finding latest version of %releasesShare%\%proj%
	dir /o-n /b "%releasesShare%\%proj%" > rc-versions.txt
	set /p label=<rc-versions.txt
)
if "%label%" == "" goto findlatesterr

set relDir=%releasesShare%\%proj%\%label%
if not exist "%relDir%" goto noreldir

robocopy "%relDir%\client" .\client /E %roboopts%

echo Install the client to the usual location
echo . | powershell -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -File .\client\deploy\install.ps1
set clientInstallArgs=-dest .\dist
if not "%tenant%" == "" set clientInstallArgs=%clientInstallArgs% -web "sp_%tenant:-=_%"
echo Install the client and tests locally at .\client\deploy, install args: "%clientInstallArgs%"
echo . | powershell -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -File .\client\deploy\install.ps1 %clientInstallArgs%
echo . | powershell -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -File .\client\deploy\installDbg.ps1 %clientInstallArgs%
echo . | powershell -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -File .\client\deploy\installTests.ps1 -dest .\tests

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

:usageerr
echo %time% %SCRIPT%: Missing required arguments. Try -h for usage.
cd %PWD%
exit /b 1

:usagehelp
echo .
echo -remote        (default to none, runs locally)
echo -proj          (no default - you need this, e.g. "Trunk-Client")
echo -label         (default to %label%)
echo -tenant        (default to %tenant%)
echo -releasesShare (default to %releasesShare%)
cd %PWD%
exit /b 1
