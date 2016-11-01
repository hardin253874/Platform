REM @echo off
setlocal
set PC="C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe"

if '%1'=='/?' goto Help
if '%1'=='' goto Help
if '%2'=='' goto Help

rem ------------------- Get params and remove quotes -------------------
set APPNAME=%1
set APPNAME=%APPNAME:"=%
set TENANT=%2
set TENANT=%TENANT:"=%

IF NOT .%3==. GOTO ELSE
GOTO ENDIF

:ELSE
set UPGRADE=%3
set UPGRADE=%UPGRADE:'=%
:ENDIF

REM ECHO %UPGRADE%
REM Pause
rem echo APPNAME=%APPNAME%
rem echo TENANT=%TENANT%

rem ------------------- Find app -------------------
if exist "..\ApplicationCache\%APPNAME%.xml" (
	set APPPATH="..\ApplicationCache\%APPNAME%.xml"
) else if exist ".\Apps\%APPNAME%.xml" (
	set APPPATH=".\Apps\%APPNAME%.xml"
) else if exist ".\%APPNAME%.xml" (
	set APPPATH=".\%APPNAME%.xml"
) else (
	echo Could not find "..\ApplicationCache\%APPNAME%.xml"
	echo Could not find ".\Apps\%APPNAME%.xml"
	echo Could not find ".\%APPNAME%.xml"
	goto End
)
rem echo APPPATH=%APPPATH%


rem ------------------- Import -------------------
echo %date% %time% - Importing '%APPNAME%'...
%PC% -importApp -package %APPPATH% > nul

IF '%UPGRADE%'=='"importonly"' GOTO END

rem ------------------- Deploy -------------------
echo %date% %time% - Deploying '%APPNAME%' to '%TENANT%'...
%PC% -da -tenant "%TENANT%" -app "%APPNAME%" > nul
goto End


rem ------------------- Help Text -------------------
:Help
@echo off
echo Usage: DeployApp.exe AppName TenantName [importonly]
echo.
echo This will install into the app library and then deploy to the tenant.
echo The file name must be the same as the app name, with a .xml extension.
echo.

:End