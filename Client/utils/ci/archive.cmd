rem echo off
rem archive the client build

set PWD=%CD%

set clientProj=%~1
set clientLabel=%~2
set releasesShare=%~3

if "%clientProj%" == "" goto usageerr
if "%clientLabel%" == "" goto usageerr
if "%releasesShare%" == "" set releasesShare=\\SPDEVNAS01.SP.LOCAL\Development\BuildArchives\Releases

if %releasesShare:~-1%==\ set releasesShare=%releasesShare:~0,-1%

pushd %~dp0
cd ..\..\client
if not exist .\deploy goto locationerr
robocopy .\deploy "%releasesShare%\%clientProj%\%ClientLabel%\client\deploy" /E /NP /NJH /NJS /R:2
if errorlevel 8 (echo robocopy failed & exit /b %ERRORLEVEL%)

popd
exit /b 0

:usageerr
echo "Archive.cmd requires arguments clientProj clientLabel and optional releasesShare"
cd %PWD%
exit /b 1

:locationerr
echo "Cannot find client deploy folder in %CD%"
cd %PWD%
exit /b 1