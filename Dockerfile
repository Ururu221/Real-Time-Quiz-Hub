# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore as a separate layer for better caching: copy only the project file first.
COPY ["RealTimeQuizHub/RealTimeQuizHub.csproj", "RealTimeQuizHub/"]
RUN dotnet restore "RealTimeQuizHub/RealTimeQuizHub.csproj"

# Copy the rest of the application source and publish a Release build.
COPY RealTimeQuizHub/ RealTimeQuizHub/
RUN dotnet publish "RealTimeQuizHub/RealTimeQuizHub.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# The published output already contains wwwroot (static web assets) and
# Data/questions.json, so copying the publish folder brings them into the image.
COPY --from=build /app/publish ./

# Serve over plain HTTP on 8080 inside the container.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RealTimeQuizHub.dll"]
