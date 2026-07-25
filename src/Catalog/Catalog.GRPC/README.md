# Catalog.GRPC — gRPC contracts

The **public gRPC contract** of the Catalog service: the `.proto` definition and the C# code
generated from it (server stubs and client stubs).

This project contains **no business logic and no implementation** — only the contract. It is what
other services reference to talk to Catalog in a strongly-typed way.

---

## Contents

- `Protos/catalog.proto` — the contract: the `ProductProtoService` service and its request/response
  messages.
- Generated C# (server + client stubs) produced at build time by `Grpc.Tools`. The generated code
  lives in `obj/` and is **not** committed — only the `.proto` is versioned; the code regenerates on
  every build.

`GrpcServices="Both"` is set in the `.csproj`, so both the server base class (implemented by
`Catalog.Api`) and the client (used by consumers) are generated from the single `.proto`.

---

## How it is consumed

- **Server side — `Catalog.Api`** references this project and implements `ProductProtoServiceBase`
  to expose the endpoint.
- **Client side — a consumer** (e.g. a future Ordering service) references this project to generate
  its client and call Catalog, obtaining **only the contract**, never Catalog's internals.

---

## Why a separate project (and not inside `Catalog.Api`)?

The contract could technically have lived directly inside `Catalog.Api`. It is deliberately kept in
its own project for one core reason:

> **The contract must be shareable without dragging the implementation along.**

If the `.proto` lived in `Catalog.Api`, then any consumer wanting to generate its client would have
to reference `Catalog.Api` in full — pulling in its REST controllers, `DbContext`, repositories,
PostgreSQL connection and business logic. That would couple the consumer to Catalog's **internal
implementation**, which is exactly what microservices must avoid: *a service never references another
service's internal code.*

A dedicated contracts project isolates **what Catalog promises** (the contract, shareable) from
**how Catalog does it** (the implementation in `Catalog.Api`, private).

- `Catalog.GRPC` = the **public façade** of Catalog.
- `Catalog.Api` = the **kitchen** — private, seen by no one else.

The contract also **belongs to the provider, not the consumer**: Catalog decides what it exposes, so
the `.proto` lives on Catalog's side — never inside a consumer.

---

## How it is shared: same solution vs separate repos

Sharing a contract does **not** break independent deployment. The contract compiles to a DLL that
each service **embeds** in its own container at build time. Sharing happens **at compile time**, not
at runtime — each service runs with its own embedded copy.

### This project — monorepo + `ProjectReference`

All services live in the same solution, so consumers reference `Catalog.GRPC` via a plain
`ProjectReference`. Simple, no extra tooling, and each service still has its own `Dockerfile` and
container → deployment stays independent. **Monorepo ≠ monolith.**

### Alternative — separate repos + NuGet package

In an enterprise setup with a repo per service, a `ProjectReference` is no longer possible. The
contract would instead be published as a **versioned NuGet package** to a registry (private
NuGet.org, Azure Artifacts, GitHub Packages…), and each service would install it like any external
dependency. Cleaner decoupling across repos, but heavier: a package registry, versioning and a
publish step to maintain.

The principle is the same either way: **the contract is shared at compile time; services deploy
independently.** This project uses the monorepo + `ProjectReference` approach as the right trade-off
for its size.

---

## Packages

This project intentionally uses only the lightweight gRPC packages:

- `Google.Protobuf` — protobuf serialization.
- `Grpc.Tools` — code generation from the `.proto` (build-only, `PrivateAssets="all"`, so it does
  not propagate to referencing projects).
- `Grpc.Net.Client` — so the **client** stub can be generated for consumers.

It does **not** reference `Grpc.AspNetCore`: that metapackage is for *hosting* a gRPC server and
belongs in `Catalog.Api`, not in a shared contracts library (a client consumer has no business
pulling in server-hosting dependencies).