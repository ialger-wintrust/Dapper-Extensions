this is a test

push to nuget using dotnet cli

make sure the Wintrust Nuget Source is added and is named Winturst Nuget

https://nuget.veteransfirst.com/

This needs to be ran in developer powershell
dotnet nuget push .\DapperExtensions\bin\Release\DapperExtensions.1.7.1.nupkg --source "Wintrust Nuget"