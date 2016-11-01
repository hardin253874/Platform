pushd %~dp0\..
@set leincmd=%userprofile%\.lein\bin\lein.bat

call "%leincmd%" clean
call "%leincmd%" cljsbuild clean
call "%leincmd%" cljsbuild once dev

rem this call to lein always returns 0 even if it fails, so redirect output to file to verify error condition textually
call "%leincmd%" uberjar >lein-uberjar.out
type lein-uberjar.out
find /i /c "exception" lein-uberjar.out
if errorlevel 1 goto leinOk
echo lein failed
exit /b 1

:leinOk
if not exist ".\target\*.jar" (echo ReadiTest jar file not found & exit /b 1)

popd
