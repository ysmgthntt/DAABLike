using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CA1822, CA2100

namespace DAABLike
{
    /// <summary>
    /// Provides methods for executing database commands and managing database operations.
    /// </summary>
    public sealed class Database
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _dbProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the database.</param>
        /// <param name="dbProviderFactory">The provider factory used to create database connections and commands.</param>
        public Database(string connectionString, DbProviderFactory dbProviderFactory)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentNullException.ThrowIfNull(dbProviderFactory);

            _connectionString = connectionString;
            _dbProviderFactory = dbProviderFactory;
        }

        /// <summary>
        /// Gets the connection string used to connect to the database.
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Gets the provider factory used to create database connections and commands.
        /// </summary>
        public DbProviderFactory DbProviderFactory => _dbProviderFactory;

        #region AddParameter

        /// <summary>
        /// Adds an input parameter to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        public void AddInParameter(DbCommand command, string name, DbType dbType)
            => AddParameter(command, name, dbType, ParameterDirection.Input, "", DataRowVersion.Default, null);

        /// <summary>
        /// Adds an input parameter with the specified value to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public void AddInParameter(DbCommand command, string name, DbType dbType, object? value)
            => AddParameter(command, name, dbType, ParameterDirection.Input, "", DataRowVersion.Default, value);

        /// <summary>
        /// Adds an input parameter with the specified source column information to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="sourceColumn">The source column mapped to the parameter.</param>
        /// <param name="sourceVersion">The <see cref="System.Data.DataRowVersion"/> used when retrieving the parameter value.</param>
        public void AddInParameter(DbCommand command, string name, DbType dbType, string? sourceColumn, DataRowVersion sourceVersion)
            => AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null);

        /// <summary>
        /// Adds an output parameter to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="size">The maximum size of the parameter.</param>
        public void AddOutParameter(DbCommand command, string name, DbType dbType, int size)
            => AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, DBNull.Value);

        /// <summary>
        /// Adds a parameter with the specified settings to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="size">The maximum size of the parameter.</param>
        /// <param name="direction">The direction of the parameter.</param>
        /// <param name="nullable"><see langword="true"/> if the parameter accepts null values; otherwise, <see langword="false"/>.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        /// <param name="sourceColumn">The source column mapped to the parameter.</param>
        /// <param name="sourceVersion">The <see cref="System.Data.DataRowVersion"/> used when retrieving the parameter value.</param>
        /// <param name="value">The value of the parameter.</param>
        public void AddParameter(DbCommand command, string name, DbType dbType, int size, ParameterDirection direction, bool nullable, byte precision, byte scale, string? sourceColumn, DataRowVersion sourceVersion, object? value)
        {
            ArgumentNullException.ThrowIfNull(command);

            var param = _dbProviderFactory.CreateParameter()!;
            param.ParameterName = name;
            param.DbType = dbType;
            param.Size = size;
            param.Direction = direction;
            param.IsNullable = nullable;
            ((IDbDataParameter)param).Precision = precision;
            ((IDbDataParameter)param).Scale = scale;
            param.SourceColumn = sourceColumn;
            param.SourceVersion = sourceVersion;
            param.Value = value ?? DBNull.Value;

            command.Parameters.Add(param);
        }

        /// <summary>
        /// Adds a parameter with the specified settings to the specified command.
        /// </summary>
        /// <param name="command">The command to which the parameter is added.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="dbType">The data type of the parameter.</param>
        /// <param name="direction">The direction of the parameter.</param>
        /// <param name="sourceColumn">The source column mapped to the parameter.</param>
        /// <param name="sourceVersion">The <see cref="System.Data.DataRowVersion"/> used when retrieving the parameter value.</param>
        /// <param name="value">The value of the parameter.</param>
        public void AddParameter(DbCommand command, string name, DbType dbType, ParameterDirection direction, string? sourceColumn, DataRowVersion sourceVersion, object? value)
            => AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);

        #endregion

        /// <summary>
        /// Returns the specified parameter name without modification.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The specified parameter name.</returns>
        public string BuildParameterName(string name) => name;

        /// <summary>
        /// Clears the cached stored procedure parameter definitions.
        /// </summary>
        /// <remarks>
        /// Parameter definitions retrieved by <see cref="AssignParameters"/> are cached internally.
        /// Call this method to clear the cache.
        /// </remarks>
        public static void ClearParameterCache() => ParameterCache.Clear();

        private DbCommand CreateCommand(CommandType commandType, string commandText)
        {
            ArgumentException.ThrowIfNullOrEmpty(commandText);

            var command = _dbProviderFactory.CreateCommand()!;
            command.CommandType = commandType;
            command.CommandText = commandText;
            return command;
        }

        /// <summary>
        /// Creates a database connection using the configured connection string.
        /// The connection is not opened automatically.
        /// </summary>
        /// <returns>A new, unopened database connection.</returns>
        public DbConnection CreateConnection()
        {
            var connection = _dbProviderFactory.CreateConnection()!;
            connection.ConnectionString = _connectionString;
            return connection;
        }

        private DbConnection OpenConnection()
        {
            var connection = CreateConnection();
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the database provider supports parameter discovery.
        /// </summary>
        public bool SupportsParameterDiscovery
            => _deriveParametersMethodProxy is not null ? true
            : _noDeriveParametersMethod ? false
            : (_deriveParametersMethodProxy = CreateDeriveParametersMethodProxy()) is not null;

        private bool _noDeriveParametersMethod;
        private Action<DbCommand>? _deriveParametersMethodProxy;

        private Action<DbCommand>? CreateDeriveParametersMethodProxy()
        {
            using var cb = _dbProviderFactory.CreateCommandBuilder();
            if (cb is null)
            {
                _noDeriveParametersMethod = true;
                return null;
            }
            var mi = cb.GetType().GetMethod(nameof(DeriveParameters), BindingFlags.Public | BindingFlags.Static);
            if (mi is null)
            {
                _noDeriveParametersMethod = true;
                return null;
            }

            var parameters = mi.GetParameters();
            if (parameters.Length != 1)
            {
                _noDeriveParametersMethod = true;
                return null;
            }

            // void CallDeriveParameters(DbCommand command) => XxxCommandBuilder.DeriveParameters((XxxCommand)command);
            var inputParam = Expression.Parameter(typeof(DbCommand), "command");
            var targetParamType = parameters[0].ParameterType;
            var castParam = Expression.Convert(inputParam, targetParamType);
            var body = Expression.Call(mi, castParam);
            var lambda = Expression.Lambda<Action<DbCommand>>(body, inputParam);
            var method = lambda.Compile();
            return method;
        }

        private void DeriveParameters(DbCommand discoveryCommand)
        {
            if (_deriveParametersMethodProxy is null)
            {
                if (_noDeriveParametersMethod || (_deriveParametersMethodProxy = CreateDeriveParametersMethodProxy()) is null)
                    throw new NotSupportedException($"{_dbProviderFactory.GetType().Name} does not support parameter discovery.");
            }
            _deriveParametersMethodProxy(discoveryCommand);
        }

        /// <summary>
        /// Discovers the parameters of the stored procedure specified by the command
        /// and adds them to the command.
        /// </summary>
        /// <param name="command">The command representing the stored procedure.</param>
        public void DiscoverParameters(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            using var discoveryCommand = CreateCommand(command.CommandType, command.CommandText);
            discoveryCommand.Connection = connection;
            DeriveParameters(discoveryCommand);
            foreach (var param in discoveryCommand.Parameters)
                command.Parameters.Add(((ICloneable)param).Clone());
        }

        #region ExecuteDataSet

        /// <summary>
        /// Executes the specified command and returns the results in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(DbCommand command)
        {
            var dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table");
            return dataSet;
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns the results
        /// in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(DbCommand command, DbTransaction transaction)
        {
            var dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table", transaction);
            return dataSet;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns the results in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The parameter values to assign to the stored procedure, in parameter order.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteDataSet(command);
        }

        /// <summary>
        /// Executes the specified stored procedure within the given transaction and returns
        /// the results in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the stored procedure.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The parameter values to assign to the stored procedure, in parameter order.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteDataSet(command, transaction);
        }

        /// <summary>
        /// Executes the specified command and returns the results in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text to execute.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteDataSet(command);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns the results
        /// in a <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text to execute.</param>
        /// <returns>A <see cref="System.Data.DataSet"/> containing the query results.</returns>
        public DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteDataSet(command, transaction);
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the specified command within the given transaction.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the specified stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The parameter values to assign to the stored procedure, in parameter order.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQuery(command);
        }

        /// <summary>
        /// Executes the specified stored procedure within the given transaction.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the stored procedure.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The parameter values to assign to the stored procedure, in parameter order.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQuery(command, transaction);
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQuery(command);
        }

        /// <summary>
        /// Executes the specified command within the given transaction.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text to execute.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQuery(command, transaction);
        }

        #endregion

        #region ExecuteReader

        /// <summary>
        /// Executes the specified command and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="command">The command to execute.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Connection = OpenConnection();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteReader();
        }

        /// <summary>
        /// Executes the specified stored procedure and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReader(command);
        }

        /// <summary>
        /// Executes the specified stored procedure within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReader(command, transaction);
        }

        /// <summary>
        /// Executes the specified command and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReader(command);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>A data reader containing the query results.</returns>
        public IDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReader(command, transaction);
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Executes the specified command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Executes the specified stored procedure and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalar(command);
        }

        /// <summary>
        /// Executes the specified stored procedure within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalar(command, transaction);
        }

        /// <summary>
        /// Executes the specified command and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalar(command);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns>
        /// The first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public object? ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalar(command, transaction);
        }

        #endregion

        /// <summary>
        /// Creates a database adapter.
        /// </summary>
        /// <returns>A new database adapter.</returns>
        public DbDataAdapter GetDataAdapter() => _dbProviderFactory.CreateDataAdapter()!;

        /// <summary>
        /// Gets the value of the specified parameter from the command.
        /// </summary>
        /// <param name="command">The command containing the parameter.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The value of the specified parameter.</returns>
        public object? GetParameterValue(DbCommand command, string name)
        {
            ArgumentNullException.ThrowIfNull(command);

            return command.Parameters[name].Value;
        }

        /// <summary>
        /// Creates a command for executing the specified SQL query.
        /// </summary>
        /// <param name="query">The SQL query to execute.</param>
        /// <returns>A command configured to execute the specified query.</returns>
        public DbCommand GetSqlStringCommand(string query)
        {
            ArgumentException.ThrowIfNullOrEmpty(query);

            return CreateCommand(CommandType.Text, query);
        }

        /// <summary>
        /// Creates a command for executing the specified stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <returns>A command configured to execute the specified stored procedure.</returns>
        public DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            ArgumentException.ThrowIfNullOrEmpty(storedProcedureName);

            return CreateCommand(CommandType.StoredProcedure, storedProcedureName);
        }

        /// <summary>
        /// Creates a command for executing the specified stored procedure and assigns the specified parameter values.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A command configured to execute the specified stored procedure.</returns>
        public DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            ArgumentException.ThrowIfNullOrEmpty(storedProcedureName);

            var command = CreateCommand(CommandType.StoredProcedure, storedProcedureName);
            AssignParameters(command, parameterValues);
            return command;
        }

        /// <summary>
        /// Discovers the parameters of the stored procedure specified by the command
        /// and assigns the specified values to them in order.
        /// </summary>
        /// <param name="command">
        /// The command representing the stored procedure.
        /// The <see cref="System.Data.Common.DbCommand.CommandType"/>
        /// must be <see cref="System.Data.CommandType.StoredProcedure"/>.
        /// </param>
        /// <param name="parameterValues">
        /// The values to assign to the discovered parameters, in parameter order.
        /// </param>
        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(parameterValues);

            ParameterCache.DiscoverParametersWithCache(command, this);

            var parameters = command.Parameters;
            int startIndex = (parameters.Count > 0 && parameters[0].Direction == ParameterDirection.ReturnValue) ? 1 : 0;
            if (parameters.Count - startIndex != parameterValues.Length)
                throw new InvalidOperationException("Number of parameters do not match.");

            for (int i = 0; i < parameterValues.Length; i++)
                parameters[startIndex + i].Value = parameterValues[i] ?? DBNull.Value;
        }

        private static class ParameterCache
        {
            private static readonly Dictionary<string, DbParameter[]> _cache = new();
            private static readonly ReaderWriterLockSlim _lock = new();

            public static void DiscoverParametersWithCache(DbCommand command, Database database)
            {
                string key = database.ConnectionString + ":" + command.CommandText;

                DbParameter[]? cachedParameters;

                _lock.EnterReadLock();
                try
                {
                    _cache.TryGetValue(key, out cachedParameters);
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                // ロック時間を最小にするため、外側で処理する。
                if (cachedParameters is not null)
                {
                    foreach (var param in cachedParameters)
                        command.Parameters.Add(((ICloneable)param).Clone());
                    return;
                }

                // 複数スレッドで同じキーのパラメータを取得する可能性があるが、そのうちどれかをキャッシュすれば問題ない。
                database.DiscoverParameters(command);

                var sourceParams = command.Parameters;
                cachedParameters = new DbParameter[sourceParams.Count];
                for (int i = 0; i < cachedParameters.Length; i++)
                    cachedParameters[i] = (DbParameter)((ICloneable)sourceParams[i]).Clone();

                _lock.EnterWriteLock();
                try
                {
                    _cache[key] = cachedParameters;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            public static void Clear()
            {
                _lock.EnterWriteLock();
                try
                {
                    _cache.Clear();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Creates a command for executing the specified stored procedure and associates the specified source columns with its parameters.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="sourceColumns">The source column names to associate with the stored procedure parameters, in parameter order.</param>
        /// <returns>A command configured to execute the specified stored procedure.</returns>
        public DbCommand GetStoredProcCommandWithSourceColumns(string storedProcedureName, params string[] sourceColumns)
        {
            ArgumentException.ThrowIfNullOrEmpty(storedProcedureName);
            ArgumentNullException.ThrowIfNull(sourceColumns);

            var command = GetStoredProcCommand(storedProcedureName);

            // ?
            using (var connection = CreateConnection())
            {
                command.Connection = connection;
                DiscoverParameters(command);
            }

            int i = 0;
            foreach (DbParameter param in command.Parameters)
            {
                if (param.Direction is ParameterDirection.Input or ParameterDirection.InputOutput)
                {
                    param.SourceColumn = sourceColumns[i];
                    i++;
                }
            }

            return command;
        }

        #region LoadDataSet

        /// <summary>
        /// Executes the specified command and loads the result into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="dataSet">The data set into which the result is loaded.</param>
        /// <param name="tableName">The name of the table into which the result is loaded.</param>
        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName)
            => LoadDataSet(command, dataSet, new[] { tableName });

        /// <summary>
        /// Executes the specified command within the given transaction and loads the result into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="dataSet">The data set into which the result is loaded.</param>
        /// <param name="tableName">The name of the table into which the result is loaded.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName, DbTransaction transaction)
            => LoadDataSet(command, dataSet, new[] { tableName }, transaction);

        /// <summary>
        /// Executes the specified command and loads the result into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            DoLoadDataSet(command, dataSet, tableNames);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and loads the results into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            DoLoadDataSet(command, dataSet, tableNames);
        }

        /// <summary>
        /// Executes the specified stored procedure and loads the results into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        public void LoadDataSet(string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            LoadDataSet(command, dataSet, tableNames);
        }

        /// <summary>
        /// Executes the specified stored procedure within the given transaction and loads the results into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        public void LoadDataSet(DbTransaction transaction, string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            LoadDataSet(command, dataSet, tableNames, transaction);
        }

        /// <summary>
        /// Executes the specified command and loads the result into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        public void LoadDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            using var command = CreateCommand(commandType, commandText);
            LoadDataSet(command, dataSet, tableNames);
        }

        /// <summary>
        /// Executes the specified command within the given transaction and loads the results into the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="dataSet">The data set into which the results are loaded.</param>
        /// <param name="tableNames">The names of the tables into which the results are loaded, in result set order.</param>
        public void LoadDataSet(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            using var command = CreateCommand(commandType, commandText);
            LoadDataSet(command, dataSet, tableNames, transaction);
        }

        private void DoLoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames)
        {
            ArgumentNullException.ThrowIfNull(dataSet);
            ArgumentNullException.ThrowIfNull(tableNames);
            if (tableNames.Length == 0)
                throw new ArgumentException($"{nameof(tableNames)} cannot be empty.", nameof(tableNames));
            for (int i = 0; i < tableNames.Length; i++)
            {
                if (string.IsNullOrEmpty(tableNames[i]))
                    throw new ArgumentException($"{nameof(tableNames)}[{i}] cannot be empty.", nameof(tableNames));
            }

            using var adapter = GetDataAdapter();
            adapter.SelectCommand = command;

            for (int i = 0; i < tableNames.Length; i++)
                adapter.TableMappings.Add((i == 0) ? "Table" : "Table" + i, tableNames[i]);

            adapter.Fill(dataSet);
        }

        #endregion

        /// <summary>
        /// Sets the value of the specified parameter in the command.
        /// </summary>
        /// <param name="command">The command containing the parameter.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to assign to the parameter.</param>
        public void SetParameterValue(DbCommand command, string parameterName, object? value)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Parameters[parameterName].Value = value ?? DBNull.Value;
        }

        #region UpdateDataSet

        /// <summary>
        /// Updates the database with the changes in the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="dataSet">The data set containing the changes to apply.</param>
        /// <param name="tableName">The name of the table in the data set to update.</param>
        /// <param name="insertCommand">The command to execute for inserting new rows, or <see langword="null"/> if no insert operation is required.</param>
        /// <param name="updateCommand">The command to execute for updating existing rows, or <see langword="null"/> if no update operation is required.</param>
        /// <param name="deleteCommand">The command to execute for deleting rows, or <see langword="null"/> if no delete operation is required.</param>
        /// <param name="updateBehavior">The behavior to apply when an error occurs during the update.</param>
        /// <param name="updateBatchSize">The batch size to use for the update operation, or <see langword="null"/> to use the default value.</param>
        /// <returns>The number of rows successfully updated in the database.</returns>
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, UpdateBehavior updateBehavior, int? updateBatchSize)
        {
            using var connection = OpenConnection();
            if (updateBehavior == UpdateBehavior.Transactional)
            {
                using var transaction = connection.BeginTransaction();
                int rowsAffected = UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, transaction, updateBatchSize);
                transaction.Commit();
                return rowsAffected;
            }

            insertCommand?.Connection = connection;
            updateCommand?.Connection = connection;
            deleteCommand?.Connection = connection;

            return DoUpdateDataSet(updateBehavior, dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBatchSize);
        }

        /// <summary>
        /// Updates the database with the changes in the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="dataSet">The data set containing the changes to apply.</param>
        /// <param name="tableName">The name of the table in the data set to update.</param>
        /// <param name="insertCommand">The command to execute for inserting new rows, or <see langword="null"/> if no insert operation is required.</param>
        /// <param name="updateCommand">The command to execute for updating existing rows, or <see langword="null"/> if no update operation is required.</param>
        /// <param name="deleteCommand">The command to execute for deleting rows, or <see langword="null"/> if no delete operation is required.</param>
        /// <param name="updateBehavior">The behavior to apply when an error occurs during the update.</param>
        /// <returns>The number of rows successfully updated in the database.</returns>
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, UpdateBehavior updateBehavior)
            => UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBehavior, null);

        /// <summary>
        /// Updates the database with the changes in the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="dataSet">The data set containing the changes to apply.</param>
        /// <param name="tableName">The name of the table in the data set to update.</param>
        /// <param name="insertCommand">The command to execute for inserting new rows, or <see langword="null"/> if no insert operation is required.</param>
        /// <param name="updateCommand">The command to execute for updating existing rows, or <see langword="null"/> if no update operation is required.</param>
        /// <param name="deleteCommand">The command to execute for deleting rows, or <see langword="null"/> if no delete operation is required.</param>
        /// <param name="transaction">The transaction in which to execute the update operations.</param>
        /// <param name="updateBatchSize">The batch size to use for the update operation, or <see langword="null"/> to use the default value.</param>
        /// <returns>The number of rows successfully updated in the database.</returns>
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, DbTransaction transaction, int? updateBatchSize)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            if (insertCommand is not null)
            {
                insertCommand.Connection = transaction.Connection;
                insertCommand.Transaction = transaction;
            }
            if (updateCommand is not null)
            {
                updateCommand.Connection = transaction.Connection;
                updateCommand.Transaction = transaction;
            }
            if (deleteCommand is not null)
            {
                deleteCommand.Connection = transaction.Connection;
                deleteCommand.Transaction = transaction;
            }

            return DoUpdateDataSet(UpdateBehavior.Transactional, dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBatchSize);
        }

        /// <summary>
        /// Updates the database with the changes in the specified <see cref="System.Data.DataSet"/>.
        /// </summary>
        /// <param name="dataSet">The data set containing the changes to apply.</param>
        /// <param name="tableName">The name of the table in the data set to update.</param>
        /// <param name="insertCommand">The command to execute for inserting new rows, or <see langword="null"/> if no insert operation is required.</param>
        /// <param name="updateCommand">The command to execute for updating existing rows, or <see langword="null"/> if no update operation is required.</param>
        /// <param name="deleteCommand">The command to execute for deleting rows, or <see langword="null"/> if no delete operation is required.</param>
        /// <param name="transaction">The transaction in which to execute the update operations.</param>
        /// <returns>The number of rows successfully updated in the database.</returns>
        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, DbTransaction transaction)
            => UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, transaction, null);

        private int DoUpdateDataSet(UpdateBehavior updateBehavior, DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, int? updateBatchSize)
        {
            ArgumentException.ThrowIfNullOrEmpty(tableName);
            ArgumentNullException.ThrowIfNull(dataSet);
            if (insertCommand is null && updateCommand is null && deleteCommand is null)
                throw new ArgumentException("At least one command must be specified.");

            using var adapter = GetDataAdapter();
            if (updateBehavior == UpdateBehavior.Continue)
                SetUpRowUpdatedEvent(adapter);

            if (insertCommand is not null)
                adapter.InsertCommand = insertCommand;
            if (updateCommand is not null)
                adapter.UpdateCommand = updateCommand;
            if (deleteCommand is not null)
                adapter.DeleteCommand = deleteCommand;

            if (updateBatchSize is not null)
            {
                adapter.UpdateBatchSize = updateBatchSize.Value;
                insertCommand?.UpdatedRowSource = UpdateRowSource.None;
                updateCommand?.UpdatedRowSource = UpdateRowSource.None;
                deleteCommand?.UpdatedRowSource = UpdateRowSource.None;
            }

            return adapter.Update(dataSet.Tables[tableName]!);
        }

        private Delegate? onRowUpdatedEventHandler;

        private void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            var t = adapter.GetType();
            var evt = t.GetEvent("RowUpdated");
            if (evt is null)
                throw new NotSupportedException($"{nameof(UpdateBehavior)}.{nameof(UpdateBehavior.Continue)} is not supported.");

            if (onRowUpdatedEventHandler is null)
            {
                var handler = typeof(Database).GetMethod(nameof(OnRowUpdated), BindingFlags.NonPublic | BindingFlags.Static)!;
                onRowUpdatedEventHandler = CreateEventProxy(evt, handler);
            }
            evt.AddEventHandler(adapter, onRowUpdatedEventHandler);
        }

        private static void OnRowUpdated(object sender, RowUpdatedEventArgs e)
        {
            if (e.RecordsAffected == 0)
            {
                if (e.Errors is not null)
                {
                    e.Row.RowError = "Failed to update row";
                    e.Status = UpdateStatus.SkipCurrentRow;
                }
            }
        }

        // https://jonnybekkum.wordpress.com/2012/02/19/how-to-c-connect-to-dynamic-event/
        private static Delegate CreateEventProxy(EventInfo evt, MethodInfo handler)
        {
            var handlerType = evt.EventHandlerType!;
            var eventParams = handlerType.GetMethod("Invoke")!.GetParameters();
            var parameters = new ParameterExpression[2];
            parameters[0] = Expression.Parameter(eventParams[0].ParameterType, "sender");
            parameters[1] = Expression.Parameter(eventParams[1].ParameterType, "e");
            var body = Expression.Call(handler, parameters[0], parameters[1]);
            var lambda = Expression.Lambda(body, parameters);
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke");
        }

        #endregion

        // Async

