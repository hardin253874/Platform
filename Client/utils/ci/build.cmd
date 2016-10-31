@echo off
rem build the client 

set PWD=%CD%

pushd %~dp0
cd ..\..\client

if not exist .\BuildAll.ps1 goto locationerr
powershell -ExecutionPolicy Unrestricted -File .\BuildAll.ps1
if errorlevel 1 exit /b %ERRORLEVEL%

popd
exit /b 0

:locationerr
echo "Cannot find client buildall.ps1 in %CD%"
cd %PWD%
exit /b 1