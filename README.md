# DAABLike

**DAABLike** is a DAAB-style data access library for .NET and .NET Framework.

It provides a familiar and straightforward way to perform database operations using ADO.NET, including creating database commands, executing queries, managing transactions, and working with `DataSet` objects.

## Installation

Install DAABLike from NuGet.

Using the .NET CLI:

```bash
dotnet add package DAABLike
```

Using the Package Manager Console:

```powershell
Install-Package DAABLike
```

## Getting Started

DAABLike supports both .NET and .NET Framework. The way you create a `Database` instance differs depending on the target framework.

### .NET

On .NET, create a `Database` instance by specifying a connection string and a `DbProviderFactory`.

For example, when using SQL Server:

```csharp
using DAABLike;
using Microsoft.Data.SqlClient;

var db = new Database(
    "Server=localhost;Database=MyDatabase;Integrated Security=True",
    SqlClientFactory.Instance);
```

You can then use the `Database` instance to perform database operations.

### .NET Framework

On .NET Framework, you can create a `Database` instance directly from the connection strings configured in `app.config`.

Add a connection string named `DefaultConnection` to the `connectionStrings` section:

```xml
<configuration>
  <connectionStrings>
    <add name="DefaultConnection"
         connectionString="Data Source=localhost;Initial Catalog=MyDatabase;Integrated Security=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

Then create a `Database` instance using `DatabaseFactory.CreateDatabase()`:

```csharp
using DAABLike;

var db = DatabaseFactory.CreateDatabase();
```

The `DatabaseFactory` automatically creates the database provider factory using the connection string configuration.

## Basic Usage

Once you have created a `Database` instance, you can use its methods to execute database operations.

For example, to execute a SQL query:

```csharp
using (var command = db.GetSqlStringCommand("SELECT COUNT(*) FROM Users"))
{
    var count = db.ExecuteScalar(command);
}
```

To execute a stored procedure:

```csharp
using (var command = db.GetStoredProcCommand("GetUsers"))
using (var reader = db.ExecuteReader(command))
{
    while (reader.Read())
    {
        // Process each row.
    }
}
```

See the API documentation and examples for more details.

## Working with Multiple Databases

DAABLike supports multiple database configurations, allowing you to connect to different databases within the same application.

### .NET Framework

On .NET Framework, you can define multiple connection strings in the `connectionStrings` section of `app.config`.

```xml
<configuration>
  <connectionStrings>
    <add name="DefaultConnection"
         connectionString="Data Source=localhost;Initial Catalog=MainDatabase;Integrated Security=True"
         providerName="System.Data.SqlClient" />
    <add name="Reporting"
         connectionString="Data Source=localhost;Initial Catalog=ReportingDatabase;Integrated Security=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

Use `DatabaseFactory.CreateDatabase()` to create a database instance using the default connection string, or specify the connection string name to select another database.

```csharp
var defaultDb = DatabaseFactory.CreateDatabase();
var reportingDb = DatabaseFactory.CreateDatabase("Reporting");
```

### Using DatabaseProviderFactoryBuilder

On .NET, or when you want to configure multiple databases directly in code on either .NET or .NET Framework, you can use `DatabaseProviderFactoryBuilder`.

```csharp
var factory = DatabaseProviderFactory.Builder()
    .RegisterDefault(
        "Data Source=localhost;Initial Catalog=MainDatabase;Integrated Security=True",
        SqlClientFactory.Instance)
    .Register(
        "Reporting",
        "Data Source=localhost;Initial Catalog=ReportingDatabase;Integrated Security=True",
        SqlClientFactory.Instance)
    .Build();

var defaultDb = factory.CreateDefault();
var reportingDb = factory.Create("Reporting");
```

The builder allows you to register multiple database configurations and select them by name when creating `Database` instances.

## Supported Frameworks

DAABLike targets the following frameworks:

- .NET 7 or later
- .NET Standard 2.1
- .NET Framework 3.5 or later

## Documentation

This documentation was prepared with assistance from AI.

## License

DAABLike is licensed under the [MIT License](LICENSE.txt).
