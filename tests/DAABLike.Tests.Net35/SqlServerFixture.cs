using System.Data.SqlClient;

namespace DAABLike.Tests.Net35
{
    public class SqlServerFixture : IDisposable
    {
        private const string TestDatabaseName = "DAABLikeTestDB";

        public string ConnectionString { get; }

        public SqlServerFixture()
        {
            ConnectionString = $@"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;Database={TestDatabaseName};";

            EnsureDatabaseCreated();
            InitializeSchemaAndData();
        }

        private void EnsureDatabaseCreated()
        {
            using var connection = new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;Database=master;");
            connection.Open();
            using var checkCommand = new SqlCommand($"SELECT DB_ID('{TestDatabaseName}')", connection);
            var result = checkCommand.ExecuteScalar();
            bool exists = (result != DBNull.Value && result != null);
            if (!exists)
            {
                using var createCommand = new SqlCommand($"CREATE DATABASE [{TestDatabaseName}]", connection);
                createCommand.ExecuteNonQuery();
            }
        }

        private void InitializeSchemaAndData()
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            using var command = new SqlCommand("", connection);
            command.CommandText = "DROP TABLE IF EXISTS TestTable";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE TestTable (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            command.ExecuteNonQuery();
            for (int i = 1; i <= 3; i++)
            {
                command.CommandText = $"INSERT INTO TestTable (Id, Value) VALUES ({i}, 'Val{i}')";
                command.ExecuteNonQuery();
            }

            command.CommandText = "DROP TABLE IF EXISTS TestTable2";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE TestTable2 (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            command.ExecuteNonQuery();
            for (int i = 1; i <= 3; i++)
            {
                command.CommandText = $"INSERT INTO TestTable2 (Id, Value) VALUES ({i}, 'Val{i}')";
                command.ExecuteNonQuery();
            }

            command.CommandText = "DROP TABLE IF EXISTS NonQueryTestTable";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE NonQueryTestTable (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            command.ExecuteNonQuery();
            for (int i = 1; i <= 6; i++)
            {
                command.CommandText = $"INSERT INTO NonQueryTestTable (Id, Value) VALUES ({i}, 'Val{i}')";
                command.ExecuteNonQuery();
            }

            command.CommandText = "DROP PROCEDURE IF EXISTS GetTestTableData";
            command.ExecuteNonQuery();
            command.CommandText = @"
CREATE PROCEDURE GetTestTableData(@FromId INT, @ToId INT)
AS
BEGIN
  SELECT * FROM TestTable
   WHERE Id BETWEEN @FromId AND @ToId
   ORDER BY Id;
END;
";
            command.ExecuteNonQuery();

            command.CommandText = "DROP PROCEDURE IF EXISTS GetTestTablesData";
            command.ExecuteNonQuery();
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
            command.ExecuteNonQuery();

            command.CommandText = "DROP PROCEDURE IF EXISTS GetTestData";
            command.ExecuteNonQuery();
            command.CommandText = @"
CREATE PROCEDURE GetTestData(@Id INT)
AS
BEGIN
  SELECT Value
    FROM TestTable
   WHERE Id = @Id;
END;
";
            command.ExecuteNonQuery();

            command.CommandText = "DROP PROCEDURE IF EXISTS UpdateNonQueryTableData";
            command.ExecuteNonQuery();
            command.CommandText = @"
CREATE PROCEDURE UpdateNonQueryTableData(@Id INT, @Value NVARCHAR(10))
AS
BEGIN
  UPDATE NonQueryTestTable
     SET Value = @Value
   WHERE Id = @Id;
END;
";
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
        }
    }
}
