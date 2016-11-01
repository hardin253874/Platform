#requires -Version 3.0

$ErrorActionPreference = "Stop"

function Log-Message($message)
{
	Write-Host $message
}

function Log-Warning($message)
{
	Write-Warning $message
}

function Get-SoftwarePlatformApplication
{
	return Get-WmiObject -Class Win32_Product -Filter "Name = 'SoftwarePlatform.com'"
}

function Uninstall-SoftwarePlatform
{
	Log-Message "Searching for installed SoftwarePlatform Msi..."

    $app = Get-SoftwarePlatformApplication
	
    if( $app -ne $null)
	{
		$name = $app.Name
		
		Log-Message "Found existing '$name' Msi with version '$($app.Version)'"
		
		Log-Message "Uninstalling '$name'..." $True

		$ret = $app.Uninstall()
	}
	else
	{
		Log-Message "No existing SoftwarePlatform Msi found."
	}
	
	Log-Message ""
}

function Remove-Assemblies($basePath, $wildcard)
{
	$files = [System.IO.Directory]::EnumerateFiles( $basePath, $wildcard, "AllDirectories" )
	
	$assembly = [System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
	$publish = New-Object System.EnterpriseServices.Internal.Publish
		
	foreach ($file in $files)
	{
		Try
		{
			$dll = Split-Path $file -Leaf
			$parent = Split-Path (Split-Path $file -Parent) -Parent
			
			Log-Message "Removing assembly '$dll'..."
			
			$publish.GacRemove($file)
			
			$count = @( Get-ChildItem $parent ).Count
			
			if ( $count -eq 0 )
			{	
				Log-Message "Removing folder '$parent'..."
				
				Remove-Item $parent
			}
		}
		Catch
		{
			Log-Warning "Failed to remove assembly '$file'"
		}
	}
}

function Remove-Directories($basePath, $wildcard)
{
	$directories = [System.IO.Directory]::EnumerateDirectories( $basePath, $wildcard, "TopDirectoryOnly" )
		
	foreach ($directory in $directories)
	{
		Try
		{
			$count = @( [System.IO.Directory]::EnumerateFiles( $directory, "*", "AllDirectories" ) ).Count
			
			if ( $count -eq 0 )
			{	
				Log-Message "Removing folder '$directory'..."
				
				Remove-Item -Recurse $directory
			}
		}
		Catch
		{
			Log-Warning "Failed to remove directory '$file'"
		}
	}
}

function Clean-Gac()
{
	Log-Message "Cleaning GAC..."

	$basePath = "C:\Windows\Microsoft.NET\assembly\GAC_64"
	
	Remove-Assemblies $basePath "EDC*.dll"
	Remove-Directories $basePath "EDC*"
	
	Remove-Assemblies $basePath "ReadiNow*.dll"
	Remove-Directories $basePath "ReadiNow*"

    $basePath = "C:\Windows\Microsoft.NET\assembly\GAC_MSIL"
	
	Remove-Assemblies $basePath "Microsoft.Data.Tools.Utilities.dll"
	Remove-Directories $basePath "Microsoft.Data.Tools.Utilities"
	
	Remove-Assemblies $basePath "Microsoft.SqlServer.Dac.Extensions.dll"
	Remove-Directories $basePath "Microsoft.SqlServer.Dac.Extensions"
	
	Remove-Assemblies $basePath "Microsoft.SqlServer.Dac.dll"
	Remove-Directories $basePath "Microsoft.SqlServer.Dac"
	
	Remove-Assemblies $basePath "Microsoft.Data.Tools.Schema.Sql.dll"
	Remove-Directories $basePath "Microsoft.Data.Tools.Schema.Sql"
	
	Log-Message ""
}

function Clean-Registry()
{
	Log-Message "Cleaning Registry..."
	
	if ( Test-Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow\Server )
	{
		Log-Message "Removing registry key 'HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow\Server'"
		Remove-Item -Recurse -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow\Server
	}
	
	if ( Test-Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow )
	{
		$count = @( Get-ChildItem Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow ).Count
			
		if ($count -eq 0)
		{
			Log-Message "Removing registry key 'HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow'"
			Remove-Item -Recurse -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC\ReadiNow
		}
	}
	
	if ( Test-Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC )
	{
		$count = @( Get-ChildItem Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC ).Count

		if ($count -eq 0)
		{
			Log-Message "Removing registry key 'HKEY_LOCAL_MACHINE\SOFTWARE\EDC'"
			Remove-Item -Recurse -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\EDC
		}
	}
	
	Log-Message ""
}

function Clean-FileSystem()
{
	Log-Message "Cleaning FileSystem..."
	
	if ( Test-Path 'C:\Program Files\EDC\SoftwarePlatform' )
	{
		Log-Message "Removing folder 'C:\Program Files\EDC\SoftwarePlatform'"
		Remove-Item -Recurse -Path 'C:\Program Files\EDC\SoftwarePlatform'
	}
	
	if ( Test-Path 'C:\Program Files\EDC' )
	{
		$count = @( Get-ChildItem 'C:\Program Files\EDC' ).Count

		if ($count -eq 0)
		{
			Log-Message "Removing folder 'C:\Program Files\EDC'"
			Remove-Item -Recurse -Path 'C:\Program Files\EDC'
		}
	}
	
	Log-Message ""
}

function Clean-AssemblyFileInfo()
{
	Log-Message "Cleaning AssemblyFileInfo.cs.tmp files..."
	
	$path = Split-Path -Parent $Script:MyInvocation.MyCommand.Definition
	$path = Split-Path -Parent $path
	
	Get-ChildItem $path -Include AssemblyFileInfo.cs.tmp -Recurse | foreach ($_) {Remove-Item $_.fullname}
	
	Log-Message ""
}

function Clean-Contributors()
{
	Log-Message "Cleaning DacPac contributors..."
	
	if ( Test-Path "C:\Program Files (x86)\Microsoft SQL Server\120\DAC\bin\Extensions\ReadiNowDeploymentPlanContributors.dll" )
	{
		Log-Message "Removing 'ReadiNowDeploymentPlanContributors.dll'"
		
		Remove-Item -Path "C:\Program Files (x86)\Microsoft SQL Server\120\DAC\bin\Extensions\ReadiNowDeploymentPlanContributors.dll"
	}
	
	Log-Message ""
}

function Clean-ApplicationHost()
{
	Log-Message "Cleaning applicationHost.config..."
	
	Try
	{
		$path = "C:\Windows\System32\inetsrv\config\applicationHost.config"
		
		if ( Test-Path $path )
		{		
			$xml = [xml](Get-Content $path)
			$section = $xml.configuration | Select -ExpandProperty "system.applicationHost"
			$node = $section.serviceAutoStartProviders.add | where {$_.name -eq 'SpApiPreload'}
			if ($node)
			{
				Log-Message "Updating applicationHost.config"
				$node.ParentNode.RemoveChild($node)
				$xml.Save($path)
			}
		}
		
		Log-Message ""
	}
	Catch
	{
		$ErrorActionPreference = "Continue"
		Write-Error $_
	}
}

Try
{
	Log-Message ""
	
	Uninstall-SoftwarePlatform
	
	Clean-Gac
	Clean-Registry
	Clean-FileSystem
	
	Clean-AssemblyFileInfo
	
	Clean-Contributors
	Clean-ApplicationHost
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}