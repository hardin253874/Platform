#requires -Version 3.0
Add-Type -AssemblyName System.IO.Compression.FileSystem

Import-Module WebAdministration

##############
# Path functions
##############

# Get the script location
function Get-ScriptLocation
{
    return Split-Path -Parent $Script:MyInvocation.MyCommand.Definition
}

# Determines whether the specified path contains a root.
function Get-IsPathRooted($path)
{
	return [System.IO.Path]::IsPathRooted($path)
}

# Returns the paths root.
function Get-PathRoot($path)
{
	return [System.IO.Path]::GetPathRoot($path)
}

# Returns the absolute path for the specified path string.
function Get-FullPath($path)
{
	return [System.IO.Path]::GetFullPath($path)
}

# Returns the specified path if it contains a root otherwise
# returns the path with the specified base path prepended.
function Get-AbsolutePath($basePath, $path)
{
	if ( -Not ( [System.IO.Path]::IsPathRooted($path) ) )
	{
		$path = Join-PathEx $basePath $path 
	}
	
	return $path
}

# Joins two path values ensuring the result is absolute.
# I.e. 'C:\A' + '..\B' = 'C:\B' and not 'C:\A\..\B'
function Join-PathEx($path1, $path2)
{
	$path = Join-Path $path1 $path2
	$path = Get-FullPath $path
	
	return $path
}

