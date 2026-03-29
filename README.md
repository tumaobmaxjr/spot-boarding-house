# SpotBoardingHouse

SpotBoardingHouse is a monorepo containing:
- ASP.NET Core Web API backend
- React + TypeScript frontend

## Projects

- `SpotBoardingHouse.Api` - REST API (JWT auth, roles/users endpoints)
- `SpotBoardingHouse.Application` - application services/interfaces
- `SpotBoardingHouse.Domain` - domain entities
- `SpotBoardingHouse.Infrastructure` - EF Core, repositories, migrations, seeders
- `SpotBoardingHouse.Web` - React + TypeScript frontend (Vite)

## Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Node.js (v20+) & npm](https://nodejs.org/)
- SQL Server LocalDB *(or your configured SQL Server)* 

## Backend Setup (API)

1. Restore & Build
First, navigate to the root directory and restore dependencies:
```
dotnet restore
dotnet build
```

2. Configure secrets (from repo root):
Initialize and set your local development secrets (JWT key and Admin password):
```
# Initialize User Secrets
dotnet user-secrets init --project "SpotBoardingHouse.Api/SpotBoardingHouse.Api.csproj"

# Set JWT Key (Must be 32+ characters)
dotnet user-secrets set "Jwt:Key" "your-super-long-random-secret-key-32-plus-chars" --project "SpotBoardingHouse.Api/SpotBoardingHouse.Api.csproj"

# Set Initial Admin Password
dotnet user-secrets set "SeedAdmin:Password" "YourStrongPassword123!" --project "SpotBoardingHouse.Api/SpotBoardingHouse.Api.csproj"
```

3. Database Migaration
Ensure your connection string in appsettings.Development.json is correct, then update the database:
```
dotnet ef database update --project Project.Infrastructure --startup-project Project.Api
```

4. Run API (HTTPS profile):
```bash
dotnet run --project "SpotBoardingHouse.Api" --launch-profile https
```
- API URL: `https://localhost:7289`
- Swagger UI: `https://localhost:7289/swagger`

## Frontend Setup (React)

1. Install dependencies:
```
cd SpotBoardingHouse.Web
npm install
```

2. Environment Configuration:
Create a `.env.development` file in the `Project.Web` folder:
```
VITE_API_URL=https://localhost:7289
```

3. Run frontend:
```
npm run dev
```
Frontend URL: `http://localhost:5173`

## CORS (Important)
For React local development, backend CORS must allow:
```
http://localhost:5173
https://localhost:5173 (optional if using https dev server)
```

## Authentication
- Login endpoint: `POST /api/v1/Auth/login`
- JWT token is required for protected admin endpoints:
  `/api/v1/Users`
  `/api/v1/Roles`

- Admin seed user email defaults to: `admin@spotboardinghouse.com`
- Password: Defined in your local User Secrets (`SeedAdmin:Password`).

## Development Workflow
- Run backend and frontend in separate terminals
- Build small vertical features (API + UI together)
- Commit frequently with conventional commit messages

## Useful Commands
Backend build:
```
dotnet build
```

Run API:
```bash
dotnet run --project "SpotBoardingHouse.Api" --launch-profile https
```

Frontend dev:
```bash
cd SpotBoardingHouse.Web
npm run dev
```

Frontend build:
```bash
npm run build
```
