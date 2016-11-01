@ECHO OFF
SETLOCAL

set /p NEWTENANT=Type the Tenant ID that you would like to Deploy Shared Business to:

Echo %NEWTENANT%
Echo If the Tenant name above is not correct press Ctrl C to end job. Otherwise
Pause

REM Deploy the 'Shared Business' application to the %NEWTENANT% tenant.
REM Requires 'Shared Business 1.1.db' to be in C:\Program Files\ReadiNow\SoftwarePlatform\Applications !!!!!

CLS
ECHO Requires 'Shared Business 1.1.db' to be in 'C:\Program Files\ReadiNow\SoftwarePlatform\Applications' if not already, please copy it there now. Once done
Pause

ECHO %date% %time%   - Importing 'Shared Business'...
REM "C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -importApp -package "C:\Program Files\ReadiNow\SoftwarePlatform\Applications\Shared Business 1.1.db" > nul
ECHO %date% %time%   - Deploying 'Shared Business'...
REM "C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "Shared Business"  > nul
