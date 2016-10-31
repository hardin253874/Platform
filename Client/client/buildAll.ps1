# $config parameter is passed in by the VS Post Build Step. If not specified, it defaults to Release.
param([string]$config = 'Release')

Write-Output( 'buildall.ps1 starting: ' + $( Get-Date ) )
Write-Output( 'Configuration: ' + $config )

$pwd0 = $pwd
$errorCode=0

try 
{
    $gruntfile = 'gruntfile.js'
    $getGruntModules = '..\utils\getGruntModules.ps1'
    $nupkg_root = 'nupkg'
    $nupkg_content =  Join-Path $nupkg_root 'content'
    $dist_dir = 'dist'
    $automation_dir = '..\..\Automation'

    Set-Location (Split-Path $MyInvocation.MyCommand.Path)

	if (!(Test-Path -Path $gruntfile)) 
    {
        throw 'missing gruntfile.js... has this been run in the correct working folder??'
    }

	if (!(Test-Path -Path $getGruntModules)) 
    {
        throw "missing $getGruntModules"
    }

	### ensure a local copy of grunt is available
    
    Invoke-Expression -Command $getGruntModules

	$gruntPath = Join-Path -path (Get-Item env:APPDATA).Value -childpath 'npm\grunt.cmd'

	if ((Test-Path -Path $gruntPath))
	{
		$pinfo = New-Object System.Diagnostics.ProcessStartInfo
		$pinfo.FileName = $gruntPath
		$pinfo.RedirectStandardError = $true
		$pinfo.RedirectStandardOutput = $true
		$pinfo.UseShellExecute = $false
		$pinfo.CreateNoWindow = $true
		$pinfo.WorkingDirectory = (Split-Path $MyInvocation.MyCommand.Path)

		if ($config -eq 'Release')
		{
			$pinfo.Arguments = "-no-color build-release-notests package"
		}
		else
		{
			$pinfo.Arguments = "-no-color build-debug-notests package"
		}

		$p = New-Object System.Diagnostics.Process
		$p.StartInfo = $pinfo

		$global:errors = @()

		Register-ObjectEvent -InputObject $p -EventName "OutputDataReceived" -SourceIdentifier Common.LaunchProcess.Output -Action {
			Write-Host $args[1].Data
		} | Out-Null

		Register-ObjectEvent -InputObject $p -EventName "ErrorDataReceived" -SourceIdentifier Common.LaunchProcess.Error -Action {
			$global:errors += $args[1].Data
		} | Out-Null

		$p.Start() | Out-Null

		$p.BeginOutputReadLine()
		$p.BeginErrorReadLine()

		while (!$p.HasExited)
		{
			Start-Sleep -m 50
		}

		Start-Sleep -m 500

		Unregister-Event -SourceIdentifier Common.LaunchProcess.Output
		Unregister-Event -SourceIdentifier Common.LaunchProcess.Error

		if ( $p.ExitCode -ne 0 )
		{
			$errorCode = $p.ExitCode

			if ( ($global:errors.length-1) -gt 0 )
			{
				foreach ($errorVal in $global:errors) {$Host.UI.WriteErrorLine($errorVal)}
			}
			else
			{
				throw "grunt failure: " + $p.ExitCode
			}
		}
	}
	else
	{
		if (Test-Path grunt-output.txt)
		{
			Remove-Item -Path grunt-output.txt
		}

		### doing this without tests - we do the tests later
	
		if ($config -eq 'Release')
		{
			$p = Start-Process grunt -ArgumentList "-no-color build-release-notests package >grunt-output.txt" -wait -NoNewWindow -PassThru
		}
		else
		{
			$p = Start-Process grunt -ArgumentList "-no-color build-debug-notests package >grunt-output.txt" -wait -NoNewWindow -PassThru
		}

		# Ensure the grunt output gets shown
		type grunt-output.txt
		Remove-Item -Path grunt-output.txt

		if ( $p.ExitCode -ne 0 )
		{
			$errorCode = $p.ExitCode
			throw "grunt failure: " + $p.ExitCode
		}
	}
} 
catch 
{
	#This is not the correct error code but will do for now.
	Write-Error( "buildall.ps1(1,1): error CS1519: " + $($_.Exception.Message + " - check Output Window"))
}

Set-Location $pwd0

Write-Output( 'buildall.ps1 complete: ' + $( Get-Date ) )

exit $errorCode