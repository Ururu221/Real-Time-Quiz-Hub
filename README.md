# Real-Time Quiz Hub

A web quiz app where users play quizzes and see a live leaderboard update in real time.

## Tech Stack

- ASP.NET Core 8 (Web API)
- SignalR (real-time updates)
- Entity Framework Core + PostgreSQL
- JWT authentication
- Static HTML/CSS/JavaScript frontend
- Swagger (API docs)
- Docker / Docker Compose

## Features

- **User accounts** — register and log in with JWT-based authentication.
- **Quizzes** — browse available quizzes, play through questions, and submit answers.
- **Quiz management** — admins can create and delete quizzes and manage questions and answers.
- **Real-time leaderboard** — player progress is pushed to everyone live over SignalR.
- **Scoring & levels** — answers earn points; total score maps to named levels (Новачок → Майстер вікторин).
- **Badges** — players unlock badges for milestones such as a first win, a perfect quiz, or completing several quizzes.
- **Leaderboards** — a global all-time leaderboard plus per-quiz leaderboards.
- **API documentation** — interactive Swagger UI in development.

## Getting Started

### With Docker (recommended)

1. Copy the env template and fill in real values:
   ```bash
   cp .env.example .env
   # set POSTGRES_PASSWORD and a JWT_SECRET of at least 32 characters
   ```
2. Build and start the app and database:
   ```bash
   docker-compose up --build
   ```
3. Open `http://localhost:8080`.

Database migrations are not applied automatically. Apply them from the host
(the database port `5432` is published by Compose):
```bash
# Requires EF tools: dotnet tool install --global dotnet-ef
dotnet ef database update --project RealTimeQuizHub \
  --connection "Host=localhost;Port=5432;Database=quizhub;Username=quizhub_user;Password=<your-password>"
```
Optionally seed sample quizzes: `docker-compose exec app dotnet RealTimeQuizHub.dll seed`

### Without Docker

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download) and a running PostgreSQL instance.

1. Set the connection string in `RealTimeQuizHub/appsettings.json`
   (`ConnectionStrings:DefaultConnection`) to point at your database.
2. Apply migrations:
   ```bash
   dotnet ef database update --project RealTimeQuizHub
   ```
3. Run the app:
   ```bash
   dotnet run --project RealTimeQuizHub
   ```
4. Open the URL shown in the console (e.g. `https://localhost:5001`).
   Swagger UI is available at `/swagger`.

To load sample quizzes: `dotnet run --project RealTimeQuizHub -- seed`

## Environment Variables

When running with Docker, configuration comes from a `.env` file.
See [`.env.example`](.env.example) for the full list:

- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` — database settings.
- `JWT_SECRET` — signing key for JWTs (at least 32 characters).

## Running Tests

```bash
dotnet test
```

Tests live in `RealTimeQuizHub.Tests/` and cover the auth and leaderboard
controllers and the scoring, badge, and quiz-session services.

## Project Structure

```
RealTimeQuizHub/
├─ Controllers/    REST API endpoints (auth, quizzes, scores, leaderboard, admin)
├─ Services/       Business logic (scoring, badges, sessions, quizzes)
├─ Repository/     Data access over EF Core
├─ Hubs/           SignalR hub for live leaderboard updates
├─ Models/         Entities and DTOs
├─ Migrations/     EF Core database migrations
└─ wwwroot/        Static frontend (login, quiz list, play, create-quiz)
RealTimeQuizHub.Tests/   xUnit test project
```
