@set NUGET_PACK_OPTS= -Version 4.0.0-rc3-160601
@set NUGET_PACK_OPTS= %NUGET_PACK_OPTS% -OutputDirectory Publish

%~dp0nuget.exe pack %~dp0CuteAnt.IdentityModel.Protocol.Extensions.nuspec %NUGET_PACK_OPTS%
%~dp0nuget.exe pack %~dp0CuteAnt.IdentityModel.Tokens.Jwt.nuspec %NUGET_PACK_OPTS%
