using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace DAABLike.Tests
{
    partial class DatabaseAccessTest
    {
        private static void AssertTestTableDataTable(DataTable dt, string tableName = "Table")
        {
            Assert.Equal(tableName, dt.TableName);
            Assert.Equal(3, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(2, dt.Rows[1]["Id"]);
            Assert.Equal("Val2", dt.Rows[1]["Value"]);
            Assert.Equal(3, dt.Rows[2]["Id"]);
            Assert.Equal("Val3", dt.Rows[2]["Value"]);
        }

        [Fact]
        public void ExecuteDataSet1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var ds = db.ExecuteDataSet(command);
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            AssertTestTableDataTable(dt);
        }

        [Fact]
        public void ExecuteDataSet2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var ds = db.ExecuteDataSet(command, trans);
            trans.Rollback();
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            Assert.Equal("Table", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
        }

        [Fact]
        public void ExecuteDataSet3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var ds = db.ExecuteDataSet("GetTestTableData", 1, 3);
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            AssertTestTableDataTable(dt);
        }

        [Fact]
        public void ExecuteDataSet4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            using var ds = db.ExecuteDataSet(trans, "GetTestTableData", 1, 3);
            trans.Rollback();
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            Assert.Equal("Table", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
        }

        [Fact]
        public void ExecuteDataSet5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var ds = db.ExecuteDataSet(CommandType.Text, "SELECT * FROM TestTable ORDER BY Id");
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            AssertTestTableDataTable(dt);
        }

        [Fact]
        public void ExecuteDataSet6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            using var ds = db.ExecuteDataSet(trans, CommandType.Text, "SELECT * FROM TestTable ORDER BY Id");
            trans.Rollback();
            var dt = (DataTable)Assert.Single(ds.Tables)!;
            Assert.Equal("Table", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
        }

        //

        [Fact]
        public void ExecuteNonQuery1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("UPDATE NonQueryTestTable SET Value = 'Val1Update' WHERE Id = 1");
            var result = db.ExecuteNonQuery(command);
            Assert.Equal(1, result);
            using var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 1");
            dr.Read();
            Assert.Equal("Val1Update", dr["Value"]);
        }

        [Fact]
        public void ExecuteNonQuery2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            using var command = new SqlCommand("UPDATE NonQueryTestTable SET Value = 'Val2Update' WHERE Id = 2");
            var result = db.ExecuteNonQuery(command, trans);
            Assert.Equal(1, result);
            using (var dr = db.ExecuteReader(trans, CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 2"))
            {
                dr.Read();
                Assert.Equal("Val2Update", dr["Value"]);
            }
            trans.Rollback();
            using (var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 2"))
            {
                dr.Read();
                Assert.Equal("Val2", dr["Value"]);
            }
        }

        [Fact]
        public void ExecuteNonQuery3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = db.ExecuteNonQuery("UpdateNonQueryTableData", 3, "Val3Update");
            Assert.Equal(1, result);
            using var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 3");
            dr.Read();
            Assert.Equal("Val3Update", dr["Value"]);
        }

        [Fact]
        public void ExecuteNonQuery4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            var result = db.ExecuteNonQuery(trans, "UpdateNonQueryTableData", 4, "Val4Update");
            Assert.Equal(1, result);
            using (var dr = db.ExecuteReader(trans, CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 4"))
            {
                dr.Read();
                Assert.Equal("Val4Update", dr["Value"]);
            }
            trans.Rollback();
            using (var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 4"))
            {
                dr.Read();
                Assert.Equal("Val4", dr["Value"]);
            }
        }

        [Fact]
        public void ExecuteNonQuery5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = db.ExecuteNonQuery(CommandType.Text, "UPDATE NonQueryTestTable SET Value = 'Val5Update' WHERE Id = 5");
            Assert.Equal(1, result);
            using var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 5");
            dr.Read();
            Assert.Equal("Val5Update", dr["Value"]);
        }

        [Fact]
        public void ExecuteNonQuery6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            var result = db.ExecuteNonQuery(trans, CommandType.Text, "UPDATE NonQueryTestTable SET Value = 'Val6Update' WHERE Id = 6");
            Assert.Equal(1, result);
            using (var dr = db.ExecuteReader(trans, CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 6"))
            {
                dr.Read();
                Assert.Equal("Val6Update", dr["Value"]);
            }
            trans.Rollback();
            using (var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM NonQueryTestTable WHERE Id = 6"))
            {
                dr.Read();
                Assert.Equal("Val6", dr["Value"]);
            }
        }

        //

        [Fact]
        public void ExecuteReader1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var dr = db.ExecuteReader(command);
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public void ExecuteReader2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3");
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var dr = db.ExecuteReader(command, trans);
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public void ExecuteReader3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var dr = db.ExecuteReader("GetTestTableData", 1, 3);
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public void ExecuteReader4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3");
            using var dr = db.ExecuteReader(trans, "GetTestTableData", 1, 3);
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(2, count);
        }

        [Fact]
        public void ExecuteReader5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var dr = db.ExecuteReader(CommandType.Text, "SELECT * FROM TestTable ORDER BY Id");
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(3, count);
        }

        [Fact]
        public void ExecuteReader6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 3");
            using var dr = db.ExecuteReader(trans, CommandType.Text, "SELECT * FROM TestTable ORDER BY Id");
            int count = 0;
            while (dr.Read())
            {
                count++;
                Assert.Equal(count, Convert.ToInt32(dr["Id"]));
                Assert.Equal($"Val{count}", dr["Value"]);
            }
            Assert.Equal(2, count);
        }

        //

        [Fact]
        public void ExecuteScalar1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT Value FROM TestTable WHERE Id = @Id");
            db.AddInParameter(command, "@Id", DbType.Int32, 1);
            var result = db.ExecuteScalar(command);
            Assert.Equal("Val1", result);
        }

        [Fact]
        public void ExecuteScalar2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')");
            using var command = new SqlCommand("SELECT Value FROM TestTable WHERE Id = @Id");
            db.AddInParameter(command, "@Id", DbType.Int32, 4);
            var result = db.ExecuteScalar(command, trans);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        [Fact]
        public void ExecuteScalar3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = db.ExecuteScalar("GetTestData", 2);
            Assert.Equal("Val2", result);
        }

        [Fact]
        public void ExecuteScalar4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')");
            var result = db.ExecuteScalar(trans, "GetTestData", 4);
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        [Fact]
        public void ExecuteScalar5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            var result = db.ExecuteScalar(CommandType.Text, "SELECT Value FROM TestTable WHERE Id = 2");
            Assert.Equal("Val2", result);
        }

        [Fact]
        public void ExecuteScalar6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "INSERT INTO TestTable (Id, Value) VALUES (4, 'Val4')");
            var result = db.ExecuteScalar(trans, CommandType.Text, "SELECT Value FROM TestTable WHERE Id = 4");
            trans.Rollback();
            Assert.Equal("Val4", result);
        }

        //

        [Fact]
        public void ExecuteDataSetException()
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
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteDataSet(nullCommand));
            // 2
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteDataSet(nullCommand, nullTrans));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteDataSet(command, nullTrans));
            // 3
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteDataSet(nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteDataSet(""));
            // 4
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteDataSet(nullTrans, nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteDataSet(nullTrans, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteDataSet(nullTrans, "GetTestTableData", 1, 2));
            // 5
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteDataSet(CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteDataSet(CommandType.Text, ""));
            // 6
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteDataSet(nullTrans, CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteDataSet(nullTrans, CommandType.Text, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteDataSet(nullTrans, CommandType.Text, "SELECT 1"));
        }

        [Fact]
        public void ExecuteNonQueryException()
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
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteNonQuery(nullCommand));
            // 2
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteNonQuery(nullCommand, nullTrans));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteNonQuery(command, nullTrans));
            // 3
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteNonQuery(nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteNonQuery(""));
            // 4
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteNonQuery(nullTrans, nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteNonQuery(nullTrans, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteNonQuery(nullTrans, "GetTestTableData", 1, 2));
            // 5
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteNonQuery(CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteNonQuery(CommandType.Text, ""));
            // 6
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteNonQuery(nullTrans, CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteNonQuery(nullTrans, CommandType.Text, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteNonQuery(nullTrans, CommandType.Text, "SELECT 1"));
        }

        [Fact]
        public void ExecuteReaderException()
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
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteReader(nullCommand));
            // 2
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteReader(nullCommand, nullTrans));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteReader(command, nullTrans));
            // 3
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteReader(nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteReader(""));
            // 4
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteReader(nullTrans, nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteReader(nullTrans, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteReader(nullTrans, "GetTestTableData", 1, 2));
            // 5
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteReader(CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteReader(CommandType.Text, ""));
            // 6
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteReader(nullTrans, CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteReader(nullTrans, CommandType.Text, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteReader(nullTrans, CommandType.Text, "SELECT 1"));
        }

        [Fact]
        public void ExecuteScalarException()
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
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteScalar(nullCommand));
            // 2
            Assert.Throws<ArgumentNullException>("command", () => db.ExecuteScalar(nullCommand, nullTrans));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteScalar(command, nullTrans));
            // 3
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteScalar(nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteScalar(""));
            // 4
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.ExecuteScalar(nullTrans, nullText));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.ExecuteScalar(nullTrans, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteScalar(nullTrans, "GetTestTableData", 1, 2));
            // 5
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteScalar(CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteScalar(CommandType.Text, ""));
            // 6
            Assert.Throws<ArgumentNullException>("commandText", () => db.ExecuteScalar(nullTrans, CommandType.Text, nullText));
            Assert.Throws<ArgumentException>("commandText", () => db.ExecuteScalar(nullTrans, CommandType.Text, ""));
            Assert.Throws<ArgumentNullException>("transaction", () => db.ExecuteScalar(nullTrans, CommandType.Text, "SELECT 1"));
        }
    }
}
