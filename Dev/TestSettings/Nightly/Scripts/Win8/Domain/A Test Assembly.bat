@echo off
@echo Assembly Name eg: Dev2.Infrastructure.Tests (no .dll):
set /P assembly=
cd %CD%\..\..\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
copy /Y "%CD%\TestSettings\Nightly\Win8\Domain\UnitTestWithCoverage.testsettings" "%CD%\UnitTestWithCoverage.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\%assembly%\bin\Debug\%assembly%.dll" /testSettings:"UnitTestWithCoverage.testsettings"
DEL /F /Q "%CD%\UnitTestWithCoverage.testsettings"
pause