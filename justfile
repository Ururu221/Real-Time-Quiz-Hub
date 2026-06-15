# Image name MUST match `app_services.app.image` in appspec.yaml.
# `simpled app-bundle create` looks for <image>:latest locally, then retags it to
# ghcr.io/plaingoodtech/quiz-hub-app:<version> and pushes it.
image := "plaingood/quiz-hub-app"

# Default recipe: list available commands.
default:
    @just --list

# Build the app image as <image>:latest. This is the recipe the release workflow
# (and simpled) calls before `simpled app-bundle create`.
build:
    docker buildx build --load -t {{image}}:latest .

# --- Local convenience commands (docker-compose) ---

# Start app + db locally (rebuilds the app image).
run:
    docker compose up --build -d

# Stop and remove the local containers.
down:
    docker compose down

# Tail the app logs.
logs:
    docker compose logs -f app

# Apply EF Core migrations against the database published on localhost:5432.
migrate:
    dotnet ef database update --project RealTimeQuizHub