# Gets the current working directory.
function Get-CurrentPath
{
	# Get the current directory
    $path = Get-ScriptLocation
	
	# Ensure the path ends with a trailing backslash
	$path = $path.TrimEnd("\") + "\"
	
	return $path
}

# Gets the installation folder. This is read from the settings file.
function Get-InstallPath($settings)
{
	# Is there an install path specified in the setting file?
	$path = $settings.installPath
	
	if ([string]::IsNullOrEmpty($path) )
	{
		# Fallback to using the current path
		$path = Get-CurrentPath
		
		if ([string]::IsNullOrEmpty($path) )
		{
			Write-Error 'Failed to determine installation path.'
			Exit 1
		}
	}
	
	# Ensure the path ends with a trailing backslash
	$path = $path.TrimEnd("\") + "\"
	
	return $path
}

# Returns the path to the 'PlatformConfigure' executable.
function Get-PlatformConfigurePath($settings)
{
	$path = $settings.platformConfigure.path
	
	if ([string]::IsNullOrEmpty($path) )
	{
		# Look in the install path
		$basePath = Get-InstallPath $settings
		$path = Join-PathEx $basePath 'Tools\PlatformConfigure.exe'
	}
	else
	{
		$path = Get-AbsolutePath (Get-CurrentPath) $path
	}

	Log-Message "Attempting to use PlatformConfigure.exe from location '$path'."
	return $path
}

# Resolve the path to the specified log file.
function Resolve-LogFilePath($filePath)
{
	if ( String-IsNullOrEmpty $filePath )
	{
		return $Null
	}
	
	if ( -Not (Get-IsPathRooted $filePath ) )
	{
		$filePath = Join-PathEx (Get-CurrentPath) $filePath
	}
	
	return $filePath
}

##############
# Settings file functions
##############

# Reads the specified environment settings file in JSON format.
function Read-EnvironmentSettingFile($environmentFilePath)
{
    $file = Resolve-Path $environmentFilePath
	
	# Read the contents of the file
    $jsonText = Get-Content $file -Raw
	
	# Convert the text to a JSON object
    $jsonObject = ConvertFrom-Json $jsonText
	
    return $jsonObject
}

# Validate the read environment settings.
function Validate-EnvironmentSettings($settings)
{
	Log-Message "Validating environment settings..."
	
	Validate-Setting $settings.installPath 'installPath'
	
	Validate-Setting $settings.database.path 'database.path'
	Validate-Setting $settings.database.server 'database.server'
	Validate-Setting $settings.database.catalog 'database.catalog'
	Validate-Setting $settings.database.domain 'database.domain'
	Validate-Setting $settings.database.user 'database.user'
	Validate-Setting $settings.database.password 'database.password'
	Validate-Setting $settings.database.role 'database.role'
	
	foreach ( $logSecurity in $settings.logFolder.security )
	{
		Validate-Setting $logSecurity.identity 'logFolder.security.identity'
		Validate-Setting $logSecurity.access 'logFolder.security.access'
	}
	
	foreach ( $mailSecurity in $settings.outgoingEmailFolder.security )
	{
		Validate-Setting $mailSecurity.identity 'outgoingEmailFolder.security.identity'
		Validate-Setting $mailSecurity.access 'outgoingEmailFolder.security.access'
	}
	
	Validate-Setting $settings.uploadFolder.path 'uploadFolder.path'
	
	foreach ( $uploadSecurity in $settings.uploadFolder.security )
	{
		Validate-Setting $uploadSecurity.identity 'uploadFolder.security.identity'
		Validate-Setting $uploadSecurity.access 'uploadFolder.security.access'
	}
	
	Validate-Setting $settings.errorsFolder.path 'errorsFolder.path'
	
	foreach ( $errorSecurity in $settings.errorsFolder.security )
	{
		Validate-Setting $errorSecurity.identity 'errorsFolder.security.identity'
		Validate-Setting $errorSecurity.access 'errorsFolder.security.access'
	}
	
	Validate-Setting $settings.repositories.basePath 'repositories.basePath'
	Validate-Setting $settings.repositories.applicationLibraryDirectory 'repositories.applicationLibraryDirectory'
	Validate-Setting $settings.repositories.binaryDirectory 'repositories.binaryDirectory'
	Validate-Setting $settings.repositories.documentDirectory 'repositories.documentDirectory'
	Validate-Setting $settings.repositories.tempDirectory 'repositories.tempDirectory'
	
	foreach ( $repoSecurity in $settings.repositories.security )
	{
		Validate-Setting $repoSecurity.identity 'repositories.security.identity'
		Validate-Setting $repoSecurity.access 'repositories.security.access'
	}
	
	Validate-Setting $settings.redis.server 'redis.server'
	Validate-Setting $settings.redis.port 'redis.port'
	Validate-Setting $settings.redis.domain 'redis.domain'
	Validate-Setting $settings.redis.user 'redis.user'
	Validate-Setting $settings.redis.password 'redis.password'
	Validate-Setting $settings.redis.serviceName 'redis.serviceName'
	Validate-Setting $settings.redis.serviceDisplayName 'redis.serviceDisplayName'
	
	foreach ( $redisSecurity in $settings.redis.folderSecurity )
	{
		Validate-Setting $redisSecurity.identity 'redis.folderSecurity.identity'
		Validate-Setting $redisSecurity.access 'redis.folderSecurity.access'
	}
	
	Validate-Setting $settings.scheduler.domain 'scheduler.domain'
	Validate-Setting $settings.scheduler.user 'scheduler.user'
	Validate-Setting $settings.scheduler.password 'scheduler.password'
	Validate-Setting $settings.scheduler.serviceName 'scheduler.serviceName'
	Validate-Setting $settings.scheduler.serviceDisplayName 'scheduler.serviceDisplayName'
	
	Validate-Setting $settings.webServer.virtualDirectoryPath 'webServer.virtualDirectoryPath'
	
	foreach ($appPool in $settings.webServer.appPools)
	{
		Validate-Setting $appPool.name 'appPool.name'
		Validate-Setting $appPool.domain 'appPool.domain'
		Validate-Setting $appPool.user 'appPool.user'
		Validate-Setting $appPool.password 'appPool.password'
	}
	
	foreach ($app in $settings.webServer.applications)
	{
		Validate-Setting $app.virtualPath 'app.virtualPath'
		Validate-Setting $app.physicalPath 'app.physicalPath'
	}
	
	Log-Message "Validation successful."
}

# Validates the specified setting by ensuring it is not null or empty.
function Validate-Setting($setting, $name)
{
	if ( ( String-IsNullOrEmpty $setting ) -eq $True )
	{
		Write-Error "Validation error in environment settings file. Missing property '$name'."
	}
}

##############
# Process functions
##############

# Stops the IIS Worker process by killing the w3wp executable.
function Stop-IisWorkerProcess
{
    Log-Message 'Stop w3wp.exe IIS Worker Process if running'
    Stop-Process -f -processname w3wp -ErrorAction SilentlyContinue
}

# Stops the Log Viewer process by killing the LogViewer executable.
function Stop-LogViewerProcess
{
    Log-Message 'Stop LogViewer.exe Process if running'
    Stop-Process -f -processname LogViewer -ErrorAction SilentlyContinue
}

# Start IIS
function Start-Iis
{
    Log-Message 'Start IIS'
    & iisreset /start | Out-Null
}

# Restart IIS
function Restart-Iis
{
    Log-Message 'Restart IIS'
    & iisreset /restart | Out-Null
}

# Stop IIS
function Stop-Iis
{
    Log-Message 'Stop IIS'
    & iisreset /stop | Out-Null
}

# Determine if a service is present
function Has-Service($serviceName)
{
   $service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
   return $service -ne $null    
}

# Stop the scheduler service
function Stop-SchedulerService($settings)
{
    Log-Message "Stop $($settings.scheduler.serviceName)"
	if (Has-Service $settings.scheduler.serviceName)
	{
		Stop-Service $settings.scheduler.serviceName -ErrorAction SilentlyContinue | Out-Null
	}
}

# Start the scheduler service
function Start-SchedulerService($settings)
{
    Log-Message "Start $($settings.scheduler.serviceName)"
    Start-Service $settings.scheduler.serviceName | Out-Null
}

# Stop the redis service
function Stop-RedisService($settings)
{
    Log-Message "Stop $($settings.redis.serviceName)"
	if (Has-Service $settings.redis.serviceName)
	{
		Stop-Service $settings.redis.serviceName | Out-Null
	}
}

# Start the redis service
function Start-RedisService($settings)
{
    Log-Message "Start $($settings.redis.serviceName)"
    Start-Service $settings.redis.serviceName | Out-Null
}

# Starts the specified process.
function Start-ProcessAndWait($fileName, $arguments)
{
    Log-Message "Starting process '$fileName $arguments'"

    $pinfo = New-Object System.Diagnostics.ProcessStartInfo
    $pinfo.FileName = $fileName
    $pinfo.RedirectStandardError = $true
    $pinfo.RedirectStandardOutput = $true
    $pinfo.UseShellExecute = $false
    $pinfo.Arguments = $arguments

    $p = New-Object System.Diagnostics.Process
    $p.StartInfo = $pinfo
    $p.Start() | Out-Null
    $p.WaitForExit()

    return $p
}

# Stop any local processes that are using the specified folder.
function Stop-LocalProcessesUsingFolder($folder)
{
    Log-Message "Stopping local processes using folder '$folder'"

    $args = '/accepteula "' + $folder + '"'

    $p = Start-ProcessAndWait "handle.exe" $args

    $handleOut = $p.StandardOutput.ReadToEnd()
    $handleError = $p.StandardError.ReadToEnd()

    if ($handleError.Length -gt 0)
    {
        Log-Message $handleError
    }

    Log-Message $handleOut

    if ($handleOut -match "(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)")
    {
        Log-Message "Stopping processes with id" $matches[1]
        Stop-Process -Id $matches[1] -Force -ErrorAction SilentlyContinue
    }
}

# Stop any remote processes that are using the specified folder.
function Close-RemoteFolderHandles($folder)
{
    Log-Message "Closing remote handles to folder '$folder'"

    $args = '/accepteula "' + $folder + '" -c'

    $p = Start-ProcessAndWait "psfile.exe" $args

    $psFileOut = $p.StandardOutput.ReadToEnd()
    $psFileError = $p.StandardError.ReadToEnd()

    if ($psFileError.Length -gt 0)
    {
        Log-Message $psFileError
    }

    Log-Message $psFileOut
}

##############
# Cleanup functions
##############

# Attempt to fix any SSDT problems by ensuring the ScriptDom assembly is in the GAC.
# Previous versions of the MSI installer incorrectly removed this during uninstall.
function Fix-SsdtReferences
{
	Log-Message 'Fix SSDT References'
	
	if (Test-Path 'Microsoft.SqlServer.TransactSql.ScriptDom.dll')
	{
		# Load EnterpriseServices assembly
        [System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")
		$publish = New-Object System.EnterpriseServices.Internal.Publish
		
		# Install assembly in the GAC
		$publish.GacInstall("Microsoft.SqlServer.TransactSql.ScriptDom.dll")
		Log-Message 'Added Microsoft.SqlServer.TransactSql.ScriptDom.dll to the GAC'
    }
}

# Clear the installation directory
function Clear-InstallationDirectory($settings)
{
	$path = Get-InstallPath $settings
	
    Log-Message "Clear installation directory $path"
	
    if (Test-Path $path)
	{
        Remove-Item "$path*" -Recurse
    }
}

# Clear the installation directory except the database files (but do delete the DatPac under it), and also the file repos
function Clear-InstallationDirectory-ForUpgrade($settings)
{
	$path = Get-InstallPath $settings
	$reposPath = Get-AbsolutePath $path $settings.repositories.basePath
	$logPath = Get-AbsolutePath $path "Log"
	$dbPath = Get-AbsolutePath $path "Database"

	Get-ChildItem -Path  $path -Recurse -exclude *.mdf, *.ldf |
		Select -ExpandProperty FullName |
		Where {$_ -ne "$dbPath"} |
		Where {$_ -notLike "$logPath*"} |
		Where {$_ -notLike "$reposPath*"} |
		Where {$_ -ne $path} |
		sort length -Descending |
		Remove-Item -force 
}

# Clear the file repository directory
function Clear-FileRepositoryDirectory($settings)
{
	$installPath = Get-InstallPath $settings
	$path = Get-AbsolutePath $installPath $settings.repositories.basePath
	
    Log-Message 'Clear file repository directory' $path
	
    if (Test-Path $path)
	{
        Remove-Item $path -Recurse
    }
}

# Clear cookies
function Clear-Cookies
{
    Log-Message 'Run cookie monster'
	
    $Path = Join-PathEx (Get-CurrentPath) ClearCookies.bat
	
	# Run the file.
    & $Path
}

##############
# Install/Uninstall/Upgrade functions
##############

# Ensure the log file exists.
function Create-LogFile
{
	if ( $global:logFile -ne $Null )
	{
		if ( Test-Path $global:logFile )
		{
			del $global:logFile
		}
		
		$file = New-Item $global:logFile -type file
		
		Set-FileSecurity $global:logFile 'Everyone' 'FullControl'
	}
	else
	{
		Log-Message "No Log file specified."
	}
}

# Attempt to install the specified test controller files in the specified
# path into the install location specified in the environment settings file.
function Install-TestControllersIfExist($settings, $path, $files)
{
	Log-Message "Probing path '$path' for Test Controllers..."
	
	$installPath = Get-InstallPath $settings
	$spApiPath = Join-PathEx $installPath '\SpApi\bin'
	
	foreach($file in $files)
	{
		$filePath = Join-PathEx $path $file	
		
		if(Test-Path $filePath)
		{
			Copy-Item $filePath $spApiPath -Force
		}
		else
		{
			return $False
		}
	}
	
	return $True
}

# Install the Test Controller WebApi files.
function Install-TestControllers($settings)
{
	Log-Message 'Installing TestController Web Apis...'
	
	# Files to be installed.
	$files = "EDC.SoftwarePlatform.WebApiTestControllers.dll","EDC.SoftwarePlatform.WebApiTestControllers.dll.config","EDC.SoftwarePlatform.WebApiTestControllers.pdb"
	
	$path = Join-PathEx (Get-CurrentPath) '..\Build\Debug\Tests\'

	$succeeded = Install-TestControllersIfExist $settings $path $files

	if($succeeded -eq $False)
	{
		$path = Join-PathEx (Get-CurrentPath) 'Tests'
		
		$succeeded = Install-TestControllersIfExist $settings $path $files
		
		if($succeeded -eq $False)
		{
			Log-Message "Failed to install Test Controller"
		}
		else
		{
			Log-Message "Test Controller successfully installed from '$path'"
		}
	}
	else
	{
		Log-Message "Test Controller successfully installed from '$path'"
	}
	
	if ($succeeded -eq $True)
	{
		$applications = $settings.webServer.applications
		
		$spApiApp = $Null
		
		foreach ( $application in $applications )
		{
			if ( $application.virtualPath -eq "/SpApi" )
			{
				$spApiApp = $application
				break
			}
		}
	
		if ( $spApiApp -ne $Null )
		{
			$spApiPhysicalPath = $spApiApp.physicalPath
			
			if ( -Not (Get-IsPathRooted $spApiPhysicalPath))
			{
				if ( $spApiApp.relativeTo -eq "current" )
				{
					$spApiPhysicalPath = Join-PathEx (Get-CurrentPath) $spApiPhysicalPath
				}
				else
				{
					$spApiPhysicalPath = Join-PathEx (Get-InstallPath $settings) $spApiPhysicalPath
				}
			}
			
			$webConfigPath = Join-PathEx $spApiPhysicalPath 'Web.Config'

			if ( Test-Path $webConfigPath )
			{
				Log-Message "Found Web.Config at '$webConfigPath'"
			
				Log-Message "Scanning Web.Config for 'EDC.SoftwarePlatform.WebApiTestControllers'..."
			
				$webConfig = $webConfigPath
				$doc = (Get-Content $webConfig) -as [Xml]
				
				$root = $doc.get_DocumentElement()
				
				$found = 0
				
				foreach( $item in $root.'system.web'.compilation.assemblies.add )
				{
					if($item.assembly -eq "EDC.SoftwarePlatform.WebApiTestControllers, Version=1.0.0.0, Culture=neutral" )
					{
						$found = 1
						break
					}
				}
				
				if ( $found -eq 0 )
				{
					Log-Message "WebApiTestControllers not found in Web.Config. Adding..."
					
					$element = $doc.CreateElement( "add" )
					$assembly = $doc.CreateAttribute( "assembly" )
					$assembly.value = "EDC.SoftwarePlatform.WebApiTestControllers, Version=1.0.0.0, Culture=neutral"
					$attrib = $element.SetAttributeNode( $assembly )

					$child = $doc.configuration.'system.web'.compilation.assemblies.AppendChild( $element )
				}
				else
				{
					Log-Message "WebApiTestControllers already exists in Web.Config."
				}

				Log-Message "Saving Web.config..."
				
				$doc.Save($webConfigPath)
				
				Log-Message "Web.Config successfully saved."
			}
			else
			{
				Log-Warning "Failed to locate Web.Config at '$webConfigPath'"
			}
		}
		else
		{
			Log-Warning "Failed to locate 'SpApi' web application in configuration file."
		}
	}
}

# Grant the 'CanModify' permission on the test and sample applications.
function Grant-CanModifyApplications($process, $settings)
{    				
	if ($settings.defaultTenant.grantCanModifyTestAndSampleApps)
	{			
		Grant-CanModifyApplication $process $settings $settings.defaultTenant.name 'Test Solution'
		Grant-CanModifyApplication $process $settings $settings.defaultTenant.name 'Foster University'
		Grant-CanModifyApplication $process $settings $settings.defaultTenant.name 'Foster University DATA'
	}
	else
	{
		Log-Message "Grant Can-Modify on test and sample apps has been disabled for the default tenant."
	}
}

# Decompress the platform to the install folder.
function Unzip-SoftwarePlatform($settings, $redistributableFile)
{
	$path = Get-InstallPath $settings
	$currentPath = Get-CurrentPath
		
	if (-Not (Test-Path $path))
	{
		Log-Message 'Installation folder does not exist. Creating...'
		$dir = mkdir $path
	}
	
	if ( [string]::IsNullOrEmpty( $redistributableFile ) )
	{
		$setupFile = @(Get-ChildItem SoftwarePlatformSetup*.zip)[0]	
	}
	else
	{
		$setupFile = $redistributableFile
	}
	
	if ( ( -Not ( [string]::IsNullOrEmpty( $setupFile ) ) ) -And ( Test-Path $setupFile ) )
	{
		$setupFile = Get-AbsolutePath $currentPath $setupFile
		
		Log-Message "Found SoftwarePlatform setup file at '$setupFile'"
		
		Log-Message "Unzipping '$setupFile' to '$path'..."
		
		Unzip $setupFile $path
	}
	else
	{
		Log-Message "Failed to locate SoftwarePlatform setup file."
		Exit -1
	}
}

# Apply security settings
function Apply-Security($settings)
{
	$installPath = Get-InstallPath $settings
	$databaseInstallFolder = $settings.database.installPath
	
	if ( [string]::IsNullOrEmpty( $databaseInstallFolder ) )
	{
		$databaseInstallFolder = 'Database'
	}
	
	$databaseInstallFolder = Get-AbsolutePath $installPath $databaseInstallFolder
		
	Log-Message "Granting 'FullControl' to 'NT Service\MSSQLSERVER' for folder '$databaseInstallFolder'..."
	
	Set-FolderSecurity $databaseInstallFolder 'NT Service\MSSQLSERVER' 'FullControl'
}

# Install scheduler
function Install-Scheduler($settings)
{
	$installPath = Get-InstallPath $settings

	$name = $settings.scheduler.serviceName
			
	if ( String-IsNullOrEmpty $name )
	{
		Log-Warning "Invalid Scheduler service name specified '$name'. Scheduler is REQURIED for server operations."
		return
	}
	
	$displayName = $settings.scheduler.serviceDisplayName
	
	if ( String-IsNullOrEmpty $displayName )
	{
		$displayName = $name
	}
	
	$binaryPath = Join-PathEx $installPath 'Scheduler\SchedulerService.exe'
	
	if ( -Not ( Test-Path $BinaryPath ) )
	{
		Log-Warning "Failed to locate scheduler binary at '$binaryPath'. Scheduler is REQUIRED for server operations."
		return
	}
	
	$user = $settings.scheduler.user
	
	if ( String-IsNullOrEmpty $user )
	{
		Log-Warning "Invalid Scheduler user name specified '$user'. Scheduler is REQURIED for server operations."
		return
	}
	
	$domain = $settings.scheduler.domain
	
	if ( String-IsNullOrEmpty $domain )
	{
		Log-Warning "Invalid Scheduler domain specified '$domain'. Scheduler is REQURIED for server operations."
		return
	}
	
	$login = "$domain\$user"
	
	$password = $settings.scheduler.password
	
	if ( String-IsNullOrEmpty $password )
	{
		Log-Warning "Invalid Scheduler password specified '$password'. Scheduler is REQURIED for server operations."
		return
	}
	
	$securePassword = ConvertTo-SecureString $password -AsPlainText -Force
	
	$credentials = New-Object System.Management.Automation.PSCredential ($login, $securePassword)
	
	Log-Message "Installing Scheduler service into the service control manager..."
	
	$service = New-Service -Name $name -BinaryPathName "$binaryPath" -Credential $credentials -Description 'ReadiNow Scheduler Service' -DisplayName $displayName -StartupType 'Automatic'
	
	if (Get-Service $name -ErrorAction SilentlyContinue)
	{
		Log-Message "Scheduler service successfully installed."
	}
	else
	{
		Log-Error "Failed to install Scheduler service"
		Exit 1
	}
	
    return $name
}

# Given the scheduler service name, start it
function Start-Scheduler($name)
{
	Log-Message "Starting Scheduler service..."
	
	$serviceStart = Start-Service $name -WarningAction SilentlyContinue
	
	Log-Message "Scheduler service successfully started."
}

# Install and configure redis
function Install-Redis($settings)
{
	$server = $settings.redis.server
	$port = $settings.redis.port
	
	if ( String-IsNullOrEmpty $server )
	{
		Log-Warning "No Redis server specified. Redis is REQURIED for server operations."
		return
	}
	
	if ( String-IsNullOrEmpty $port )
	{
		$port = 6379
	}
	
	$isLocal = Is-LocalAddress $server
	
	Log-Message "Testing connection to Redis server '$server' on port '$port'..."
	
	$connected = Test-NetConnection -ComputerName $server -Port $port -InformationLevel Quiet
	
	if ( $connected -eq $False )
	{
		if ( $isLocal )
		{
			Log-Message "Failed to connect to Redis server '$server' on port '$port'."
			
			$installPath = Get-InstallPath $settings
			
			if ($port -ne 6379)
			{
				$redisConfigPath = Join-PathEx $installPath 'Redis\redis.windows.conf'
				
				Log-Message "Searching for local redis configuration file..."
				
				if ( -Not ( Test-Path $redisConfigPath ) )
				{
					Log-Warning "Cannot locate redis configuration file at '$redisConfigPath'."
					return
				}
				
				Log-Message "Found redis configuration file at '$redisConfigPath'"
				
				Log-Message "Updating redis configuration file..."
				
				( Get-Content $redisConfigPath ) | ForEach-Object { $_ -replace "^port .+","port $port" } | Set-Content $redisConfigPath
				
				Log-Message "Redis configuration file successfully updated."
			}
			
			$name = $settings.redis.serviceName
			
			if ( String-IsNullOrEmpty $name )
			{
				Log-Warning "Invalid Redis server name specified '$name'. Redis is REQURIED for server operations."
				return
			}
			
			$displayName = $settings.redis.serviceDisplayName
			
			if ( String-IsNullOrEmpty $displayName )
			{
				$displayName = $name
			}
			
			$binaryPath = Join-PathEx $installPath 'Redis\redis-server.exe'
			
			if ( -Not ( Test-Path $BinaryPath ) )
			{
				Log-Warning "Failed to locate redis binary at '$binaryPath'. Redis is REQUIRED for server operations."
				return
			}
			
			$user = $settings.redis.user
			
			if ( String-IsNullOrEmpty $user )
			{
				Log-Warning "Invalid Redis user name specified '$user'. Redis is REQURIED for server operations."
				return
			}
			
			$domain = $settings.redis.domain
			
			if ( String-IsNullOrEmpty $domain )
			{
				Log-Warning "Invalid Redis domain specified '$domain'. Redis is REQURIED for server operations."
				return
			}
			
			$login = "$domain\$user"
			
			$password = $settings.redis.password
			
			if ( String-IsNullOrEmpty $password )
			{
				Log-Warning "Invalid Redis password specified '$password'. Redis is REQURIED for server operations."
				return
			}
			
			$securePassword = ConvertTo-SecureString $password -AsPlainText -Force
			
			$credentials = New-Object System.Management.Automation.PSCredential ($login, $securePassword)
			
			Log-Message "Installing Redis service into the service control manager..."
			
			$service = New-Service -Name $name -BinaryPathName "$binaryPath --service-run redis.windows.conf" -Credential $credentials -Description 'ReadiNow Redis Service' -DisplayName $displayName -StartupType 'Automatic'
			
			if (Get-Service $name -ErrorAction SilentlyContinue)
			{
				Log-Message "Redis service successfully installed."
			}
			else
			{
				Log-Error "Failed to install Redis service"
				Exit 1
			}
			
			Log-Message "Starting Redis service..."
			
			$serviceStart = Start-Service $name -WarningAction SilentlyContinue
			
			Log-Message "Redis service successfully started."
		}
		else
		{
			Log-Warning "Failed to connect to Redis server '$server' on port '$port'. Please install redis on the remote system."
		}
	}
}

# Install the database.
function Install-Database($settings)
{
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SMO") | Out-Null
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoExtended") | Out-Null
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo") | Out-Null
	[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SmoEnum") | Out-Null

	$exists = $False
	
	$server = $settings.database.server
	
	if ( String-IsNullOrEmpty $server )
	{
		$server = "localhost"
	}
	
	$exists = $False
	
	Try
	{
		$serverObject = New-Object ("Microsoft.SqlServer.Management.Smo.Server") $server
	
		$catalog = $settings.database.catalog
		
		$databases = $serverObject.Databases
	}
	Catch
	{
		Log-Error $_
		Exit 1
	}
	
	$found = $False
		
	foreach ( $database in $databases )
	{	
		Log-Message "Inspecting database '$($database.Name)'..."
		
		if ( $database.Name -eq $catalog )
		{
			$found = $True
			break
		}
	}
	
	if ($found -eq $True)
	{
		Log-Message "Found existing database '$catalog' database on server '$server'."
	}
	else
	{
		Log-Message "Failed to locate database '$catalog' on server '$server'. Creating..."
	}
	
	$installPath = Get-InstallPath $settings
	$databasePath = Get-AbsolutePath $installPath ($settings.database.path)
	$dacPacPath = Join-PathEx $installPath 'Database\\DacPac'
	$readiNowDacPacPath = Join-PathEx $dacPacPath 'ReadiNowDatabase.dacpac'

	if ( Test-Path $readiNowDacPacPath)
	{
		Log-Message "Found dacpac deployment package at '$readiNowDacPacPath'"
	}
	else
	{
		Log-Warning "Failed to locate dacpac deployment package at '$readiNowDacPacPath'."
		return
	}
	
	Log-Message "Initializing DacPac database deployment..."
	
	Add-Type -Path ( Join-PathEx $dacPacPath "Microsoft.SqlServer.Dac.dll" )
	
	$dacService = New-Object Microsoft.SqlServer.Dac.DacServices "Data Source=$server;Integrated Security=True"
	

	
	Try
	{
		$package = [Microsoft.SqlServer.Dac.DacPackage]::Load("$readiNowDacPacPath", "Memory")
	
		$mdfPath = Join-PathEx $databasePath ("{0}_Dat.mdf" -f $catalog)
		$ldfPath = Join-PathEx $databasePath ("{0}_Log.ldf" -f $catalog)
	
		Try
		{
			#$ctx = Impersonate-Identity $settings.database.domain $settings.database.user $settings.database.password

			$options = New-Object Microsoft.SqlServer.Dac.DacDeployOptions
			$options.CommandTimeout = 300
			$options.BlockOnPossibleDataLoss = $False
			$options.AdditionalDeploymentContributors = "ReadiNowDeploymentPlanContributors.DatabaseCreationLocationModifier"
			$options.AdditionalDeploymentContributorArguments = "DatabaseCreationLocationModifier.MdfFilePath=$mdfPath;DatabaseCreationLocationModifier.LdfFilePath=$ldfPath"
	
			Register-ObjectEvent -in $dacService -EventName Message -Source "msg" -Action { Log-Message $Event.SourceArgs[1].Message.Message } | Out-Null
	
			$dacService.Deploy( $package, $catalog, $True, $options )
		}
		Finally
		{
			#Undo-Identity $ctx

			UnRegister-Event -Source "msg" | Out-Null
		}
	}
	Finally
	{
		$package.Dispose( )
	}
	
	$login = $settings.database.user
	$domain = $settings.database.domain
	
	if ($domain)
	{
		$login = "$domain\$login"
	}
			
	if ($found -eq $False)
	{
		# Install roles
		$role = $settings.database.role
		
		if ($role)
		{
			Log-Message "Creating database role '$role'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "DECLARE @role NVARCHAR(MAX) = NULL; SELECT @role = name FROM sys.database_principals WHERE LOWER(name) = LOWER(N'$role'); IF (@role IS NULL) CREATE ROLE [$role]"
			
			Log-Message "Granting 'EXECUTE,SELECT,INSERT,UPDATE,DELETE' permissions to database role '$role'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "GRANT EXECUTE,SELECT,INSERT,UPDATE,DELETE TO [$role]"
			
			Log-Message "Granting certificate control to database role '$role'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "GRANT CONTROL ON CERTIFICATE::cert_keyProtection TO [$role]"
			
			Log-Message "Granting view definition to database role '$role'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "GRANT VIEW DEFINITION ON SYMMETRIC KEY::key_Secured TO [$role]"
			
			Log-Message "Creating database login '$login'..."
			& sqlcmd -E -S $server -d "master" -b -Q "DECLARE @login NVARCHAR(MAX) = NULL; SELECT @login = name FROM sys.server_principals WHERE LOWER(name) = LOWER(N'$login'); IF (@login IS NULL) CREATE LOGIN [$login] FROM WINDOWS"
			
			Log-Message "Creating database user for windows login '$login'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "DECLARE @user NVARCHAR(MAX) = NULL; SELECT @user = name FROM sys.database_principals WHERE LOWER(name) = LOWER(N'$login'); IF (@user IS NOT NULL) EXEC ('ALTER USER [' + @user + '] WITH LOGIN = [$login]') ELSE CREATE USER [$login] FOR LOGIN [$login]"
			
			Log-Message "Assigning database login '$login' to role '$role'..."
			& sqlcmd -E -S $server -d $catalog -b -Q "exec sp_addrolemember N'$role', N'$login'"
		}
	}
	
	Log-Message "Granting View Server State to database login '$login'..."
	& sqlcmd -E -S $server -d "master" -b -Q "GRANT VIEW SERVER STATE TO [$login]"
}

# Perform post-datpac database upgrade tasks
function Repair-System-Applications($settings)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog

	Log-Message "Verifying/repairing system applications in tenants..."
	& sqlcmd -E -S $server -d $catalog -b -Q "EXEC dbo.spRepairApplicationReferences"
	Log-Message "Updating statistics"
	& sqlcmd -E -S $server -d $catalog -b -Q "EXEC sp_updatestats"	
}

# Ensure the class names for EDC assemblies are not strong named
function Upgrade-QuartzJobs($settings)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	Log-Message "Updating quartz job details in database '$catalog' on server '$server'..."
	& sqlcmd -E -S $server -d $catalog -b -Q "UPDATE QRTZ_JOB_DETAILS SET JOB_CLASS_NAME = SUBSTRING( JOB_CLASS_NAME, 0, CHARINDEX( 'publickeytoken=', LOWER( JOB_CLASS_NAME ) ) + LEN( 'publickeytoken=' ) ) + 'null' WHERE LOWER(JOB_CLASS_NAME) LIKE 'edc%' AND LOWER(JOB_CLASS_NAME) LIKE '%publickeytoken=%' AND LOWER(JOB_CLASS_NAME) NOT LIKE '%publickeytoken=null%'" | Out-Null
}

# Install the SoftwarePlatform.config file
function Install-SoftwarePlatformConfig($settings)
{
	$path = Get-InstallPath $settings
	
	$pristineFilePath = Join-PathEx $path 'Configuration\SoftwarePlatform.pristine'
	$configFilePath = Join-PathEx $path 'Configuration\SoftwarePlatform.config'
	
	if (Test-Path $pristineFilePath)
	{
		Log-Message "Moving 'SoftwarePlatform.pristine' to 'SoftwarePlatform.config'..."
	
		Rename-Item -path $pristineFilePath -newName 'SoftwarePlatform.config'
	}
	else
	{
		Log-Warning "Failed to locate 'SoftwarePlatform.pristine' at '$pristineFilePath'."
	}
}

# Set the xml attribute value.
function Set-XmlAttribute($xml, $elementXpath, $attributeName, $value, $elementParentXpath, $elementParentName)
{
	Log-Message "Searching for xpath '$elementXpath'..."
	
 	$element = [System.Xml.XmlElement]$xml.SelectSingleNode( $elementXpath )
	
	if ( $element -eq $Null )
	{
		if ( ( $elementParentXpath -ne $Null ) -and ( $elementParentName -ne $Null ) )
		{
			Log-Message "Element with xpath 'elementXpath' does not exist. Creating..."
			
			$parentElement = [System.Xml.XmlElement]$xml.SelectSingleNode( $elementParentXpath )
			
			if ( $parentElement -eq $Null )
			{
				Log-Warning "Failed to locate parent element at '$elementParentXpath' in SoftwarePlatform.config."
				return $False
			}
			else
			{
				$element = $xml.CreateElement( "$elementParentName" )

				$parentElement.AppendChild( $element ) | Out-Null
				
				Log-Message "Element '$elementParentName' created beneath '$elementParentXpath'."
			}
		}
		else
		{
			Log-Warning "Failed to locate element at '$elementXpath' in SoftwarePlatform.config."
			return $False
		}
	}
	
	Log-Message "Searching for attribute '$attributeName'..."
	
	$attribute = $element.GetAttributeNode( $attributeName )
		
	if ( $attribute -eq $Null )
	{
		Log-Message "Attribute '$attributeName' not found. Creating..."
		
		$attribute = $xml.CreateAttribute( $attributeName )
		$element.SetAttributeNode( $attribute ) | Out-Null
	}
	
	if ($attribute.Value -ne $value )
	{
		Log-Message "Updating xml attribute '$attributeName' of element '$elementXpath' to '$value'..."
							
		$attribute.Value = [string]$value
		
		return $True
	}
	else
	{
		Log-Message "Attribute '$attributeName' value is already set to '$value'."
	}
	
	return $False
}

# Retrieve the site name.
function Get-SiteName($settings)
{
	$siteName = $settings.configuration.site.name
	
	if ( String-IsNullOrEmpty $siteName )
	{
		$siteName = [System.Net.Dns]::GetHostName( )
	}
	
	return $siteName
}

# Get the fully qualified domain name.
function Get-Fqdn()
{
	$domainName = [System.Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties( ).DomainName
	$hostName = [System.Net.Dns]::GetHostName( )
	
	$domainName = ".$domainName"
	
	if ( -Not ( $hostName.EndsWith( $domainName ) ) )
	{
		$hostName = "$hostName$domainName"
	}
	
	return $hostName
}

# Retrieve the site address
function Get-SiteAddress($settings)
{
	$siteAddress = $settings.configuration.site.address
	
	if ( String-IsNullOrEmpty $siteAddress )
	{
		$siteAddress = Get-Fqdn
	}
	
	return $siteAddress
}

# Retrieve the service root address
function Get-ServiceRootAddress($settings)
{
	$serviceRootAddress = $settings.configuration.site.serviceRootAddress
	
	if ( String-IsNullOrEmpty $serviceRootAddress )
	{
		$serviceRootAddress = "/SoftwarePlatform/Services"
	}
	
	return $serviceRootAddress
}

# Get the current branch value
function Get-CurrentBranch($settings)
{
	$path = Get-InstallPath $settings
	$assemblyPath = Join-PathEx $path "Tools\\EDC.ReadiNow.Common.dll"
	
	$assembly = [Reflection.Assembly]::LoadFile($assemblyPath)
	
	$attributeType = [System.Reflection.AssemblyConfigurationAttribute]
	
	$attributes = $assembly.GetCustomAttributes( $attributeType, $false )
	
	return $attributes[0].Configuration
}

# Get the current version
function Get-CurrentVersion($settings)
{
	$path = Get-InstallPath $settings
	$assemblyPath = Join-PathEx $path "Tools\\EDC.ReadiNow.Common.dll"
	
	$assembly = [Reflection.Assembly]::LoadFile($assemblyPath)
	
	$attributeType = [System.Reflection.AssemblyFileVersionAttribute]
	
	$attributes = $assembly.GetCustomAttributes( $attributeType, $false )
	
	return $attributes[0].Version
}

# Set the configuration file values.
function Process-ConfigurationFile($settings, $environmentFile)
{
	$path = Get-InstallPath $settings
	
	$configFilePath = Join-PathEx $path 'Configuration\SoftwarePlatform.config'
	
	if ( -Not ( Test-Path $configFilePath ) )
	{
		Log-Warning "Failed to locate SoftwarePlatform.config at '$configFilePath'."
		return
	}
	
	Log-Message "Found SoftwarePlatform.config at '$configFilePath'."	
	
	$xml = (Get-Content $configFilePath) -as [Xml]
		
	$saveRequired = $False
	
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/databaseSettings/connectionSettings" "server" $settings.database.server)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/databaseSettings/connectionSettings" "database" $settings.database.catalog)
	
	$siteName = Get-SiteName $settings
	$siteAddress = Get-SiteAddress $settings
	$serviceRootAddress = Get-ServiceRootAddress $settings
	
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/siteSettings/site" "name" $siteName)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/siteSettings/site" "address" $siteAddress)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/siteSettings/site" "serviceRootAddress" $serviceRootAddress)
	
	$uploadFolder = Get-AbsolutePath $path $settings.uploadFolder.path
	$activationDate = [System.DateTime]::UtcNow.ToString( "u" )
	$logFilePath = Join-PathEx $path "Log"
	$currentBranch = Get-CurrentBranch $settings
	$currentVersion = Get-CurrentVersion $settings
	$envronmentFilePath = Resolve-Path $environmentFile
	
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/uploadDirectory" "path" $uploadFolder)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "activationDate" $activationDate)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "installFolder" $path)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "logFilePath" $logFilePath)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "currentBranch" $currentBranch)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "currentVersion" $currentVersion)
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/serverSettings/systemInfo" "environmentFile" $envronmentFilePath)
	
	$repoBasePath = Get-AbsolutePath $path $settings.repositories.basePath
	$appLibRepo = Join-PathEx $repoBasePath $settings.repositories.applicationLibraryDirectory
	$binRepo = Join-PathEx $repoBasePath $settings.repositories.binaryDirectory
	$docRepo = Join-PathEx $repoBasePath $settings.repositories.documentDirectory
	$tempRepo = Join-PathEx $repoBasePath $settings.repositories.tempDirectory
	
	Configure-Folder $settings $appLibRepo $settings.repositories.security
	Configure-Folder $settings $binRepo $settings.repositories.security
	Configure-Folder $settings $docRepo $settings.repositories.security
	Configure-Folder $settings $tempRepo $settings.repositories.security
	
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Application Library']" "name" "Application Library" "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Application Library']" "path" $appLibRepo "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Binary']" "name" "Binary" "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Binary']" "path" $binRepo "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Document']" "name" "Document" "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Document']" "path" $docRepo "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Temporary']" "name" "Temporary" "configuration/fileRepositorySettings/fileRepositories" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/fileRepositorySettings/fileRepositories/add[@name='Temporary']" "path" $tempRepo "configuration/fileRepositorySettings/fileRepositories" "add")
	
	$redisServer = $settings.redis.server
	$redisPort = $settings.redis.port
	
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/redisSettings/servers/add[@hostName='$redisServer']" "hostName" $redisServer "configuration/redisSettings/servers" "add")
	$saveRequired = $saveRequired -bor (Set-XmlAttribute $xml "configuration/redisSettings/servers/add[@hostName='$redisServer']" "port" $redisPort "configuration/redisSettings/servers" "add")
	
	$entries = $settings.configuration.misc
	
	if ( ( $entries -ne $Null ) -And ( $entries.Count -gt 0 ) )
	{		
		foreach ($entry in $entries)
		{
			Try
			{
				$name = $entry.name
				$xpath = $entry.xpath
				$value = $entry.value
				
				Log-Message "Searching for xml setting '$name' using xpath '$xpath'..."
				
				$nodes = $xml.SelectNodes($xpath)
				
				if ( ( $nodes -ne $Null ) -And ( $nodes.Count -gt 0 ) )
				{
					$node = $nodes[0]
					
					if ( $node -ne $Null )
					{
						if ( $node -is [System.Xml.XmlAttribute] )
						{
							$nodeValue = $node.'#text'
							
							Log-Message "Found existing node '$xpath' with value '$nodeValue'."
							
							if ($nodeValue -ne $value)
							{
								Log-Message "Updating xpath '$xpath' value to '$value'..."
								
								$node.'#text' = [string]$value
								
								$saveRequired = $True
							}
							else
							{
								Log-Message "Xml setting '$name' value is already set to '$value'."
							}
						}
						else
						{
							if ( $node -is [System.Xml.XmlElement] )
							{
								$nodeValue = $node.InnerText
								
								Log-Message "Found existing node '$xpath' with value '$nodeValue'."
								
								if ($nodeValue -ne $value)
								{
									Log-Message "Updating xpath '$xpath' value to '$value'..."
									
									$node.InnerText = [string]$value
									
									$modified = $True
								}
								else
								{
									Log-Message "Xml setting '$name' value is already set to '$value'."
								}
							}
						}
					}
				}
				else
				{
					Log-Warning "Failed to locate xml setting '$name' using xpath '$xpath'"
				}
			}
			Catch
			{
				Log-Message $_
				Log-Warning "Failed to find xpath '$($entry.name)'"
			}
		}
		
		if ( $modified -eq $True )
		{
			Log-Message "Saving SoftwarePlatform.config..."
			
			$xml.Save($configFilePath)
			
			Log-Message "SoftwarePlatform.Config successfully saved."
		}
	}
	
	if ( $saveRequired )
	{
		Log-Message "Saving SoftwarePlatform.config..."
			
		$xml.Save($configFilePath)
		
		Log-Message "SoftwarePlatform.Config successfully saved."
	}
}

