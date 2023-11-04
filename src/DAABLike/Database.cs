using System.Data;
using System.Data.Common;

namespace DAABLike
{
    public sealed class Database
    {
        public string ConnectionString => "";
        public DbProviderFactory DbProviderFactory => null;

        //

        public void AddInParameter(DbCommand command, string name, DbType dbType) { }
        public void AddInParameter(DbCommand command, string name, DbType dbType, object value) { }
        public void AddInParameter(DbCommand command, string name, DbType dbType, string sourceColumn, DataRowVersion sourceVersion) { }

        public void AddOutParameter(DbCommand command, string name, DbType dbType, int size) { }

        public void AddParameter(DbCommand command, string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value) { }
        public void AddParameter(DbCommand command, string name, DbType dbType, ParameterDirection direction, string sourceColumn, DataRowVersion sourceVersion, object value) { }

        //

        public string BuildParameterName(string name) => name;

        public static void ClearParameterCache() { }

        public DbConnection CreateConnection() => null;

        public bool SupportsParameterDiscovery => true;

        public void DiscoverParameters(DbCommand command) { }

        //

        public DataSet ExecuteDataSet(DbCommand command) => null;
        public DataSet ExecuteDataSet(DbCommand command, DbTransaction transaction) => null;
        public DataSet ExecuteDataSet(string storedProcedureName, params object[] parameterValues) => null;
        public DataSet ExecuteDataSet(DbTransaction transaction, string storedProcedureName, params object[] parameterValues) => null;
        public DataSet ExecuteDataSet(CommandType commandType, string commandText) => null;
        public DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText) => null;

        //

        public int ExecuteNonQuery(DbCommand command) => 0;
        public int ExecuteNonQuery(DbCommand command, DbTransaction transaction) => 0;
        public int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues) => 0;
        public int ExecuteNonQuery(DbTransaction transaction, string storedProcedureName, params object[] parameterValues) => 0;
        public int ExecuteNonQuery(CommandType commandType, string commandText) => 0;
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText) => 0;

        //

        public IDataReader ExecuteReader(DbCommand command) => null;
        public IDataReader ExecuteReader(DbCommand command, DbTransaction transaction) => null;
        public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues) => null;
        public IDataReader ExecuteReader(DbTransaction transaction, string storedProcedureName, params object[] parameterValues) => null;
        public IDataReader ExecuteReader(CommandType commandType, string commandText) => null;
        public IDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText) => null;

        //

        public object ExecuteScalar(DbCommand command) => null;
        public object ExecuteScalar(DbCommand command, DbTransaction transaction) => null;
        public object ExecuteScalar(string storedProcedureName, params object[] parameterValues) => null;
        public object ExecuteScalar(DbTransaction transaction, string storedProcedureName, params object[] parameterValues) => null;
        public object ExecuteScalar(CommandType commandType, string commandText) => null;
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText) => null;

        //

        public DbDataAdapter GetDataAdapter() => null;

        public object GetParameterValue(DbCommand command, string name) => null;

        public DbCommand GetSqlStringCommand(string query) => null;
        public DbCommand GetStoredProcCommand(string storedProcedureName) => null;
        public DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues) => null;
        public void AssignParameters(DbCommand command, object[] parameterValues) { }
        public DbCommand GetStoredProcCommandWithSourceColumns(string storedProcedureName, params string[] sourceColumns) => null;

        //

        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName) { }
        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName, DbTransaction transaction) { }
        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames) { }
        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames, DbTransaction transaction) { }
        public void LoadDataSet(string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues) { }
        public void LoadDataSet(DbTransaction transaction, string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues) { }
        public void LoadDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames) { }
        public void LoadDataSet(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames) { }

        //

        public void SetParameterValue(DbCommand command, string parameterName, object value) { }

        //

        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand, UpdateBehavior updateBehavior, int? updateBatchSize) => 0;
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand, UpdateBehavior updateBehavior) => 0;
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand, DbTransaction transaction, int? updateBatchSize) => 0;
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand insertCommand, DbCommand updateCommand, DbCommand deleteCommand, DbTransaction transaction) => 0;
    }
}
