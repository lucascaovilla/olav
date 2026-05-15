# Getting Started

This guide walks you from installation to your first CQRS command in a running project.

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| Docker | Any recent version |
| Git | Any recent version |

## Installation

```bash
dotnet tool install --global Olav.Cli
```

Verify:

```bash
olav --version
```

## Step 1: Create a Project

```bash
olav new MyApi
cd MyApi
```

This generates a full DDD solution with architecture tests, Docker configuration, and a GitHub Actions CI pipeline.

## Step 2: Build and Verify

```bash
dotnet build
dotnet test
```

All tests should pass immediately. The architecture tests confirm your layers are correctly wired.

## Step 3: Add a Database Plugin

```bash
olav add infrastructure postgres
```

Olav prompts for connection parameters, then injects: NuGet packages, DI wiring, and a Docker Compose service. Start the database:

```bash
docker compose -f docker/docker-compose.local.yml up -d
```

## Step 4: Add a Domain Entity

```bash
olav add entity Order
olav add repository Order
olav add enum OrderStatus --entity Order   # optional: scoped enum
```

Generates a vertical aggregate slice under `src/MyApi.Domain/Order/`:

```
src/MyApi.Domain/Order/
├── Entities/
│   └── Order.cs                  ← MyApi.Domain.Order.Entities
└── Repositories/
    └── IOrderRepository.cs       ← MyApi.Domain.Order.Repositories
```

Enums without `--entity` land in `src/MyApi.Domain/Shared/Enums/`.

## Step 5: Add a Service (optional)

```bash
# Application-layer service (default)
olav add service Order OrderService

# Infrastructure-layer service (e.g. an email sender)
olav add service Email EmailService --layer infrastructure
```

Application services land in `src/MyApi.Application/Services/`. Infrastructure services split: interface in Application, implementation in `src/MyApi.Infrastructure/Services/`. DI registration and `Program.cs` wiring are automatic.

## Step 6: Scaffold CQRS

```bash
# Command with handler and API endpoint
olav add command Order PlaceOrder --with-handler --with-endpoint

# Query with handler and API endpoint
olav add query Order GetOrderById --with-handler --with-endpoint
```

Generated files:

```
src/MyApi.Application/Order/Commands/PlaceOrder/
├── PlaceOrderCommand.cs
├── PlaceOrderCommandResult.cs
├── IPlaceOrderCommandHandler.cs
└── PlaceOrderCommandHandler.cs    ← implement this

src/MyApi.Application/Order/Queries/GetOrderById/
├── GetOrderByIdQuery.cs
├── GetOrderByIdQueryResult.cs
├── IGetOrderByIdQueryHandler.cs
└── GetOrderByIdQueryHandler.cs    ← implement this

src/MyApi.Api/Controllers/
└── OrderController.cs             ← POST and GET actions injected
```

The handlers are auto-registered in `ApplicationExtensions.cs` and `Program.cs`. No manual DI wiring needed.

## Step 7: Generate a Migration

Once you have EF Core entities mapped, create a migration:

```bash
olav make migration postgres InitialCreate
```

This runs `dotnet ef migrations add` against the postgres persistence project.

## Step 8: Explore the API

When running in the Development environment, the generated API exposes an interactive explorer at:

```
http://localhost:8080/scalar/v1
```

All controllers and endpoints are listed automatically. No additional setup needed.

## Next Steps

- Read the [Introduction](introduction.md) for a full architecture overview
- Explore the plugin system: `olav plugin list`, `olav source add`
- Validate your architecture at any time: `olav verify`
