@echo off
cd %CD%\..\..\..\..\..
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" AllProjects.sln /Build Debug
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" Dev2.UI.Tests.sln /Build Debug
copy /Y "%CD%\TestSettings\Nightly\Win8\Domain\UI.testsettings" "%CD%\UI.testsettings"
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\MSTest.exe" /testcontainer:"%CD%\Dev2.UI.Tests\Dev2.Studio.UI.Tests\bin\Debug\Dev2.Studio.UITests.dll" /testSettings:"UI.testsettings"
DEL /F /Q "%CD%\UI.testsettings"
pause
FOR /D %%p IN ("%CD%\TestResults\*.*") DO rmdir "%%p" /s /q