# Process symbolic links
function Process-SymbolicLinks($settings)
{
	$currentPath = Get-CurrentPath
	$path = Get-InstallPath $settings
	
	$links = $settings.symbolicLinks
	
	$signature = '
        [DllImport("kernel32.dll")]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
        '
    Add-Type -MemberDefinition $signature -Name Creator -Namespace SymbolicLink 
	
	foreach ($link in $links)
	{
		$source = Get-AbsolutePath $currentPath ($link.source)
		$target = Get-AbsolutePath $path ($link.target)
		$linkType = $link.linkType
		
		$flags = [Int]($linkType -eq 'Directory')
		
		if ( Test-Path $source )
		{
			Log-Message "$linkType already exists at '$source'. Deleting..."
			
			Remove-Item $source -Recurse -Force
		}
		
		Log-Message "Creating symbolic link from '$source' to '$target' of type '$linkType'..."
		
		[SymbolicLink.Creator]::CreateSymbolicLink($source, $target, $flags) | Out-Null
		
		Log-Message "Symbolic link created from '$source' to '$target' of type '$linkType'."
	}
}

# Uninstall Redis.
function Uninstall-Redis($settings)
{
	$server = $settings.redis.server
	
	if ( String-IsNullOrEmpty $server )
	{
		Log-Warning "No Redis server specified. Skipping uninstall."
		return
	}
	
	$isLocal = Is-LocalAddress $server
	
	if ( $isLocal )
	{
		$serviceName = $settings.redis.serviceName
		
		if (Get-Service "$serviceName" -ErrorAction SilentlyContinue)
		{
			Log-Message "Stopping Redis service..."
			
			Stop-RedisService $settings
			
			Log-Message "Uninstalling Redis service '$serviceName'..."
		
			$service = Get-WmiObject -Class Win32_Service -Filter "Name='$serviceName'"
			
			if ($service -ne $Null)
			{
				$service.delete( ) | Out-Null
				
				Log-Message "Redis service '$serviceName' successfully uninstalled."
			}
		}
		else
		{
			Log-Message "No redis service with name '$serviceName' found."
		}
	}
}

