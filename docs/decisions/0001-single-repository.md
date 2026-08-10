# ADR 0001: Keep the product in one repository

- Status: Accepted

## Decision

Keep the ASP.NET Core backend, React frontend, tests, container definitions, and CI configuration in one repository.

## Rationale

The code is one independently reviewed and deployed product. A monorepo keeps API contract changes atomic, enables a single CI workflow, and provides a one-command local environment. Separate repositories would add coordination overhead without providing independent ownership or release value.
