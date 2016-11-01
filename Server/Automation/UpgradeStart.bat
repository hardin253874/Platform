@ECHO OFF

CALL Upgrade.bat

iisreset

"%SystemRoot%\System32\net" start SoftwarePlatform

