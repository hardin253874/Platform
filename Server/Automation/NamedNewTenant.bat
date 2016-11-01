@ECHO OFF
SETLOCAL
Rem IF NOT .%1==. GOTO ELSE
set /p NEWTENANT=Type Tenant ID (no spaces):

Echo %NEWTENANT%
Echo If the Tenant name above is not correct press Ctrl C to end job. Otherwise
Pause

REM Activate the %NEWTENANT% tenant
ECHO %date% %time%   - Activating the %NEWTENANT% Tenant...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -ct -tenant %NEWTENANT% > nul

REM Deploy the 'ReadiNow Core' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'ReadiNow Core'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "ReadiNow Core" -ver 1.0 > nul

REM Deploy the 'ReadiNow Console' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'ReadiNow Console'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "ReadiNow Console" -ver 1.0 > nul

REM Deploy the 'Core Data' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'Core Data'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "ReadiNow Core Data" -ver 1.0 > nul

REM Deploy the 'Shared' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'Shared'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "Shared Resources" -ver 1.0 > nul

REM Deploy the 'Business Continuity Management' application to the %NEWTENANT% tenant.
Rem ECHO %date% %time%   - Deploying 'Business Continuity Management'...
Rem "C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "Business Continuity Management" -ver 1.0 > nul

REM Deploy the 'Test Solution' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'Test Solution'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app "Test Solution" -ver 1.0 > nul