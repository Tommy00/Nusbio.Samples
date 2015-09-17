cls
@echo off 
echo Creating MadeInTheUSB.Nusbio.Init NuGet Package

del ..\DynamicSugar.dll
del ..\FTD2XX_NET.dll
del ..\Nancy.dll
del ..\Nancy.Hosting.Self.dll
del ..\Newtonsoft.Json.dll
del *.nupkg

NuGet.exe pack MadeInTheUSB.Nusbio.Lib.nuspec

:: copy "D:\DVT\MadeInTheUSB\MadeInTheUSB.lDevice.Lib\bin\Debug\NuGet\*.nupkg" "C:\Users\TorresF\AppData\Local\NuGet\Cache"
:: rd /s/q D:\DVT\MadeInTheUSB\packages\MadeInTheUSB.lDevice.Lib.1.?.?

echo Done
pause