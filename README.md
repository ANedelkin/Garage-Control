# GarageControl

A full-stack workshop management system for automotive service shops. GarageControl lets you manage work orders, jobs, mechanics, clients, vehicles, parts inventory, and more — all in one place.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Development](#local-development)
  - [Docker](#docker)
- [Deployment](#deployment)
- [Access Control](#access-control)
- [Environment Variables](#environment-variables)

---

## Features

| Module | Description |
|---|---|
| **Dashboard** | A dashboard with all key metrics and stats of the workshop |
| **Orders** | Create and manage orders with multiple jobs per order |
| **Jobs** | Track job progress, assign mechanics, time slots and parts |
| **Parts Stock** | Hierarchical parts inventory with folder organisation and deficit tracking |
| **Workers** | Manage mechanics and staff; manage access and schedules |
| **Clients** | Clients list with their details and vehicles|
| **Cars** | Vehicle registry showing cars and their details |
| **To Do** | Task list of active assigned jobs for a mechanic |
| **Activity Log** | Full audit trail of all workshop actions |
| **Job Types** | Define and customise categories of work |
| **Makes & Models** | Manage vehicle makes and models |
| **Workshop Details** | Configure your workshop profile |
| **Notifications** | In-app notification system with read/unread tracking |
| **Export** | Export data to Excel and PDF |
| **Admin Panel** | Platform-level administration (users, workshops, makes/models) |
| **Auth** | Username/password login, registration, Google and Microsoft OAuth |

---

## Tech Stack

**Frontend**
- React 19 (Vite)
- React Router v7
- Vanilla CSS with CSS custom properties

**Backend**
- ASP.NET Core 10.0 (C#)
- Entity Framework Core (PostgreSQL)
- ASP.NET Core Identity
- JWT authentication

**Infrastructure**
- Docker (multi-stage build)
- PostgreSQL
- Deployed on [Render](https://garage-control.onrender.com)

---

## Project Structure

```
Garage-Control/
├── frontend/                  # React/Vite application
│   └── src/
│       ├── components/        # UI components (auth, orders, parts, workers, …)
│       ├── services/          # API client functions
│       ├── context/           # Auth, Popup, Status React contexts
│       ├── hooks/             # Custom hooks
│       └── assets/css/        # Global and module-level stylesheets
│
├── backend/
│   ├── GarageControl/         # ASP.NET Core entry point (controllers, Program.cs)
│   ├── GarageControl.Core/    # Business logic, services, ViewModels, contracts
│   ├── GarageControl.Infrastructure/ # EF Core DbContext, models, migrations
│   └── GarageControl.Shared/  # Constants, enums shared across layers
│
├── Dockerfile                 # Multi-stage Docker build
└── render.yaml                # Render deployment configuration
```

---

## Getting Started

### Prerequisites

- [Node.js 20+](https://nodejs.org/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/)

### Local Development

#### 1. Clone the repository

```bash
git clone https://github.com/your-org/Garage-Control.git
cd Garage-Control
```

#### 2. Set up the backend

Create `backend/GarageControl/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=garagecontrol;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Key": "your-super-secret-jwt-key-at-least-32-chars",
    "Issuer": "https://localhost:5173",
    "Audience": "https://localhost:5173"
  },
  "Google": {
    "ClientSecret": "your-google-oauth-client-secret"
  },
  "Microsoft": {
    "ClientSecret": "your-microsoft-oauth-client-secret"
  },
  "Admin": {
    "Password": "your-admin-password"
  }
}
```

> [!NOTE]
> `appsettings.json` already contains the non-secret parts of the OAuth config (Client IDs, Tenant ID, etc.). Only the secrets above need to be added to `appsettings.Development.json`, which should **never** be committed to source control.

Run database migrations and start the backend:

```bash
cd backend
dotnet restore
dotnet ef database update --project GarageControl.Infrastructure --startup-project GarageControl
dotnet run --project GarageControl
```

#### 3. Start the frontend

```bash
cd frontend
npm install
npm run dev
```

The API and frontend will both be available at `https://localhost:5173`.

---

### Docker

Build and run the full application in a single container:

```bash
docker build -t garage-control .
docker run -p 10000:10000 \
  -e ConnectionStrings__DefaultConnection="Host=..." \
  -e Jwt__Key="your-jwt-secret" \
  -e Jwt__Issuer="GarageControl" \
  -e Jwt__Audience="GarageControlUsers" \
  garage-control
```

---

## Deployment

The project is configured for one-click deployment to [Render](https://render.com) via `render.yaml`.

It provisions:
- A **Web Service** running the Docker image
- A managed **PostgreSQL** database

All environment variables (JWT secret, database connection string) are injected automatically by Render.

---

## Access Control

GarageControl uses a role/access system. Each user is granted a set of named accesses that determine which modules they can see and use:

| Access | Module |
|---|---|
| `Dashboard` | Home dashboard |
| `Orders` | Work orders and jobs |
| `Parts Stock` | Parts inventory |
| `Workers` | Worker management |
| `Clients` | Client directory |
| `Cars` | Vehicle registry |
| `Activity Log` | Audit trail |
| `Job Types` | Job type configuration |
| `Makes and Models` | Vehicle makes and models |
| `Workshop Details` | Workshop settings |
| `Admin` | Admin pages |

---

## Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | Secret key for signing JWT tokens (min 32 chars) |
| `Jwt__Issuer` | JWT issuer — set to the app URL (e.g. `https://localhost:5173`) |
| `Jwt__Audience` | JWT audience — set to the app URL (e.g. `https://localhost:5173`) |
| `Google__ClientSecret` | Google OAuth client secret (get from Google Cloud Console) |
| `Microsoft__ClientSecret` | Microsoft OAuth client secret (get from Azure App Registrations) |
| `Admin__Password` | Password for the built-in admin account (`Admin__Username` is set in `appsettings.json`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `ASPNETCORE_HTTP_PORTS` | Port the server listens on (default `10000` on Render) |