# Uninstall Scheduler.
function Uninstall-Scheduler($settings)
{
	$serviceName = $settings.scheduler.serviceName
	
	if ( String-IsNullOrEmpty $serviceName )
	{
		Log-Warning "No scheduler service name specified. Skipping uninstall."
		return
	}
	
	if (Get-Service "$serviceName" -ErrorAction SilentlyContinue)
	{
		Log-Message "Stopping Scheduler service..."
		
		Stop-SchedulerService $settings
			
		Log-Message "Uninstalling Scheduler service '$serviceName'..."
		
		$service = Get-WmiObject -Class Win32_Service -Filter "Name='$serviceName'"
		
		if ($service -ne $Null)
		{
			$service.delete( ) | Out-Null
			
			Log-Message "Scheduler service '$serviceName' successfully uninstalled."
		}
	}
	else
	{
		Log-Message "No scheduler service with name '$serviceName' found."
	}
	
	# Due to a scheduler service name change, we also look
	# for the legacy name and uninstall it if found
	if (Get-Service "Scheduler" -ErrorAction SilentlyContinue)
	{
		Log-Message "Stopping legacy Scheduler service..."
		
		Log-Message "Stop Scheduler"
		Stop-Service "Scheduler" | Out-Null
			
		Log-Message "Uninstalling legacy Scheduler service '$serviceName'..."
		
		$service = Get-WmiObject -Class Win32_Service -Filter "Name='Scheduler'"
		
		if ($service -ne $Null)
		{
			$service.delete( ) | Out-Null
			
			Log-Message "Legacy Scheduler service 'Scheduler' successfully uninstalled."
		}
	}
	else
	{
		Log-Message "No legacy scheduler service with name 'Scheduler' found."
	}
}

