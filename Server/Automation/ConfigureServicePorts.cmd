@ECHO ON
SETLOCAL

REM - ***************** Notes *****************
REM - Run this script in Visual Studio x64 command prompt running as Administrator 
REM - to configure the ports used by ReadiNow product.
REM - When promoted for a password enter: Passw0rd!
REM - ***************** Notes *****************

REM - A hardcoded serial number which we can use to find our created certificate.
REM - makecert.exe does not provide any ability to specify the friendly name.
SET CERTSERIALNUMBER="1769472304"
REM - The number above in hex
SET CERTSERIALNUMBERHEX="69780130"

SET ISSUER=ReadiNow Dev Certification Authority
SET ISSUERCN=CN=%ISSUER%
SET HOSTFQDN=
SET THUMBPRINT=

REM - Get the host name
powershell.exe -ExecutionPolicy RemoteSigned -NoLogo -File .\GetLocalHostName.ps1 1> hostname.txt
FOR /f %%n in (hostname.txt) DO SET HOSTFQDN=%%n
DEL hostname.txt

IF "%HOSTFQDN%"=="" GOTO ErrorGettingHostName
IF "%HOSTFQDN%"=="+" GOTO ErrorGettingHostName

ECHO Creating self-sign certificates for %HOSTFQDN%

REM ECHO Creating ReadiNow Dev Certification Authority root certificate
REM certutil.exe -delstore "Root" "%ISSUER%"
REM makecert.exe -sv %HOSTFQDN%.pvk -cy authority -r %HOSTFQDN%.cer -a sha1 -n "%ISSUERCN%" -ss root -sr localmachine

ECHO Creating ReadiNow Dev Certification Authority application certificate from trusted root certificate
certutil.exe -delstore "My" %CERTSERIALNUMBERHEX%
makecert.exe -# %CERTSERIALNUMBER% -iv ReadiNowRootCert.pvk -ic ReadiNowRootCert.cer -cy end -pe -n CN="%HOSTFQDN%" -eku 1.3.6.1.5.5.7.3.1 -ss my -sr localmachine -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12

powershell.exe -ExecutionPolicy RemoteSigned -NoLogo -File .\GetCertThumbPrint.ps1 %CERTSERIALNUMBERHEX% "%ISSUERCN%" 1> thumbprint.txt
FOR /f %%t in (thumbprint.txt) DO SET THUMBPRINT=%%t
DEL thumbprint.txt

IF "%THUMBPRINT%"=="" GOTO ErrorGettingThumbPrint
IF "%THUMBPRINT%"=="+" GOTO ErrorGettingThumbPrint

ECHO Binding application ports to use created certificate with certhash %THUMBPRINT%

netsh http delete sslcert ipport=0.0.0.0:8000
netsh http add sslcert ipport=0.0.0.0:8000 certhash=%THUMBPRINT% appid={a46cf890-596d-4fda-8fb8-bdb1fcb35ddf} 

netsh http delete sslcert ipport=0.0.0.0:8001
netsh http add sslcert ipport=0.0.0.0:8001 certhash=%THUMBPRINT% appid={a46cf890-596d-4fda-8fb8-bdb1fcb35ddf} 

netsh http delete sslcert ipport=0.0.0.0:8010
netsh http add sslcert ipport=0.0.0.0:8010 certhash=%THUMBPRINT% appid={a46cf890-596d-4fda-8fb8-bdb1fcb35ddf} 

netsh http delete sslcert ipport=0.0.0.0:8011
netsh http add sslcert ipport=0.0.0.0:8011 certhash=%THUMBPRINT% appid={a46cf890-596d-4fda-8fb8-bdb1fcb35ddf} 

REM - Required for IIS SSL support
netsh http delete sslcert ipport=0.0.0.0:443
netsh http add sslcert ipport=0.0.0.0:443 certhash=%THUMBPRINT% appid={a46cf890-596d-4fda-8fb8-bdb1fcb35ddf} 

REM - Set IIS default web to use this certificate
%windir%\system32\inetsrv\appcmd set site "Default Web Site" /-bindings.[protocol='https',bindingInformation='*:443:']
%windir%\system32\inetsrv\appcmd set site "Default Web Site" /+bindings.[protocol='https',bindingInformation='*:443:']

iisreset

%windir%\system32\inetsrv\appcmd start sites "Default Web Site"

ECHO Restarting services

REM - Stop the ReadiNow monitor service 
"%SystemRoot%\System32\net" stop ReadiNowMonSvc
REM - Stop the ReadiNow service 
"%SystemRoot%\System32\net" stop ReadiNowSvc

REM - Start the ReadiNow monitor service 
"%SystemRoot%\System32\net" start ReadiNowMonSvc
REM - Start the ReadiNow service 
"%SystemRoot%\System32\net" start ReadiNowSvc

GOTO END

:ErrorGettingHostName
echo An error occurred. Failed to get the host name of the current machine.
goto END

:ErrorGettingThumbPrint
echo An error occurred. Failed to get the thumbprint of the created certificate.
goto END

:help
echo E.g. ConfigureServicePorts [Development  Server Name]
goto END

:END