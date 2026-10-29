# Order Management

ASP.NET Core 10 order-management application with a Clean Architecture solution:

- `OrderManagement.Domain`: customer, order, order-item, and order factory domain model.
- `OrderManagement.Application`: MediatR requests/handlers, repository contracts, specifications, discount strategies, and logging pipeline decorator.
- `OrderManagement.Infrastructure`: EF Core SQL Server persistence and Unit of Work.
- `OrderManagement.Web`: REST API controllers and Razor Pages UI.

## Run

### Start SQL Server and create the database

The default connection uses the full local SQL Server instance (`Server=localhost`).

1. Make sure the SQL Server service is running. In PowerShell, check its status:

   ```powershell
   Get-Service MSSQLSERVER
   ```

   If it is stopped, open PowerShell as Administrator and start it:

   ```powershell
   Start-Service MSSQLSERVER
   ```

2. **Before running the app for the first time, run `database update`** from the repository root. This applies the included migration and creates the `OrderManagement` database and its tables. Install the EF Core command-line tool first if needed:

   ```powershell
   dotnet tool install --global dotnet-ef --version 10.0.14
   dotnet ef database update --project src\OrderManagement.Infrastructure --startup-project src\OrderManagement.Web
   ```

   If `dotnet-ef` is already installed, update it with:

   ```powershell
   dotnet tool update --global dotnet-ef --version 10.0.14
   ```

   To use LocalDB instead of the full SQL Server service, start its instance:

   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```

   Then change `ConnectionStrings:Orders` in `src/OrderManagement.Web/appsettings.json` to `Server=(localdb)\MSSQLLocalDB;Database=OrderManagement;Trusted_Connection=True;TrustServerCertificate=True` (or set `ConnectionStrings__Orders`) and run the database update command above.

### Start the application

From the repository root, run:

```powershell
dotnet run --project src\OrderManagement.Web
```

Open [http://localhost:5069](http://localhost:5069). The order endpoints are under `/api/orders`. The UI uses Fetch and native ES modules.

For later model changes, create a migration with `dotnet ef migrations add <Name> --project src\OrderManagement.Infrastructure --startup-project src\OrderManagement.Web --output-dir Migrations`, then apply it using `dotnet ef database update`.

## API outline

- `GET /api/orders?search=&sortBy=date&page=1&pageSize=10`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PUT /api/orders/{id}`
- `DELETE /api/orders/{id}`
- `GET /api/orders/discounts`
- `POST /api/orders/discounts/preview`
