set version=1.0
set dotnetcore=netcoreapp3.1
set solutionfolder=C:\Users\SomeUser\source\repos\MonitorAndNotifyOpenVPNLogins\
set projectsubfolder=source\MonitorAndNotifyOpenVPNLogins\
set project=MonitorAndNotifyOpenVPNLogins
set release=Release

rem Root solution folder
if not exist "%solutionfolder%" (goto error)
if not exist "%solutionfolder%%projectsubfolder%" (goto error)
cd /d %solutionfolder%

rem Delete previous Builds folder
if exist "Builds\%dotnetcore%\" del Builds\%dotnetcore%\%project%-%version%-*-*-%dotnetcore%.rar
mkdir Builds\%dotnetcore%

rem Delete previous bin folder of project (will get auto created when building)
if exist "%projectsubfolder%bin\%release%\%dotnetcore%\" rmdir /q /s "%projectsubfolder%bin\%release%\%dotnetcore%\"

rem Public .NET Core versions
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\linux-arm-%dotnetcore%.pubxml --configuration %release%
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\linux-arm64-%dotnetcore%.pubxml --configuration %release%
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\linux-x64-%dotnetcore%.pubxml --configuration %release%
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\osx-x64-%dotnetcore%.pubxml --configuration %release%
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\win-x64-%dotnetcore%.pubxml --configuration %release%
dotnet publish %projectsubfolder%%project%.csproj /p:PublishProfile=%projectsubfolder%Properties\PublishProfiles\win-x86-%dotnetcore%.pubxml --configuration %release%

rem Check if building above worked
if not exist "%projectsubfolder%bin\%release%\%dotnetcore%\publish\" (goto error)

rem Rar all project builds
cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\linux-arm\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-linux-arm-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\linux-arm64\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-linux-arm64-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\linux-x64\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-linux-x64-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\osx-x64\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-osx-x64-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\win-x64\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-win-x64-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"
cd %projectsubfolder%bin\%release%\%dotnetcore%\publish\win-x86\
"C:\Program Files\WinRAR\rar.exe" a -r ..\..\..\..\..\..\..\Builds\%dotnetcore%\%project%-%version%-win-x86-%dotnetcore%.rar *.*

cd /d "%solutionfolder%"

goto done

:error
echo Something went wrong. Directory does not exist!
goto done

:done
