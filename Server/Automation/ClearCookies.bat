@ECHO OFF

set Cookies=C:\Users\%USERNAME%\AppData\Roaming\Microsoft\Windows\Cookies

if exist %Cookies% (
    for /F "delims=" %%a in ('findstr /m entdata "%Cookies%\*.txt"') do del /F /Q "%%a"
    for /F "delims=" %%a in ('findstr /m localhost "%Cookies%\*.txt"') do del /F /Q "%%a"
    for /F "delims=" %%a in ('findstr /m syd1 "%Cookies%\*.txt"') do del /F /Q "%%a"
)

for /D %%u in ("C:\Users\%USERNAME%\AppData\Local\Google\Chrome\User Data\*") do if exist "%%u\Cookies" (del /F /Q "%%u\Cookies" "%%u\Cookies-journal")

for /D %%u in ("C:\Users\%USERNAME%\AppData\Roaming\Mozilla\Firefox\Profiles\*") do if exist "%%u\cookies.sqlite" (del /F /Q "%%u\cookies.sqlite")