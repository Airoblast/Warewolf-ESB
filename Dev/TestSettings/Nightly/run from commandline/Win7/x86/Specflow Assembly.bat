@echo off
@echo Assembly Name eg: Dev2.Activities.Specs.dll:
set /P assembly=
cd %CD%\..\..\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Test
copy /Y "%CD%\TestSettings\Nightly\Win7\x86\SpecFlow.testsettings" "%CD%\SpecFlow.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\TestBinaries\%assembly%" /testSettings:"SpecFlow.testsettings"
pause