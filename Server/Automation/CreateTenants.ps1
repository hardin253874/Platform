#requires -Version 3.0

param (
	[string]$tenants = "T001,T002",
	[string]$applications = "Test Solution,Foster University,Foster University DATA"
)

$ErrorActionPreference = "Stop"

# Starts the PlaformConfigure process in REPL mode. This allows multiple actions to be
# performed on the one process saving startup/shutdown time. Autofac in particular causes
# the process startup time to be significant due to assembly scanning and factory construction.
function Start-PlatformConfigure
{
	$path = "C:\Program Files\ReadiNow\SoftwarePlatform\Tools\PlatformConfigure.exe"
	
	if ( -Not ( Test-Path $path ) )
	{
		Write-Error "Failed to locate PlatformConfigure.exe at '$path'"
	}
	else
	{
		Write-Host "Found PlatformConfigure.exe at '$path'"
	}

	$psi = New-Object System.Diagnostics.ProcessStartInfo;
	$psi.FileName = $path
	$psi.Arguments = '-repl'
	$psi.UseShellExecute = $false;
	$psi.RedirectStandardInput = $true;
	$psi.RedirectStandardOutput = $true;
	$psi.RedirectStandardError = $true;

	$p = [System.Diagnostics.Process]::Start($psi);

	Start-Sleep -s 1
	
	Write-Host "PlatformConfigure.exe REPL started (PID: $($p.Id))"

	return $p
}

# Ends the PlatformConfigure process using a hard quit.
function End-PlatformConfigure($process)
{
	$processId = $process.Id

	Stop-Process $processId
	
	Write-Host "PlatformConfigure.exe REPL shutdown (PID: $processId)"
}

# Waits for the PlatformConfigure process to output a 'DONE' command indicating that the
# previous operation has completed.
function Process-WaitForDone($process)
{
	while (!$process.HasExited)
	{
		$output = $process.StandardOutput.ReadLine( );

		if ( $output -eq 'DONE' )
		{
			break;
		}
	}
}

# Provisions a new tenant by invoking the 'provisionTenant' operation on the running
# PlatformConfigure process. All required switches are sent via standard input.
function Provision-Tenant($process, $tenantName)
{
	if ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) )
	{
		$process.StandardInput.WriteLine('provisionTenant'); #command
		$process.StandardInput.WriteLine($tenantName); #tenant
		$process.StandardInput.WriteLine('False'); #all	
		$process.StandardInput.WriteLine('True'); #updateStats
		$process.StandardInput.WriteLine('True'); #disableFts
		$process.StandardInput.WriteLine(''); #server
		$process.StandardInput.WriteLine(''); #database
		$process.StandardInput.WriteLine(''); #dbUser
		$process.StandardInput.WriteLine(''); #dbPassword
		
		Process-WaitForDone $process
	}
}

# Deploys the specified application to the specified tenant by invoking the 'deployApp'
# operation on the PlatformConfigure process. All required switches are sent via standard
# input.
function Deploy-App($process, $tenantName, $appName)
{
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $appName ) ) ) )
	{
		$process.StandardInput.WriteLine('deployApp'); #command
		$process.StandardInput.WriteLine($tenantName); #tenant
		$process.StandardInput.WriteLine($appName); #app
		$process.StandardInput.WriteLine(''); #ver
		$process.StandardInput.WriteLine('True'); #updateStats
		$process.StandardInput.WriteLine('True'); #disableFts
		$process.StandardInput.WriteLine(''); #server
		$process.StandardInput.WriteLine(''); #database
		$process.StandardInput.WriteLine(''); #dbUser
		$process.StandardInput.WriteLine(''); #dbPassword

		Process-WaitForDone $process
	}
}

# Grants modify access to the specified application for the specified tenanta by invoking
# the 'deployApp' operation on the PlatformConfigure process. All required switches are
# sent via standard input.
function Grant-CanModifyApplication($process, $tenantName, $appName)
{
	if ( ( -Not ( [string]::IsNullOrEmpty( $tenantName ) ) ) -and ( -Not ( [string]::IsNullOrEmpty( $appName ) ) ) )
	{
		$process.StandardInput.WriteLine('grantModifyApp'); #command
		$process.StandardInput.WriteLine($tenantName); #tenant
		$process.StandardInput.WriteLine($appName); #app	
		$process.StandardInput.WriteLine(''); #server
		$process.StandardInput.WriteLine(''); #database
		$process.StandardInput.WriteLine(''); #dbUser
		$process.StandardInput.WriteLine(''); #dbPassword
		
		Process-WaitForDone $process
	}
}

# Provisions the specified tenants and deploys the specified applications to each.
# All applications are also granted the Modify permission to each tenant.
function Provision-Tenants($process, $tenants, $applications)
{
	$tenantArray = $tenants.Split(",")
	$applicationsArray = $applications.Split(",")
	
	foreach( $tenant in $tenantArray )
	{
		Write-Host "Provisioning tenant '$tenant'"
		Provision-Tenant $process $tenant
		
		foreach( $app in $applicationsArray )
		{
			Write-Host "Deploying application '$app' to tenant '$tenant'"
			Deploy-App $process $tenant $app
		
			Write-Host "Granting modify on application '$app' for tenant '$tenant'"
			Grant-CanModifyApplication $process $tenant $app
		}
	}
}

# Main script entry point.
try
{
	if ( [string]::IsNullOrEmpty( $tenants ) )
	{
		Write-Host "No tenant specified."
		return
	}
	
	$platformConfigureProcess = Start-PlatformConfigure
	
	try
	{
		Provision-Tenants $platformConfigureProcess $tenants $applications
	}
	finally
	{
		End-PlatformConfigure $platformConfigureProcess
	}
}
catch
{
    $ErrorActionPreference = "Continue"
    Write-Error $_
    Exit 1
}