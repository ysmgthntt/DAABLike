using System.Data;
using System.Data.Common;
#if !NET35
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace DAABLike.Tests
{
    partial class DatabaseAccessTest
    {
        [Fact]
        public void LoadDataSet1()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var ds = new DataSet();
            ds.Tables.Add(new DataTable());
            db.LoadDataSet(command, ds, "TestTable");
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[1];
            AssertTestTableDataTable(dt, "TestTable");
        }

        [Fact]
        public void LoadDataSet2()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id");
            using var ds = new DataSet();
            ds.Tables.Add(new DataTable());
            db.LoadDataSet(command, ds, "TestTable", trans);
            trans.Rollback();
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[1];
            Assert.Equal("TestTable", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
        }

        [Fact]
        public void LoadDataSet3()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id;SELECT * FROM TestTable2 ORDER BY Id");
            using var ds = new DataSet();
            db.LoadDataSet(command, ds, ["TestTableData1", "TestTableData2"]);
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            AssertTestTableDataTable(dt, "TestTableData1");
            dt = ds.Tables[1];
            AssertTestTableDataTable(dt, "TestTableData2");
        }

        [Fact]
        public void LoadDataSet4()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            db.ExecuteNonQuery(trans, CommandType.Text, "UPDATE TestTable2 SET Value = 'ValX' WHERE Id = 3");
            using var command = new SqlCommand("SELECT * FROM TestTable ORDER BY Id;SELECT * FROM TestTable2 WHERE Id = 3");
            using var ds = new DataSet();
            db.LoadDataSet(command, ds, ["TestTableData1", "TestTableData2"], trans);
            trans.Rollback();
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            Assert.Equal("TestTableData1", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
            dt = ds.Tables[1];
            Assert.Equal("TestTableData2", dt.TableName);
            Assert.Equal(1, dt.Rows.Count);
            Assert.Equal(3, dt.Rows[0]["Id"]);
            Assert.Equal("ValX", dt.Rows[0]["Value"]);
        }

        [Fact]
        public void LoadDataSet5()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var ds = new DataSet();
            db.LoadDataSet("GetTestTablesData", ds, ["TestTableData1", "TestTableData2"], 1, 4);
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            AssertTestTableDataTable(dt, "TestTableData1");
            dt = ds.Tables[1];
            AssertTestTableDataTable(dt, "TestTableData2");
        }

        [Fact]
        public void LoadDataSet6()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            db.ExecuteNonQuery(trans, CommandType.Text, "UPDATE TestTable2 SET Value = 'ValX' WHERE Id = 3");
            using var ds = new DataSet();
            db.LoadDataSet(trans, "GetTestTablesData", ds, ["TestTableData1", "TestTableData2"], 0, 3);
            trans.Rollback();
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            Assert.Equal("TestTableData1", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
            dt = ds.Tables[1];
            Assert.Equal("TestTableData2", dt.TableName);
            Assert.Equal(3, dt.Rows.Count);
            Assert.Equal(3, dt.Rows[2]["Id"]);
            Assert.Equal("ValX", dt.Rows[2]["Value"]);
        }

        [Fact]
        public void LoadDataSet7()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand();
            using var ds = new DataSet();
            db.LoadDataSet(CommandType.Text, "SELECT * FROM TestTable ORDER BY Id;SELECT * FROM TestTable2 ORDER BY Id", ds, ["TestTableData1", "TestTableData2"]);
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            AssertTestTableDataTable(dt, "TestTableData1");
            dt = ds.Tables[1];
            AssertTestTableDataTable(dt, "TestTableData2");
        }

        [Fact]
        public void LoadDataSet8()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            db.ExecuteNonQuery(trans, CommandType.Text, "DELETE FROM TestTable WHERE Id = 2");
            db.ExecuteNonQuery(trans, CommandType.Text, "UPDATE TestTable2 SET Value = 'ValX' WHERE Id = 3");
            using var ds = new DataSet();
            db.LoadDataSet(trans, CommandType.Text, "SELECT * FROM TestTable ORDER BY Id;SELECT * FROM TestTable2 WHERE Id = 3", ds, ["TestTableData1", "TestTableData2"]);
            trans.Rollback();
            Assert.Equal(2, ds.Tables.Count);
            var dt = ds.Tables[0];
            Assert.Equal("TestTableData1", dt.TableName);
            Assert.Equal(2, dt.Rows.Count);
            Assert.Equal(1, dt.Rows[0]["Id"]);
            Assert.Equal("Val1", dt.Rows[0]["Value"]);
            Assert.Equal(3, dt.Rows[1]["Id"]);
            Assert.Equal("Val3", dt.Rows[1]["Value"]);
            dt = ds.Tables[1];
            Assert.Equal("TestTableData2", dt.TableName);
            Assert.Equal(1, dt.Rows.Count);
            Assert.Equal(3, dt.Rows[0]["Id"]);
            Assert.Equal("ValX", dt.Rows[0]["Value"]);
        }

        [Fact]
        public void LoadDataSetException()
        {
            var db = new Database(ConnectionString, SqlClientFactory.Instance);
            using var command = new SqlCommand();
            using var connection = db.CreateConnection();
            connection.Open();
            using var trans = connection.BeginTransaction();
            using var ds = new DataSet();
            DbCommand nullCommand = null!;
            string nullText = null!;
            string[] nullTextArray = null!;
            object[] nullObjArray = null!;
            ds.Tables.Add(new DataTable());
            // 1
            Assert.Throws<ArgumentNullException>("command", () => db.LoadDataSet(nullCommand, null!, nullText));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(command, null!, nullText));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, nullText));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, ""));
            // 2
            Assert.Throws<ArgumentNullException>("command", () => db.LoadDataSet(nullCommand, null!, nullText, null!));
            Assert.Throws<ArgumentNullException>("transaction", () => db.LoadDataSet(command, null!, nullText, null!));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(command, null!, nullText, trans));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, nullText, trans));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, "", trans));
            // 3
            Assert.Throws<ArgumentNullException>("command", () => db.LoadDataSet(nullCommand, null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(command, null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet(command, ds, nullTextArray));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, []));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, [""]));
            // 4
            Assert.Throws<ArgumentNullException>("command", () => db.LoadDataSet(nullCommand, null!, nullTextArray, null!));
            Assert.Throws<ArgumentNullException>("transaction", () => db.LoadDataSet(command, null!, nullTextArray, null!));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(command, null!, nullTextArray, trans));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet(command, ds, nullTextArray, trans));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, [], trans));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(command, ds, [""], trans));
            // 5
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.LoadDataSet(nullText, null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.LoadDataSet("", null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentNullException>("parameterValues", () => db.LoadDataSet("GetTestTableData", null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet("GetTestTableData", null!, nullTextArray, 1, 2));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet("GetTestTableData", ds, nullTextArray, 1, 2));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet("GetTestTableData", ds, [], 1, 2));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet("GetTestTableData", ds, [""], 1, 2));
            // 6
            Assert.Throws<ArgumentNullException>("storedProcedureName", () => db.LoadDataSet(null!, nullText, null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentException>("storedProcedureName", () => db.LoadDataSet(null!, "", null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentNullException>("parameterValues", () => db.LoadDataSet(null!, "GetTestTableData", null!, nullTextArray, nullObjArray));
            Assert.Throws<ArgumentNullException>("transaction", () => db.LoadDataSet(null!, "GetTestTableData", null!, nullTextArray, 1, 2));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(trans, "GetTestTableData", null!, nullTextArray, 1, 2));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet(trans, "GetTestTableData", ds, nullTextArray, 1, 2));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(trans, "GetTestTableData", ds, [], 1, 2));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(trans, "GetTestTableData", ds, [""], 1, 2));
            // 7
            Assert.Throws<ArgumentNullException>("commandText", () => db.LoadDataSet(CommandType.Text, nullText, null!, nullTextArray));
            Assert.Throws<ArgumentException>("commandText", () => db.LoadDataSet(CommandType.Text, "", null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(CommandType.Text, "test", null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet(CommandType.Text, "test", ds, nullTextArray));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(CommandType.Text, "test", ds, []));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(CommandType.Text, "test", ds, [""]));
            // 8
            Assert.Throws<ArgumentNullException>("commandText", () => db.LoadDataSet(null!, CommandType.Text, nullText, null!, nullTextArray));
            Assert.Throws<ArgumentException>("commandText", () => db.LoadDataSet(null!, CommandType.Text, "", null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("transaction", () => db.LoadDataSet(null!, CommandType.Text, "test", null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("dataSet", () => db.LoadDataSet(trans, CommandType.Text, "test", null!, nullTextArray));
            Assert.Throws<ArgumentNullException>("tableNames", () => db.LoadDataSet(trans, CommandType.Text, "test", ds, nullTextArray));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(trans, CommandType.Text, "test", ds, []));
            Assert.Throws<ArgumentException>("tableNames", () => db.LoadDataSet(trans, CommandType.Text, "test", ds, [""]));
        }
    }
}
