@pushd %~d0%~p0
PowerShell.exe -NoProfile -NoLogo -ExecutionPolicy Unrestricted -File .\Install.ps1 Environment\CleanPM.json
@popd
@pause