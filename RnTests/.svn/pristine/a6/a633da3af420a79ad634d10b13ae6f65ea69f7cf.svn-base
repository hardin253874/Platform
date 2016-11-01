@echo off
ECHO Set working directory
pushd %~dp0
ECHO Deleting existing combined file
del combined.csv
setlocal ENABLEDELAYEDEXPANSION
REM set count to 1
set cnt=1
REM for each file that matches *.csv
for /r remoteResults %%i in (suitemetrics*.csv) do (
REM if count is 1 it's the first time running
  if !cnt!==1 (
REM push the entire file complete with header into combined.csv - this will also create combined.csv
    for /f "delims=" %%j in ('type "%%i"') do echo %%j >> combined.csv
REM otherwise, make sure we're not working with the combined file and
  ) else if %%i NEQ combined.csv (
REM push the file without the header into combined.csv
    for /f "skip=1 delims=" %%j in ('type "%%i"') do echo %%j >> combined.csv
  )
REM increment count by 1
  set /a cnt+=1
)
