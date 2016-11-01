@echo off

rem do a build and deploy
rem either do it remotely or locally based on the -remote switch

set PWD=%CD%
set SCRIPT=%~n0
set SCRIPTDIR=%~dp0
set roboopts=/NP /NJH /NJS /R:2 /XD .svn .git
set scriptsDir=.\ci
set psexecOutFile=psexec-%SCRIPT%.log

rem **************** process command line args ********************

rem the following are mandatory
set proj=

rem the following are optional and these are the defaults
set remote=
set label=latest
set releasesShare=\\SPDEVNAS01.SP.LOCAL\Development\BuildArchives\Releases

:parseargs
if "%~1" == "" goto endparseargs
if "%~1" == "-h" goto usagehelp
if "%~1" == "-remote" set remote=%~2
if "%~1" == "-proj" set proj=%~2
if "%~1" == "-label" set label=%~2
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

if "%remote%" == "" goto runLocal

rem **************** deploy to target and run with psexec ********************

set psexec=C:\Program Files\SysinternalsSuite\PSExec.exe
if not exist "%psexec%" set psexec=C:\Sysinternals\PSExec.exe
if not exist "%psexec%" set psexec=C:\Tools\Sysinternals\PSExec.exe

set destPath=Deployment\%proj%
set destDir=\\%remote%\%destPath%
set destDirLocal=c:\%destPath%
set remoteCmd=%destDirLocal%\ci\build.cmd

echo Running build remotely: %destDir%

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr

echo %date% %time% - Copy ...a bunch of files... to "%destDir%" %roboopts%
robocopy .\ci "%destDir%\ci" %roboopts% /purge
robocopy .\src "%destDir%\src" /s %roboopts% /purge
robocopy .\resources "%destDir%\resources" /s %roboopts% /purge
robocopy . "%destDir%" *.clj *.edn %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

set STATUS=0

echo %date% %time% - Building RT on %remote%
set REMOTEEXEC="%psexec%" \\%remote% -i -w "%destDirLocal%" -s "%remoteCmd%" -releasesShare="%releasesShare%" -proj="%proj%" -label="%label%"
echo %REMOTEEXEC%
%REMOTEEXEC%  ^>"%psexecOutFile%" >nul 2>&1
if errorlevel 1 (set STATUS=%ERRORLEVEL% & echo remote build.cmd failed)

echo %date% %time% - Copy back results

if exist "%psexecOutFile%" del /Q "%psexecOutFile%"
robocopy "%destDir%" . psexec*.log %roboopts%

rem NOT copying these back to the CI working dir as there are lots of files (thus slow)
rem and I don't recall why we want them anyway
rem robocopy "%destDir%\resources" .\resources /s %roboopts%
rem robocopy "%destDir%\target" .\target /s %roboopts%

popd
echo %date% %time% - All done
exit /b %STATUS%

rem **************** running locally...  ********************

:runLocal

echo Running build locally

rem authenticate on the releases share.. assuming it is on SP domain
net use r: "%releasesShare%" /user:sp\svc-build 35Cat247

pushd %SCRIPTDIR%
cd ..
if not exist %scriptsDir% goto locationerr
if exist "%psexecOutFile%" del /Q "%psexecOutFile%"

echo if exist "%psexecOutFile%" del /Q "%psexecOutFile%"
echo pwd=%CD%

if "%label%"=="latest" (
	echo finding latest version of %releasesShare%\%proj%
	dir /o-n /b "%releasesShare%\%proj%" > rt-versions.txt
	set /p label=<rt-versions.txt
)
set relDir=%releasesShare%\%proj%\%label%

echo %time% %SCRIPT%: Building and deploying to %relDir%
rem the following assumes leiningen installed as svc-dev-ci user
rem - we need to set the userprofile as when running from psexec the env isn't set
set userprofile=c:\users\svc-dev-ci
set LEINCMD=%userprofile%\.lein\bin\lein

echo Calling LEININGEN

echo %time% %SCRIPT%: lein clean
echo . | "%LEINCMD%" clean

echo %time% %SCRIPT%: lein cljsbuild
echo . | "%LEINCMD%" cljsbuild clean
echo . | "%LEINCMD%" cljsbuild once dev

rem skip doc gen for now... speed up the builds
rem echo %time% %SCRIPT%: lein doc
rem echo . | "%LEINCMD%" doc

echo %time% %SCRIPT%: lein uberjar
echo . | "%LEINCMD%" uberjar >lein-uberjar.out 2>&1

rem this call to lein always returns 0 even if it fails, so search output

echo %time% %SCRIPT%: lein uberjar output
type lein-uberjar.out

find /i /c "exception" lein-uberjar.out
if errorlevel 1 goto leinOk
echo lein failed
exit /b 1

:leinOk
echo %time% %SCRIPT%: lein DONE

if not exist ".\target\*.jar" (echo ReadiTest jar file not found & exit /b 1)

echo robocopy .\target "%relDir%" *.jar %roboopts%
robocopy .\target "%relDir%" *.jar %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

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
echo -proj          (no default - you need this, e.g. "Trunk-RT")
echo -label         (default to %label%)
echo -releasesShare (default to %releasesShare%)
cd %PWD%
exit /b 1