# Delete application
function Delete-Application($settings, $application)
{
	$virtualDirPath = $settings.webServer.virtualDirectoryPath
		
	$appPath = $application.virtualPath
	
	$path = "$($virtualDirPath.TrimEnd('/'))/$($appPath.TrimStart('/'))"
	
	$virtualPath = $path.Replace( '/', '\' )
	
	$appName = $application.virtualPath.TrimStart('/')
		
	Log-Message "Searching for web application '$virtualPath'..."
	
	$app = Get-WebApplication -Site "Default Web Site" -Name "$path"
	
	$virtualPath = "IIS:\Sites\Default Web Site$virtualPath"
	
	if ( $app -ne $Null )
	{
		Log-Message "Found web application '$appName' at '$virtualPath'."
		Log-Message "Deleting web application '$appName'."
		
		Remove-WebApplication -Name "$path" -Site "Default Web Site"
		
		Log-Message "Web application '$appName' successfully deleted."
	}
}

# Delete AutoStart Providers
function Delete-AutoStartProvider($settings)
{
	$apps = Get-WebApplication -Site "Default Web Site"
	
	foreach ( $app in $apps )
	{
		if ( $app.serviceAutoStartProvider -eq "SpApiPreload" )
        {
            Log-Warning "Found service auto start provider in use by application '$($app.path)'. Skipping uninstall."
            return
        }
	}
	
	$autoStartProviders = Get-WebConfiguration -filter /system.applicationHost/serviceAutoStartProviders
	
	$foundProvider = $False
	
	$providerName = "SpApiPreload"
	
    $found = $False

	foreach( $autoStartProvider in $autoStartProviders )
	{
		foreach($provider in $autoStartProvider.Collection)
		{
			if ( $provider.name -eq $providerName )
			{
				$found = $True
				break;
			}
		}

        if ( $found -eq $True )
        {
            break;
        }
	}

    if ( $found -eq $True )
    {
		Log-Message "Found service auto start provider '$providerName'. Deleting..."
		
        Clear-WebConfiguration -filter /system.applicationHost/serviceAutoStartProviders
    }
	
}

# Delete virtual directory
function Delete-VirtualDirectory($settings)
{
	$path = Get-InstallPath $settings

	$vDirPath = $settings.webServer.virtualDirectoryPath
	
	if  ( ( $vDirPath -eq $Null ) -Or ( $vDirPath -eq "/" ) )
	{
		return
	}
	else
	{
		Log-Message "Searching for virtual directory '$vDirPath'"
		
		$name = $vDirPath.TrimStart('/')
		
		$virtualDir = Get-WebVirtualDirectory -Site "Default Web Site" -Application "/" -Name "$name"
		
		if ( $virtualDir -ne $Null )
		{
			Log-Message "Removing virtual directory '$name'..."
		
			Remove-WebVirtualDirectory -Site "Default Web Site" -Application "/" -Name "$name"
		}
	}
}

# Delete app pool
function Delete-AppPool($settings)
{
    $apps = Get-WebApplication -Site "Default Web Site"
	

	$existingAppPools = Get-ChildItem "IIS:\AppPools"
	
	$appPools = $settings.webServer.appPools
	
	$foundAppPool = $False

	foreach ($appPool in $appPools)
	{
		$name = $appPool.name
        
        $inUse = $False

        foreach ( $app in $apps )
	    {
		    if ( $app.applicationPool -eq "$name" )
            {
                Log-Warning "Found application pool '$name' in use by application '$($app.path)'. Skipping uninstall."
                $inUse = $True
                break
            }
	    }
		
        if ( $inUse -eq $True )
        {
            continue
        }

		Log-Message "Searching for application pool '$name'..."
		
		foreach ($existingAppPool in $existingAppPools)
		{
			if ($existingAppPool.Name -eq $appPool.name)
			{
				$foundAppPool = $True
				break;
			}
		}
		
		if ( $foundAppPool -eq $True )
		{
			Log-Message "Deleting application pool '$name'..."
			
			Remove-WebAppPool -Name "$name"
		}
	}
}

# Uninstall the web server.
function Uninstall-WebServer($settings)
{
	foreach ($application in $settings.webServer.applications)
	{
		Delete-Application $settings $application
	}
	
	Delete-AutoStartProvider $settings
	Delete-VirtualDirectory $settings
	Delete-AppPool $settings
}

# Remove symbolic links
function Remove-SymbolicLinks($settings)
{
	$links = $settings.symbolicLinks
	
	$currentPath = Get-CurrentPath
	
	foreach ( $link in $links )
	{
		$source = Get-AbsolutePath $currentPath ($link.source)
		
		if ( Test-Path $source )
		{
			Log-Message "Removing symbolic link to '$($link.linkType)' '$source'..."
			
			if ( $link.linkType -eq "Directory" )
			{
				[System.IO.Directory]::Delete($source, $true)
			}
			else
			{
				[System.IO.File]::Delete($source)
			}
		}
	}
}

# Uninstall the platform.
function Uninstall-SoftwarePlatform($settings)
{
	$path = Get-InstallPath $settings
    Log-Message "Uninstall SoftwarePlatform from '$path'"
    
	Uninstall-Redis $settings
	Uninstall-Scheduler $settings
	Remove-SymbolicLinks $settings
	Unregister-PerformanceCounters $settings
}

# Provision additional tenants.
function Provision-AdditionalTenants( $process, $settings)
{
	$tenantArray = $settings.additionalTenants
	
	if ( [string]::IsNullOrEmpty( $tenantArray ) )
	{
		Log-Message "No additional tenants found."
		return
	}
	
	foreach($tenant in $tenantArray)
	{
		$tenantName = $tenant.name
		
		if ( [string]::IsNullOrEmpty( $tenantName ) )
		{
			Log-Message "Missing tenant name."
			continue
		}
		
		Log-Message "Provisioning tenant '$tenantName'"
		
		Provision-Tenant $process $settings $tenantName
		
		$apps = $tenant.apps
		
		if ( [string]::IsNullOrEmpty( $apps ) )
		{
			Log-Message "No additional apps specified for tenant '$tenantName'."
			continue
		}
		
		foreach($app in $apps)
		{
			$appName = $app.name
			
			if ( [string]::IsNullOrEmpty( $appName ) )
			{
				Log-Message "Missing application name when deploying app for tenant '$tenantName'."
				continue
			}
			
			Log-Message "Deploying application '$appName' to tenant '$tenantName'..."
			
			Deploy-App $process $settings $tenantName $appName
			
			$canModify = $app.canModify
			
			if ( $canModify -eq $True )
			{
				Log-Message "Granting modify access to application '$appName' for tenant '$tenantName'..."
				
				Grant-CanModifyApplication $process $settings $tenantName $appName
			}
		}
	}
}

# Install the specified application to the default tenant
function Install-App($process, $appName, $settings, $bootstrap)
{
    Log-Message "Checking installation of app '$appName'"
	
    $basePath = Get-InstallPath $settings
    $applicationCacheDb = Join-PathEx $basePath "Applications\$appName.xml"
    $appsDb = Join-PathEx $basePath "Apps\$appName.xml"
    $dbInSameFolder = Join-PathEx $basePath "$appName.xml"

    if (Test-Path $applicationCacheDb)
	{
        $appPath = $applicationCacheDb
    }
    elseif (Test-Path $appsDb)
	{
        $appPath = $appsDb
    }
    elseif (Test-Path $dbInSameFolder)
	{
        $appPath = $dbInSameFolder
    }
    else
	{
        Write-Warning "Could not find $applicationCacheDb"
        Write-Warning "Could not find $appsDb"
        Write-Warning "Could not find $dbInSameFolder"
        return #silently ignore - behaviour inspired from InstallApp.bat
    }

    Log-Message "Importing app '$appName'"
	
	Import-App $process $settings $appPath $bootstrap

    Log-Message "Installation of app $appName succeeded."
}

# Upgrades the core application on each tenant
function Upgrade-TenantCoreApplications($process, $settings)
{
	$tenants = Get-Tenants $process $settings
	
	if ( ( $tenants -eq $Null ) -Or ( $tenants.Count -le 0 ) )
	{
		Log-Message "No tenants available to update."
		return
	}
	
	foreach ( $tenant in $tenants )
	{
		# Upgrade Core
		Upgrade-App $process $settings $tenant "7062aade-2e72-4a71-a7fa-a412d20d6f01"
		
		# Upgrade Console
		Upgrade-App $process $settings $tenant "34ff4d95-70c6-4ae8-8f6f-38d88546d4c4"
		
		# Upgrade Core Data
		Upgrade-App $process $settings $tenant "abf12077-6fa5-43da-b608-b8b7514d07bb"
	}
}

# Creates a system restore point for each tenant
function Create-TenantRestorePoints($process, $settings)
{
	$tenants = Get-Tenants $process $settings
	
	if ( ( $tenants -eq $Null ) -Or ( $tenants.Count -le 0 ) )
	{
		Log-Message "No tenants available to update."
		return
	}
	
	foreach ( $tenant in $tenants )
	{
		# Create the system restore point
		Create-RestorePoint $process $settings $tenant "Tenant Restore Point" "false" "true" $Null
	}
}

# Install the builtin ReadiNow applications
function Install-BuiltInReadiNowApplications($process, $settings)
{
	Install-App $process 'Shared' $settings $false

	if ($settings.importTestAndSampleApps)
	{
		Log-Message "Importing test and sample applications into the application library..."
		
		Install-App $process 'Power Tools' $settings $false 'ReadiNow Power Tools'
		Install-App $process 'Test Solution' $settings $false
		Install-App $process 'Foster University' $settings $false
		Install-App $process 'Foster University DATA' $settings $false
		
		Log-Message "Test and sample applications imported successfully."
	}
}

# Install the core applications.
function Install-CoreApplications($process, $settings)
{
	# This function should no longer be called by external scripts.
	Log-Warning "Install-CoreApplications is deprecated. Remove it from your script."
}

# Install the core applications.
function Install-CoreApplications-Impl($process, $settings, $bootstrap)
{
	Install-App $process 'coreSolution' $settings $bootstrap
	Install-App $process 'consoleSolution' $settings $bootstrap
	Install-App $process 'coreDataSolution' $settings $bootstrap
	Install-App $process 'systemSolution' $settings $bootstrap
}

# Create the default tenant.
function Create-DefaultTenant($process, $settings)
{
	$tenantName = $settings.defaultTenant.name
	
	if ($tenantName)
	{
		Log-Message "Creating default tenant '$tenantName'..."
		Provision-Tenant $process $settings $tenantName
	}
	
	Deploy-App $process $settings $tenantName 'Shared'
	
	if ($settings.defaultTenant.deployTestAndSampleApps)
	{
		Deploy-App $process $settings $tenantName 'ReadiNow Power Tools'
		Deploy-App $process $settings $tenantName 'Test Solution'
		Deploy-App $process $settings $tenantName 'Foster University'
		Deploy-App $process $settings $tenantName 'Foster University DATA'
	}
}

# Install the client if found.
function Install-ClientIfExists($settings, $install)
{
	if ($install -eq $True)
	{
		$zipPath = $settings.client.zipPath
		
		if ( [string]::IsNullOrEmpty( $zipPath ) )
		{
			$zipPath = "client\deploy"
		}
		
		$zipPath = Get-AbsolutePath (Get-CurrentPath) $zipPath
		
		Log-Message "Checking for client deployment files at '$zipPath' folder..."
		
		$path = Join-PathEx $zipPath install-all.ps1
		
		if (Test-Path -Path $path)
		{
			Log-Message "Invoking client installation script at '$path'."
			$installPath = Get-InstallPath $settings
			$installPath = Join-PathEx $installPath 'Client'
			& $path $installPath
		}
    }
	else
	{
		Log-Message 'Client install is set to false. Skipping client install.'
	}
}

# Create an IIS application pool.
function Create-AppPool($settings)
{
	$existingAppPools = Get-ChildItem "IIS:\AppPools"
	
	$appPools = $settings.webServer.appPools
	
	$foundAppPool = $False

	foreach ($appPool in $appPools)
	{
		$name = $appPool.name
		
		Log-Message "Searching for application pool '$name'..."
		
		foreach ($existingAppPool in $existingAppPools)
		{
			Log-Message "Inspecting application pool '$($existingAppPool.name)'..."
			if ($existingAppPool.Name -eq $appPool.name)
			{
				$pool = $existingAppPool
				$foundAppPool = $True
				break;
			}
		}
		
		if ( $foundAppPool -ne $True )
		{
			Log-Message "Creating application pool '$name'..."
			
			$pool = New-Item "IIS:\AppPools\$name"
		}
		else
		{
			Log-Message "Found existing application pool '$($pool.Name)'."
		}
		
		$user = $appPool.user
		$domain = $appPool.domain
		$password = $appPool.password
		
		if ($domain)
		{
			$user = "$domain\$user"
		}
		
		$saveRequired = $False
		
		if ( $pool.processmodel.identityType -ne "SpecificUser" )
		{
			Log-Message "Setting application pool '$name' identity type to 'SpecificUser'..."
			$pool.processmodel.identityType = "SpecificUser"
			$saveRequired = $True
		}
		
		if ( $pool.processmodel.username -ne "$user" )
		{
			Log-Message "Setting application pool '$name' identity to '$user'..."
			$pool.processmodel.username  = "$user"
			$saveRequired = $True
		}
		
		if ( $pool.processmodel.password -ne "$password" )
		{
			Log-Message "Setting application pool '$name' password..."
			$pool.processmodel.password = "$password"
			$saveRequired = $True
		}
		
		if ( $pool.managedPipelineMode -ne "Integrated" )
		{
			Log-Message "Setting application pool '$name' pipeline mode to 'Integrated'..."
			$pool.managedPipelineMode = "Integrated"
			$saveRequired = $True
		}
		
		if ( $pool.ManagedRuntimeVersion -ne "v4.0" )
		{
			Log-Message "Setting application pool '$name' managed runtime version to 'v4.0'..."
			$pool.ManagedRuntimeVersion = "v4.0"
			$saveRequired = $True
		}
		
		if ( $pool.StartMode -ne "AlwaysRunning" )
		{
			Log-Message "Setting application pool '$name' start mode to 'AlwaysRunning'..."
			$pool.StartMode = "AlwaysRunning"
			$saveRequired = $True
		}
		
		$timespan = [System.TimeSpan]::FromMinutes( 480 )
		if ( $pool.ProcessModel.IdleTimeout -ne $timespan )
		{
			Log-Message "Setting application pool '$name' idle timeout to '480'..."
			$pool.ProcessModel.IdleTimeout = $timespan
			$saveRequired = $True
		}
		
		if ( $saveRequired )
		{
			Log-Message "Saving application pool '$($pool.Name)' settings..."
			$pool | set-item
		}
	}
}

# Configure the virtual directory.
function Configure-VirtualDirectory($settings)
{
	$path = Get-InstallPath $settings
	
	$vDirPath = $settings.webServer.virtualDirectoryPath
	
	if  ( ( $vDirPath -eq $Null ) -Or ( $vDirPath -eq "/" ) )
	{
		return
	}
	else
	{
		$name = $vDirPath.TrimStart('/')
		
		Log-Message "Searching for virtual directory '$vDirPath'..."
		
		$virtualDir = Get-WebVirtualDirectory -site "Default Web Site" -Name "$vDirPath"
		
		if ( $virtualDir -eq $Null )
		{
			Log-Message "Creating virtual directory '$vDirPath'..."
			New-WebVirtualDirectory -Site "Default Web Site" -Name "$name" -PhysicalPath "$path" | Out-Null
		}
		else
		{
			Log-Message "Found existing virtual directory '$vDirPath'."
		}
	}
}

# Create the autostart provider
function Create-AutoStartProvider($settings)
{
	$autoStartProviders = Get-WebConfiguration -filter /system.applicationHost/serviceAutoStartProviders
	
	#Get-WebConfigurationProperty "/system.applicationHost/sites/site[@name='Default Web Site' and @id='1']/application[@path='/Test']" -Name serviceAutoStartEnabled
	
	$foundProvider = $False
	
	$providerName = "SpApiPreload"
	
	foreach( $autoStartProvider in $autoStartProviders )
	{
		foreach($provider in $autoStartProvider.Collection)
		{
			if ( $provider.name -eq $providerName )
			{
				$foundProvider = $True
				break;
			}
		}
	}
	
	if ( $foundProvider -ne $True )
	{
		Log-Message "Autostart provider '$providerName' not found. Creating..."
		
		$path = Get-InstallPath $settings
		$apiPath = Join-PathEx $path "SpApi\\Bin\\EDC.SoftwarePlatform.WebApi.dll"
		
		$assembly = [System.Reflection.Assembly]::LoadFrom("$apiPath")
		
		$assemblyType = $assembly.GetType( "EDC.SoftwarePlatform.WebApi.AppPreload" );
		
		$typeName = $assemblyType.AssemblyQualifiedName
		
		Add-WebConfiguration -filter /system.applicationHost/serviceAutoStartProviders -Value @{name="$providerName"; type="$typeName"}
	}
	else
	{
		Log-Message "AutoStart Provider '$providerName' already exists."
	}
}

# Create the web application.
function Create-Application($settings, $application)
{
	$virtualDirPath = $settings.webServer.virtualDirectoryPath
	
	$appPath = $application.virtualPath
	
	$path = "$($virtualDirPath.TrimEnd('/'))/$($appPath.TrimStart('/'))"
	
	$installPath = Get-InstallPath $settings
	$currentPath = Get-CurrentPath
	
	$virtualPath = $path.Replace( '/', '\' )
	
	if ($application.relativeTo -eq "current")
	{
		$physicalPath = Get-AbsolutePath $currentPath $application.physicalPath
	}
	else
	{
		$physicalPath = Get-AbsolutePath $installPath $application.physicalPath
	}
	
	$appName = $application.virtualPath.TrimStart('/')
	
	Log-Message "Searching for web application '$path'..."
	
	$app = Get-WebApplication -Site "Default Web Site" -Name "$path"
	
	$virtualPath = "IIS:\Sites\Default Web Site$virtualPath"
	
	Log-Message "Virtual Path: $virtualPath"
	
	if ( $app -eq $Null )
	{
		Log-Message "Creating web application '$path'..."
		
		$app = New-Item "$virtualPath" -physicalPath "$physicalPath" -type Application
		
		Log-Message "Web application '$appName' successfully created."
	}
	else
	{
		Log-Message "Found existing web application '$appName'."
	}
	
	$preloadEnabled = $application.preloadEnabled
	
	if ( $app.preloadEnabled -ne $preloadEnabled )
	{
		Log-Message "Setting application '$path' preload enabled '$preloadEnabled'..."
		Set-ItemProperty "$virtualPath" -name preloadEnabled -value $preloadEnabled
	}
	
	$serviceAutoStartEnabled = $application.serviceAutoStartEnabled
	
	if ( $app.serviceAutoStartEnabled -ne $serviceAutoStartEnabled )
	{
		Log-Message "Setting application '$path' service auto start enabled '$serviceAutoStartEnabled'..."
		Set-ItemProperty "$virtualPath" -name serviceAutoStartEnabled -value $serviceAutoStartEnabled
	}
	
	$serviceAutoStartProvider = $application.serviceAutoStartProvider
	
	if ( ( $serviceAutoStartProvider -ne $Null ) -And ( $app.serviceAutoStartProvider -ne $serviceAutoStartProvider ) )
	{
		Log-Message "Setting application '$path' service auto start provider '$serviceAutoStartProvider'..."
		Set-ItemProperty "$virtualPath" -name serviceAutoStartProvider -value $serviceAutoStartProvider
	}
	
	$applicationPoolName = $application.appPool
	
	if ( ( $applicationPoolName -ne $Null ) -And ( $app.applicationPoolName -ne $applicationPoolName ) )
	{
		Log-Message "Assigning application '$path' to application pool '$applicationPoolName'..."
		Set-ItemProperty "$virtualPath" -name applicationPool -value $applicationPoolName
	}
}

# Start each application pool.
function Start-AppPool($settings)
{
	$appPools = $settings.webServer.appPools
	
	$foundAppPool = $False

	foreach ($appPool in $appPools)
	{
		$name = $appPool.name
		
		if ( ( Get-WebAppPoolState $name ).Value -ne 'Started')
		{
			Log-Message "Starting application pool '$name'..."
			
			Start-WebAppPool -Name $name
			
			Log-Message "Application pool '$name' successfully started."
		}
		else
		{
			Log-Message "Application pool '$name' already started."
		}
	}
}

# Configure the web server
function Configure-WebServer($settings)
{
	Create-AppPool $settings
	Configure-VirtualDirectory $settings
	Create-AutoStartProvider $settings
	
	foreach ($application in $settings.webServer.applications)
	{
		Create-Application $settings $application
	}
	
	Start-AppPool $settings
}

# Removes the softwareplatform.pristine file
function Remove-SoftwarePlatformConfigPristine($settings)
{
	$path = Get-InstallPath
	
	Log-Message $path
	
	$configFilePath = Join-PathEx $path 'Configuration\SoftwarePlatform.pristine'
	
	Log-Message $configFilePath
	if (Test-Path $configFilePath)
	{
		Log-Message "Removing pristine SoftwarePlatform configuration file at '$configFilePath'..."
		
		Remove-Item $configFilePath
		
		Log-Message "Pristine SoftwarePlatform configuration file successfully removed."
	}
}

# Selectively purge the installation directory.
function Upgrade-InstallationDirectory($settings)
{
	$path = Get-InstallPath $settings
	
	Log-Message "Backing up configuration files..."
	
	$backupDir = Join-PathEx $path 'Backup'
	
	if ( -Not ( Test-Path $backupDir ) )
	{
		$newItem = New-Item $backupDir -type directory
	}
	
	#Copy-Item $filePath $spApiPath -Force
	
	Log-Message "Purging..."
	
	
	#Remove-Item -Recurse -Path $path -Exclude Log,OutgoingEmail,PlatformUploadFiles,PlatformFileRepos,Database,Configuration,Redis -Verbose –Force
	Get-ChildItem $path -exclude Log,OutgoingEmail,PlatformUploadFiles,PlatformFileRepos | foreach ($_) {
		Write-Host $_.FullName
		Remove-Item $_.FullName -Force -Recurse -Exclude "*.mdf","*.ldf","SoftwarePlatform.config","redis.windows.conf"
	}
}

# Create the Log folder if it does not already exist
function Configure-Folder($settings, $path, $security)
{
	$installPath = Get-InstallPath $settings
	
	$folderPath = Get-AbsolutePath $installPath $path
	
	if ( -Not ( Test-Path $folderPath ) )
	{
		Log-Message "Creating folder '$folderPath'..."
		
		$newItem = New-Item $folderPath -type directory
	}
	else
	{
		Log-Message "Folder '$folderPath' already exists."
	}
	
	if ( $security -ne $Null )
	{
		foreach($securityEntry in $security)
		{
			Set-FolderSecurity $folderPath $securityEntry.identity $securityEntry.access
		}
	}
}

# Configure the database folder.
function Configure-DatabaseFolder($settings)
{
	$databasePath = $settings.database.path
	
	if (!$databasePath)
	{
		$databasePath = 'Database'
	}
	
	$server = $deploymentSettings.database.server
	
	if ( Is-LocalAddress $server )
	{
		$installPath = Get-InstallPath $settings
		$path = Get-AbsolutePath $installPath $databasePath
		
		Log-Message "Local database path is '$path'"
		
		Configure-Folder $deploymentSettings $path $deploymentSettings.database.folderSecurity
	}
	else
	{
		if ( !( Get-IsPathRooted $databasePath ) )
		{
			Log-Warning "Remote database path does not contain a root. Setting root to 'C:\'"
			
			$path = Get-AbsolutePath 'C:\\' $databasePath
			
			$settings.database.path = $path
			
			$root = Get-PathRoot $path
			
			if ( -Not ( String-IsNullOrEmpty $root ) )
			{
				$drive = $root.Substring(0,1)
				
				$remainder = $path.Substring( $root.Length )
								
				$uncPath = "\\\\$server\\$drive$\\$remainder"
				
				Log-Message "Testing remote database folder at '$uncPath'..."
				
				Try
				{
					if ( -Not ( Test-Path $uncPath ) )
					{
						$newItem = New-Item $uncPath -type directory
					}
					
					$security = $settings.database.folderSecurity
					
					if ( $security -ne $Null )
					{
						foreach($securityEntry in $security)
						{
							Set-FolderSecurity $uncPath $securityEntry.identity $securityEntry.access
						}
					}
				}
				Catch
				{
					Log-Warning "Failed to create remote database folder at '$uncPath'. Please create it manually."
				}
			}
		}
		
		
		Log-Message "Remote database path is '$databasePath'"
	}
}

# Upgrade the installed server.
function Upgrade-SoftwarePlatform($settings, $redistributableFile)
{
	Unzip-SoftwarePlatform $settings $redistributableFile
	
	Remove-SoftwarePlatformConfigPristine $settings
}

# Register performance counters
function Register-PerformanceCounters($settings)
{
	Register-AccessControlPerformanceCounters $settings
	Register-AccessControlCacheInvalidationPerformanceCounters $settings
	Register-AccessControlPermissionPerformanceCounters $settings
	Register-WorkflowPerformanceCounters $settings
	Register-ReportsPerformanceCounters $settings
	Register-PlatformTracePerformanceCounters $settings
	Register-CachePerformanceCounters $settings
	Register-EntityInfoServicePerformanceCounters $settings
	
	Add-UserToPerformanceCounterGroup $settings
}

# Register performance counters
function Unregister-PerformanceCounters($settings)
{
	Unregister-AccessControlPerformanceCounters $settings
	Unregister-AccessControlCacheInvalidationPerformanceCounters $settings
	Unregister-AccessControlPermissionPerformanceCounters $settings
	Unregister-WorkflowPerformanceCounters $settings
	Unregister-ReportsPerformanceCounters $settings
	Unregister-PlatformTracePerformanceCounters $settings
	Unregister-CachePerformanceCounters $settings
	Unregister-EntityInfoServicePerformanceCounters $settings
}

# Register the Access Control Performance performance counters
function Register-AccessControlPerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
	
	Add-AverageTimer32 "Average Check Duration" "Average duration of access control checks" $counterCollection
	Add-RatePerSecond32 "Check Rate" "Number of access control checks per second" $counterCollection
	Add-NumberOfItems64 "Check Count" "Total number of security checks" $counterCollection
	Add-PercentageRate "Cache Hit Percentage" "Percentage of access control cache checks that were resolved from the cache" $counterCollection
	
	Create-PerformanceCounter "Access Control" "Software platform access control counters." $categoryType $counterCollection
}

# Register the Access Control Cache Invalidation performance counters
function Register-AccessControlCacheInvalidationPerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance
	
	Add-RatePerSecond32 "Cache Invalidation Rate" "Cache invalidation rate broken down by type." $counterCollection
	Add-NumberOfItems64 "Cache Invalidation Count" "Total cache invalidations broken down by type." $counterCollection
	
	Create-PerformanceCounter "Access Control Cache Invalidation" "Software platform access control counters." $categoryType $counterCollection
}

