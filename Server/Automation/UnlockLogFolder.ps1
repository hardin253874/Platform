#requires -Version 3.0

$ErrorActionPreference = "Stop"

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

Try
{
	$serverReg = Get-ItemProperty "hklm:\SOFTWARE\EDC\ReadiNow\Server"
	$logFileFolder = $serverReg.LogFileFolder

	Close-RemoteFolderHandles $logFileFolder
	Stop-LocalProcessesUsingFolder $logFileFolder
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}
