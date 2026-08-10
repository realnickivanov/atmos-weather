# Security policy

## Reporting a vulnerability

Please report security issues privately through the repository security advisory feature. Do not open a public issue for a suspected vulnerability.

## Secrets

The WeatherAPI key is a backend-only secret. Keep it in environment variables or a local `.env` file and never commit it to source control. The React application does not receive or expose the provider key.
