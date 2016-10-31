### we assume this script is run in sibling folder to the client

function UpdateIisApp ($parentIisApp, $childName, $folder) {
    $iisAppPath = (Join-Path $parentIisApp $childName)
    if (Test-Path $iisAppPath)
    {
        Remove-Item $iisAppPath -Recurse
    }
    New-Item $iisAppPath -type Application -physicalPath $folder -applicationPool (Get-Item $parentIisApp).ApplicationPool

    #Write-Output "Set $iisAppPath to $folder"
}

$projDir = Split-Path (Split-Path $MyInvocation.MyCommand.Path) -Parent
$clientDistDir = Join-Path $projDir 'client\dist'
$clientTestDir = Join-Path $projDir 'client\tests'
$clientErrorsDir = Join-Path $projDir 'client\dist\customerrors'
$iisApp = 'IIS:\Sites\Default Web Site'
$iisAppLocation = 'Default Web Site/sp'

if (!(Test-Path $clientDistDir)) {
    Write-Output "Failed to find '$clientDistDir'"
    Exit 1
}

Import-Module WebAdministration

UpdateIisApp $iisApp 'sp' $clientDistDir
UpdateIisApp $iisApp 'sperrors' $clientErrorsDir
UpdateIisApp $iisApp 'spdev' (Join-Path $projDir 'client')

# Set Requires SSL on sp web
Set-WebConfiguration -Location $iisAppLocation -Filter 'system.webServer/security/access' -Value 'Ssl, None'




