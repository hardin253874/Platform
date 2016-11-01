@ECHO OFF

SET WORKPATH="C:\Program Files (x86)\Microsoft Visual Studio 10.0\vc"

SET SERVERNAME=%1

IF "%1"=="" goto error1

CD %WORKPATH% 

makecert -n "CN=%SERVERNAME%" -r -sv %SERVERNAME%.pvk %SERVERNAME%.cer

makecert -crl -n "CN=%SERVERNAME%" -r -sv %SERVERNAME%.pvk %SERVERNAME%.crl

makecert -sk %SERVERNAME% -iv %SERVERNAME%.pvk -n "CN=%SERVERNAME%" -ic %SERVERNAME%.cer -sr LocalMachine -ss My -sky exchange -pe

goto END

:error1
echo missing Development Server Name  e.g. syd1wks01.entdata.local
REM goto help
goto END

:help
echo E.g. CreateCertificate [Development  Server Name]
goto END



:END
