@echo off
setlocal

if exist bin goto Build
mkdir bin

:Build

REM Require Visual Studio on the system. Use `vswhere` for the search because it can find all VS installations.
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
for /f "usebackq tokens=*" %%i in (`%vswhere% -latest -prerelease -products * ^
    -property installationPath`) do (
  set "InstallDir=%%i"
)

if not DEFINED InstallDir (
  echo "Could not find a Visual Studio installation."
  echo Please install Visual Studio or the missing components.
  goto BuildFail
)

REM Add MSBuild to the path.
set "PATH=%InstallDir%\MSBuild\Current\Bin;%PATH%"

REM Are we running in a local dev environment (not on CI)?
if DEFINED CI (set Desktop=false) else if DEFINED TEAMCITY_VERSION (set Desktop=false) else (set Desktop=true)

pushd %~dp0
if "%1" == "" goto BuildDefaults

MSBuild "%~dp0Runtime.msbuild" /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
    /fl /fileLoggerParameters:LogFile=bin\msbuild.log;Verbosity=Normal /consoleLoggerParameters:Summary /t:%*
if %ERRORLEVEL% neq 0 goto BuildFail
goto BuildSuccess

:BuildDefaults
MSBuild "%~dp0Runtime.msbuild" /m /nr:false /p:Platform="Any CPU" /p:Desktop=%Desktop% /v:M ^
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
