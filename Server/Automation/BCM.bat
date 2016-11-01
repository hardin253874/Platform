call PlatformConfigure.bat -importApp -package "I:\Shared\Applications\ReadiBCM\ReadiBCM.db"
call PlatformConfigure.bat -importApp -package "I:\Shared\Applications\ReadiBCM\ReadiBCMData.db"
call PlatformConfigure.bat -deployApp -tenant EDC -app "ReadiBCM"
call PlatformConfigure.bat -deployApp -tenant EDC -app "ReadiBCM Data"
iisreset