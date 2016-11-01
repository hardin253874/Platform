rem build the RT UI
echo off

set PWD=%CD%

pushd %~dp0
if not exist .\resources goto locationerr
if exist resources\public\js\app.js del resources\public\js\app.js
echo lein cljsbuild once dev
lein cljsbuild once dev
popd
exit /b 0

:locationerr
echo "Cannot find resources folder"
cd %PWD%
exit /b 1
