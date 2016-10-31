### Call this from your build scripts with the current working directory where you have ./node_modules

### We use a bunch of node_modules to build and test our client and there's a lot (350Mbytes, 35000 files!).
### Typically you do a "npm install" here to use npm to get them all.
### But this isn't sensible imho so we are copying if possible, with the fallback to use npm.
### This is about pushing our node modules to the backup location locally on the machine

Write-Output( 'backupNodeModules.ps1 starting: ' + $( Get-Date ) )

if (!(Test-Path -Path './gruntfile.js'))
{
    Write-Error 'missing gruntfile.js... has this been run from the correct folder??'
    Exit 1
}

$node_modules_dest = 'C:\rnclientbackup\node_modules'
$node_modules_source = '.\node_modules'

if (Test-Path $node_modules_source)
{
    # xcopy them over
    echo ("backing up node modules via xcopy from $node_modules_source to $node_modules_dest")
    robocopy $node_modules_source $node_modules_dest /E /NP /NDL /NFL
}
else
{
    echo ("missing node_modules $node_modules_source")
}

Write-Output( 'backupNodeModules.ps1 complete: ' + $( Get-Date ) )