@echo off

IF /I .%1==. (
	PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File %~d0%~p0%~n0.ps1
) ELSE (
	IF /I .%2==. (
		PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File %~d0%~p0%~n0.ps1 -environmentSettingFile Environment\%1.json
	) ELSE (
		PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File %~d0%~p0%~n0.ps1 -environmentSettingFile Environment\%1.json -redistributableFile %2
	)
)