@echo off
pushd %~dp0
setlocal

if exist bin goto build
mkdir bin

:Build

REM Find the most recent 32bit MSBuild.exe on the system. Require v16.0 (installed with VS2019) or later.
REM Use `vswhere` for the search because it can find all VS installations.
set vswhere="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %vswhere% (
  set vswhere="%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
)
if not exist %vswhere% (
  REM vswhere.exe not in normal locations; check the Path.
  for %%X in (vswhere.exe) do (
    set vswhere="%%~$PATH:X"
  )
)
if not exist %vswhere% (
  echo Could not find vswhere.exe. Please run this from a Visual Studio developer prompt.
  goto BuildFail
)

REM We're fine w/ any .NET SDK newer than 2.1.500 but also need a 2.1.x runtime. Microsoft.Net.Core.Component.SDK.2.1
REM actually checks for only the runtime these days.
set InstallDir=
for /f "usebackq tokens=*" %%i in (`%vswhere% -version 16 -latest -prerelease -products * ^
    -requires Microsoft.Component.MSBuild ^
    -requires Microsoft.NetCore.Component.SDK ^
    -requires Microsoft.Net.Core.Component.SDK.2.1 ^
    -property installationPath`) do (
  set InstallDir="%%i"
)

if exist %InstallDir%\MSBuild\Current\Bin\MSBuild.exe (
  set MSBuild=%InstallDir%\MSBuild\Current\Bin\MSBuild.exe
) else (
  echo Could not find MSBuild.exe. Please install the VS2019 BuildTools component or a workload that includes it.
  goto BuildFail
)

REM Configure NuGet operations to work w/in this repo i.e. do not pollute system packages folder.
REM Note this causes two copies of packages restored using packages.config to land in this folder e.g.
REM StyleCpy.5.0.0/ and stylecop/5.0.0/.
set "NUGET_PACKAGES=%CD%\packages"

REM Are we running in a local dev environment (not on CI)?
if DEFINED CI (set Desktop=false) else if DEFINED TEAMCITY_VERSION (set Desktop=false) else (set Desktop=true)

if "%1" == "" goto BuildDefaults

%MSBuild% Runtime.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
    /fl /fileLoggerParameters:LogFile=bin\msbuild.log;Verbosity=Normal /consoleLoggerParameters:Summary /t:%*
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
%MSBuild% Runtime.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
    /fl /fileLoggerParameters:LogFile=bin\msbuild.log;Verbosity=Normal /consoleLoggerParameters:Summary
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildFail
echo.
echo *** BUILD FAILED ***
popd
endlocal
exit /B 999

:BuildSuccess
echo.
echo **** BUILD SUCCESSFUL ***
popd
endlocal
exit /B 0
