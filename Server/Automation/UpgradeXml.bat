@echo Clearing hash file
del ..\ApplicationCache\Solution.hash

@echo Generating apps from config
..\Build\Debug\Tools\PlatformConfigure.exe -tc -config ..\EDC.ReadiNow.Common\Config\Solution.xml -output ..\ApplicationCache -srcHash ..\EDC.ReadiNow.Common\Config\Solution.hash -dstHash ..\ApplicationCache\Solution.hash -ver 1.0.*.* > nul
@if not errorlevel 0 goto abort

@echo Importing apps into library
..\Build\Debug\Tools\PlatformConfigure.exe -importApp -package ..\ApplicationCache\coreSolution.xml
@if not errorlevel 0 goto abort
..\Build\Debug\Tools\PlatformConfigure.exe -importApp -package ..\ApplicationCache\consoleSolution.xml
@if not errorlevel 0 goto abort

@echo Updating statistics
sqlcmd -S localhost -Q "exec sp_updatestats" > nul

@echo Upgrading tenant
..\Build\Debug\Tools\PlatformConfigure.exe -upgradeApp -tenant EDC -app "ReadiNow Core"
@if not errorlevel 0 goto abort
..\Build\Debug\Tools\PlatformConfigure.exe -upgradeApp -tenant EDC -app "ReadiNow Console"
@if not errorlevel 0 goto abort

@echo Restarting web
taskkill -f -im w3wp.exe

@echo Done.
goto end

:abort
@echo Aborted!

:end
