# PulseWallet API

[![.NET 10 LTS](https://img.shields.io/badge/.NET-10.0%20LTS-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)

## Running locally

Requires the .NET 10 SDK and Docker.

**One-time setup**

```bash
cp .env.example .env          # then set MSSQL_SA_PASSWORD in .env
dotnet tool restore           # installs dotnet-ef from dotnet-tools.json

# Connection string for running the API from the IDE (use the same password as in .env)
dotnet user-secrets set "ConnectionStrings:PulseWallet" \
  "Server=localhost,1433;Database=PulseWallet;User Id=sa;Password=<password>;Encrypt=True;TrustServerCertificate=True" \
  --project src/PulseWallet.Api
```

**Database in Docker, API from the IDE** (to debug)

```bash
docker compose up -d
```

Then run the `http` profile from Rider: http://localhost:5055/swagger

**Everything in Docker**

```bash
docker compose --profile app up --build
```

Same URL: http://localhost:5055/swagger. Stop either with `docker compose --profile app down`; add `-v` to also delete the database.

In Development the API creates and migrates the database on startup.

**Migrations**

```bash
dotnet ef migrations add <Name> --project src/PulseWallet.Infrastructure --startup-project src/PulseWallet.Api --output-dir Persistence/Migrations
```

**Endpoints**

- `GET /health`: `Healthy` when the database is reachable
- `POST /api/auth/register`, `/api/auth/login`, `/api/auth/refresh`: ASP.NET Core Identity. Every other endpoint requires `Authorization: Bearer <accessToken>` unless marked `[AllowAnonymous]`.
