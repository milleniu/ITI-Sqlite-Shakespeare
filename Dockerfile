FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

COPY ITI.Sqlite.Shakespeare/*.csproj ./
RUN dotnet restore

COPY ITI.Sqlite.Shakespeare/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine3.9
WORKDIR /app

COPY --from=build-env /app/out ITI.Sqlite.Shakespeare
ENTRYPOINT [ "dotnet", "ITI.Sqlite.Shakespeare/ITI.Sqlite.Shakespeare.dll" ]
