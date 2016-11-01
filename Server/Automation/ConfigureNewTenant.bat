@ECHO OFF
SETLOCAL
IF NOT .%1==. GOTO ELSE
GOTO ENDIF
:ELSE 
Set NEWTENANT=%~1
GOTO TenantSet

:ENDIF

set /p NEWTENANT=Type Tenant ID (no spaces):

Echo %NEWTENANT%
Echo If the Tenant name above is not correct press Ctrl C to end job. Otherwise
Pause

:TenantSet

REM Activate the %NEWTENANT% tenant
ECHO %date% %time%   - Activating the %NEWTENANT% Tenant...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -ct -tenant %NEWTENANT% > nul

REM Deploy the 'SoftwarePlatform Core' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'SoftwarePlatform Core'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app {7062aade-2e72-4a71-a7fa-a412d20d6f01}  > nul

REM Deploy the 'SoftwarePlatform Console' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'SoftwarePlatform Console'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app {34ff4d95-70c6-4ae8-8f6f-38d88546d4c4}  > nul

REM Deploy the 'Core Data' application to the %NEWTENANT% tenant.
ECHO %date% %time%   - Deploying 'Core Data'...
"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -da -tenant %NEWTENANT% -app {abf12077-6fa5-43da-b608-b8b7514d07bb}  > nul

REM Install Applications
ECHO %date% %time% - Installing Applications...
IF EXIST DeployApp.bat (
	CALL DeployApp.bat "Shared" "%NewTenant%"
	CALL DeployApp.bat "Test Solution" "%NewTenant%"	
	"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -grantModifyApp -tenant %NEWTENANT% -app "Test Solution"  > nul
	
	CALL DeployApp.bat "Foster University" "%NewTenant%"
	"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -grantModifyApp -tenant %NEWTENANT% -app "Foster University"  > nul
	
	CALL DeployApp.bat "Foster University DATA" "%NewTenant%"
	"C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe" -grantModifyApp -tenant %NEWTENANT% -app "Foster University DATA"  > nul
)

:END

