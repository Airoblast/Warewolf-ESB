REM == START ENV ==

rmdir /S /Q C:\CodedUI\Binaries
mkdir C:\CodedUI\Binaries
xcopy /Q /E /Y "\\RSAKLFSVRTFSBLD\Automated Builds\ReleaseStaging" "C:\CodedUI\Binaries"

REM Copy the CodedUI configuration over

copy /Y "C:\Builds\TestRunWorkspace\Sources\BuildUtils\CodedUI\UI-Warewolf Server.exe.config" "C:\CodedUI\Binaries\Warewolf Server.exe.config"

REM Start things up

start "" /B "C:\CodedUI\Binaries\Warewolf Server.exe"
timeout 10
start "" /B "C:\CodedUI\Binaries\Warewolf Studio.exe"
REM timeout 60