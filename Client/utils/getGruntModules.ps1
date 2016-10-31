### Call this from your build scripts with the current working directory where the gruntfile is


### We use grunt to do our build.
### It requires a bunch of node_modules to run and there's a lot (35Mbytes, 3500 files!).
### Typically you do a "npm install" here to use npm to get them all.
### But this isn't sensible imho so we are copying if possible, with the fallback to use npm.

$gruntfile = 'gruntfile.js'

Write-Output( 'getGruntModules.ps1 starting: ' + $( Get-Date ) )

if (!(Test-Path -Path $gruntfile))
{
    Write-Error 'missing gruntfile.js... has this been run from the correct working folder??'
    Exit 1
}

$grunt_modules_local = '.\node_modules'
$grunt_modules_source = 'M:\BuildTools\grunt\node_modules'

### the following is to deal with the issue of administrator sessions not seeing user drive mappings
$grunt_modules_source1 = 'C:\Development\BuildTools\npm\Client\client\node_modules'
$grunt_modules_source2 = 'C:\Development\BuildTools\grunt\node_modules'

### doing our test on the most-recent grunt module added so an update will be done if needed
$module_to_check = 'grunt-contrib-less'

if (!(Test-Path (Join-Path $grunt_modules_local $module_to_check)))
{
    $done = $false

    if ((Test-Path $grunt_modules_source) -and (Test-Path (Join-Path $grunt_modules_source $module_to_check)))
    {
        # xcopy them over
        echo ('installing grunt via xcopy from ' + $grunt_modules_source)
        robocopy $grunt_modules_source $grunt_modules_local /E /NP /NDL /NFL /NJH /NJS
    }
    elseif ((Test-Path $grunt_modules_source1) -and (Test-Path (Join-Path $grunt_modules_source1 $module_to_check)))
    {
        # xcopy them over
        echo ('installing grunt via xcopy from ' + $grunt_modules_source1)
        robocopy $grunt_modules_source1 $grunt_modules_local /E /NP /NDL /NFL /NJH /NJS
    }
    elseif ((Test-Path $grunt_modules_source2) -and (Test-Path (Join-Path $grunt_modules_source2 $module_to_check)))
    {
        # xcopy them over
        echo ('installing grunt via xcopy from ' + $grunt_modules_source2)
        robocopy $grunt_modules_source2 $grunt_modules_local /E /NP /NDL /NFL /NJH /NJS
    }
    else
    {
        # get the regular way
        echo 'installing grunt via npm'
        npm install
    }
} else 
{
    Write-Debug 'grunt modules already installed'
}

Write-Output( 'getGruntModules.ps1 complete: ' + $( Get-Date ) )