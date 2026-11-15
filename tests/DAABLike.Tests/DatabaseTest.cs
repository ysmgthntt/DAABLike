using System.Data;
using System.Data.Common;
#if !NET35
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace DAABLike.Tests
{
    public class DatabaseTest
    {
        [Fact]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>("connectionString", () => new Database(null!, null!));
            Assert.Throws<ArgumentException>("connectionString", () => new Database("", null!));
            Assert.Throws<ArgumentNullException>("dbProviderFactory", () => new Database("test", null!));
            var db = new Database("test", SqlClientFactory.Instance);
        }

        [Fact]
        public void ConnectionString()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Equal("test", db.ConnectionString);
        }

        [Fact]
        public void DbProviderFactory()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
        }

        [Fact]
        public void AddInParameter1()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddInParameter(command, "@ParamName", DbType.Decimal);
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName", param.ParameterName);
            Assert.Equal(DbType.Decimal, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Equal("", param.SourceColumn);
            Assert.Equal(DataRowVersion.Default, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);
            db.AddInParameter(command, "@ParamName2", DbType.String);
            Assert.Equal(2, command.Parameters.Count);
        }

        [Fact]
        public void AddInParameter2()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddInParameter(command, "@ParamName1", DbType.Int32, 12345);
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName1", param.ParameterName);
            Assert.Equal(DbType.Int32, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Equal("", param.SourceColumn);
            Assert.Equal(DataRowVersion.Default, param.SourceVersion);
            Assert.Equal(12345, param.Value);

            db.AddInParameter(command, "@ParamName2", DbType.Int16, null);
            Assert.Equal(2, command.Parameters.Count);
            param = command.Parameters[1];
            Assert.Equal("@ParamName2", param.ParameterName);
            Assert.Equal(DbType.Int16, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Equal("", param.SourceColumn);
            Assert.Equal(DataRowVersion.Default, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);
        }

        [Fact]
        public void AddInParameter3()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddInParameter(command, "@ParamName1", DbType.AnsiString, "Column1", DataRowVersion.Original);
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName1", param.ParameterName);
            Assert.Equal(DbType.AnsiString, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Equal("Column1", param.SourceColumn);
            Assert.Equal(DataRowVersion.Original, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);

            db.AddInParameter(command, "@ParamName2", DbType.AnsiStringFixedLength, null, DataRowVersion.Current);
            Assert.Equal(2, command.Parameters.Count);
            param = command.Parameters[1];
            Assert.Equal("@ParamName2", param.ParameterName);
            Assert.Equal(DbType.AnsiStringFixedLength, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Empty(param.SourceColumn);
            Assert.Equal(DataRowVersion.Current, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);
        }

        [Fact]
        public void AddOutParameter()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddOutParameter(command, "@ParamName1", DbType.Single, 37);
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName1", param.ParameterName);
            Assert.Equal(DbType.Single, param.DbType);
            Assert.Equal(37, param.Size);
            Assert.Equal(ParameterDirection.Output, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Empty(param.SourceColumn);
            Assert.Equal(DataRowVersion.Default, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);

            db.AddOutParameter(command, "@ParamName2", DbType.Double, 1);
            Assert.Equal(2, command.Parameters.Count);
        }

        [Fact]
        public void AddParameter1()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddParameter(command, "@ParamName1", DbType.Boolean, 1, ParameterDirection.ReturnValue, false, 12, 3, "Column1", DataRowVersion.Current, true);
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName1", param.ParameterName);
            Assert.Equal(DbType.Boolean, param.DbType);
            Assert.Equal(1, param.Size);
            Assert.Equal(ParameterDirection.ReturnValue, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(12, ((IDbDataParameter)param).Precision);
            Assert.Equal(3, ((IDbDataParameter)param).Scale);
            Assert.Equal("Column1", param.SourceColumn);
            Assert.Equal(DataRowVersion.Current, param.SourceVersion);
            Assert.Equal(true, param.Value);

            db.AddParameter(command, "@ParamName2", DbType.Byte, 8, ParameterDirection.InputOutput, true, 3, 1, null, DataRowVersion.Original, null);
            Assert.Equal(2, command.Parameters.Count);
            param = command.Parameters[1];
            Assert.Equal("@ParamName2", param.ParameterName);
            Assert.Equal(DbType.Byte, param.DbType);
            Assert.Equal(8, param.Size);
            Assert.Equal(ParameterDirection.InputOutput, param.Direction);
            Assert.True(param.IsNullable);
            Assert.Equal(3, ((IDbDataParameter)param).Precision);
            Assert.Equal(1, ((IDbDataParameter)param).Scale);
            Assert.Empty(param.SourceColumn);
            Assert.Equal(DataRowVersion.Original, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);
        }

        [Fact]
        public void AddParameter2()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            db.AddParameter(command, "@ParamName1", DbType.DateTime, ParameterDirection.Input, null, DataRowVersion.Original, new DateTime(2026, 11, 12));
            var param = (DbParameter)Assert.Single(command.Parameters)!;
            Assert.Equal("@ParamName1", param.ParameterName);
            Assert.Equal(DbType.DateTime, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Empty(param.SourceColumn);
            Assert.Equal(DataRowVersion.Original, param.SourceVersion);
            Assert.Equal(new DateTime(2026, 11, 12), param.Value);

            db.AddParameter(command, "@ParamName2", DbType.DateTime2, ParameterDirection.Output, "Column2", DataRowVersion.Current, null);
            Assert.Equal(2, command.Parameters.Count);
            param = command.Parameters[1];
            Assert.Equal("@ParamName2", param.ParameterName);
            Assert.Equal(DbType.DateTime2, param.DbType);
            Assert.Equal(0, param.Size);
            Assert.Equal(ParameterDirection.Output, param.Direction);
            Assert.False(param.IsNullable);
            Assert.Equal(0, ((IDbDataParameter)param).Precision);
            Assert.Equal(0, ((IDbDataParameter)param).Scale);
            Assert.Equal("Column2", param.SourceColumn);
            Assert.Equal(DataRowVersion.Current, param.SourceVersion);
            Assert.Equal(DBNull.Value, param.Value);
        }

        [Fact]
        public void AddParameterException()
        {
            using var command = new SqlCommand();
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("command", () => db.AddInParameter(null!, null!, DbType.String));
            db.AddInParameter(command, null!, DbType.String);
            Assert.Throws<ArgumentNullException>("command", () => db.AddInParameter(null!, null!, DbType.String, null));
            db.AddInParameter(command, null!, DbType.String, null);
            Assert.Throws<ArgumentNullException>("command", () => db.AddInParameter(null!, null!, DbType.String, null, DataRowVersion.Default));
            db.AddInParameter(command, null!, DbType.String, null, DataRowVersion.Default);
            Assert.Throws<ArgumentNullException>("command", () => db.AddOutParameter(null!, null!, DbType.String, 1));
            db.AddOutParameter(command, null!, DbType.String, 1);
            Assert.Throws<ArgumentNullException>("command", () => db.AddParameter(null!, null!, DbType.String, 1, ParameterDirection.ReturnValue, false, 2, 3, null, DataRowVersion.Default, null));
            db.AddParameter(command, null!, DbType.String, 1, ParameterDirection.ReturnValue, false, 2, 3, null, DataRowVersion.Default, null);
            Assert.Throws<ArgumentNullException>("command", () => db.AddParameter(null!, null!, DbType.String, ParameterDirection.ReturnValue, null, DataRowVersion.Default, null));
            db.AddParameter(command, null!, DbType.String, ParameterDirection.ReturnValue, null, DataRowVersion.Default, null);
        }

        [Fact]
        public void BuildParameterName()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            var paramName = db.BuildParameterName("ParamName");
            Assert.Equal("ParamName", paramName);
        }

        [Fact]
        public void CreateConnection()
        {
            var db = new Database("Server=(localdb)\\test;Database=test", SqlClientFactory.Instance);
            var connection = db.CreateConnection();
            Assert.IsType<SqlConnection>(connection);
            Assert.Equal("Server=(localdb)\\test;Database=test", connection.ConnectionString);
            Assert.Equal(ConnectionState.Closed, connection.State);
        }

        [Fact]
        public void SupportsParameterDiscovery()
        {
            var db = new Database("Server=(localdb)\\test;Database=test", SqlClientFactory.Instance);
            Assert.True(db.SupportsParameterDiscovery);
            Assert.True(db.SupportsParameterDiscovery);
            Assert.True(db.SupportsParameterDiscovery);
        }

        [Fact]
        public void GetDataAdapter()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            using var da = db.GetDataAdapter();
            Assert.IsType<SqlDataAdapter>(da);
        }

        [Fact]
        public void GetParameterValue()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            using var command = new SqlCommand();
            command.Parameters.AddWithValue("@ParamName", "ParamValue");
            var paramValue = db.GetParameterValue(command, "@ParamName");
            Assert.Equal("ParamValue", paramValue);
        }

        [Fact]
        public void GetParameterValueException()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("command", () => db.GetParameterValue(null!, null!));
        }

        [Fact]
        public void GetSqlStringCommand()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            using var command = db.GetSqlStringCommand("SELECT 1");
            Assert.Equal("SELECT 1", command.CommandText);
            Assert.Equal(CommandType.Text, command.CommandType);
        }

        [Fact]
        public void GetSqlStringCommandException()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("query", () => db.GetSqlStringCommand(null!));
            Assert.Throws<ArgumentException>("query", () => db.GetSqlStringCommand(""));
        }

        [Fact]
        public void GetStoredProcCommand()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            using var command = db.GetStoredProcCommand("TestProc");
            Assert.Equal("TestProc", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);
        }

        [Fact]
        public void GetStoredProcCommandException()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.GetStoredProcCommand(null!));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.GetStoredProcCommand(""));
        }

        [Fact]
        public void SetParameterValue()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            using var command = new SqlCommand();
            command.Parameters.Add("@ParamName", SqlDbType.Int);
            db.SetParameterValue(command, "@ParamName", 37);
            Assert.Equal(37, command.Parameters[0].Value);
            db.SetParameterValue(command, "@ParamName", null);
            Assert.Equal(DBNull.Value, command.Parameters[0].Value);
        }

        [Fact]
        public void SetParameterValueException()
        {
            var db = new Database("test", SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("command", () => db.SetParameterValue(null!, null!, null));
        }
    }
}
