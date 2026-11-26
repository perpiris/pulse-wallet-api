# Build context: the repository root (see compose.yaml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project files first, so the restore layer stays cached until a package changes. .editorconfig because
# warnings are errors and its rule severities must match local builds and CI.
COPY .editorconfig Directory.Build.props Directory.Packages.props ./
COPY src/PulseWallet.Domain/PulseWallet.Domain.csproj src/PulseWallet.Domain/
COPY src/PulseWallet.Application/PulseWallet.Application.csproj src/PulseWallet.Application/
COPY src/PulseWallet.Infrastructure/PulseWallet.Infrastructure.csproj src/PulseWallet.Infrastructure/
COPY src/PulseWallet.Api/PulseWallet.Api.csproj src/PulseWallet.Api/
RUN dotnet restore src/PulseWallet.Api/PulseWallet.Api.csproj

COPY src/ src/
RUN dotnet publish src/PulseWallet.Api/PulseWallet.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

USER $APP_UID

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PulseWallet.Api.dll"]
