REM  Clean Staging Directory

@echo off
call :cleanDIR
goto :eof

:cleanDIR
for /d /r "\\rsaklfsvrtfsbld\Automated Builds\GatedTestRunStaging" %%x in (*) do rd /s /q "%%x"
attrib -R "\\rsaklfsvrtfsbld\Automated Builds\GatedTestRunStaging\*.*"
del /Q "\\rsaklfsvrtfsbld\Automated Builds\GatedTestRunStaging\*.*"
exit /b