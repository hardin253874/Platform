REM run the client tests

if exist .\client\deploy\installTests.ps1 (
    echo . | powershell -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -File .\client\deploy\installTests.ps1
)

set TEST_RESULT=0

if exist "%ProgramFiles%\EDC\SoftwarePlatform\Web\Client\tests" (
    call karma start "%ProgramFiles%\EDC\SoftwarePlatform\Web\Client\tests\karma-unit.conf.js"
    if errorlevel 1 (
        set TEST_RESULT=1
    ) 
    if exist "%ProgramFiles%\EDC\SoftwarePlatform\Web\Client\tests\test_out\unit.xml" copy "%ProgramFiles%\EDC\SoftwarePlatform\Web\Client\tests\test_out\unit.xml" .\TestResult_Client.xml
)

echo %TEST_RESULT% > TestResult_Client.txt
exit /b %TEST_RESULT%

