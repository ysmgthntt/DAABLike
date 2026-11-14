using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DAABLike.Tests
{
    partial class DatabaseAccessTest
    {
        [Fact]
        public async Task ExecuteNonQueryAsync1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("UPDATE NonQueryAsyncTest SET Value = 'Val1Update' WHERE Id = 1");
            var result = await db.ExecuteNonQueryAsync(command, TestContext.Current.CancellationToken);
            Assert.Equal(1, result);
            using var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 1", TestContext.Current.CancellationToken);
            await dr.ReadAsync(TestContext.Current.CancellationToken);
            Assert.Equal("Val1Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ExecuteNonQueryAsync2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            using var command = new SqlCommand("UPDATE NonQueryAsyncTest SET Value = 'Val2Update' WHERE Id = 2");
            var result = await db.ExecuteNonQueryAsync(command, trans, TestContext.Current.CancellationToken);
            Assert.Equal(1, result);
            using (var dr = await db.ExecuteReaderAsync(trans, CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 2", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val2Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            trans.Rollback();
            using (var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 2", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val2", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteNonQueryAsync("UpdateNonQueryAsyncData", TestContext.Current.CancellationToken, 3, "Val3Update");
            Assert.Equal(1, result);
            using var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 3", TestContext.Current.CancellationToken);
            await dr.ReadAsync(TestContext.Current.CancellationToken);
            Assert.Equal("Val3Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ExecuteNonQueryAsync4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            var result = await db.ExecuteNonQueryAsync(trans, "UpdateNonQueryAsyncData", TestContext.Current.CancellationToken, 4, "Val4Update");
            Assert.Equal(1, result);
            using (var dr = await db.ExecuteReaderAsync(trans, CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 4", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val4Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            trans.Rollback();
            using (var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 4", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val4", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteNonQueryAsync(CommandType.Text, "UPDATE NonQueryAsyncTest SET Value = 'Val5Update' WHERE Id = 5", TestContext.Current.CancellationToken);
            Assert.Equal(1, result);
            using var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 5", TestContext.Current.CancellationToken);
            await dr.ReadAsync(TestContext.Current.CancellationToken);
            Assert.Equal("Val5Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ExecuteNonQueryAsync6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            var result = await db.ExecuteNonQueryAsync(trans, CommandType.Text, "UPDATE NonQueryAsyncTest SET Value = 'Val6Update' WHERE Id = 6", TestContext.Current.CancellationToken);
            Assert.Equal(1, result);
            using (var dr = await db.ExecuteReaderAsync(trans, CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 6", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val6Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            trans.Rollback();
            using (var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 6", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val6", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
        }

        [Fact]
        public async Task ExecuteNonQueryAsync7()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteNonQueryAsync("UpdateNonQueryAsyncData", 3, "Val3Update");
            Assert.Equal(1, result);
            using var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 3", TestContext.Current.CancellationToken);
            await dr.ReadAsync(TestContext.Current.CancellationToken);
            Assert.Equal("Val3Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ExecuteNonQueryAsync8()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            var result = await db.ExecuteNonQueryAsync(trans, "UpdateNonQueryAsyncData", 4, "Val4Update");
            Assert.Equal(1, result);
            using (var dr = await db.ExecuteReaderAsync(trans, CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 4", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val4Update", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            trans.Rollback();
            using (var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM NonQueryAsyncTest WHERE Id = 4", TestContext.Current.CancellationToken))
            {
                await dr.ReadAsync(TestContext.Current.CancellationToken);
                Assert.Equal("Val4", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
        }

        //

        [Fact]
        public async Task ExecuteReaderAsync1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var dr = await db.ExecuteReaderAsync(command, TestContext.Current.CancellationToken);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3", TestContext.Current.CancellationToken);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var dr = await db.ExecuteReaderAsync(command, trans, TestContext.Current.CancellationToken);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var dr = await db.ExecuteReaderAsync("GetTestTableData", TestContext.Current.CancellationToken, 1, 3);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3", TestContext.Current.CancellationToken);
            using var dr = await db.ExecuteReaderAsync(trans, "GetTestTableData", TestContext.Current.CancellationToken, 1, 3);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var dr = await db.ExecuteReaderAsync(CommandType.Text, "SELECT * FROM TestTable ORDER BY Id", TestContext.Current.CancellationToken);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task ExecuteReader6Async()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3", TestContext.Current.CancellationToken);
            using var dr = await db.ExecuteReaderAsync(trans, CommandType.Text, "SELECT * FROM TestTable ORDER BY Id", TestContext.Current.CancellationToken);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync7()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var dr = await db.ExecuteReaderAsync("GetTestTableData", 1, 3);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public async Task ExecuteReaderAsync8()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3", TestContext.Current.CancellationToken);
            using var dr = await db.ExecuteReaderAsync(trans, "GetTestTableData", 1, 3);
            int count = 0;
            while (await dr.ReadAsync(TestContext.Current.CancellationToken))
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(await dr.GetFieldValueAsync<int>(dr.GetOrdinal("Id"), TestContext.Current.CancellationToken)));
                Assert.Equal($"Val{count}", await dr.GetFieldValueAsync<string>(dr.GetOrdinal("Value"), TestContext.Current.CancellationToken));
            }
            Assert.Equal(2, count);
        }

        //

        [Fact]
        public async Task ExecuteScalarAsync1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT Value FROM TestTable WHERE Id = @Id");
            db.AddInParameter(command, "@Id", DbType.Int32, 1);
            var result = await db.ExecuteScalarAsync(command, TestContext.Current.CancellationToken);
            Assert.Equal("Val1", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')", TestContext.Current.CancellationToken);
            using var command = new SqlCommand("SELECT Value FROM TestTable WHERE Id = @Id");
            db.AddInParameter(command, "@Id", DbType.Int32, 4);
            var result = await db.ExecuteScalarAsync(command, trans, TestContext.Current.CancellationToken);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteScalarAsync("GetTestData", TestContext.Current.CancellationToken, 2);
            Assert.Equal("Val2", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')", TestContext.Current.CancellationToken);
            var result = await db.ExecuteScalarAsync(trans, "GetTestData", TestContext.Current.CancellationToken, 4);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteScalarAsync(CommandType.Text, "SELECT Value FROM TestTable WHERE Id = 2", TestContext.Current.CancellationToken);
            Assert.Equal("Val2", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')", TestContext.Current.CancellationToken);
            var result = await db.ExecuteScalarAsync(trans, CommandType.Text, "SELECT Value FROM TestTable WHERE Id = 4", TestContext.Current.CancellationToken);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync7()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = await db.ExecuteScalarAsync("GetTestData", 2);
            Assert.Equal("Val2", result);
        }

        [Fact]
        public async Task ExecuteScalarAsync8()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync(TestContext.Current.CancellationToken);
            using var trans = connection.BeginTransaction();
            await db.ExecuteNonQueryAsync(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')", TestContext.Current.CancellationToken);
            var result = await db.ExecuteScalarAsync(trans, "GetTestData", 4);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        //

        [Fact]
        public async Task ExecuteNonQueryAsyncException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            DbCommand nullCommand = null!;
            DbTransaction nullTrans = null!;
            string nullText = null!;
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            using var command = new SqlCommand();
            // 1
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteNonQueryAsync(nullCommand, TestContext.Current.CancellationToken));
            // 2
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteNonQueryAsync(nullCommand, nullTrans, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteNonQueryAsync(command, nullTrans, TestContext.Current.CancellationToken));
            // 3
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync("", TestContext.Current.CancellationToken));
            // 4
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullTrans, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullTrans, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteNonQueryAsync(nullTrans, "GetTestTableData", TestContext.Current.CancellationToken, 1, 2));
            // 5
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteNonQueryAsync(CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteNonQueryAsync(CommandType.Text, "", TestContext.Current.CancellationToken));
            // 6
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteNonQueryAsync(nullTrans, CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteNonQueryAsync(nullTrans, CommandType.Text, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteNonQueryAsync(nullTrans, CommandType.Text, "SELECT 1", TestContext.Current.CancellationToken));
            // 7
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(""));
            // 8
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullTrans, nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteNonQueryAsync(nullTrans, ""));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteNonQueryAsync(nullTrans, "GetTestTableData", 1, 2));
        }

        [Fact]
        public async Task ExecuteReaderAsyncException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            DbCommand nullCommand = null!;
            DbTransaction nullTrans = null!;
            string nullText = null!;
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            using var command = new SqlCommand();
            // 1
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteReaderAsync(nullCommand, TestContext.Current.CancellationToken));
            // 2
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteReaderAsync(nullCommand, nullTrans, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteReaderAsync(command, nullTrans, TestContext.Current.CancellationToken));
            // 3
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteReaderAsync("", TestContext.Current.CancellationToken));
            // 4
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullTrans, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullTrans, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteReaderAsync(nullTrans, "GetTestTableData", TestContext.Current.CancellationToken, 1, 2));
            // 5
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteReaderAsync(CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteReaderAsync(CommandType.Text, "", TestContext.Current.CancellationToken));
            // 6
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteReaderAsync(nullTrans, CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteReaderAsync(nullTrans, CommandType.Text, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteReaderAsync(nullTrans, CommandType.Text, "SELECT 1", TestContext.Current.CancellationToken));
            // 7
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteReaderAsync(""));
            // 8
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullTrans, nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteReaderAsync(nullTrans, ""));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteReaderAsync(nullTrans, "GetTestTableData", 1, 2));
        }

        [Fact]
        public async Task ExecuteScalarAsyncException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            DbCommand nullCommand = null!;
            DbTransaction nullTrans = null!;
            string nullText = null!;
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            using var command = new SqlCommand();
            // 1
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteScalarAsync(nullCommand, TestContext.Current.CancellationToken));
            // 2
            await Assert.ThrowsAsync<ArgumentNullException>("command", async () => await db.ExecuteScalarAsync(nullCommand, nullTrans, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteScalarAsync(command, nullTrans, TestContext.Current.CancellationToken));
            // 3
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteScalarAsync("", TestContext.Current.CancellationToken));
            // 4
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullTrans, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullTrans, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteScalarAsync(nullTrans, "GetTestTableData", TestContext.Current.CancellationToken, 1, 2));
            // 5
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteScalarAsync(CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteScalarAsync(CommandType.Text, "", TestContext.Current.CancellationToken));
            // 6
            await Assert.ThrowsAsync<ArgumentNullException>("commandText", async () => await db.ExecuteScalarAsync(nullTrans, CommandType.Text, nullText, TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentException>("commandText", async () => await db.ExecuteScalarAsync(nullTrans, CommandType.Text, "", TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteScalarAsync(nullTrans, CommandType.Text, "SELECT 1", TestContext.Current.CancellationToken));
            // 7
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteScalarAsync(""));
            // 8
            await Assert.ThrowsAsync<ArgumentNullException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullTrans, nullText));
            await Assert.ThrowsAsync<ArgumentException>("storedProcedureName", async () => await db.ExecuteScalarAsync(nullTrans, ""));
            await Assert.ThrowsAsync<ArgumentNullException>("transaction", async () => await db.ExecuteScalarAsync(nullTrans, "GetTestTableData", 1, 2));
        }
    }
}
