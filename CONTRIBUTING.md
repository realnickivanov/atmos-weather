# Contributing

1. Create a focused branch.
2. Keep changes small and covered by tests.
3. Run backend and frontend checks before opening a pull request.
4. Do not commit API keys, generated build output, or local environment files.

## Required checks

```bash
dotnet restore Atmos.slnx
dotnet build Atmos.slnx --configuration Release --no-restore
dotnet test --solution Atmos.slnx --configuration Release --no-build --coverlet --results-directory ./TestResults

cd src/frontend/atmos-web
npm ci
npm run check
```
