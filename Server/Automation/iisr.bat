@rem Recycle app pool - much quicker than iisreset
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:"SoftwarePlatformAppPool"
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:"SoftwarePlatformAppPool"