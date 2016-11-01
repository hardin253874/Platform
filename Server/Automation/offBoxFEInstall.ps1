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
	
	Log-Message 'Upgrading SoftwarePlatform...'
	Log-Message "Using environment settings file '$environmentSettingFile'"
	Log-Message "Argument redistributableFile = '$redistributableFile'"
	
	Uninstall-SoftwarePlatform $deploymentSettings
	Stop-Iis
	Clear-InstallationDirectory $deploymentSettings
	Delete-StartMenu $deploymentSettings
	Uninstall-WebServer $deploymentSettings
	Start-Iis
	
	# Decompress installation files
	Unzip-SoftwarePlatform $deploymentSettings $redistributableFile
	Create-StartMenu $deploymentSettings
	
	# Move 'SoftwarePlatform.pristine' to 'SoftwarePlatform.config'
	Install-SoftwarePlatformConfig $deploymentSettings

	Install-ClientIfExists $deploymentSettings $True

	Configure-Folder $deploymentSettings 'OutgoingEmail' $deploymentSettings.outgoingEmailFolder.security
	Configure-Folder $deploymentSettings 'Log' $deploymentSettings.logFolder.security
	Configure-Folder $deploymentSettings 'Redis' $deploymentSettings.redis.folderSecurity
	Configure-Folder $deploymentSettings $deploymentSettings.uploadFolder.path $deploymentSettings.uploadFolder.security
	Configure-Folder $deploymentSettings $deploymentSettings.errorsFolder.path $deploymentSettings.errorsFolder.security
	Configure-DatabaseFolder $deploymentSettings
	
	Install-Redis $deploymentSettings
			
	Process-ConfigurationFile $deploymentSettings $environmentSettingFile
	
	Register-PerformanceCounters $deploymentSettings
	
	Install-Scheduler $deploymentSettings
		
	$platformConfigureProcess = Start-PlatformConfigure $deploymentSettings
	
	Try
	{
		# Install the bootstrap. This will early out if already exists.
		Install-Bootstrap $platformConfigureProcess $deploymentSettings
		
		Upgrade-Bootstrap $platformConfigureProcess $deploymentSettings
		
		# Import Shared, Power Tools, Test Solution, Foster University and Foster University Data into the application library
		Install-BuiltInReadiNowApplications $platformConfigureProcess $deploymentSettings
		
		# Upgrade Core, Console and Core Data for each tenant on the system
		Upgrade-TenantCoreApplications $platformConfigureProcess $deploymentSettings
		
		Switch-IntegrationTestModeOn $platformConfigureProcess		
		Grant-CanModifyApplications $platformConfigureProcess $deploymentSettings
        Install-FeatureSwitches $platformConfigureProcess $deploymentSettings
		
		Create-TenantRestorePoints $platformConfigureProcess $deploymentSettings
	}
	Finally
	{
		End-PlatformConfigure $platformConfigureProcess
	}
	
   	
	Configure-WebServer $deploymentSettings
	
    Log-Message 'SoftwarePlatform successfully upgraded.'
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}