FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
WORKDIR /src
COPY . .
RUN dotnet restore TriDict.sln
RUN dotnet publish src/TriDict.HttpApi.Host/TriDict.HttpApi.Host.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/TriDict.DbMigrator/TriDict.DbMigrator.csproj -c Release -o /app/migrator --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS api
WORKDIR /app
COPY --from=build /app/api .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TriDict.HttpApi.Host.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0.12 AS migrator
WORKDIR /app
COPY --from=build /app/migrator .
ENTRYPOINT ["dotnet", "TriDict.DbMigrator.dll"]
