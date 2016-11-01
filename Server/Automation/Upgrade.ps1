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
	Create-LogFile
	
	Validate-EnvironmentSettings $deploymentSettings
	
	Log-Message 'Upgrading SoftwarePlatform...'
	Log-Message "Using environment settings file '$environmentSettingFile'"
	Log-Message "Argument redistributableFile = '$redistributableFile'"
	Log-Message "Current Machine: $([Environment]::MachineName)"
	Log-Message "Current User: $([Environment]::UserName)"
	Log-Message "Current Directory: $(Get-CurrentPath)"
	
	Stop-SchedulerService $deploymentSettings
	Stop-RedisService $deploymentSettings
	
	Uninstall-SoftwarePlatform $deploymentSettings
	Stop-Iis
	Clear-InstallationDirectory-ForUpgrade $deploymentSettings
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
	
	Install-Database $deploymentSettings
	
	Repair-System-Applications $deploymentSettings
	
	Process-ConfigurationFile $deploymentSettings $environmentSettingFile

	Register-PerformanceCounters $deploymentSettings
	
    $schedulerName = Install-Scheduler $deploymentSettings
	
	Process-SymbolicLinks $deploymentSettings
	
	$platformConfigureProcess = Start-PlatformConfigure $deploymentSettings
	
	Try
	{
		# Install core apps and upgrade global tenant
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
	
    Create-DatabaseBackup $deploymentSettings $backupDatabase
    Install-TestControllers $deploymentSettings
	
	Configure-WebServer $deploymentSettings
	
    Start-Scheduler $schedulerName

    Log-Message 'SoftwarePlatform successfully upgraded.'
	Log-Message $global:logFile
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}
