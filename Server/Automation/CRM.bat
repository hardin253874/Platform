call PlatformConfigure.bat -importApp -package "I:\Shared\Applications\CRM\CRM.db"
call PlatformConfigure.bat -importApp -package "I:\Shared\Applications\CRM\CRM Data.db"
call PlatformConfigure.bat -deployApp -tenant EDC -app "CRM"
call PlatformConfigure.bat -deployApp -tenant EDC -app "CRM Data"
iisreset