# Register the Access Control Permission performance counters
function Register-AccessControlPermissionPerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance
	
	Add-RatePerSecond32 "Check Rate" "Access control checks broken down by permission" $counterCollection
	Add-NumberOfItems64 "Count" "Total number of access control checks broken down by permission" $counterCollection
	
	Create-PerformanceCounter "Access Control Permission Checks" "Software platform access control counters." $categoryType $counterCollection
}

# Register the Workflow performance counters
function Register-WorkflowPerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
	
	Add-AverageTimer32 "Queue Duration" "Average duration a workflow queues until started." $counterCollection
	Add-AverageTimer32 "Run Duration" "Average duration of workflow runs until paused or completed" $counterCollection
	Add-RatePerSecond32 "Run Rate" "Number of workflow runs per second" $counterCollection
	Add-NumberOfItems64 "Run Count" "Total number of workflow runs" $counterCollection
	Add-RatePerSecond32 "Trigger Rate" "Number of workflows triggered per second by entity creates of updates" $counterCollection
	Add-RatePerSecond32 "Scheduler fire Rate" "Number of schedule events triggered per second" $counterCollection
	Add-AverageTimer32 "Schedule job Duration" "Average duration of scheduler job" $counterCollection
	
	Create-PerformanceCounter "Workflow" "Software platform workflow counters." $categoryType $counterCollection
}

# Register the Reports performance counters
function Register-ReportsPerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
	
	Add-CodeBlock "Run" "report runs" $counterCollection
	
	Create-PerformanceCounter "Reports" "Software platform reports counters." $categoryType $counterCollection
}

# Register the Platform Trace performance counters
function Register-PlatformTracePerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
	
	Add-RatePerSecond32 "Entity Cache Hit Rate" "Number of cache hits per second" $counterCollection
	Add-RatePerSecond32 "Entity Cache Miss Rate" "Number of entity cache misses per second" $counterCollection
	Add-RatePerSecond32 "Entity Save  Rate" "Number of entity saves per second" $counterCollection
	Add-RatePerSecond32 "Ipc Broadcast Rate" "Number of Ipc broadcasts per second" $counterCollection
	
	Create-PerformanceCounter "PlatformTrace" "Software platform platform trace counters." $categoryType $counterCollection
}

# Register the Cache performance counters
function Register-CachePerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance
	
	Add-NumberOfItems64 "Cache Size" "Number of entries in the cache" $counterCollection
	Add-PercentageRate  "Hit Rate" "Cache hit rate" $counterCollection
	Add-NumberOfItems64 "Total Hits" "Total hits over the life of the cache" $counterCollection
	Add-NumberOfItems64 "Total Misses" "Total misses over the life of the cache" $counterCollection
	
	Create-PerformanceCounter "Caches" "Software platform cache counters." $categoryType $counterCollection
}

# Register the Entity Info Service performance counters
function Register-EntityInfoServicePerformanceCounters($settings)
{
	$counterCollection = New-Object System.Diagnostics.CounterCreationDataCollection
	$categoryType = [System.Diagnostics.PerformanceCounterCategoryType]::SingleInstance
	
	Add-CodeBlock "Request" "entity info service requests" $counterCollection
	
	Create-PerformanceCounter "EntityInfo" "Software platform platform trace counters." $categoryType $counterCollection
}

# Unregister the Access Control Performance performance counters
function Unregister-AccessControlPerformanceCounters($settings)
{
	Delete-PerformanceCounter "Access Control"
}

# Unregister the Access Control Cache Invalidation performance counters
function Unregister-AccessControlCacheInvalidationPerformanceCounters($settings)
{
	Delete-PerformanceCounter "Access Control Cache Invalidation"
}

# Unregister the Access Control Permission performance counters
function Unregister-AccessControlPermissionPerformanceCounters($settings)
{
	Delete-PerformanceCounter "Access Control Permission Checks"
}

# Unregister the Workflow performance counters
function Unregister-WorkflowPerformanceCounters($settings)
{
	Delete-PerformanceCounter "Workflow"
}

# Unregister the Reports performance counters
function Unregister-ReportsPerformanceCounters($settings)
{
	Delete-PerformanceCounter "Reports"
}

# Unregister the Platform Trace performance counters
function Unregister-PlatformTracePerformanceCounters($settings)
{
	Delete-PerformanceCounter "PlatformTrace"
}

# Unregister the Cache performance counters
function Unregister-CachePerformanceCounters($settings)
{
	Delete-PerformanceCounter "Caches"
}

# Unregister the Entity Info Service performance counters
function Unregister-EntityInfoServicePerformanceCounters($settings)
{
	Delete-PerformanceCounter "EntityInfo"
}

# Add the specified user to the specified group
function Add-UserToPerformanceCounterGroup($settings)
{
	$user = $settings.performanceCounters.user
	$domain = $settings.performanceCounters.domain
	$group = $settings.performanceCounters.groupName
	
	if ( String-IsNullOrEmpty $user )
	{
		Log-Warning "Missing performance counter user name."
		return
	}
	
	if ( String-IsNullOrEmpty $domain )
	{
		Log-Warning "Missing performance counter domain name."
		return
	}
	
	if ( String-IsNullOrEmpty $group )
	{
		$group = "Performance Log Users"
	}
	
	$machineName = [System.Environment]::MachineName
	
	$userPath = "WinNT://$domain/$user"
	$groupPath = "WinNT://$machineName/$group,group"
	$directoryEntry = [ADSI]$groupPath
	
	Log-Message "Checking user path '$userPath' against group '$groupPath'..."
	
	$members = @($directoryEntry.psbase.Invoke("Members"))
	
	$exists = ($members | foreach {([ADSI]$_).InvokeGet("Name")}) -contains $user
	
	if ( -Not ( $exists ) )
	{
		Log-Message "Adding user '$user' to group '$group'..."
		$directoryEntry.psbase.Invoke( "Add", ([ADSI]$userPath).path)
	}
	else
	{
		Log-Message "User '$user' already belongs to group '$group'."
	}
}

##############
# PlatformConfigure REPL functions
##############

# Install bootstrap
function Install-Bootstrap($process, $settings)
{
	Log-Message "Bootstrapping install..."
	
	Install-CoreApplications-Impl $platformConfigureProcess $deploymentSettings $true
	
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	$process.StandardInput.WriteLine('installBootstrap'); 		#command
	$process.StandardInput.WriteLine($server);                  #server
	$process.StandardInput.WriteLine($catalog);                 #database
	
	Process-WaitForDone $process
}

# Upgrade bootstrap
function Upgrade-Bootstrap($process, $settings)
{
	Log-Message "Bootstrapping upgrade..."
	
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	# note: install-bootstrap called BEFORE core apps are upgraded
	$process.StandardInput.WriteLine('upgradeBootstrap');       #command
	$process.StandardInput.WriteLine($server); 					#server
	$process.StandardInput.WriteLine($catalog);					#database
	
	Process-WaitForDone $process
		
	Install-CoreApplications-Impl $platformConfigureProcess $deploymentSettings $false
	
	# Upgrade Core in Global
	Upgrade-App $process $settings "Global" "7062aade-2e72-4a71-a7fa-a412d20d6f01"
	
	# Upgrade Console in Global
	Upgrade-App $process $settings "Global" "34ff4d95-70c6-4ae8-8f6f-38d88546d4c4"
	
	# Upgrade Core Data in Global
	Upgrade-App $process $settings "Global" "abf12077-6fa5-43da-b608-b8b7514d07bb"
	
	# Upgrade System in Global
	Upgrade-App $process $settings "Global" "3e67c1c4-aa65-4a9f-95d2-908a9f3614d1"
	
}

# Turn integration mode on
function Switch-IntegrationTestModeOn($process)
{
    Log-Message 'Turn on Integation test mode'
    $process.StandardInput.WriteLine('intgTestModeOn'); 				#command

	Process-WaitForDone $process
}

