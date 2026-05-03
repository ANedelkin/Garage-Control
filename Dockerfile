# Stage 1: Build the frontend (React/Vite)
FROM node:20 AS frontend-build
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build -- --outDir dist

# Stage 2: Build the backend (.NET)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app
COPY backend/ ./
RUN dotnet restore GarageControl/GarageControl.csproj
RUN dotnet publish GarageControl/GarageControl.csproj -c Release -o /app/publish

# Stage 3: Setup the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy backend files
COPY --from=backend-build /app/publish .

# Copy frontend static files into wwwroot so ASP.NET Core can serve them
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Optional: Add cultures for localization support
RUN apt-get update && apt-get install -y --no-install-recommends locales && rm -rf /var/lib/apt/lists/*

# Render provides a PORT env variable. ASPNETCORE_HTTP_PORTS tells .NET to listen on it.
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "GarageControl.dll"]
