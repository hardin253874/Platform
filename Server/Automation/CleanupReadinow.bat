@echo off
ECHO %date% %time% - Nuking the 'Readinow Monitor' service...
sc delete "ReadiNowMonSvc"

ECHO %date% %time% - Setting 'Readinow' database to single user mode...
sqlcmd -E -S localhost -Q "ALTER DATABASE Readinow SET SINGLE_USER WITH ROLLBACK IMMEDIATE"

ECHO %date% %time% - Dropping 'Readinow' database...
sqlcmd -E -S localhost -Q "DROP DATABASE Readinow"

ECHO %date% %time% - Cleaning up IIS certificate associations...
netsh http delete sslcert ipport=0.0.0.0:8000
netsh http delete sslcert ipport=0.0.0.0:8001
netsh http delete sslcert ipport=0.0.0.0:8010
netsh http delete sslcert ipport=0.0.0.0:8011

REM Reset IIS for good measure and start the sites
ECHO %date% %time% - Resetting IIS...
iisreset /restart > nul

%windir%\system32\inetsrv\appcmd start sites "Default Web Site"

ECHO %date% %time% - The old 'Readinow' stuff is goneski!