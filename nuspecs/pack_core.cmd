@set NUGET_PACK_OPTS= -Version 2.0.0-rc3-160710
@set NUGET_PACK_OPTS= %NUGET_PACK_OPTS% -OutputDirectory Publish

%~dp0nuget.exe pack %~dp0CuteAnt.nuspec %NUGET_PACK_OPTS%
%~dp0nuget.exe pack %~dp0CuteAnt.AsyncEx.nuspec %NUGET_PACK_OPTS%
%~dp0nuget.exe pack %~dp0CuteAnt.SharpSerializer.nuspec %NUGET_PACK_OPTS%
