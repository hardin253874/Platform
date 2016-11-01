#requires -Version 3.0

Param($Version="1.0.0.0")

$ErrorActionPreference = "Stop"
$progressPreference = "silentlyContinue"

#import common.ps1
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Definition) Common.ps1)

function Invoke-ReadiNowSolutionBuild($solutionFolder)
{
    $solutionFile = "$solutionFolder\Platform.sln"
    Write-Host "Build solution '$solutionFile'"
    $msbuild = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
#    & $msbuild $solutionFile /noconsolelogger /v:m /p:Configuration=Debug /nologo "/p:Platform=Mixed Platforms" "/p:Version=$Version;ProjectName=Dev"
    & $msbuild $solutionFile /filelogger /v:m /p:Configuration=Debug /nologo "/p:Platform=Mixed Platforms" "/p:Version=$Version;ProjectName=Dev"
	if ($lastExitCode -gt 0) {
		throw "msbuild failed"
	}
}

Try
{
    $solutionFolder = Split-Path -Parent (Get-ScriptLocation)
	
	Stop-IisWorkerProcess
	
    Invoke-ReadiNowSolutionBuild $solutionFolder

    # Perform installation for "Dev" environment i.e. local developer machine
	if ($env:computername.ToUpper().StartsWith("RNDEV") -Or $env:computername.ToUpper().StartsWith("RNI")) {
		.\Install.ps1 Environment\rnidev.json
	} else {
		.\Install.ps1 Environment\syddev.json
	}
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}
