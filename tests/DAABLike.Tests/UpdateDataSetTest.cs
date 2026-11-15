using System.Data;
#if !NET35
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace DAABLike.Tests
{
    partial class DatabaseAccessTest
    {
        private void PrepareUpdateTest(string tableName, out SqlCommand insertCommand, out SqlCommand updateCommand, out SqlCommand deleteCommand)
        {
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();

            using var command = new SqlCommand("", connection);
            command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            command.ExecuteNonQuery();
            command.CommandText = $"CREATE TABLE {tableName} (Id INT PRIMARY KEY, Value NVARCHAR(10))";
            command.ExecuteNonQuery();
            for (int i = 1; i <= 3; i++)
            {
                command.CommandText = $"INSERT INTO {tableName} (Id, Value) VALUES ({i}, 'Val{i}')";
                command.ExecuteNonQuery();
            }

            insertCommand = new SqlCommand($"INSERT INTO {tableName} (Id, Value) VALUES (@Id, @Value)");
            insertCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
            insertCommand.Parameters.Add("@Value", SqlDbType.NVarChar, 0, "Value");

            updateCommand = new SqlCommand($"UPDATE {tableName} SET Id = @Id, Value = @Value WHERE Id = @Old_Id");
            updateCommand.Parameters.Add("@Id", SqlDbType.Int, 0, "Id");
            updateCommand.Parameters.Add("@Value", SqlDbType.NVarChar, 0, "Value");
            updateCommand.Parameters.Add("@Old_Id", SqlDbType.Int, 0, "Id")
                .SourceVersion = DataRowVersion.Original;

            deleteCommand = new SqlCommand($"DELETE {tableName} WHERE Id = @Old_Id");
            deleteCommand.Parameters.Add("@Old_Id", SqlDbType.Int, 0, "Id")
                .SourceVersion = DataRowVersion.Original;
        }

        [Fact]
        public void UpdateDataSet1Standard()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest1Standard";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Standard);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Standard));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("1234567890", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Fact]
        public void UpdateDataSet1Transactional()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest1Transactional";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Transactional);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Transactional));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("Val1", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Fact]
        public void UpdateDataSet1Continue()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest1Continue";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Continue);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Continue);

                Assert.False(dt.Rows[0].HasErrors);
                Assert.Empty(dt.Rows[0].RowError);
                Assert.True(dt.Rows[1].HasErrors);
                Assert.NotEmpty(dt.Rows[1].RowError);
                Assert.False(dt.Rows[2].HasErrors);
                Assert.Empty(dt.Rows[2].RowError);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("1234567890", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("1234567890", dt.Rows[2]["Value"]);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void UpdateDataSet2Standard(int batchSize)
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest2Standard";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Standard, batchSize);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Standard, batchSize));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                //Assert.Equal("1234567890", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                //Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void UpdateDataSet2Transactional(int batchSize)
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest2Transactional";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Transactional, batchSize);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Transactional, batchSize));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("Val1", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void UpdateDataSet2Continue(int batchSize)
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest2Continue";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Continue, batchSize);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";
                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, UpdateBehavior.Continue, batchSize);

                int errorCount = 0;
                foreach (DataRow row in dt.Rows)
                {
                    if (row.HasErrors)
                    {
                        errorCount++;
                        Assert.NotEmpty(row.RowError);
                    }
                    else
                    {
                        Assert.Empty(row.RowError);
                    }
                }
                Assert.Equal(1, errorCount);
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("1234567890", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("1234567890", dt.Rows[2]["Value"]);
            }
        }

        [Fact]
        public void UpdateDataSet3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest3";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                using var connection = db.CreateConnection();
                connection.Open();
                using var trans = connection.BeginTransaction();
                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, trans);
                trans.Commit();
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";

                using var connection = db.CreateConnection();
                connection.Open();
                using var trans = connection.BeginTransaction();
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, trans));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("Val1", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void UpdateDataSet4(int batchSize)
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            string tableName = "UpdateTest4";
            PrepareUpdateTest(tableName, out var insertCommand, out var updateCommand, out var deleteCommand);
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                dt.Rows[1]["Value"] = "Val2Update";
                dt.Rows[2].Delete();

                var newrow = dt.NewRow();
                newrow["Id"] = 4;
                newrow["Value"] = "Val4";
                dt.Rows.Add(newrow);

                using var connection = db.CreateConnection();
                connection.Open();
                using var trans = connection.BeginTransaction();
                db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, trans, batchSize);
                trans.Commit();
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal(3, dt.Rows.Count);

                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal(4, dt.Rows[2]["Id"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);

                dt.Rows[0]["Value"] = "1234567890";
                dt.Rows[1]["Value"] = "12345678901";
                dt.Rows[2]["Value"] = "1234567890";

                using var connection = db.CreateConnection();
                connection.Open();
                using var trans = connection.BeginTransaction();
                Assert.Throws<SqlException>(() => db.UpdateDataSet(ds, "Table", insertCommand, updateCommand, deleteCommand, trans, batchSize));
            }
            using (var ds = db.ExecuteDataSet(CommandType.Text, $"SELECT * FROM {tableName} ORDER BY Id"))
            {
                var dt = ds.Tables[0];

                Assert.Equal("Val1", dt.Rows[0]["Value"]);
                Assert.Equal("Val2Update", dt.Rows[1]["Value"]);
                Assert.Equal("Val4", dt.Rows[2]["Value"]);
            }
        }

        [Fact]
        public void UpdateDataSetException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var ds = new DataSet();
            using var connection = db.CreateConnection();
            connection.Open();
            using var trans = connection.BeginTransaction();
            // 1
            Assert.Throws<ArgumentNullException>("tableName", () => db.UpdateDataSet(null!, null!, null, null, null, UpdateBehavior.Standard));
            Assert.Throws<ArgumentException>("tableName", () => db.UpdateDataSet(null!, "", null, null, null, UpdateBehavior.Standard));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.UpdateDataSet(null!, "test", null, null, null, UpdateBehavior.Standard));
            Assert.Throws<ArgumentException>(() => db.UpdateDataSet(ds, "test", null, null, null, UpdateBehavior.Standard));
            // 2
            Assert.Throws<ArgumentNullException>("tableName", () => db.UpdateDataSet(null!, null!, null, null, null, UpdateBehavior.Standard, 1));
            Assert.Throws<ArgumentException>("tableName", () => db.UpdateDataSet(null!, "", null, null, null, UpdateBehavior.Standard, 1));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.UpdateDataSet(null!, "test", null, null, null, UpdateBehavior.Standard, 1));
            Assert.Throws<ArgumentException>(() => db.UpdateDataSet(ds, "test", null, null, null, UpdateBehavior.Standard, 1));
            // 3
            Assert.Throws<ArgumentNullException>("transaction", () => db.UpdateDataSet(null!, null!, null, null, null, null!));
            Assert.Throws<ArgumentNullException>("tableName", () => db.UpdateDataSet(null!, null!, null, null, null, trans));
            Assert.Throws<ArgumentException>("tableName", () => db.UpdateDataSet(null!, "", null, null, null, trans));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.UpdateDataSet(null!, "test", null, null, null, trans));
            Assert.Throws<ArgumentException>(() => db.UpdateDataSet(ds, "test", null, null, null, trans));
            // 4
            Assert.Throws<ArgumentNullException>("transaction", () => db.UpdateDataSet(null!, null!, null, null, null, null!, 2));
            Assert.Throws<ArgumentNullException>("tableName", () => db.UpdateDataSet(null!, null!, null, null, null, trans, 2));
            Assert.Throws<ArgumentException>("tableName", () => db.UpdateDataSet(null!, "", null, null, null, trans, 2));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.UpdateDataSet(null!, "test", null, null, null, trans, 2));
            Assert.Throws<ArgumentException>(() => db.UpdateDataSet(ds, "test", null, null, null, trans, 2));
        }
    }
}
