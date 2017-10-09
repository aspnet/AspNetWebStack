@echo off
pushd %~dp0
setlocal

if exist bin goto build
mkdir bin

:Build

REM Find the most recent 32bit MSBuild.exe on the system. Require v15.0 (installed with VS2017) or later since .NET
REM Core projects are coming soon.
REM Use `vswhere` for the search since %ProgramFiles(x86)%\msbuild\15.0\Bin\MSBuild.exe almost never exists.
set vswhere="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %vswhere% (
  set VsWhere="%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
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
for /f "usebackq tokens=*" %%i in (`%vswhere% -latest -prerelease -products * -requires Microsoft.Component.MSBuild -property installationPath`) do (
  set InstallDir=%%i
)
if exist "%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe" (
  set MSBuild="%InstallDir%\MSBuild\15.0\Bin\MSBuild.exe"
) else (
  echo Could not find MSBuild.exe. Please install the VS2017 BuildTools component or a workload that includes it.
  goto BuildFail
)

if "%1" == "" goto BuildDefaults

%MSBuild% Runtime.msbuild /m /nr:false /t:%* /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
%MSBuild% Runtime.msbuild /m /nr:false /p:Platform="Any CPU" /p:Desktop=true /v:M /fl /flp:LogFile=bin\msbuild.log;Verbosity=Normal
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
