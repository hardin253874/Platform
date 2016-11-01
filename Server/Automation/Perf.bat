call PlatformConfigure.bat -importApp -package "I:\Shared\Applications\Performance Test\Performance Test 1.41.db"
call PlatformConfigure.bat -deployApp -tenant EDC -app "Performance Test"
iisreset

