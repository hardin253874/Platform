#requires -Version 3.0

Param(
    [string]$environmentSettingFile=$(throw "Missing environment settings file")
)

$ErrorActionPreference = "Stop"
$progressPreference = "silentlyContinue"

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

Try
{
    $deploymentSettings = Read-EnvironmentSettingFile $environmentSettingFile
	
	$global:logFile = Resolve-LogFilePath $deploymentSettings.uninstallLogFile
    Create-LogFile
	
	Validate-EnvironmentSettings $deploymentSettings
	
	Log-Message 'Uninstalling SoftwarePlatform...'
	Log-Message "Using environment settings file '$environmentSettingFile'"
	Log-Message "Current Machine: $([Environment]::MachineName)"
	Log-Message "Current User: $([Environment]::UserName)"
	Log-Message "Current Directory: $(Get-CurrentPath)"
	
	Stop-LogViewerProcess
	Stop-IisWorkerProcess

	Uninstall-SoftwarePlatform $deploymentSettings
	Delete-Database $deploymentSettings
	Stop-Iis
	Clear-FileRepositoryDirectory $deploymentSettings
	Clear-InstallationDirectory $deploymentSettings
	Delete-StartMenu $deploymentSettings
	Uninstall-WebServer $deploymentSettings
	Start-Iis
	
    Log-Message 'SoftwarePlatform successfully uninstalled.'
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}
