# ADR 0001 — Product search stays case-sensitive

**Status:** Accepted · **Date:** 2026-07-21 · **Scope:** Catalog service

---

## Context

The product filter is written as provider-agnostic LINQ:

```csharp
query = query.Where(p => p.Name.Contains(filterRequest.Name));
```

EF Core translates `string.Contains` to `LIKE '%…%'`. On PostgreSQL, `LIKE` is **case-sensitive**,
so this filter behaves differently than the same code would on SQL Server, whose default collation
(`SQL_Latin1_General_CP1_CI_AS`) is case-insensitive.

This is not a PostgreSQL quirk: the SQL standard mandates nothing here, and it is SQL Server's
default that hides the question.

The behaviour was found by an integration test running against a real PostgreSQL 17 container
(Testcontainers), not by reading the code — and it is worth noting that a test against the EF Core
InMemory provider would never have surfaced it.

## Evidence

[`GetFilteredTests`](../tests/Catalog.IntegrationsTests/ProductTestCases/GetFilteredTests.cs)
pins the behaviour with an A/B pair against seeded products named `"Alpha keyboard"` and
`"Beta keyboard"`:

| Query | Result |
| --- | --- |
| `"keyboard"` | 2 products |
| `"Keyboard"` | 0 products |

Same fragment, only the case differs, opposite results. Neither test can pass by accident: if the
filter returned nothing at all, the first one would fail.

## Options considered

| Option | DB-agnostic | Works with `LIKE '%…%'` | Granularity |
| --- | --- | --- | --- |
| `ToLower()` on both sides | ✅ | ✅ | per query |
| `EF.Functions.ILike` | ❌ Npgsql-specific | ✅ | per query |
| Non-deterministic ICU collation | ✅ (lives in the mapping) | ⚠️ not before PostgreSQL 18 | per column |
| `citext` | ❌ PG extension | ✅ | per column |

Two findings drove the shortlist:

- **Collations** are the architecturally cleanest option — the provider-specific concern would live
  in `ProductConfiguration`, leaving the filter untouched. But Npgsql documents that pattern
  matching via `LIKE` on a non-deterministic collation was not possible before PostgreSQL 18, and
  this project targets 17. PostgreSQL also documents a performance penalty and the loss of B-tree
  deduplication.
- **`citext`** is discouraged by PostgreSQL's own documentation, which recommends non-deterministic
  collations instead as they "handle more Unicode special cases correctly". It also ignores accents
  and forces the choice at column level: a `citext` column can never be compared case-sensitively
  elsewhere.

## Decision

**Keep the case-sensitive behaviour**, and pin it with a characterisation test.

Rationale:

1. The real problem was not case sensitivity — it was *not knowing about it*. The behaviour is now
   explicit, tested and locked.
2. `Filter.cs` stays generic over `IQueryable<T>` with no Npgsql dependency, which is what its
   design is for.
3. No product requirement currently calls for case-insensitive search. Introducing one now would be
   a change made for its own sake.

If the requirement appears, `ToLower()` is the chosen path: portable, explicit, and with no real
index cost here — a leading-wildcard pattern (`'%…%'`) cannot use a B-tree index in the first place,
so the usual objection to `LOWER()` does not apply. At scale, the answer for substring search would
be a `pg_trgm` GIN index, which works equally well over `LOWER("Name")`.

## Consequences

- `Should_Not_Match_Name_With_A_Different_Case` is a **characterisation test**: it documents current
  behaviour, not desired behaviour. It is expected to turn red the day the decision is revisited —
  that is its purpose, forcing the change to be deliberate rather than discovered in production.
- Callers must be aware that product search is case-sensitive. Worth surfacing in the API docs when
  the endpoint is published.

## References

- [Npgsql — Collations and Case Sensitivity](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html)
- [PostgreSQL — Collation Support](https://www.postgresql.org/docs/current/collation.html)
- [PostgreSQL — citext](https://www.postgresql.org/docs/current/citext.html)
- [PostgreSQL — Pattern Matching](https://www.postgresql.org/docs/current/functions-matching.html)
