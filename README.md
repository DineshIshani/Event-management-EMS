# Event Management

Event management application: ASP.NET Core 8 Web API backend and Angular 18 frontend.

## Features

- JWT authentication with `Organizer` and `Attendee` roles
- Organizers create, update and delete events and view their attendee lists
- Attendees browse/search events, register, cancel, and see their registrations
- Capacity handling: registrations beyond capacity are waitlisted and auto-promoted when someone cancels
- EF Core (SQLite) with migrations and demo seed data, Swagger UI in development

## Layout

```
backend/EventManagement.Api   ASP.NET Core 8 Web API (controllers, EF Core, JWT)
frontend                      Angular 18 standalone-component SPA
```

## Backend

```bash
cd backend/EventManagement.Api
dotnet run
```

Runs on `http://localhost:5080` (see `Properties/launchSettings.json`), applies migrations on startup and seeds demo data.
Swagger UI: `http://localhost:5080/swagger`.

Configuration (`appsettings.json`, overridable with environment variables):

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQLite connection string |
| `Jwt:Key` | HMAC signing key — required, set `Jwt__Key` in production |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes` | Token settings |
| `Cors:AllowedOrigins` | Allowed SPA origins |

A development-only signing key is provided in `appsettings.Development.json`; the app refuses to start without a key configured.

### API

| Method | Route | Auth |
| --- | --- | --- |
| POST | `/api/auth/register` | anonymous |
| POST | `/api/auth/login` | anonymous |
| GET | `/api/events?search=&upcomingOnly=` | anonymous |
| GET | `/api/events/{id}` | anonymous |
| POST | `/api/events` | Organizer |
| PUT | `/api/events/{id}` | Organizer (owner) |
| DELETE | `/api/events/{id}` | Organizer (owner) |
| GET | `/api/events/{id}/registrations` | Organizer (owner) |
| GET | `/api/registrations/me` | authenticated |
| POST | `/api/registrations/events/{eventId}` | authenticated |
| DELETE | `/api/registrations/events/{eventId}` | authenticated |

### Migrations

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Name> --project backend/EventManagement.Api
```

## Frontend

```bash
cd frontend
npm install
npm start        # http://localhost:4200
npm run build
npm test
```

The API base URL lives in `src/environments/environment.ts` (`environment.production.ts` for production builds).

## Demo accounts

| Email | Password | Role |
| --- | --- | --- |
| organizer@example.com | Password123! | Organizer |
| attendee@example.com | Password123! | Attendee |
