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

set InstallDir=
for /f "usebackq tokens=*" %%i in (`%vswhere% -version 16 -latest -prerelease -products * ^
    -requires Microsoft.Net.Component.4.5.TargetingPack ^
    -requires Microsoft.Net.Component.4.5.2.TargetingPack ^
    -requires Microsoft.Net.Component.4.6.2.TargetingPack ^
    -property installationPath`) do (
  set InstallDir="%%i"
)

if not DEFINED InstallDir (
  echo "Could not find a VS2019 installation with the necessary components (targeting packs for v4.5, v4.5.2, and v4.6.2)."
  echo Please install VS2019 or the missing components.
  goto BuildFail
)

PowerShell -NoProfile -NoLogo -ExecutionPolicy Bypass  -Command ^
  "try { & '%~dp0eng\GetXCopyMSBuild.ps1'; exit $LASTEXITCODE } catch { write-host $_; exit 1 }"
if %ERRORLEVEL% neq 0 goto BuildFail

REM Configure NuGet operations to work w/in this repo i.e. do not pollute system packages folder.
REM Note this causes two copies of packages restored using packages.config to land in this folder e.g.
REM StyleCpy.5.0.0/ and stylecop/5.0.0/.
set "NUGET_PACKAGES=%CD%\packages"

REM Are we running in a local dev environment (not on CI)?
if DEFINED CI (set Desktop=false) else if DEFINED TEAMCITY_VERSION (set Desktop=false) else (set Desktop=true)

if "%1" == "" goto BuildDefaults

MSBuild Runtime.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
    /fl /fileLoggerParameters:LogFile=bin\msbuild.log;Verbosity=Normal /consoleLoggerParameters:Summary /t:%*
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
MSBuild Runtime.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
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
