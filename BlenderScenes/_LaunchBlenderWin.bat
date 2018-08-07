@echo off

set DIR=%cd%
echo "Running from..."
echo %DIR%


rem See: https://docs.blender.org/manual/de/dev/advanced/command_line/arguments.html
set BLENDER_USER_SCRIPTS=%DIR%\..\BlenderScripts
echo %BLENDER_USER_SCRIPTS%
set BLENDER_USER_CONFIG=%DIR%\..\BlenderConfig
rem set TEMP="%DIR%\..\BlenderTemp"
..\BlenderExe\blender.exe

pause
