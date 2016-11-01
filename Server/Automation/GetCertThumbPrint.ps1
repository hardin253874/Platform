$certns = "System.Security.Cryptography.X509Certificates"

$serialNumber = $args[0]
$issuer = $args[1]

if ([System.String]::IsNullOrEmpty($serialNumber))
{
	write-host "The certificate serial number was not specified.";	
	exit
}

$x509store = New-Object "$certns.X509Store"("My","LocalMachine")
$x509store.Open("ReadOnly,OpenExistingOnly");
$certificates = $x509store.Certificates.Find("FindBySerialNumber", $serialNumber.ToString(), $TRUE);
$thumbprint = ""

# Find the thumb print of our certificate
foreach ($certificate in $certificates)
{  
  if ($certificate.Issuer.equals($issuer.ToString()))
  {
     $thumbprint = $certificate.Thumbprint;
	 write-host $thumbprint
	 
	 break;
  }
}