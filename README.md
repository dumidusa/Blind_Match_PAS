# Blind Match PAS — Project Allocation System
 
An ASP.NET Core 9 MVC web application that implements a **blind matching** workflow between students and supervisors for academic project allocation. Student identities remain hidden from supervisors until a match is confirmed.
 
---
 
## Tech Stack
 
| | |
|---|---|
| Framework | ASP.NET Core 9 MVC |
| ORM | Entity Framework Core 9 |
| Database | Microsoft SQL Server |
| Auth | ASP.NET Core Cookie Authentication + BCrypt |
| Frontend | Razor Views + Tailwind CSS (CDN) |
 
---
 
## Prerequisites
 
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Microsoft SQL Server (local or Docker)
- `dotnet-ef` CLI tool — install with:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
 
---
 
## Getting Started
 
### 1. Clone & restore
 
```bash
git clone <repository-url>
cd Blind_Match_PAS
dotnet restore
```
 
### 2. Configure the database
 
Edit `appsettings.json` and update the connection string:
 
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=BlindMatchPAS;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```
 
### 3. Apply migrations
 
```bash
dotnet ef database update
```
 
### 4. Run
 
```bash
dotnet run
```
 
Open `http://localhost:5277` in your browser.
 
---
 
## User Roles
 
| Role | What they do |
|---|---|
| **Student** | Submit and manage a project proposal |
| **Supervisor** | Browse anonymised proposals, express interest, confirm a match |
| **Module Leader** | Manage research areas, monitor all allocations |
| **Admin** | User management, system diagnostics |
 
---
 
## Project Workflow
 
```
Student submits proposal
        │
        ▼
    [ PENDING ]
        │
        │  Supervisor expresses interest (student name hidden)
        ▼
  [ UNDER REVIEW ]
        │
        │  Supervisor confirms match
        ▼
    [ MATCHED ]  ← student identity revealed
 
  Student can Withdraw at any point before MATCHED
```
 
---
