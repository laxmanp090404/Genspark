# BusBook Angular Client

Sleek Angular 21 + TailwindCSS frontend aligned to the implemented server APIs.

## Implemented UI modules

- **Auth**: Login, Signup, session bootstrap with refresh flow.
- **User**: Bus search, my bookings, operator-role request.
- **Operator**: Dashboard, route management, bus management.
- **Admin**: Dashboard, route approvals, operator request approvals, platform fee config.
- **Layout**: Role-aware shell navigation with protected routes.

## Architecture choices

- **Standalone components + route guards** for clean modular structure.
- **Centralized API service** for all HTTP integration.
- **Auth interceptor** adds `withCredentials` globally.
- **Role guard + auth guard** enforce route-level access control.
- **Tailwind utility styling** for consistent sleek UI.

## Environment and secrets

No secrets are hardcoded in source.

1. Copy env template:
```bash
cp .env.example .env
```
2. Update:
```bash
NG_APP_API_BASE_URL=http://localhost:5017/api
```
3. Env is synced into `src/environments/environment.ts` automatically by scripts before `start` and `build`.

> `.env` is git-ignored.

## Run locally

```bash
npm install
npm start
```

Open `http://localhost:4200`.

## Build

```bash
npm run build
```

## Key folders

- `src/app/core`: auth, guards, interceptor, API client, shared models
- `src/app/layout`: main shell
- `src/app/pages`: auth/user/operator/admin pages
- `scripts/sync-env.mjs`: `.env` to Angular environment sync

## Notes

- Backend uses cookie-based auth; frontend always sends credentials.
- Place suggestions are fetched once and reused client-side for better UX/performance.
- Search form enforces source, destination, and date to match server constraints.
