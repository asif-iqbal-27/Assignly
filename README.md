# Assignly

A role-based assignment and submission system for a school. There are three roles.
An Admin manages classes, subjects, user accounts, and which teacher is assigned to
which subject. A Teacher creates and publishes assignments for the subjects they are
assigned to, and grades student submissions. A Student sees published assignments for
their own class, submits work, and can resubmit if the assignment allows it and the
deadline has not passed.

## Tech stack

| Layer    | Stack |
|----------|-------|
| Backend  | ASP.NET Core (.NET 8), EF Core + Npgsql, ASP.NET Core Identity + JWT Bearer, MediatR, FluentValidation, ErrorOr, Serilog, Swashbuckle |
| Frontend | Next.js 16 (App Router), React 19, TypeScript, plain CSS, a small typed `fetch` wrapper |
| Database | PostgreSQL 16, run via Docker |
| Tests    | xUnit, FluentAssertions, EF Core InMemory |

## Demo credentials

| Role    | Username   | Password     |
|---------|------------|--------------|
| Admin   | `admin`    | `Admin123!`  |
| Teacher | `teacher1` | `Teacher123!`|
| Student | `student1` | `Student123!`|

More teacher (`teacher2`) and student (`student2`–`student4`) accounts are seeded too,
all with the same pattern of password.

## Setup

### 1. Database

```bash
docker compose up -d
```

Starts PostgreSQL 16 in a container named `assignly-db`, listening on `localhost:5433`.

### 2. Backend

```bash
cp Assignly.Host/appsettings.Example.json Assignly.Host/appsettings.json
dotnet run --project Assignly.Host
```

The first command creates your local config from the placeholder file. The second
builds and runs the API. **The backend must run in the `Development` environment for
the database migration and demo data seeding to happen** — `dotnet run` does this
automatically via `Assignly.Host/Properties/launchSettings.json`; if you run the
published DLL directly instead, set `ASPNETCORE_ENVIRONMENT=Development` yourself.

Runs on `http://localhost:5058`. Swagger UI is at `http://localhost:5058/swagger`.

### 3. Frontend

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

Runs on `http://localhost:3000` and talks to the backend at the URL in `.env.local`.

## Running the tests

```bash
dotnet test
```

26 tests, all at the handler level against a real `ApplicationDbContext` (EF Core
InMemory) wrapped in the real `Repository<T>` — no mocked repositories.

## Project structure

```
Assignly.Domain/           Entities and enums. No dependencies on anything else.
Assignly.Application/      CQRS commands/queries, validators, handlers. The actual business logic.
Assignly.Infrastructure/   EF Core DbContext, repository, JWT issuing, demo data seeder.
Assignly.Host/             ASP.NET Core Web API — controllers, auth wiring, Program.cs.
Assignly.Tests/            xUnit tests for the business rules in Application.
frontend/                  Next.js app — one route group per role.
```

## How it works

Logging in returns a JWT carrying the user's role, and — for students only — the id of
their class. Every other endpoint reads that token and checks two things, in order:
first, a broad `[Authorize(Roles = "...")]` on the controller action, which is enough to
tell a Student from a Teacher from an Admin; second, inside the handler itself, an
ownership check against the database, because the role attribute alone can't tell
whether *this* teacher is allowed to touch *this* assignment.

That ownership check is what actually scopes the data. Teachers are linked to subjects
through a `ClassSubjectTeacher` row — a teacher can only create, update, delete,
publish, or grade against a subject they hold that link for, even if they know the
id of a row that belongs to someone else. Students are linked to a class directly on
their user record, and can only see Published assignments for that one class — a Draft,
or another class's assignment, returns the same 404 as an assignment that doesn't
exist, so ids can't be enumerated to find out what's there. Admin has no such
restriction and can see everything.

## Assumptions

- One user table (`ApplicationUser`) with a `Role` enum covers all three roles. Role
  rules are enforced in the handlers, not in the database schema.
- Resubmitting an assignment inserts a new row with a higher `AttemptNumber` rather
  than editing the old one. Queries always return the latest attempt.
- Resubmission requires `AllowResubmission == true` **and** the deadline not yet
  passed. `AllowLateSubmission` does not extend that window — the two flags are
  independent.
- Submissions are text only.
- A student belongs to exactly one class. A subject belongs to exactly one class.

## Known limitations

- The JWT is stored in `localStorage` with no refresh token, so deactivating a user
  leaves their existing token valid until it expires on its own.
- No pagination anywhere. Fine for the amount of seed data here, not for a real
  school's worth of records.
- `Submission.FileUrl` exists as a column but there is no upload endpoint that writes
  to it.
- No notifications.
- Demo data seeding only runs in the `Development` environment.
- Password policy is relaxed to 4 characters, to keep demo logins easy to type.
- Unit tests cover the business rules at the handler level. Role-based 401/403
  enforcement (the `[Authorize]` attributes themselves) was checked manually against
  the running API, not by an automated integration test.
