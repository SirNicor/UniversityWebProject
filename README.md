# University Administration System

Full-stack system for managing university data (students, groups, teachers, schedules). 
Built to demonstrate clean architecture, performance tracking, and proper separation of concerns in a modern .NET + Vue stack.

## Tech Stack

**Backend**
- .NET 9 / C#
- ASP.NET Core Web API
- Dapper & Entity Framework Core (used side-by-side: Dapper for high-performance read queries, EF Core for complex relational writes)
- FluentMigrator (schema versioning)
- MiniProfiler (SQL tracking and request profiling)
- Telegram Bot API (for async notifications)
- Dependency Injection
- Minimal API

**Frontend**
- Vue 3 / TypeScript
- Element Plus
- Pinia (State management), LocalStorage

**Database & Tools**
- Microsoft SQL Server
- Git
- API DaData

## Solution Structure

The backend is split into logical projects to enforce architectural boundaries:

- `UCore`: Domain models, DTOs, and core business logic. No external dependencies.
- `IRepository` / `IRepositoryAll`: Contracts for data access.
- `DapperRepository` / `EFRepository`: Concrete implementations. Allows switching or combining ORM strategies based on the use case.
- `UniversityDB`: EF Core DbContext and FluentMigrator configurations.
- `Start`: API entry point. Contains DI setup, middleware pipeline (logging, exception handling, MiniProfiler).
- `ApiTelegramBot`: Isolated service for telegram bot logic.
- `UJob`: Background processing and scheduled tasks.

## Key Technical Details

- **Performance:** MiniProfiler is hooked up to track query execution time and identify bottlenecks during development. Outputting critical data about working with the database to logs
- **Async:** Full `async/await` implementation with proper `CancellationToken` propagation from the API down to the database layer.
- **Resilience:** Global exception handling middleware and structured logging to catch and trace errors at the boundaries.
- **Safety** Using JWT tokens for authorization and authentication via middleware; role validation using .NET policies.
- **Deployability** Using FluenMigrator to create migrations and the ability to automatically deploy the database if it is missing from the target system.
- **Adaptability** Using JSON-based configuration to influence critical project elements without modifying the code.
- **Cohesion** Using DI to connect system components and ensure the cohesion of the entire program
- **Repeatability** Collecting the necessary statistical data using a CronJob with a configurable frequency.
- **Repository** Using the Repository pattern to separate concerns and encapsulate database interaction logic.
- **Three-Layered architecture** Use of a three-Layered architecture to separate responsibilities for routing, the service layer, and the database layer.
- **SOLID** - Using SOLID to improve code readability, scalability, and maintainability. Activity use Interface Segregation Principle.
- **REST API** - Implementation of RESTful requests. Minimal APIs were used for the implementation.	
- **Validation of sensitive data** - Using the Dadata API for address validation.
- **Frontend:** Reactive UI with form validation and persistent state (cookies/local storage) for auth tokens.

## Getting Started

### Prerequisites
- .NET 9 SDK
- TypeScript, npm, Vue, Element plus
- Microsoft SQL Server (LocalDB or full instance)

### Backend
1. Open `University.sln` in Visual Studio or Rider.
2. Update the connection string in `Start/appsettings.json`. Enter your values ​​for DaData keys and Telegram bot tokens. 
3. Migrations are set to run automatically on startup. To run them manually via CLI:
   ```bash
   dotnet tool install --global FluentMigrator.DotNet.Cli
   dotnet fm migrate -p sqlserver -c "YourConnectionString" -a "UniversityDB.dll"
   ```
4. Run the `Start` project. MiniProfiler results will be available at `/mini-profiler-resources/results` and check in log.

### Frontend
1. Navigate to the `Frontend` folder.
2. Install and run:
   ```bash
   npm install
   npm run dev
   ```
