# PulseWallet API

[![CI](https://github.com/perpiris/pulse-wallet-api/actions/workflows/ci.yml/badge.svg)](https://github.com/perpiris/pulse-wallet-api/actions/workflows/ci.yml)
[![.NET 10 LTS](https://img.shields.io/badge/.NET-10.0%20LTS-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/10.0)

## Running locally

### Setup

Create `.env` and set a SQL Server password:

```bash
cp .env.example .env
```

To run the API from your IDE, also store the connection string in user secrets, with the same password:

```bash
dotnet user-secrets set "ConnectionStrings:PulseWallet" \
  "Server=localhost,1433;Database=PulseWallet;User Id=sa;Password=<password>;Encrypt=True;TrustServerCertificate=True" \
  --project src/PulseWallet.Api
```

### Database in Docker, API from your IDE

```bash
docker compose up -d
```

Then run the `http` launch profile.

### Everything in Docker

```bash
docker compose --profile app up --build
```

### Stopping

```bash
docker compose --profile app down      # keep the data
docker compose --profile app down -v   # delete the database
```

Swagger UI: http://localhost:5055/swagger
