# ADR 0003: Use one forecast request

- Status: Accepted

## Decision

Call `forecast.json` with `days=3` and consume both its current-weather object and forecast objects.

## Rationale

The forecast response already contains the current conditions required by the interface. Calling `current.json` separately would duplicate upstream work, increase latency and failure surface, and could produce data sampled at slightly different moments.
