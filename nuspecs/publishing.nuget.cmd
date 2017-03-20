@echo off
set url=https://www.nuget.org/api/v2/package
set apikey=9eadb97d-1e16-4b28-bcb8-1929257f2588

%~dp0nuget.exe push publish\*.nupkg -Source %url% -ApiKey %apikey%

pause