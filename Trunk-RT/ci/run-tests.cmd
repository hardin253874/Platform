@echo off

rem do test run
rem either do it remotely or locally based on the -remote switch

set PWD=%CD%
set SCRIPT=%~n0
set SCRIPTDIR=%~dp0
set roboopts=/NP /NJH /NJS /R:2 /XD .svn .git
set scriptsDir=.\ci

rem **************** process command line args ********************

rem the following are mandatory
set proj=
set thisProj=

rem the following are optional and these are the defaults
set remote=
set label=latest
set releasesShare=\\SPDEVNAS01.SP.LOCAL\Development\BuildArchives\Releases
set test=post-build-fast

:parseargs
if "%~1" == "" goto endparseargs
if "%~1" == "-h" goto usagehelp
if "%~1" == "-remote" set remote=%~2
if "%~1" == "-thisProj" set thisProj=%~2
if "%~1" == "-proj" set proj=%~2
if "%~1" == "-label" set label=%~2
if "%~1" == "-test" set test=%~2
if "%~1" == "-releasesShare" set releasesShare=%~2
shift
goto parseargs
:endparseargs

if "%proj%" == "" goto usageerr
if "%thisProj%" == "" goto usageerr

if %releasesShare:~-1%==\ set releasesShare=%releasesShare:~0,-1%

echo %time% %SCRIPT%: Parameters are:
echo .      -remote="%remote%"
echo .      -releasesShare="%releasesShare%"
echo .      -proj="%proj%"
echo .      -thisProj="%thisProj%"
echo .      -label="%label%"
echo .      -test="%test%"

if "%remote%" == "" goto runLocal

rem **************** deploy to target and run with psexec ********************

set psexec=C:\Program Files\SysinternalsSuite\PSExec.exe
if not exist "%psexec%" set psexec=C:\Sysinternals\PSExec.exe
if not exist "%psexec%" set psexec=C:\Tools\Sysinternals\PSExec.exe

set destPath=Deployment\%thisProj%
set destDir=\\%remote%\%destPath%
set destDirLocal=c:\%destPath%
set remoteCmd=%destDirLocal%\ci\run-tests.cmd

echo Running tests remotely: %destDir%

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr

echo robocopy .\ci "%destDir%\ci" %roboopts%
robocopy .\ci "%destDir%\ci" %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

set STATUS=0

echo %date% %time% - Building RT on %remote%
set REMOTEEXEC="%psexec%" \\%remote% -w "%destDirLocal%" -s -i -h "%remoteCmd%" -releasesShare="%releasesShare%" -thisProj="%thisProj%" -proj="%proj%" -label="%label%" -test="%test%"
echo %REMOTEEXEC%
%REMOTEEXEC%  ^>"psexec-%SCRIPT%.log" >nul 2>&1
if errorlevel 1 (set STATUS=%ERRORLEVEL% & echo remote run-tests.cmd failed)

echo Copy back results

if exist "psexec-%SCRIPT%.log" del /Q "psexec-%SCRIPT%.log"
robocopy "%destDir%" . psexec*.log junit*.xml %roboopts%

popd
exit /b %STATUS%

rem **************** running locally... actually doing the tests ********************

:runLocal

echo Running tests locally

rem authenticate on the releases share.. assuming it is on SP domain
net use r: "%releasesShare%" /user:sp\svc-build 35Cat247

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr
if exist ".\psexec-%SCRIPT%.log" del /Q ".\psexec-%SCRIPT%.log"

if "%label%"=="latest" (
	echo finding latest version of %releasesShare%\%proj%
	dir /o-n /b "%releasesShare%\%proj%" > rt-versions.txt
	set /p label=<rt-versions.txt
)
set relDir=%releasesShare%\%proj%\%label%
if not exist "%relDir%" goto noreldir

robocopy "%relDir%" . %roboopts%

rem save the previous junit-results.xml as we can use it in ccnet dashboard
rem if exist junit-results.xml copy junit-results.xml junit-results-prev.xml

echo %time% %SCRIPT%: Running RT tests
java -jar rt-0.1.0-standalone.jar test -t %test%
if errorlevel 1 (echo tests failed & exit /b %ERRORLEVEL%)

echo %time% %SCRIPT%: archive the test-runs folder
set DEST=\\spdevnas01.sp.local\Development\Shared\ReadiTest
if exist "%DEST%" robocopy test-runs "%DEST%\test-runs" /E /DCOPY:T /MINAGE:3 /MOVE /R:2 /NFL /NDL

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
echo -thisProj      (no default - you need this, e.g. "Trunk-RT")
echo -proj          (no default - you need this, e.g. "Trunk-RT-build")
echo -label         (default to %label%)
echo -test         (default to %test%)
echo -releasesShare (default to %releasesShare%)
cd %PWD%
exit /b 1
