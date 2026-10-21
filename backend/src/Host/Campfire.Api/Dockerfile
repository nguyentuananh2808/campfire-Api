FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:0d99ebf1ccad15037b5c98c9fe8979d6b8dbac23812feae2277a0b277b5c5ffa AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY backend ./backend

RUN dotnet restore backend/Campfire.slnx --locked-mode --disable-parallel
RUN dotnet publish backend/src/Host/Campfire.Api/Campfire.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /out/api
RUN dotnet publish backend/tools/Campfire.DbMigrator/Campfire.DbMigrator.csproj \
    --configuration Release \
    --no-restore \
    --output /out/migrator

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:d5633778c754e89a386882eddf8dbee6942447a8a4b8267c19cf1e1158d0b0ac AS runtime
WORKDIR /app
COPY --from=build /out/api ./api
COPY --from=build /out/migrator ./migrator

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "/app/api/Campfire.Api.dll"]

