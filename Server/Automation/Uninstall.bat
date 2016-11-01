@echo off

IF /I .%1==. (
	PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File %~d0%~p0%~n0.ps1
) ELSE (
	PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File %~d0%~p0%~n0.ps1 -environmentSettingFile Environment\%1.json
)