# Grant the 'CanModify' permission on the specified application.
function Grant-CanModifyApplication($process, $settings, $tenantName, $appName)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $appName ) ) ) )
	{
		Log-Message "Grant can modify application for tenant '$tenantName' application '$appName'"
		$process.StandardInput.WriteLine('grantModifyApp'); 			#command
		$process.StandardInput.WriteLine($tenantName); 					#tenant
		$process.StandardInput.WriteLine($appName); 					#app	
		$process.StandardInput.WriteLine($server); 						#server
		$process.StandardInput.WriteLine($catalog); 					#database
		$process.StandardInput.WriteLine(''); 							#dbUser
		$process.StandardInput.WriteLine(''); 							#dbPassword
		
		Process-WaitForDone $process
	}
}

# Import the specified application to the application library.
function Import-App($process, $settings, $appPath, $bootstrap)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( -Not ( [string]::IsNullOrEmpty( $appPath ) ) )
	{
		Log-Message "Importing application file '$appPath'..."
		if ( $bootstrap )
		{
			$process.StandardInput.WriteLine('bootstrapApp'); 			#command
		}
		else
		{
			$process.StandardInput.WriteLine('importApp'); 				#command
		}
		$process.StandardInput.WriteLine($appPath); 					#package
		$process.StandardInput.WriteLine($server); 						#server
		$process.StandardInput.WriteLine($catalog); 					#database
		$process.StandardInput.WriteLine(''); 							#dbUser
		$process.StandardInput.WriteLine(''); 							#dbPassword

		Process-WaitForDone $process
	}
}

# Import the specified application to the application library.
function Get-Tenants($process, $settings)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	Log-Message "Searching for tenants..."
	$process.StandardInput.WriteLine('listTenants'); 				#command
	$process.StandardInput.WriteLine($server); 						#server
	$process.StandardInput.WriteLine($catalog); 					#database
	$process.StandardInput.WriteLine(''); 							#dbUser
	$process.StandardInput.WriteLine(''); 							#dbPassword

	[System.Collections.ArrayList]$tenants = Process-WaitForDone $process $True

	# Ensure 'Global' is at the front of the list.
	$tenants.Insert(0, "Global") | Out-Null
	
	$tenantNames = $tenants -join ", "
	
	Log-Message "Found tenants '$tenantNames'"
	
	return ,$tenants
}

# Deploy the specified application to the specified tenant.
function Deploy-App($process, $settings, $tenantName, $appName)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $appName ) ) ) )
	{
		Log-Message "Deploying application '$appName' to tenant '$tenantName'..."
		$process.StandardInput.WriteLine('deployApp'); 					#command
		$process.StandardInput.WriteLine($tenantName); 					#tenant
		$process.StandardInput.WriteLine($appName); 					#app
		$process.StandardInput.WriteLine(''); 							#ver
		$process.StandardInput.WriteLine('True'); 						#updateStats
		$process.StandardInput.WriteLine('True'); 						#disableFts
		$process.StandardInput.WriteLine($server); 						#server
		$process.StandardInput.WriteLine($catalog); 					#database
		$process.StandardInput.WriteLine(''); 							#dbUser
		$process.StandardInput.WriteLine(''); 							#dbPassword

		Process-WaitForDone $process
	}
}

# Upgrades the specified application for the specified tenant.
function Upgrade-App($process, $settings, $tenantName, $appName)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $appName ) ) ) )
	{
		Log-Message "Upgrading application '$appName' for tenant '$tenantName'..."
		$process.StandardInput.WriteLine('upgradeApp'); 				#command
		$process.StandardInput.WriteLine($tenantName); 					#tenant
		$process.StandardInput.WriteLine($appName); 					#app
		$process.StandardInput.WriteLine($server); 						#server
		$process.StandardInput.WriteLine($catalog); 					#database
		$process.StandardInput.WriteLine(''); 							#dbUser
		$process.StandardInput.WriteLine(''); 							#dbPassword

		Process-WaitForDone $process
	}
}

# Provision a new tenant with the specified tenant name.
function Provision-Tenant($process, $settings, $tenantName)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) )
	{
		Log-Message "Provisioning tenant '$tenantName'..."
		$process.StandardInput.WriteLine('provisionTenant'); 			#command
		$process.StandardInput.WriteLine($tenantName); 					#tenant
		$process.StandardInput.WriteLine('False'); 						#all	
		$process.StandardInput.WriteLine('True');						#updateStats
		$process.StandardInput.WriteLine('True'); 						#disableFts
		$process.StandardInput.WriteLine($server); 						#server
		$process.StandardInput.WriteLine($catalog);						#database
		$process.StandardInput.WriteLine(''); 							#dbUser
		$process.StandardInput.WriteLine(''); 							#dbPassword
		
		Process-WaitForDone $process
	}
}

# Activate feature switches.
function Install-FeatureSwitches($process, $settings)
{
	$tenantName = $settings.defaultTenant.name
	$featureSwitches = $settings.featureSwitches
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $featureSwitches ) ) ) )
	{
		Log-Message "Turn on feature switches: '$featureSwitches'"

		if ($settings.featureSwitches)
		{
			$process.StandardInput.WriteLine('fs');						#command
			$process.StandardInput.WriteLine($tenantName);				#tenant
			$process.StandardInput.WriteLine($server);					#server
			$process.StandardInput.WriteLine($catalog);					#database
			$process.StandardInput.WriteLine('');						#user
			$process.StandardInput.WriteLine('');						#password
			$process.StandardInput.WriteLine($featureSwitches);			#list
			$process.StandardInput.WriteLine('');						#feature
			$process.StandardInput.WriteLine('');						#set
			Process-WaitForDone $process
		}
	}
}

# Create a system restore point for the specified tenant
function Create-RestorePoint($process, $settings, $tenant, $message, $userDefined, $systemUpgrade, $revertTo)
{
	$server = $settings.database.server
	$catalog = $settings.database.catalog
	
	if ( -Not ( [string]::IsNullOrEmpty( $tenant ) ) )
	{
		Log-Message "Creating System Restore Point for tenant '$tenant'..."

		$process.StandardInput.WriteLine('crp');					#command
		$process.StandardInput.WriteLine($tenant);					#tenant
		$process.StandardInput.WriteLine($message);					#context
		$process.StandardInput.WriteLine($userDefined);				#userDefined
		$process.StandardInput.WriteLine($systemUpgrade);			#systemUpgrade
		$process.StandardInput.WriteLine($revertTo);				#revertTo
		$process.StandardInput.WriteLine($server);					#server
		$process.StandardInput.WriteLine($catalog);					#database
		Process-WaitForDone $process
	}
}

# Start platform configure in REPL mode.
function Start-PlatformConfigure($settings)
{
	$path = Get-PlatformConfigurePath $settings

	$psi = New-Object System.Diagnostics.ProcessStartInfo;
	$psi.FileName = $path
	$psi.Arguments = '-repl'
	$psi.UseShellExecute = $false;
	$psi.RedirectStandardInput = $true;
	$psi.RedirectStandardOutput = $true;
	$psi.RedirectStandardError = $true;

	$p = [System.Diagnostics.Process]::Start($psi);

	Start-Sleep -s 1
	
	Log-Message "PlatformConfigure.exe REPL started (PID: $($p.Id))"

	return $p
}

# Terminate the REPL PlatformConfigure process.
function End-PlatformConfigure($process)
{
	Log-Message "Shutting down PlatformConfigure.exe REPL"

	$processId = $process.Id

	Stop-Process $processId
	
	Log-Message "PlatformConfigure.exe REPL shutdown (PID: $processId)"
}

# Wait for the REPL command to complete.
function Process-WaitForDone($process, $captureOutput)
{
	$outputArray = New-Object System.Collections.ArrayList
	
	while (!$process.HasExited)
	{
		$output = $process.StandardOutput.ReadLine( );

		if ( $output -eq 'DONE' )
		{
			break;
		}
		else
		{
			if ( -Not ( String-IsNullOrEmpty $output ) )
			{
				if ( $output -match "^ERROR: .*" )
				{
					Log-Error $output
				}
				else
				{
					if ( ( $output -match "^Copying .* data\.\.\.$" ) -Or ( $output -match "^Committing .* data\.\.\.$" ) -Or ( $output -match "^Processing .*\.\.\.$" ) )
					{
						if ( $lastOutput )
						{
							if ($captureOutput)
							{
								if ( -Not ( $output -match "^Repl .*$" ) )
								{
									$outputArray.Add($lastOutput) | Out-Null
								}
							}
							else
							{
								Log-Message $lastOutput
							}
							
							$lastOutput = $Null
						}

						# Throw away these lines.
					}
					else
					{
						if ( ( $output -match "^Copying .* rows$" ) -Or ( $output -match "^Writing .* rows$" ) )
						{
							$lastOutput = $output
						}
						else
						{
							if ( $lastOutput )
							{
								if ($captureOutput)
								{
									if ( -Not ( $output -match "^Repl .*$" ) )
									{
										$outputArray.Add($lastOutput) | Out-Null
									}
								}
								else
								{
									Log-Message $lastOutput
								}
							
								$lastOutput = $Null
							}
							
							if ($captureOutput)
							{
								if ( -Not ( $output -match "^Repl .*$" ) )
								{
									$outputArray.Add($output) | Out-Null
								}
							}
							else
							{
								Log-Message $output
							}
						}
					}
				}
			}
		}
	}
	
	if ($captureOutput)
	{
		return ,$outputArray
	}
}

##############
# Database functions
##############

# Restore the database from a backup
function Restore-DatabaseBackup($settings, $restore)
{
    if ($restore -eq $True)
    {
		$path = Get-CurrentPath
		$installPath = Get-InstallPath $settings
		$databaseInstallFolder = $settings.database.installPath
		
		if ( [string]::IsNullOrEmpty( $databaseInstallFolder ) )
		{
			$databaseInstallFolder = 'Database'
		}
		
		$databaseInstallFolder = Get-AbsolutePath $installPath $databaseInstallFolder
		
		$catalog = $settings.database.catalog
		
		$datFile = Join-PathEx $databaseInstallFolder ($catalog + '_Dat.mdf')
		$logFile = Join-PathEx $databaseInstallFolder ($catalog + '_Log.ldf')
		
        $backupFile = Join-PathEx ($path) ($catalog + '.bak')
		
        Log-Message "Restoring database '$catalog' from '$backupFile'..."
		
		Log-Message "Setting database '$catalog' to single user mode"
		& sqlcmd -E -S localhost -b -Q "ALTER DATABASE $catalog SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
		
		Log-Message "Moving MDF file to '$datFile'..."
		Log-Message "Moving LDF file to '$logFile'..."
        & sqlcmd -E -S localhost -b -Q "RESTORE DATABASE $catalog FROM DISK = '$backupFile' WITH REPLACE, MOVE '$catalog' TO '$datFile', MOVE '$($catalog + '_log')' TO '$logFile'"
		
		Log-Message "Setting database '$catalog' to multi user mode"
		& sqlcmd -E -S localhost -b -Q "ALTER DATABASE $catalog SET MULTI_USER"

		$backupFileRepoZip = Join-PathEx ($path) ($catalog + '_FileRepo.zip')

		if ((Test-Path $backupFileRepoZip))
		{
			$restorePath = Get-AbsolutePath $installPath $settings.repositories.basePath
			Log-Message "Restore file repository from '$backupFileRepoZip' to '$restorePath'."
			
			if (-Not (Test-Path $backupFileRepoZip))
			{
				$dir = mkdir $restorePath -force
			}
			
			Unzip $backupFileRepoZip $restorePath
		}
    }
}

# Backup the database
function Create-DatabaseBackup($settings, $backup)
{
    if ($backup -eq $True)
    {
		$databaseName = $settings.database.catalog
        $backupFile = Join-PathEx (Get-CurrentPath) ($databaseName + '.bak')
        Log-Message "Backing up database '$databaseName' to '$backupFile'."
        & sqlcmd -E -S localhost -b -Q "BACKUP DATABASE $databaseName TO DISK = '$backupFile' WITH INIT"

		$backupFileRepoZip = Join-PathEx (Get-CurrentPath) ($settings.database.catalog + '_FileRepo.zip')

		Log-Message "Backup file repository to '$backupFileRepoZip'."

		if ((Test-Path $backupFileRepoZip))
		{
			Log-Message "Deleting existing '$backupFileRepoZip'"
			del $backupFileRepoZip
		}

		$path = Get-InstallPath $settings
		$sourcePath = Get-AbsolutePath $path $settings.repositories.basePath
		
		Zip $sourcePath $backupFileRepoZip
    }
}

# Drop the database
function Delete-Database($settings)
{
	$databaseName = $settings.database.catalog
	
	$persist = $settings.database.persist
	
	if ( $persist -ne $True )
	{
		Log-Message "Setting database '$databaseName' to single user mode"
		$output = & sqlcmd -E -S $settings.database.server -Q "ALTER DATABASE $databaseName SET SINGLE_USER WITH ROLLBACK IMMEDIATE"

		Log-Message "Dropping database '$databaseName'"
		$output = & sqlcmd -E -S $settings.database.server -Q "DROP DATABASE $databaseName"
	}
}

##############
# Security functions
##############

# Set the permissions on a folder.
function Set-FolderSecurity($path, $identity, $access)
{
	if ( String-IsNullOrEmpty $path )
	{
		Log-Warning "Invalid path '$path' specified in 'Set-FolderSecurity'."
		return
	}

	if ( String-IsNullOrEmpty $identity )
	{
		Log-Warning "Invalid identity '$identity' specified in 'Set-FolderSecurity'"
		return
	}

	if ( String-IsNullOrEmpty $access )
	{
		Log-Warning "Invalid access '$access' specified in 'Set-FolderSecurity'"
		return
	}
	
	Log-Message "Setting '$access' for identity '$identity' on folder '$path'..."

	Try
	{
		$acl = Get-Acl $path

		$ar = New-Object System.Security.AccessControl.FileSystemAccessRule("$identity", "$access", 'ContainerInherit,ObjectInherit', 'None', 'Allow')

		$acl.SetAccessRule($ar)

		Set-Acl $path $acl
	}
	Catch
	{
		Log-Warning "Failed to set '$access' for identity '$identity' on folder '$path'. $_"
	}
}

