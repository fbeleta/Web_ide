# Multi-stage build:
# Stage 1 (tailwind): Node 22 Alpine — compile Tailwind CSS from source
# Stage 2 (build):    .NET SDK 10   — restore + publish ASP.NET app
# Stage 3 (runtime):  ASP.NET 10    — minimal runtime image

# ── Stage 1: Tailwind CSS build ───────────────────────────────────────────────
FROM node:22-alpine AS tailwind

WORKDIR /src

# Install Tailwind CLI and forms plugin
COPY tailwind/package*.json ./tailwind/
RUN cd tailwind && npm install --ignore-scripts 2>/dev/null || \
    npm install tailwindcss @tailwindcss/forms --ignore-scripts

COPY tailwind/ ./tailwind/
COPY WebIde.Frontend/Views/ ./WebIde.Frontend/Views/
COPY WebIde.Frontend/wwwroot/js/ ./WebIde.Frontend/wwwroot/js/

RUN npx --prefix ./tailwind tailwindcss \
      -c ./tailwind/tailwind.config.js \
      -i ./tailwind/input.css \
      -o ./WebIde.Frontend/wwwroot/css/site.tailwind.css \
      --minify

# ── Stage 2: .NET build + publish ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restore dependencies (layer-cached if csproj files unchanged)
COPY WebIde.Model/WebIde.Model.csproj       WebIde.Model/
COPY WebIde.DAL/WebIde.DAL.csproj           WebIde.DAL/
COPY WebIde.Frontend/WebIde.Frontend.csproj WebIde.Frontend/
RUN dotnet restore WebIde.Frontend/WebIde.Frontend.csproj

# Copy source
COPY WebIde.Model/   WebIde.Model/
COPY WebIde.DAL/     WebIde.DAL/
COPY WebIde.Frontend/ WebIde.Frontend/

# Bring in the compiled Tailwind CSS from stage 1
COPY --from=tailwind /src/WebIde.Frontend/wwwroot/css/site.tailwind.css \
                          WebIde.Frontend/wwwroot/css/site.tailwind.css

RUN dotnet publish WebIde.Frontend/WebIde.Frontend.csproj \
      -c Release \
      -r linux-x64 \
      --self-contained false \
      -o /app/publish

# ── Stage 3: Runtime image ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Non-root user for defense-in-depth
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebIde.Frontend.dll"]
