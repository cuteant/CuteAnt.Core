@set NUGET_PACK_OPTS= -Version 2.0.0-rc3-161101
@set NUGET_PACK_OPTS= %NUGET_PACK_OPTS% -OutputDirectory Publish

%~dp0nuget.exe pack %~dp0CuteAnt.Extensions.Logging.NLog.nuspec %NUGET_PACK_OPTS%
%~dp0nuget.exe pack %~dp0CuteAnt.Extensions.Logging.Serilog.nuspec %NUGET_PACK_OPTS%
