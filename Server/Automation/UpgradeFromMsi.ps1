#requires -Version 3.0

Param(
    [string]$environmentSettingFile=$(throw "Missing environment settings file"),
	[string]$redistributableFile=$Null
)

$ErrorActionPreference = "Stop"
$progressPreference = "silentlyContinue"

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

Try
{
	
    $deploymentSettings = Read-EnvironmentSettingFile $environmentSettingFile
		
	$global:logFile = Resolve-LogFilePath $deploymentSettings.upgradeLogFile
	
	Log-Message "Preparing machine for Msi->Zip transition..."
	.\DevPrep.ps1
	
	Upgrade-QuartzJobs $deploymentSettings
	
	.\Upgrade.ps1 $environmentSettingFile $redistributableFile
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}