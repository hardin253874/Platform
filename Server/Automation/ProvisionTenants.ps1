param (
    [string]$tenants = "T001"
)

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

$ErrorActionPreference = "Stop"

function restartAppPool($web) {
	Import-Module WebAdministration
	Restart-WebAppPool (Get-Item "IIS:\Sites\Default Web Site\$web").ApplicationPool
}

try
{
    Write-Output "Provisioning $tenants"
	
	$platformConfigureProcess = Start-PlatformConfigure $settings
	
	Try
	{
		$tenants.Split(",") | ForEach { 
			Write-Host "Provisioning tenant '$_'"
			Provision-Tenant $platformConfigureProcess $_
			
			Write-Host "Deploying 'Test Solution' to tenant '$_'"
			Deploy-App $platformConfigureProcess $_ "Test Solution"
			Grant-CanModifyApplication $platformConfigureProcess $_ "Test Solution"
			
			Write-Host "Deploying 'Foster University' to tenant '$_'"
			Deploy-App $platformConfigureProcess $_ "Foster University"
			Grant-CanModifyApplication $platformConfigureProcess $_ "Foster University"
			
			Write-Host "Deploying 'Foster University DATA' to tenant '$_'"
			Deploy-App $platformConfigureProcess $_ "Foster University DATA"
			Grant-CanModifyApplication $platformConfigureProcess $_ "Foster University DATA"
		}	
	}
	Finally
	{
		End-PlatformConfigure $platformConfigureProcess
	}
		
	restartAppPool 'spapi'
}
catch
{
    $ErrorActionPreference = "Continue"
    write-error $_
    exit 1
}