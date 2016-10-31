echo off

rem this is a quick and dirty. it would be nice to just do this in a grunt task - to have it
rem collect all the test output files, concatenate them, and then render them to html

pushd %~dp0
cd

set TESTSDIR=%~dp0\test_out
set JUNITXSL=%~dp0\junit.xsl

if not exist "%TESTSDIR%" (
	echo cannot find "%TESTSDIR%" folder... have you run the tests?
	popd
	exit /b
)

powershell -file .\renderUnitTestResults.ps1 -xml "%TESTSDIR%\unit.xml" -xsl "%JUNITXSL%"
powershell -file .\renderUnitTestResults.ps1 -xml "%TESTSDIR%\integration.xml" -xsl "%JUNITXSL%"
powershell -file .\renderUnitTestResults.ps1 -xml "%TESTSDIR%\e2e.xml" -xsl "%JUNITXSL%"

if exist "%TESTSDIR%\unit.xml.html" start "%TESTSDIR%\unit.xml.html"
if exist "%TESTSDIR%\integration.xml.html" start "%TESTSDIR%\integration.xml.html"
if exist "%TESTSDIR%\e2e.xml.html" start "%TESTSDIR%\e2e.xml.html"

popd
