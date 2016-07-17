@echo off
set url=http://zyeemcom.vicp.io:8090/
set apikey=67EB78E5-3E50-4D56-B03A-CF07B5B89BCD

%~dp0nuget.exe push publish\*.nupkg -s %url% %apikey%

pause