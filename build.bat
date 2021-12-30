@echo off
dotnet build && dotnet test .\ILang.Tests\ILang.Tests.csproj && dotnet run --project ilc
@echo on

