# Microservices Playground — .NET

A learning-oriented showcase project demonstrating a modern **microservices architecture** built with **.NET** and **C#**, orchestrated with **Docker Compose**.

The goal is not the business domain itself (a small e-commerce system) but to illustrate — with each technology justified by a real need rather than added for show — how independent services are designed, communicate, and stay decoupled.

> 🚧 **Work in progress.** This project is being built incrementally, one service and one concept at a time.

---

## Objectives

This project intentionally combines several patterns commonly expected in a production microservices system:

- **Domain-Driven Design (DDD)** on the service with rich business rules (Ordering).
- **Synchronous internal communication** via **gRPC** (strongly typed, fast internal calls).
- **Asynchronous communication** via **RabbitMQ** (event-driven, fully decoupled services).
- **Saga pattern** with compensation for distributed transactions (order ↔ payment).
- **Outbox pattern** to guarantee consistency between the database and published events.
- **Idempotency** on message consumers to safely handle duplicated events.
- **API Gateway** as a single entry point for external clients.
- **Containerized infrastructure** — everything runs with a single `docker compose up`.

---

## Tech stack

- **.NET / C#** — services and workers
- **Entity Framework Core** (Npgsql provider) — data access
- **PostgreSQL** — database (one per service, running in Docker)
- **RabbitMQ** + **Rebus** — message bus for asynchronous events
- **gRPC** — synchronous internal service-to-service calls
- **YARP** (or Ocelot) — API Gateway
- **Docker** & **Docker Compose** — local orchestration
- **NUnit** + **Testcontainers** — integration tests against a real PostgreSQL container, not an in-memory substitute

---

## Architecture

The system is split into independent services, each with its own database, its own lifecycle, and its own responsibility.

```
                            ┌──────────────┐
     REST (external client) │ API Gateway  │
     ─────────────────────► │ (YARP)       │
                            └──────┬───────┘
                     ┌─────────────┴─────────────┐
                     ▼                           ▼
               ┌───────────┐               ┌───────────┐
               │  Catalog  │ ◄──── gRPC ───┤ Ordering  │
               │  (CRUD)   │   stock/price │  (DDD)    │
               └───────────┘               └─────┬─────┘
                                                 │ publishes OrderCreated
                                                 ▼
                                          ┌──────────────┐
                                          │   RabbitMQ   │
                                          └───┬──────┬───┘
                              OrderCreated ▼      ▼ OrderCreated / PaymentCompleted
                                    ┌───────────┐ ┌──────────────┐
                                    │  Payment  │ │ Notification │
                                    │  (Saga)   │ │  (Worker)    │
                                    └───────────┘ └──────────────┘
```

**Two communication styles, each with a purpose:**

- **Synchronous** (REST at the edge, gRPC internally) — when an immediate answer is required. Example: Ordering asks Catalog "is this product in stock, what's the price?" before creating an order.
- **Asynchronous** (events through RabbitMQ) — when services should stay decoupled. Example: once an order is created, `OrderCreated` is published; Payment and Notification react independently, without Ordering knowing they exist.

### Services

| Service | Type | Role | Key concepts |
|---|---|---|---|
| **API Gateway** | Web (YARP) | Single external entry point, routes to internal services | Reverse proxy, routing |
| **Catalog** | Web API | Product catalog (CRUD), exposes stock/price over gRPC | Simple CRUD, gRPC server |
| **Ordering** | Web API | Order lifecycle and business rules | DDD, aggregates, domain events, Outbox |
| **Payment** | Worker / API | Processes payments, drives the saga | Saga, compensation, idempotency |
| **Notification** | Worker Service | Sends emails in reaction to events | Pure async consumer, decoupling |

### Design decisions

Non-obvious trade-offs are recorded as ADRs, so the reasoning survives the commit that implements them.

| ADR | Decision |
|---|---|
| [0001](docs/adr-0001-case-sensitive-product-search.md) | Product search stays case-sensitive — PostgreSQL `LIKE` vs. SQL Server's case-insensitive default collation, and why `citext` and non-deterministic collations were ruled out |

---

## Roadmap

- [ ] **Catalog** — project scaffolded, CRUD + PostgreSQL (in progress)
- [ ] **Docker Compose** — PostgreSQL for Catalog
- [ ] **gRPC** — Catalog exposes stock/price
- [ ] **Ordering** — DDD model + gRPC call to Catalog
- [ ] **RabbitMQ + Rebus** — Ordering publishes `OrderCreated`
- [ ] **Payment** — consumer + Saga with compensation
- [ ] **Notification** — Worker Service consuming events
- [ ] **API Gateway** — YARP routing
- [ ] **Outbox + Idempotency** — reliability patterns

---

## Getting started

> ⚠️ Prerequisites and run instructions will grow as services are added.

```bash
# Clone the repository
git clone https://github.com/benoitVaisse/dotnet-micro-service.git
cd dotnet-micro-service

# Start the infrastructure (PostgreSQL for now)
docker compose up -d

# Run a service from your IDE (e.g. Catalog.Api) or via the CLI
dotnet run --project src/Catalog/Catalog.Api
```

---

## Project structure

```
dotnet-micro-service/
├── docker-compose.yml
├── README.md
├── docs/                          # Architecture Decision Records
├── src/
│   └── Catalog/
│       └── Catalog.Api/
└── tests/
    ├── Catalog.IntegrationsTests/  # NUnit + Testcontainers (real PostgreSQL)
    └── Catalog.ModelBuilder/       # test data builders
```

*(This structure will expand as new services are added.)*

---

## About

Personal learning project — feedback and suggestions welcome.