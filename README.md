# University Administration System

Full-stack system for managing university data (students, groups, teachers, schedules). 
Built to demonstrate clean architecture, performance tracking, and proper separation of concerns in a modern .NET + Vue stack.

## Tech Stack

**Backend**
- .NET 8 / C#
- ASP.NET Core Web API
- Dapper & Entity Framework Core (used side-by-side: Dapper for high-performance read queries, EF Core for complex relational writes)
- FluentMigrator (schema versioning)
- MiniProfiler (SQL tracking and request profiling)
- Telegram Bot API (for async notifications)

**Frontend**
- Vue 3 / TypeScript
- Element Plus
- Pinia/Vuex (State management), LocalStorage

**Database & Tools**
- Microsoft SQL Server
- xUnit (TestProject)
- Git

## Solution Structure

The backend is split into logical projects to enforce architectural boundaries:

- `UCore`: Domain models, DTOs, and core business logic. No external dependencies.
- `IRepository` / `IRepositoryAll`: Contracts for data access.
- `DapperRepository` / `EFRepository`: Concrete implementations. Allows switching or combining ORM strategies based on the use case.
- `UniversityDB`: EF Core DbContext and FluentMigrator configurations.
- `Start`: API entry point. Contains DI setup, middleware pipeline (logging, exception handling, MiniProfiler).
- `ApiTelegramBot`: Isolated service for bot logic.
- `UJob`: Background processing and scheduled tasks.
- `TestProject`: Unit and integration tests for repositories and core services.

## Key Technical Details

- **Performance:** MiniProfiler is hooked up to track query execution time and identify bottlenecks during development.
- **Async:** Full `async/await` implementation with proper `CancellationToken` propagation from the API down to the database layer.
- **Resilience:** Global exception handling middleware and structured logging to catch and trace errors at the boundaries.
- **Frontend:** Reactive UI with form validation and persistent state (cookies/local storage) for auth tokens.

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js (v18+)
- Microsoft SQL Server (LocalDB or full instance)

### Backend
1. Open `University.sln` in Visual Studio or Rider.
2. Update the connection string in `Start/appsettings.json`.
3. Migrations are set to run automatically on startup. To run them manually via CLI:
   ```bash
   dotnet tool install --global FluentMigrator.DotNet.Cli
   dotnet fm migrate -p sqlserver -c "YourConnectionString" -a "UniversityDB.dll"
   ```
4. Run the `Start` project. MiniProfiler results will be available at `/mini-profiler-resources/results`.

### Frontend
1. Navigate to the `Frontend` folder.
2. Install and run:
   ```bash
   npm install
   npm run dev
   ```

### Tests
Run tests from the `TestProject` directory:
```bash
dotnet test
```
