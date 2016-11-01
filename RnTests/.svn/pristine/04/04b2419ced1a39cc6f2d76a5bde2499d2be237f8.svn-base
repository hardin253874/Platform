echo off

set remote=
set test=
set sessions=2
set server=
set rampup=--ramp-up 60
set pace=--pace 60
set testpkg=--test-pkg "RnTests.zip"


:parseargs
if "%~1" == "" goto endparseargs
if "%~1" == "--remote" set remote=%~2
if "%~1" == "--test" set test=--test "%~2"
if "%~1" == "--sessions" set sessions=%~2
if "%~1" == "--server" set server=--server %~2
if "%~1" == "--ramp-up" set rampup=--ramp-up %~2
if "%~1" == "--pace" set pace=--pace %~2
if "%~1" == "--test-pkg" set testpkg=--test-pkg %~2
shift
goto parseargs
:endparseargs

if "%remote%"=="" (echo missing -remote arg & exit /b 1)

echo Running %sessions% x %test% on %remote% reporting to %server% %rampup% %pace%

set PWD=%CD%
set SCRIPTDIR=%~dp0
set roboopts=/NP /NJH /NJS /R:2 /XD .svn .git
set psexec=C:\Program Files\SysinternalsSuite\PSExec.exe
if not exist "%psexec%" set psexec=C:\Sysinternals\PSExec.exe
if not exist "%psexec%" set psexec=C:\Tools\Sysinternals\PSExec.exe

rem WARNING - actual working folder needs to be named RnTests
set destPath=\RnTests\%COMPUTERNAME%\RnTests
set destDir=\\%remote%\c$\%destPath%
set destDirLocal=c:%destPath%

pushd %SCRIPTDIR%

robocopy .\test-db "%destDir%\test-db" /E %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)
robocopy .\data "%destDir%\data" /E %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)
robocopy . "%destDir%" *.jar *.edn RnTests.zip %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

set REMOTEEXEC="%psexec%" -accepteula \\%remote% -w "%destDirLocal%" -s "cmd" /c java -jar rt-standalone.jar test --sessions=%sessions% %test% %server% %rampup% %pace% %testpkg%
echo %REMOTEEXEC%
%REMOTEEXEC%
if errorlevel 1 (echo psexec failed & exit /b %ERRORLEVEL%)

robocopy "%destDir%" ".\remoteResults\%remote%" psexec*.log junit*.xml %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)
robocopy "%destDir%\test-runs" ".\remoteResults\%remote%\test-runs" /E %roboopts%
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

popd
exit /b 0
