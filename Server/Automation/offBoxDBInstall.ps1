#requires -Version 3.0

Param(
    [string]$environmentSettingFile=$(throw "Missing environment settings file"),
	[string]$redistributableFile=$Null
	
)

$ErrorActionPreference = "Stop"
$progressPreference = "silentlyContinue"

##############
# Setting functions
##############

# Unzips the specified zip file to the specified path
function Unzip($zipFile, $outPath)
{
	[System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
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

# Gets the installation folder. This is read from the settings file.
function Get-InstallPath($settings)
{
	# Is there an install path specified in the setting file?
	$path = $settings.installPath
	
	Write-Host "path = $path"
	
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
	
	return $path
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


# Logs the specified message.
function Log-Message($message)
{
	Write-Host "$(([System.DateTime]::Now).ToString()) - $message"
	Add-Content $global:logFile "$(([System.DateTime]::Now).ToString()) - $message"
}

# Returns true if the specified string is null or empty.
function String-IsNullOrEmpty($string)
{
	return [string]::IsNullOrEmpty( $string )
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

# Ensure the log file exists.
function Create-LogFile
{
	if ( Test-Path $global:logFile )
	{
		del $global:logFile
	}
	
	$file = New-Item $global:logFile -type file
	
	Set-FileSecurity $global:logFile 'Everyone' 'FullControl'
}

# Returns the absolute path for the specified path string.
function Get-FullPath($path)
{
	return [System.IO.Path]::GetFullPath($path)
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
    $path = Split-Path -Parent $Script:MyInvocation.MyCommand.Definition
	
	# Ensure the path ends with a trailing backslash
	$path = $path.TrimEnd("\") + "\"
	
	return $path
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

# Resolve the path to the specified log file.
function Resolve-LogFilePath($filePath)
{
	if ( -Not (Get-IsPathRooted $filePath ) )
	{
		$filePath = Join-PathEx (Get-CurrentPath) $filePath
	}
	
	return $filePath
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
		Log-Message "Inspecing database '$($database.Name)'..."
		
		if ( $database.Name -eq $catalog )
		{
			$found = $True
			break
		}
	}
	
	if ($found -eq $True)
	{
		Log-Message "Found existing database '$catalog' database on server '$server'. Upgrading..."
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
		
		$login = $settings.database.user
		$domain = $settings.database.domain
		
		if ($domain)
		{
			$login = "$domain\$login"
		}
		
		Log-Message "Creating database login '$login'..."
		& sqlcmd -E -S $server -d "master" -b -Q "DECLARE @login NVARCHAR(MAX) = NULL; SELECT @login = name FROM sys.server_principals WHERE LOWER(name) = LOWER(N'$login'); IF (@login IS NULL) CREATE LOGIN [$login] FROM WINDOWS"
		
		Log-Message "Granting View Server State to database login '$login'..."
		& sqlcmd -E -S $server -d "master" -b -Q "GRANT VIEW SERVER STATE TO [$login]"
		
		Log-Message "Creating database user for windows login '$login'..."
		& sqlcmd -E -S $server -d $catalog -b -Q "DECLARE @user NVARCHAR(MAX) = NULL; SELECT @user = name FROM sys.database_principals WHERE LOWER(name) = LOWER(N'$login'); IF (@user IS NOT NULL) EXEC ('ALTER USER [' + @user + '] WITH LOGIN = [$login]') ELSE CREATE USER [$login] FOR LOGIN [$login]"
		
		Log-Message "Assigning database login '$login' to role '$role'..."
		& sqlcmd -E -S $server -d $catalog -b -Q "exec sp_addrolemember N'$role', N'$login'"
	}
}

Try
{
    $deploymentSettings = Read-EnvironmentSettingFile $environmentSettingFile
	
    $global:logFile = Resolve-LogFilePath $deploymentSettings.installLogFile
	
    Create-LogFile
	
	#Validate-EnvironmentSettings $deploymentSettings

	Log-Message 'Installing SoftwarePlatform...'
	Log-Message "Using environment settings file '$environmentSettingFile'"
	Log-Message "Argument redistributableFile = '$redistributableFile'"
	Log-Message "Current Machine: $([Environment]::MachineName)"
	Log-Message "Current User: $([Environment]::UserName)"
	
	Clear-InstallationDirectory $deploymentSettings
	
	# Decompress installation files
	Unzip-SoftwarePlatform $deploymentSettings $redistributableFile

	Install-Database $deploymentSettings

	Log-Message 'SoftwarePlatform successfully installed.'
}
Catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}