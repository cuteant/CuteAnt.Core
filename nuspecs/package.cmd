SET /P VERSION_SUFFIX=Please enter version-suffix (can be left empty): 

dotnet "pack" "..\src\CuteAnt.Extensions.Logging.Sources" -c "Release" -o "." --version-suffix "%VERSION_SUFFIX%"
dotnet "pack" "..\src\Nito.AsyncEx.Tasks.Sources" -c "Release" -o "." --version-suffix "%VERSION_SUFFIX%"

%~dp0nuget.exe pack %~dp0CuteAnt.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.AsyncEx.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.SharpSerializer.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.Extensions.Logging.NLog.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.Extensions.Logging.Serilog.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.IdentityModel.Protocol.Extensions.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0CuteAnt.IdentityModel.Tokens.Jwt.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0MySql.Data.nuspec -OutputDirectory Publish
%~dp0nuget.exe pack %~dp0System.Data.SQLite.nuspec -OutputDirectory Publish
