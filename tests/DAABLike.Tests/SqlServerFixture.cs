using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace DAABLike.Tests
{
    public class SqlServerFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();

        public async ValueTask InitializeAsync()
        {
            await _msSqlContainer.StartAsync();

            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("", connection);
            command.CommandText = "CREATE TABLE TestTable (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            await command.ExecuteNonQueryAsync();
            for (int i = 1; i <= 3; i++)
            {
                command.CommandText = $"INSERT INTO TestTable (Id, Value) VALUES ({i}, 'Val{i}')";
                await command.ExecuteNonQueryAsync();
            }

            command.CommandText = "CREATE TABLE TestTable2 (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            await command.ExecuteNonQueryAsync();
            for (int i = 1; i <= 3; i++)
            {
                command.CommandText = $"INSERT INTO TestTable2 (Id, Value) VALUES ({i}, 'Val{i}')";
                await command.ExecuteNonQueryAsync();
            }

            command.CommandText = "CREATE TABLE NonQueryTestTable (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            await command.ExecuteNonQueryAsync();
            for (int i = 1; i <= 6; i++)
            {
                command.CommandText = $"INSERT INTO NonQueryTestTable (Id, Value) VALUES ({i}, 'Val{i}')";
                await command.ExecuteNonQueryAsync();
            }

            command.CommandText = "CREATE TABLE NonQueryAsyncTest (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            await command.ExecuteNonQueryAsync();
            for (int i = 1; i <= 6; i++)
            {
                command.CommandText = $"INSERT INTO NonQueryAsyncTest (Id, Value) VALUES ({i}, 'Val{i}')";
                await command.ExecuteNonQueryAsync();
            }

            command.CommandText = @"
CREATE PROCEDURE GetTestTableData(@FromId INT, @ToId INT)
AS
BEGIN
  SELECT * FROM TestTable
   WHERE Id BETWEEN @FromId AND @ToId
   ORDER BY Id;
END;
";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"
CREATE PROCEDURE GetTestTablesData(@FromId INT, @ToId INT)
AS
BEGIN
  SELECT * FROM TestTable
   WHERE Id BETWEEN @FromId AND @ToId
   ORDER BY Id;
  SELECT * FROM TestTable2
   WHERE Id BETWEEN @FromId AND @ToId
   ORDER BY Id;
END;
";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"
CREATE PROCEDURE GetTestData(@Id INT)
AS
BEGIN
  SELECT Value
    FROM TestTable
   WHERE Id = @Id;
END;
";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"
CREATE PROCEDURE UpdateNonQueryTableData(@Id INT, @Value NVARCHAR(10))
AS
BEGIN
  UPDATE NonQueryTestTable
     SET Value = @Value
   WHERE Id = @Id;
END;
";
            await command.ExecuteNonQueryAsync();

            command.CommandText = @"
CREATE PROCEDURE UpdateNonQueryAsyncData(@Id INT, @Value NVARCHAR(10))
AS
BEGIN
  UPDATE NonQueryAsyncTest
     SET Value = @Value
   WHERE Id = @Id;
END;
";
            await command.ExecuteNonQueryAsync();
        }


        public ValueTask DisposeAsync()
            => _msSqlContainer.DisposeAsync();

        public string ConnectionString => _msSqlContainer.GetConnectionString();
    }
}
