# Library Information System — C# Windows Forms

A desktop application built with **C# .NET 6 Windows Forms** and **MySQL 8**, with a UI inspired by the LibraryIS_App.html design system.

---

## Setup Instructions

### 1. Database
1. Open MySQL Workbench (or any MySQL client).
2. Run the included `library_system_full.sql` to create the `library_system` database, all tables, views, and seed data.

### 2. Connection Settings
Open `DatabaseConnection.cs` and update if needed:
```csharp
private static readonly string Server   = "localhost";
private static readonly string Database = "library_system";
private static readonly string Username = "root";
private static readonly string Password = "";   // your MySQL password
private static readonly uint   Port     = 3306;
```

### 3. Build & Run
```bash
# Restore NuGet packages and build
dotnet restore
dotnet run
```
Or open `LibraryIS.csproj` in Visual Studio 2022 and press **F5**.

---

## Project Structure

| File | Purpose |
|---|---|
| `Program.cs` | Entry point |
| `DatabaseConnection.cs` | MySQL singleton — all DB calls |
| `PasswordHelper.cs` | SHA-256 hash / verify / OTP pin |
| `UIHelper.cs` | Design tokens + UI factory helpers |
| `Form1_Login.cs` | Login form (two-column hero layout) |
| `FormPasswordRecovery.cs` | 3-step OTP password recovery |
| `FormUserManagement.cs` | Add / Edit / Activate / Deactivate members |
| `FormDashboard.cs` | Main dashboard — sidebar nav + content panels |
| `library_system_full.sql` | Full database schema + seed data |

---

## Design System

Colors match the HTML prototype:

| Token | Hex |
|---|---|
| Navy | `#0F1B2D` |
| Gold | `#E8A923` |
| Cream | `#F7F3EC` |
| Muted | `#7A8FA6` |
| Green | `#2ECC71` |
| Red | `#E74C3C` |
| Blue | `#3498DB` |

---

## Requirements
- .NET 6 SDK (Windows)
- MySQL 8.0+
- NuGet: `MySql.Data 8.3.0` (auto-restored by `dotnet restore`)
