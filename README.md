# Movies App (Vite + .NET)

This is a full-stack web application built with a React frontend (using Vite) and a .NET backend.

## 🚀 Tech Stack

- **Frontend:** React, TypeScript, Vite, Tailwind CSS
- **Backend:** .NET 8, Entity Framework Core
- **Database:** PostgreSQL

## 📋 Prerequisites

Before you begin, ensure you have the following installed:
- [Node.js](https://nodejs.org/) (or [Bun](https://bun.sh/))
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)

## 🛠️ Setup & Installation

### 1. Database Setup

Ensure your PostgreSQL service is running. Update the connection string in `server/appsettings.json` if your credentials differ from the default:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=MoviesDb;Username=postgres;Password=admin"
}
```

### 2. Backend Setup (Server)

Navigate to the server directory:

```bash
cd server
```

Restore dependencies:

```bash
dotnet restore
```

Apply migrations and update the database:

```bash
dotnet ef database update
```

Run the server:

```bash
dotnet run
```

The API will be available at `http://localhost:5292` (or your configured port).

### 3. Frontend Setup (Client)

Navigate to the client directory:

```bash
cd client
```

Install dependencies:

```bash
npm install
# or
bun install
```

Start the development server:

```bash
npm run dev
# or
bun dev
```

The application will be available at `http://localhost:5173`.

## 🧪 Troubleshooting

- **Database Connection Error:** Ensure PostgreSQL is running and the credentials in `appsettings.json` are correct.
- **Port Conflicts:** If ports 5173 or 5292 are in use, you may need to kill the process using them or configure different ports.
