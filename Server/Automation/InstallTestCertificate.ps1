#
# Filename: InstallTestCertificate.ps1
#
#     Date: 13-June-2013
#
#   Author: Martin Kalitis
#
# Comments:
# =========
#
# To run from a command prompt type:
#
#	powershell -command .\InstallTestCertificate.ps1
#
# This may fail if the execution policy on the machine does not allow running of 
# PS scripts (which is the default) to fix this type the following into the powershell 
# prompt and click yes to the alert message.
#
# 	Set-ExecutionPolicy -ExecutionPolicy Unrestricted
# Refer to http://technet.microsoft.com/library/hh847748.aspx for further details
#
#
# save our current location
$initialDirectory = $PWD
# Extract the FQDN for the local machine
$iishost = [System.Net.Dns]::GetHostByName(($env:computerName)) | select hostname
# If we already have a certificate, remove it
$existingSelfsignedCert = get-childitem cert:\LocalMachine\my -dnsname $iishost.HostName
if ($existingSelfsignedCert -ne $null) { get-childitem cert:\LocalMachine\my -dnsname $iishost.HostName | Remove-Item }
# Create a new self signed certioficate in the local machine personal (MY) store
New-SelfSignedCertificate -DnsName $iishost.HostName -CertStoreLocation cert:\LocalMachine\My
# Create a password for the certificate to be exported
$certificatepassword = convertto-securestring "Gr8P@ssW0rD!" -asplaintext -force
get-childitem cert:\LocalMachine\my -dnsname $iishost.HostName  | Export-PfxCertificate -FilePath $initialDirectory\ExportedCert.pfx -Password $certificatepassword
# Do we have certs in our root that exist?
$existingSelfsignedCert = get-childitem cert:\LocalMachine\Root -dnsname $iishost.HostName
# If so nuke them before importing the new one
if ($existingSelfsignedCert -ne $null) { get-childitem cert:\LocalMachine\Root -dnsname $iishost.HostName | Remove-Item }
# Import the certificate into the trusted root
Import-PfxCertificate -FilePath $initialDirectory\ExportedCert.pfx cert:\localMachine\Root -Password $certificatepassword
# Delete the file
Remove-Item $initialDirectory\ExportedCert.pfx
# Get any current binding
$currentBinding = Get-WebBinding -Name "Default Web Site" -IPAddress "*" -Port 443 -Protocol https
# If the binding exists, delete it
if ($currentBinding -ne $null) { Remove-WebBinding -Name "Default Web Site" -IPAddress "*" -Port 443 -Protocol https }
# Create a binding for port 443 in IIS (used for https)
New-WebBinding -Name "Default Web Site" -IPAddress "*" -Port 443 -Protocol https
# Associate our certificate against it
# Switch to the SSL binding area in IIS
cd IIS:\SslBindings
# Get any binding against the 443 (default SSL) port
$currentSslBinding = Get-Item 0.0.0.0!443 -ErrorAction SilentlyContinue
# if we have a binding, nuke it
if ($currentSslBinding -ne $null) { Remove-Item 0.0.0.0!443 }
# Create the new association
get-childitem cert:\LocalMachine\my -dnsname $iishost.HostName | new-item 0.0.0.0!443
# Game Over Man!
cd $initialDirectory