# Set the permissions on a file.
function Set-FileSecurity($path, $identity, $access)
{
	if ( String-IsNullOrEmpty $path )
	{
		Log-Warning "Invalid path '$path' specified in 'Set-FolderSecurity'."
		return
	}
	
	if ( String-IsNullOrEmpty $identity )
	{
		Log-Warning "Invalid identity '$identity' specified in 'Set-FolderSecurity'"
		return
	}

	if ( String-IsNullOrEmpty $access )
	{
		Log-Warning "Invalid access '$access' specified in 'Set-FolderSecurity'"
		return
	}
	
	Log-Message "Setting '$access' for identity '$identity' on file '$path'..."

	$acl = Get-Acl $path
	
	$ar = New-Object System.Security.AccessControl.FileSystemAccessRule("$identity", "$access", 'Allow')
	
	$acl.SetAccessRule($ar)
	
	Set-Acl $path $acl
}

# Impersonate the specified credentials
function Impersonate-Identity($domain, $username, $password)
{
    Try
    {
		Log-Message "Impersonating $domain\\$username with password '$password'..."
	
		$logonUserSignature =
@'
[DllImport( "advapi32.dll" )]
public static extern bool LogonUser( string lpszUserName, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken );
'@

		$advApi32 = Add-Type -MemberDefinition $logonUserSignature -Name "AdvApi32" -Namespace "PsInvoke.NativeMethods" -PassThru
	
		[IntPtr]$userToken = [IntPtr]::Zero
	
		if(!$advApi32::LogonUser( $username, $domain, $password, 8, 0, [ref]$userToken) )
		{
			throw (new-object System.ComponentModel.Win32Exception( [System.Runtime.InteropServices.Marshal]::GetLastWin32Error() ) )
		}

		$identity = New-Object Security.Principal.WindowsIdentity $userToken
	
		$context = $identity.Impersonate()
	
		$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent( ).Name
		
		Log-Message "Successfully impersonated identity '$currentIdentity'."

		$ctx = @{"context" = $context; "token" = $userToken; "identity" = $currentIdentity }

		return $ctx
    }
    Catch
    {
        Log-Error "Failed to impersonate $domain\\$username with password '$password'."
    }

}

# Undo the impersonation context.
function Undo-Identity($ctx)
{
    Log-Message "Reverting identity $($ctx.identity)..."

    $closeHandleSignature =
@'
[DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
public static extern bool CloseHandle( IntPtr handle );
'@

    $kernel32 = Add-Type -MemberDefinition $closeHandleSignature -Name "Kernel32" -Namespace "PsInvoke.NativeMethods" -PassThru

    if (!$ctx)
    {
        Log-Warning "No impersonation context specified."
        return
    }

    if ($ctx.context)
    {
        $ctx.context.Undo();
	    $ctx.context.Dispose();
    }

    if ( $ctx.token )
    {
        $kernel32::CloseHandle( $ctx.token ) | Out-Null
    }
}

##############
# Performance Counters
##############

# Create the performance counter.
function Create-PerformanceCounter($categoryName, $categoryHelp, $categoryType, $counterCollection)
{
	$name = "Software Platform $categoryName"

	Delete-PerformanceCounter $categoryName
	
	Log-Message "Creating performance counter category '$name'..."
	
	[System.Diagnostics.PerformanceCounterCategory]::Create($name, $categoryHelp, $categoryType, $counterCollection) | Out-Null
}

# Delete the performance counter
function Delete-PerformanceCounter($categoryName)
{
	$name = "Software Platform $categoryName"

	$categoryExists = [System.Diagnostics.PerformanceCounterCategory]::Exists($name)
	
	if ($categoryExists)
	{
		Log-Message "Found existing performance category '$name'. Deleting..."
		
		[System.Diagnostics.PerformanceCounterCategory]::Delete($name)
	}
}

# Add an average timer 32 counter.
function Add-AverageTimer32($counterName, $counterHelp, $counterCollection)
{
	$counter = New-Object System.Diagnostics.CounterCreationData
	$counter.CounterName = $counterName
	$counter.CounterHelp = $counterHelp
	$counter.CounterType = "AverageTimer32"
	$counterCollection.Add($counter) | Out-Null
	
	$counterNameBase = "$counterName Base"
	
	$counterBase = New-Object System.Diagnostics.CounterCreationData
	$counterBase.CounterName = $counterNameBase
	$counterBase.CounterHelp = $counterHelp
	$counterBase.CounterType = "AverageBase"
	$counterCollection.Add($counterBase) | Out-Null
}

# Add a Rate-per-second 32 counter
function Add-RatePerSecond32($counterName, $counterHelp, $counterCollection)
{
	$counter = New-Object System.Diagnostics.CounterCreationData
	$counter.CounterName = $counterName
	$counter.CounterHelp = $counterHelp
	$counter.CounterType = "RateOfCountsPerSecond32"
	$counterCollection.Add($counter) | Out-Null
}

# Add a Number of Items counter
function Add-NumberOfItems64($counterName, $counterHelp, $counterCollection)
{
	$counter = New-Object System.Diagnostics.CounterCreationData
	$counter.CounterName = $counterName
	$counter.CounterHelp = $counterHelp
	$counter.CounterType = "NumberOfItems64"
	$counterCollection.Add($counter) | Out-Null
}

# Add a Percentage Rate counter
function Add-PercentageRate($counterName, $counterHelp, $counterCollection)
{
	$counter = New-Object System.Diagnostics.CounterCreationData
	$counter.CounterName = $counterName
	$counter.CounterHelp = $counterHelp
	$counter.CounterType = "SampleFraction"
	$counterCollection.Add($counter) | Out-Null
	
	$counterNameBase = "$counterName Base"
	
	$counterBase = New-Object System.Diagnostics.CounterCreationData
	$counterBase.CounterName = $counterNameBase
	$counterBase.CounterHelp = $counterHelp
	$counterBase.CounterType = "SampleBase"
	$counterCollection.Add($counterBase) | Out-Null
}

# Add a Code Block counter
function Add-CodeBlock($counterName, $counterHelp, $counterCollection)
{
	Add-RatePerSecond32 "$counterName Rate" "Number of $counterHelp per second" $counterCollection
	Add-NumberOfItems64 "$counterName Count" "Total number of $counterHelp" $counterCollection
	Add-AverageTimer32 "$counterName Duration" "Average duration of $counterHelp" $counterCollection
}

##############
# Start Menu functions
##############

#Creates the start menu
function Create-StartMenu($settings)
{
	$installPath = Get-InstallPath $settings
	
	$virtualDirPath = $settings.webServer.virtualDirectoryPath
	
	$subFolder = $Null
	
	if ($virtualDirPath -ne "/")
	{
		$subFolder = $virtualDirPath.TrimStart('/').Replace( '/', '\' )
	}
	
	$logViewerPath = Join-Path $installPath "Tools\LogViewer.exe"
	StartMenu-Add "Log Viewer" $logViewerPath $subFolder
	
	$applicationManagerPath = Join-Path $installPath "Tools\ApplicationManager.exe"
	StartMenu-Add "Application Manager" $applicationManagerPath $subFolder
	
	$tenantDiffPath = Join-Path $installPath "Tools\TenantDiffTool.exe"
	StartMenu-Add "Tenant Diff Tool" $tenantDiffPath $subFolder
}

#Deletes the start menu
function Delete-StartMenu($settings)
{
	$softwarePlatformPath = Join-Path $Env:ProgramData 'Microsoft\Windows\Start Menu\Programs\ReadiNow\Software Platform'
	
	$virtualDirPath = $settings.webServer.virtualDirectoryPath
	
	$fullPath = $softwarePlatformPath
	
	if ($virtualDirPath -ne "/")
	{
		$subFolder = $virtualDirPath.TrimStart('/').Replace( '/', '\' )
		$fullPath = Join-Path $fullPath $subFolder
	}
	
	if ( Test-Path $fullPath )
	{
		Log-Message "Removing start menu folder '$fullPath'..."
		Remove-Item $fullPath -recurse
	}
	
	$parentPath = Split-Path -Parent $fullPath
	$leaf = Split-Path -Leaf $parentPath
	
	while ( ( $leaf -ne 'Programs' ) )
	{
		if ( Test-Path $parentPath )
		{
			$childCount = @(Get-ChildItem $parentPath).Count
				
			if ( $childCount -le 0 )
			{
				Log-Message "Removing start menu folder '$parentPath'..."
				Remove-Item $parentPath -recurse
			}
		}
		
		$parentPath = Split-Path -Parent $parentPath
		$leaf = Split-Path -Leaf $parentPath
	}
}

#Adds an entry to the start menu
function StartMenu-Add($name, $path, $subFolder)
{
	if (String-IsNullOrEmpty $name)
	{
		Log-Warning "Failed to create start menu item. Name not specified"
		return
	}
	
	if ( -Not ( Test-Path $path ) )
	{
		Log-Warning "Failed to create start menu item with name '$name'. Target path does not exist."
		return
	}
	
	$programsPath = Join-Path $Env:ProgramData 'Microsoft\Windows\Start Menu\Programs'
	
	if ( -Not ( Test-Path $programsPath ) )
	{
		return
	}
	
	$readiNowPath = Join-Path $programsPath 'ReadiNow'
	
	if ( -Not ( Test-Path $readiNowPath ) )
	{
		Log-Message "Creating start menu folder '$readiNowPath'..."
		$newItem = New-Item $readiNowPath -type directory
	}
	
	$softwarePlatformPath = Join-Path $readiNowPath 'Software Platform'
	
	if ( -Not ( Test-Path $softwarePlatformPath ) )
	{
		Log-Message "Creating start menu folder '$softwarePlatformPath'..."
		$newItem = New-Item $softwarePlatformPath -type directory
	}
	
	$fullPath = $softwarePlatformPath
	
	if ($subFolder -ne $Null) {
		$fullPath = Join-Path $fullPath $subFolder
		
		if ( -Not ( Test-Path $fullPath ) )
		{
			Log-Message "Creating start menu folder '$fullPath'..."
			$newItem = New-Item $fullPath -type directory
		}
	}
	
	$linkName = "$name" + ".lnk"
	
	$linkPath = $fullPath + "\" + $linkName
	
	if ( -Not ( Test-Path $linkPath ) )
	{
		Log-Message "Creating start menu short cut '$name' with target '$path'..."
	
		$objShell = New-Object -ComObject ("WScript.Shell")
		$objShortCut = $objShell.CreateShortcut($linkPath)
		$objShortCut.TargetPath = $path
		$objShortCut.Save()
	}
	else
	{
		Log-Message "Start menu short cut '$name' already exists."
	}
}

##############
# Utility functions
##############

# Logs the specified message.
function Log-Message($message)
{
	Write-Host "$(([System.DateTime]::Now).ToString()) - $message"
	
	if ( $global:logFile -ne $Null )
	{
		Add-Content $global:logFile "$(([System.DateTime]::Now).ToString()) - $message"
	}
}

# Logs the specified warning message.
function Log-Warning($message)
{
	Write-Warning "$(([System.DateTime]::Now).ToString()) - $message"
	
	if ( $global:logFile -ne $Null )
	{
		Add-Content $global:logFile "WARNING: $(([System.DateTime]::Now).ToString()) - $message"
	}
}

# Logs the specified error message.
function Log-Error($message)
{
	Write-Error "$(([System.DateTime]::Now).ToString()) - $message"
	
	if ( $global:logFile -ne $Null )
	{
		Add-Content $global:logFile "ERROR: $(([System.DateTime]::Now).ToString()) - $message"
	}
}

# Unzips the specified zip file to the specified path
function Unzip($zipFile, $outPath)
{
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

# Unzips the specified zip file to the specified path overwriting individual files
function UnzipEx($zipFile, $outPath)
{
	$zip = [System.IO.Compression.ZipFile]::OpenRead($zipFile)
	
	foreach ($zipItem in $zip.Entries)
	{
		$path = Join-PathEx $outPath $zipItem.FullName
		
		$dir = [System.IO.Path]::GetDirectoryName($path)
		
		if ( -Not ( Test-Path $dir ) )
		{
			Log-Message "Creating folder '$dir'..."
			
			$newItem = New-Item $dir -type directory
		}
			
		if ( -Not ( Test-Path $path ) )
		{
			[System.IO.Compression.ZipFileExtensions]::ExtractToFile($zipItem, $path, $True)
		}
		else
		{
			Log-Message "File '$path' already exists. Skipping..."
		}
	}
}

# Zips the files in the specified path to the specified zip file.
function Zip($path, $zipFile)
{
	[System.IO.Compression.ZipFile]::CreateFromDirectory($path, $zipFile)
}

# Determines whether the server name is local or remote.
function Is-LocalAddress($serverName)
{
	if (String-IsNullOrEmpty $serverName)
	{
		Log-Warning "Invalid server name specified '$serverName' in Is-LocalAddress."
		return
	}
	
	if ( ( $serverName -eq 'localhost' ) -Or ( $serverName -eq '127.0.0.1' ) -Or ( $serverName -eq '::1' ) -Or ( $serverName -eq '.' ) )
	{
		return $True
	}
	
	$hostName = [System.Net.Dns]::GetHostName( )
	
	$localHost = [System.Net.Dns]::GetHostEntry( $hostName )
	$hostEntry = [System.Net.Dns]::GetHostEntry( $serverName )
	
	if ( $localHost.HostName -eq $hostEntry.HostName )
	{
		return $True
	}
	
	foreach( $address in $hostEntry.AddressList )
	{
		$isLoopBack = [System.Net.IPAddress]::IsLoopback( $address )
		$index = [System.Array]::IndexOf( $localHost.AddressList, $address )

		if ( $isLoopBack -Or ( $index -ne -1 ) )
		{
			return $True
		}
	}
}

# Returns true if the specified string is null or empty.
function String-IsNullOrEmpty($string)
{
	return [string]::IsNullOrEmpty( $string )
}
