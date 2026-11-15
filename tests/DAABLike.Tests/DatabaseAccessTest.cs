using System.Data;
using System.Data.Common;
#if !NET35
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace DAABLike.Tests
{
    public partial class DatabaseAccessTest
#if !NET35
        (SqlServerFixture fixture) : IClassFixture<SqlServerFixture>
#else
        : IUseFixture<DAABLike.Tests.Net35.SqlServerFixture>
#endif
    {
#if !NET35
        private string ConnectionString { get; } = fixture.ConnectionString;
#else
        private string ConnectionString { get; set; } = "";
        public void SetFixture(DAABLike.Tests.Net35.SqlServerFixture fixture)
        {
            ConnectionString = fixture.ConnectionString;
        }
#endif

        private static void AssertGetTestTableDataParameters(DbCommand command)
        {
            Assert.Equal(3, command.Parameters.Count);
            var param = command.Parameters[0];
            Assert.Equal(ParameterDirection.ReturnValue, param.Direction);
            param = command.Parameters[1];
            Assert.Equal("@FromId", param.ParameterName);
            Assert.Equal(DbType.Int32, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);
            param = command.Parameters[2];
            Assert.Equal("@ToId", param.ParameterName);
            Assert.Equal(DbType.Int32, param.DbType);
            Assert.Equal(ParameterDirection.Input, param.Direction);
        }

        [Fact]
        public void DiscoverParameters()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("GetTestTableData");
            command.CommandType = CommandType.StoredProcedure;
            db.DiscoverParameters(command);
            AssertGetTestTableDataParameters(command);
        }

        [Fact]
        public void DiscoverParametersException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("command", () => db.DiscoverParameters(null!));
        }

        //

        [Fact]
        public void GetStoredProcCommand()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = db.GetStoredProcCommand("GetTestTableData", 2, 5);
            Assert.Equal("GetTestTableData", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);

            AssertGetTestTableDataParameters(command);
            var param = command.Parameters[1];
            Assert.Equal(2, param.Value);
            param = command.Parameters[2];
            Assert.Equal(5, param.Value);

        }

        [Fact]
        public void GetStoredProcCommandException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.GetStoredProcCommand(null!, 2, 5));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.GetStoredProcCommand("", 2, 5));
        }

        [Fact]
        public void AssignParameters()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("GetTestTableData");
            command.CommandType = CommandType.StoredProcedure;
            db.AssignParameters(command, [1, 4]);

            AssertGetTestTableDataParameters(command);
            var param = command.Parameters[1];
            Assert.Equal(1, param.Value);
            param = command.Parameters[2];
            Assert.Equal(4, param.Value);
        }

        [Fact]
        public void AssignParametersException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("GetTestTableData");
            command.CommandType = CommandType.StoredProcedure;
            Assert.Throws<ArgumentNullException>("command", () => db.AssignParameters(null!, null!));
            Assert.Throws<ArgumentNullException>("parameterValues", () => db.AssignParameters(command, null!));
            Assert.Throws<InvalidOperationException>(() => db.AssignParameters(command, [1, 2, 3]));
        }

        [Fact]
        public void GetStoredProcCommandWithSourceColumns()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = db.GetStoredProcCommandWithSourceColumns("GetTestTableData", "SourceColumn1", "SourceColumn2");
            Assert.Equal("GetTestTableData", command.CommandText);
            Assert.Equal(CommandType.StoredProcedure, command.CommandType);

            AssertGetTestTableDataParameters(command);
            var param = command.Parameters[1];
            Assert.Equal("SourceColumn1", param.SourceColumn);
            param = command.Parameters[2];
            Assert.Equal("SourceColumn2", param.SourceColumn);
        }

        [Fact]
        public void GetStoredProcCommandWithSourceColumnsException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.GetStoredProcCommandWithSourceColumns(null!));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.GetStoredProcCommandWithSourceColumns(""));
            Assert.Throws<ArgumentNullException>("sourceColumns", () => db.GetStoredProcCommandWithSourceColumns("test", (string[])null!));
        }
    }
}
