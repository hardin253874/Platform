#requires -Version 3.0

Param(
    [string]$environmentSettingFile=$(throw "Missing environment settings file"),
	[string]$redistributableFile=$Null,
	[string]$restoreDatabase=$False,
	[string]$backupDatabase=$True,
	[string]$deployClient=$True
)

$ErrorActionPreference = "Stop"
$progressPreference = "silentlyContinue"

# Convert the argument values into booleans. Invoking powershell scripts with the -File
# switch only allows string arguments to be passed.
$restoreDatabase = [System.Convert]::ToBoolean($restoreDatabase)
$backupDatabase = [System.Convert]::ToBoolean($backupDatabase)
$deployClient = [System.Convert]::ToBoolean($deployClient)

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

Try
{
    $deploymentSettings = Read-EnvironmentSettingFile $environmentSettingFile
	
    $global:logFile = Resolve-LogFilePath $deploymentSettings.installLogFile
    Create-LogFile
	
	Validate-EnvironmentSettings $deploymentSettings

	Log-Message 'Installing SoftwarePlatform...'
	Log-Message "Using environment settings file '$environmentSettingFile'"
	Log-Message "Argument redistributableFile = '$redistributableFile'"
	Log-Message "Argument restoreDatabase = '$restoreDatabase'"
	Log-Message "Argument backupDatabase = '$backupDatabase'"
	Log-Message "Argument deployClient = '$deployClient'"
	Log-Message "Current Machine: $([Environment]::MachineName)"
	Log-Message "Current User: $([Environment]::UserName)"
	Log-Message "Current Directory: $(Get-CurrentPath)"

	# Cleanup
	Stop-LogViewerProcess
	Uninstall-SoftwarePlatform $deploymentSettings
	Delete-Database $deploymentSettings
	Stop-Iis
	Clear-FileRepositoryDirectory $deploymentSettings
	Clear-InstallationDirectory $deploymentSettings
	Delete-StartMenu $deploymentSettings
	Uninstall-WebServer $deploymentSettings
	Start-Iis
	
	# Decompress installation files
	Unzip-SoftwarePlatform $deploymentSettings $redistributableFile
	Create-StartMenu $deploymentSettings
	
	# Move 'SoftwarePlatform.pristine' to 'SoftwarePlatform.config'
	Install-SoftwarePlatformConfig $deploymentSettings

	Install-ClientIfExists $deploymentSettings $deployClient

	Configure-Folder $deploymentSettings 'OutgoingEmail' $deploymentSettings.outgoingEmailFolder.security
	Configure-Folder $deploymentSettings 'Log' $deploymentSettings.logFolder.security
	Configure-Folder $deploymentSettings 'Redis' $deploymentSettings.redis.folderSecurity
	Configure-Folder $deploymentSettings $deploymentSettings.uploadFolder.path $deploymentSettings.uploadFolder.security
	Configure-Folder $deploymentSettings $deploymentSettings.errorsFolder.path $deploymentSettings.errorsFolder.security
	Configure-DatabaseFolder $deploymentSettings
	
	Restore-DatabaseBackup $deploymentSettings $restoreDatabase
	
	Install-Redis $deploymentSettings
	
	Install-Database $deploymentSettings
	
	Process-ConfigurationFile $deploymentSettings $environmentSettingFile

	Register-PerformanceCounters $deploymentSettings

	$schedulerName = Install-Scheduler $deploymentSettings
	
	Process-SymbolicLinks $deploymentSettings
	
	$platformConfigureProcess = Start-PlatformConfigure $deploymentSettings
	
	Try
	{
		# Install core apps and global tenant
		Install-Bootstrap $platformConfigureProcess $deploymentSettings
		
		Install-BuiltInReadiNowApplications $platformConfigureProcess $deploymentSettings
		
		Create-DefaultTenant $platformConfigureProcess $deploymentSettings
		Switch-IntegrationTestModeOn $platformConfigureProcess		
		Grant-CanModifyApplications $platformConfigureProcess $deploymentSettings
        Install-FeatureSwitches $platformConfigureProcess $deploymentSettings
		Provision-AdditionalTenants $platformConfigureProcess $deploymentSettings
		
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

    Log-Message 'SoftwarePlatform successfully installed.'	
	Log-Message $global:logFile
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}
