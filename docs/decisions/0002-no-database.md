# ADR 0002: Do not add persistent storage

- Status: Accepted

## Decision

Do not use a relational, document, or embedded database.

## Rationale

Atmos has no mutable business state. Weather data comes from an external source of truth and becomes stale quickly. Persisting it would introduce schema, migrations, infrastructure, and consistency concerns without serving a product requirement. A short in-memory cache is sufficient for request coalescing and provider load reduction.
