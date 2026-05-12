# GarageControl

A full-stack workshop management system for automotive service shops. GarageControl lets you manage work orders, jobs, mechanics, clients, vehicles, parts inventory, and more — all from one place.

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
| **Dashboard** | Overview of active orders, jobs by status, and key metrics |
| **Orders** | Create and manage work orders with multiple jobs per order |
| **Jobs** | Track job progress, assign mechanics, log time slots and parts |
| **Parts Stock** | Hierarchical parts inventory with folder organisation, deficit tracking, and transfers |
| **Workers** | Manage mechanics and staff; assign roles and schedules |
| **Clients** | Client directory with linked vehicles and order history |
| **Cars** | Vehicle registry with makes, models, and service history |
| **To Do** | Mechanic-facing task list for active assigned jobs |
| **Activity Log** | Full audit trail of all system actions |
| **Job Types** | Define and customise categories of work |
| **Makes & Models** | Manage vehicle brands and model lookup tables |
| **Workshop Details** | Configure your workshop profile |
| **Notifications** | In-app notification system with unread badge |
| **Export** | Export data to Excel and PDF |
| **Admin Panel** | Platform-level administration (users, workshops, makes/models) |
| **Auth** | Username/password login, registration, Google and Microsoft OAuth |

---

## Tech Stack

**Frontend**
- React 18 (Vite)
- React Router v6
- Vanilla CSS with CSS custom properties

**Backend**
- ASP.NET Core 10.0 (C#)
- Entity Framework Core (PostgreSQL)
- ASP.NET Core Identity
- JWT authentication

**Infrastructure**
- Docker (multi-stage build)
- PostgreSQL
- Deployed on [Render](https://render.com)

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
    "Issuer": "GarageControl",
    "Audience": "GarageControlUsers"
  }
}
```

Run database migrations and start the backend:

```bash
cd backend
dotnet restore
dotnet ef database update --project GarageControl.Infrastructure --startup-project GarageControl
dotnet run --project GarageControl
```

The API will be available at `https://localhost:5001`.

#### 3. Start the frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend dev server will be available at `http://localhost:5173` and will proxy API requests to the backend.

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
| `To Do` | Mechanic task list |
| `Parts Stock` | Parts inventory |
| `Workers` | Worker management |
| `Clients` | Client directory |
| `Cars` | Vehicle registry |
| `Activity Log` | Audit trail |
| `Job Types` | Job type configuration |
| `Makes and Models` | Vehicle makes and models |
| `Workshop Details` | Workshop settings |
| `Admin` | Platform admin panel |

---

## Environment Variables

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | Secret key for signing JWT tokens (min 32 chars) |
| `Jwt__Issuer` | JWT issuer (e.g. `GarageControl`) |
| `Jwt__Audience` | JWT audience (e.g. `GarageControlUsers`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `ASPNETCORE_HTTP_PORTS` | Port the server listens on (default `10000`) |