#if !NET35

        private async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = CreateConnection();
            try
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return connection;
            }
            catch
            {
#if NETFRAMEWORK
                connection.Dispose();
#else
                await connection.DisposeAsync().ConfigureAwait(false);
#endif
                throw;
            }
        }

        #region ExecuteNonQueryAsync

        /// <summary>
        /// Executes the specified command asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
#if !NETFRAMEWORK
            await using var _ = connection.ConfigureAwait(false);
#else
            using var _ = connection;
#endif
            command.Connection = connection;
            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns the number of rows affected.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public Task<int> ExecuteNonQueryAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns the number of rows affected.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command, transaction).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns the number of rows affected.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously and returns the number of rows affected.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns the number of rows affected.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteNonQueryAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region ExecuteReaderAsync

        /// <summary>
        /// Executes the specified command asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteReaderAsync(cancellationToken);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command, transaction).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is automatically closed when the data reader is closed.
        /// </remarks>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns a data reader.
        /// </summary>
        /// <remarks>
        /// The connection is not automatically closed when the data reader is closed.
        /// The caller is responsible for managing the connection lifecycle.
        /// </remarks>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a data reader with the query results.</returns>
        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteReaderAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region ExecuteScalarAsync

        /// <summary>
        /// Executes the specified command asynchronously and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
#if !NETFRAMEWORK
            await using var _ = connection.ConfigureAwait(false);
#else
            using var _ = connection;
#endif
            command.Connection = connection;
            return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public Task<object?> ExecuteScalarAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command, transaction).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified stored procedure asynchronously within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="parameterValues">The values to assign to the stored procedure parameters, in parameter order.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
            var command = GetStoredProcCommand(storedProcedureName, parameterValues);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the specified command asynchronously within the given transaction and returns the first column of the first row in the result set.
        /// </summary>
        /// <param name="transaction">The transaction in which to execute the command.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the first column of the first row in the result set, or <see langword="null"/> if the result set is empty.
        /// Returns <see cref="System.DBNull.Value"/> if the value is a database null.
        /// </returns>
        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            var command = CreateCommand(commandType, commandText);
#if !NETFRAMEWORK
            await using var _ = command.ConfigureAwait(false);
#else
            using var _ = command;
#endif
            return await ExecuteScalarAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

#endif
    }
}
