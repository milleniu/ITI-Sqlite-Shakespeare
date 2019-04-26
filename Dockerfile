FROM mcr.microsoft.com/dotnet/core/runtime:2.2

COPY ITI.Sqlite.Shakespeare/bin/Release/netcoreapp2.2/publish ITI.Sqlite.Shakespeare

ENTRYPOINT [ "dotnet", "ITI.Sqlite.Shakespeare/ITI.Sqlite.Shakespeare.dll" ]
