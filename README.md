# Olav

[![CI](https://github.com/lucascaovilla/olav/actions/workflows/ci.yml/badge.svg)](https://github.com/lucascaovilla/olav/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Olav.Cli.svg)](https://www.nuget.org/packages/Olav.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Olav.Cli.svg)](https://www.nuget.org/packages/Olav.Cli)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

> **Scaffold production-grade .NET APIs with strict Domain-Driven Design -- from zero to architecture in one command.**

---

## Why Olav?

**Sir Nils Olav III** is a king penguin living at Edinburgh Zoo. He holds the rank of Brigadier Sir Nils Olav III, Knight of the Order of St. Olav, in the Norwegian King's Guard -- a military rank he inherited after decades of ceremonial inspection and promotion by the Norwegian Royal Guard. He did not earn it through battle. He earned it by showing up, standing straight, and being inspected and found correct every single time.

**Saint Olav II** -- Olav Haraldsson -- was a Viking king who unified Norway under a single set of laws and a single faith. He didn't ask for permission. He built the structure first and let history decide whether it was right. It was. He's Norway's patron saint.

Olav the tool borrows from both. It doesn't fight you. It doesn't ask questions. It shows up, stands straight, and generates a project that passes inspection before you've written a single line of business logic. Like the saint, it imposes structure not to constrain you -- but because structure is what makes things last.

---

## What is Olav?

Olav is a .NET CLI tool that generates production-ready API projects following strict **Domain-Driven Design (DDD)** architecture. Think of it as the [FastAPI](https://fastapi.tiangolo.com/) of the .NET world -- opinionated, fast to start, and designed to enforce good practices from day one rather than letting them drift in over time.

It is aimed at **beginner to mid-level .NET developers** who want to start building APIs the right way without spending days configuring architecture, tests, Docker, and CI/CD pipelines from scratch.

A single command gives you:

- A layered DDD solution (`Domain`, `Application`, `Infrastructure`, `Api`)
- Architecture tests that **fail the build** if your layers talk to the wrong neighbours
- Integration tests wired and ready
- Observability middleware out of the box
- Docker and Docker Compose configuration
- GitHub Actions CI/CD pipeline
- Git hooks enforcing code quality before every push
- A `olav.json` contract file tracking your project's template version

From there, Olav grows with you. Add entities, repositories, and services directly into your existing project. Install infrastructure plugins for Postgres, Redis, SQL Server, and more. Manage deployment pipelines. Validate your architecture at any point. Everything stays consistent because the same tool that built the project also knows how to extend it.

---

## Prerequisites

| Tool     | Version            |
| -------- | ------------------ |
| .NET SDK | 10.0+              |
| Docker   | Any recent version |
| Git      | Any recent version |

---

## Installation

```bash
dotnet tool install --global Olav.Cli
```

---

## Quick Start

```bash
olav new MyApi
```

That's it. You now have a fully structured, architecture-tested, Docker-ready .NET API.

### Options

```bash
olav new MyApi --owner "Acme Corp" --license "MIT"
```

| Option      | Default          | Description                                          |
| ----------- | ---------------- | ---------------------------------------------------- |
| `--owner`   | Your OS username | Sets the copyright owner in file headers and license |
| `--license` | `MIT`            | Sets the license type                                |

---

## Generated Project Structure

```
MyApi/
+-- src/
|   +-- MyApi.Domain/           # Entities, value objects, domain events -- no dependencies
|   +-- MyApi.Application/      # Use cases, handlers, interfaces -- depends only on Domain
|   +-- MyApi.Infrastructure/   # Repositories, services, external integrations
|   +-- MyApi.Api/              # API controllers, middleware, observability, entry point
+-- tests/
|   +-- MyApi.ArchitectureTests/   # Enforces DDD layer rules at build time
|   +-- MyApi.IntegrationTests/    # Integration tests wired and ready
+-- docker/
+-- Directory.Build.props
+-- Directory.Packages.props
+-- global.json
+-- olav.json                   # Template version contract
```

---

## Architecture Enforcement

This is the core of what Olav gives you. The generated `ArchitectureTests` project runs on every build and **fails loudly** if your code breaks DDD rules. There is no silent drift.

Rules enforced out of the box:

| Rule                                   | What it prevents                                        |
| -------------------------------------- | ------------------------------------------------------- |
| `Domain` has no outward dependencies   | Domain referencing Infrastructure or Application        |
| `Application` depends only on `Domain` | Application importing Infrastructure directly           |
| All handlers live in `Application`     | Business logic leaking into Api or Infrastructure       |
| Application services stay in `Application` | Infrastructure services bleeding into Application, or vice versa |
| `Api` is the only entry point          | Controllers or middleware defined outside the Api layer |

In a future version these rules will also be enforced at development time via **Roslyn analyzers**, catching violations before the build even runs.

---

## Commands

### `olav new`

Generates a new DDD API project.

```bash
olav new MyApi
olav new MyApi --owner "Acme Corp" --license "Apache-2.0"
```

Creates a plugin scaffold for authoring your own plugins:

```bash
olav new plugin my-plugin --category infrastructure --delivery package --author "Your Name"
```

---

### `olav add`

Adds artifacts to an existing Olav project. Run these commands from inside your project directory.

#### Entities and Enums

```bash
olav add entity Order
olav add enum OrderStatus
```

Generates the entity class inside `src/MyApi.Domain/Entities/` and the enum inside `src/MyApi.Domain/Enums/`. Both are placed in the correct namespace automatically.

#### Repositories

```bash
olav add repository Order
```

Generates two files: the interface in `src/MyApi.Domain/Repositories/IOrderRepository.cs` and the implementation stub in `src/MyApi.Infrastructure/Repositories/OrderRepository.cs`. If you have a database plugin installed, the implementation is placed inside its own persistence layer -- for example with the `postgres` plugin, it lands at `src/MyApi.Infrastructure/Persistence/Postgres/Repositories/OrderRepository.cs` -- and uses the right client instead of a stub.

#### Services

```bash
olav add service OrderService
olav add service OrderService --entity Order
olav add service EmailService infrastructure
```

Generates the interface in `src/MyApi.Application/Services/IOrderService.cs` and the implementation in the same namespace by default. With `--entity`, the generated service gets constructor injection of the matching repository. Passing `infrastructure` as the last argument keeps the interface in `Application` and places the implementation in `Infrastructure` -- useful for services that wrap external integrations or cross-cutting concerns.

#### Infrastructure Plugins

```bash
olav add infrastructure postgres
olav add infrastructure redis
olav add infrastructure sqlserver
```

Installs a database or messaging plugin into your project. The plugin injects the required NuGet packages, registers the service in your DI setup, and adds the service definition to `docker-compose.local.yml`. Missing parameters are prompted interactively.

#### Deployment Plugins

```bash
olav add deployment azure
olav add deployment docker
```

Installs a deployment plugin. For Azure, it generates a GitHub Actions deployment workflow configured with the parameters you provide. Installed plugins are recorded in `olav.json`.

#### CQRS Commands and Queries

```bash
olav add command Order PlaceOrder
olav add query Order GetOrderById
```

Scaffolds CQRS artifacts for a command or query inside an existing entity's application layer. Artifacts are generated progressively using flags:

```bash
# Record classes only
olav add command Order PlaceOrder

# Add handler interface + implementation (auto-registered in ApplicationExtensions.cs and Program.cs)
olav add command Order PlaceOrder --with-handler

# Add handler + API endpoint (creates or injects into the entity's controller)
olav add command Order PlaceOrder --with-handler --with-endpoint
```

Generated structure for `olav add command Order PlaceOrder --with-handler --with-endpoint`:

```
src/MyApi.Application/Order/Commands/PlaceOrder/
├── PlaceOrderCommand.cs           # sealed record — add properties here
├── PlaceOrderCommandResult.cs     # sealed record — returned by the handler
├── IPlaceOrderCommandHandler.cs   # handler interface
└── PlaceOrderCommandHandler.cs    # stub implementation — throws NotImplementedException

src/MyApi.Api/Controllers/
└── OrderController.cs             # created if missing; POST action injected if it exists
```

Queries work identically (`olav add query`) with GET semantics and a nullable result type (`Task<GetOrderByIdQueryResult?>`).

---

### `olav make`

Generates artifacts that require external tooling.

#### Database Migrations

```bash
olav make migration postgres InitialCreate
olav make migration sqlserver AddOrderTable
```

Requires the corresponding database plugin (`postgres` or `sqlserver`) to be installed. Runs `dotnet ef migrations add` against the infrastructure persistence project for that plugin. The project must have EF Core tools installed (`dotnet-ef`).

---

### `olav plugin`

Manages the plugins installed in your project.

```bash
olav plugin list              # Shows all installed plugins with version, category, and source
olav plugin remove postgres   # Removes a plugin record from olav.json
```

Note: removing a plugin from `olav.json` does not delete the files it generated. It only removes the tracking entry.

---

### `olav source`

Manages plugin registries. You can add custom registries to access private or team-specific plugins.

```bash
olav source add acme-plugins https://plugins.acme.com/registry
olav source list
olav source remove acme-plugins
```

Sources added this way are available globally, across all your Olav projects.

---

### `olav lint`

Validates your project's folder structure and layer organisation against Olav's rules.

```bash
olav lint
```

Run this in CI to catch structural regressions before they merge.

---

### `olav verify`

Validates architecture rules and confirms all required tests are present and passing.

```bash
olav verify
```

Stricter than lint -- this is your full architectural health check.

---

### `olav migrate`

Shows pending template upgrades for your generated project. Use `--apply` to write the changes.

```bash
olav migrate          # dry run -- shows what would change
olav migrate --apply  # applies the migration
```

Olav tracks the template version your project was generated with inside `olav.json`. When the tool is updated with new conventions, `migrate` brings your project up to date without you having to start over.

### `olav doctor` _(planned)_

Diagnoses missing observability configuration, bad environment setup, or misconfigured tooling.

---

## Plugin System

Olav ships with a set of official plugins built into the binary. No external downloads needed for the basics.

**Official plugins:**

| ID          | Category       | What it does                                              |
| ----------- | -------------- | --------------------------------------------------------- |
| `postgres`  | infrastructure | EF Core + Npgsql DI setup, Docker Compose service         |
| `sqlserver` | infrastructure | EF Core + SqlServer DI setup, Docker Compose service      |
| `redis`     | infrastructure | StackExchange.Redis DI setup, Docker Compose service      |
| `docker`    | deployment     | Docker Compose configuration for multi-environment setups |
| `azure`     | deployment     | GitHub Actions workflow for Azure App Service deployment  |

When you install a plugin, Olav resolves what it needs, prompts for any missing parameters, and applies everything in one shot: NuGet references, DI wiring code, Docker Compose service blocks, and workflow files. The plugin is then recorded in `olav.json` so the rest of the tooling knows it is there.

**Custom plugins** can be hosted anywhere and added via `olav source add`. The plugin format is a manifest file (`olav.plugin.json`) with Scriban templates. Use `olav new plugin` to scaffold the structure.

---

## The `olav.json` Contract

Every generated project contains an `olav.json` at its root:

```json
{
  "toolVersion": "0.1.0",
  "templateVersion": "1.0",
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z",
  "plugins": [
    {
      "id": "postgres",
      "version": "0.1.0",
      "category": "infrastructure",
      "delivery": "package",
      "source": "official"
    }
  ]
}
```

This file is the handshake between your project and the tool. It lets `verify`, `lint`, `migrate`, and `plugin` know exactly what version of the conventions your project was built against, which plugins are installed, and what needs updating when conventions evolve.

---

## Roadmap

Olav is at `v0.1.0`. The foundation is solid. Here's what's coming:

| Feature                        | Description                                                              |
| ------------------------------ | ------------------------------------------------------------------------ |
| **Roslyn Enforcement**         | Catch layer violations at dev time, not just build time                  |
| **Doctor Mode**                | Diagnose observability gaps, missing config, and setup issues            |
| **Modular Monolith**           | Generate modular monolith structures alongside standard DDD              |
| **Multi-Environment Support**  | Environment-aware configuration scaffolding                              |
| **Security Enforcement**       | Enforce secure defaults in generated projects                            |
| **API Contract Enforcement**   | Enforce API versioning and contract stability                             |
| **Modes System**               | Switch between DDD modes (strict, relaxed, modular)                      |
| **Documentation Generator**    | Auto-generate API documentation from your domain model                   |
| **Benchmark Suite**            | Built-in benchmarking scaffolding with BenchmarkDotNet                   |

---

## Contributing

Olav is open source and welcomes contributions. To get started locally:

```bash
git clone https://github.com/lucascaovilla/olav
cd olav
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit
chmod +x .githooks/pre-push
```

To install locally for testing:

```bash
chmod +x scripts/install-local.sh
./scripts/install-local.sh
```

Or manually:

```bash
dotnet build
dotnet pack
dotnet tool uninstall --global Olav.Cli
dotnet tool update --global --add-source ./nupkg Olav.Cli
```

---

## License

MIT (c) [Lucas Caovilla](https://github.com/lucascaovilla)

---

<p align="center">
  Named after <a href="https://en.wikipedia.org/wiki/Nils_Olav">Sir Nils Olav III</a>, Brigadier of the Norwegian King's Guard,<br/>
  and <a href="https://en.wikipedia.org/wiki/Olaf_II_of_Norway">Saint Olav II</a>, Viking king and patron saint of Norway.<br/>
  Both imposed order. Both were right.
</p>
