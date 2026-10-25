using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace DAABLike
{
    public sealed class Database
    {
        private readonly string _connectionString;
        private readonly DbProviderFactory _dbProviderFactory;

        public Database(string connectionString, DbProviderFactory dbProviderFactory)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentNullException.ThrowIfNull(dbProviderFactory);

            _connectionString = connectionString;
            _dbProviderFactory = dbProviderFactory;
        }

        public string ConnectionString => _connectionString;
        public DbProviderFactory DbProviderFactory => _dbProviderFactory;

        #region AddParameter

        public void AddInParameter(DbCommand command, string name, DbType dbType)
            => AddParameter(command, name, dbType, ParameterDirection.Input, "", DataRowVersion.Default, null);

        public void AddInParameter(DbCommand command, string name, DbType dbType, object? value)
            => AddParameter(command, name, dbType, ParameterDirection.Input, "", DataRowVersion.Default, value);

        public void AddInParameter(DbCommand command, string name, DbType dbType, string? sourceColumn, DataRowVersion sourceVersion)
            => AddParameter(command, name, dbType, 0, ParameterDirection.Input, true, 0, 0, sourceColumn, sourceVersion, null);

        public void AddOutParameter(DbCommand command, string name, DbType dbType, int size)
            => AddParameter(command, name, dbType, size, ParameterDirection.Output, true, 0, 0, "", DataRowVersion.Default, DBNull.Value);

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

        public void AddParameter(DbCommand command, string name, DbType dbType, ParameterDirection direction, string? sourceColumn, DataRowVersion sourceVersion, object? value)
            => AddParameter(command, name, dbType, 0, direction, false, 0, 0, sourceColumn, sourceVersion, value);

        #endregion

        public string BuildParameterName(string name) => name;

        public static void ClearParameterCache() { }

        private DbCommand CreateCommand(CommandType commandType, string commandText)
        {
            var command = _dbProviderFactory.CreateCommand()!;
            command.CommandType = commandType;
            command.CommandText = commandText;
            return command;
        }

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

        public bool SupportsParameterDiscovery
            => _deriveParametersMethodProxy is not null ? true
            : _noDeriveParametersMethod ? false
            : (_deriveParametersMethodProxy = CreateDeriveParametersMethodProxy()) is not null;

        private bool _noDeriveParametersMethod;
        private Action<DbCommand>? _deriveParametersMethodProxy;

        private Action<DbCommand>? CreateDeriveParametersMethodProxy()
        {
            var cb = _dbProviderFactory.CreateCommandBuilder()!;
            var t = cb.GetType();
            var mi = t.GetMethod("DeriveParameters", BindingFlags.Public | BindingFlags.Static);
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

        public DataSet ExecuteDataSet(DbCommand command)
        {
            var dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table");
            return dataSet;
        }

        public DataSet ExecuteDataSet(DbCommand command, DbTransaction transaction)
        {
            var dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            LoadDataSet(command, dataSet, "Table", transaction);
            return dataSet;
        }

        public DataSet ExecuteDataSet(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteDataSet(command);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteDataSet(command, transaction);
        }

        public DataSet ExecuteDataSet(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteDataSet(command);
        }

        public DataSet ExecuteDataSet(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteDataSet(command, transaction);
        }

        #endregion

        #region ExecuteNonQuery

        public int ExecuteNonQuery(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQuery(command);
        }

        public int ExecuteNonQuery(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQuery(command, transaction);
        }

        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQuery(command);
        }

        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQuery(command, transaction);
        }

        #endregion

        #region ExecuteReader

        public IDataReader ExecuteReader(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Connection = OpenConnection();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteReader();
        }

        public IDataReader ExecuteReader(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReader(command);
        }

        public IDataReader ExecuteReader(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReader(command, transaction);
        }

        public IDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReader(command);
        }

        public IDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReader(command, transaction);
        }

        #endregion

        #region ExecuteScalar

        public object? ExecuteScalar(DbCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteScalar();
        }

        public object? ExecuteScalar(DbCommand command, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteScalar();
        }

        public object? ExecuteScalar(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalar(command);
        }

        public object? ExecuteScalar(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalar(command, transaction);
        }

        public object? ExecuteScalar(CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalar(command);
        }

        public object? ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalar(command, transaction);
        }

        #endregion

        public DbDataAdapter GetDataAdapter() => _dbProviderFactory.CreateDataAdapter()!;

        public object? GetParameterValue(DbCommand command, string name)
        {
            ArgumentNullException.ThrowIfNull(command);

            return command.Parameters[name].Value;
        }

        public DbCommand GetSqlStringCommand(string query)
        {
            ArgumentException.ThrowIfNullOrEmpty(query);

            return CreateCommand(CommandType.Text, query);
        }

        public DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            ArgumentException.ThrowIfNullOrEmpty(storedProcedureName);

            return CreateCommand(CommandType.StoredProcedure, storedProcedureName);
        }

        public DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            ArgumentException.ThrowIfNullOrEmpty(storedProcedureName);

            var command = CreateCommand(CommandType.StoredProcedure, storedProcedureName);
            AssignParameters(command, parameterValues);
            return command;
        }

        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(parameterValues);

            // ParameterCache is not implemented.
            DiscoverParameters(command);

            var parameters = command.Parameters;
            int startIndex = (parameters.Count > 0 && parameters[0].Direction == ParameterDirection.ReturnValue) ? 1 : 0;
            if (parameters.Count - startIndex != parameterValues.Length)
                throw new InvalidOperationException("Number of parameters do not match.");

            for (int i = 0; i < parameterValues.Length; i++)
                parameters[startIndex + i].Value = parameterValues[i] ?? DBNull.Value;
        }

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

        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName)
            => LoadDataSet(command, dataSet, new[] { tableName });

        public void LoadDataSet(DbCommand command, DataSet dataSet, string tableName, DbTransaction transaction)
            => LoadDataSet(command, dataSet, new[] { tableName }, transaction);

        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames)
        {
            ArgumentNullException.ThrowIfNull(command);

            using var connection = OpenConnection();
            command.Connection = connection;
            DoLoadDataSet(command, dataSet, tableNames);
        }

        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames, DbTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            DoLoadDataSet(command, dataSet, tableNames);
        }

        public void LoadDataSet(string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            LoadDataSet(command, dataSet, tableNames);
        }

        public void LoadDataSet(DbTransaction transaction, string storedProcedureName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            LoadDataSet(command, dataSet, tableNames, transaction);
        }

        public void LoadDataSet(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            using var command = CreateCommand(commandType, commandText);
            LoadDataSet(command, dataSet, tableNames);
        }

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

        public void SetParameterValue(DbCommand command, string parameterName, object? value)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Parameters[parameterName].Value = value ?? DBNull.Value;
        }

        #region UpdateDataSet

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

            if (insertCommand is not null)
                insertCommand.Connection = connection;
            if (updateCommand is not null)
                updateCommand.Connection = connection;
            if (deleteCommand is not null)
                deleteCommand.Connection = connection;

            return DoUpdateDataSet(updateBehavior, dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBatchSize);
        }

        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, UpdateBehavior updateBehavior)
            => UpdateDataSet(dataSet, tableName, insertCommand, updateCommand, deleteCommand, updateBehavior, null);

        public int UpdateDataSet(DataSet dataSet, string tableName, DbCommand? insertCommand, DbCommand? updateCommand, DbCommand? deleteCommand, DbTransaction transaction, int? updateBatchSize)
        {
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
                if (insertCommand is not null)
                    insertCommand.UpdatedRowSource = UpdateRowSource.None;
                if (updateCommand is not null)
                    updateCommand.UpdatedRowSource = UpdateRowSource.None;
                if (deleteCommand is not null)
                    deleteCommand.UpdatedRowSource = UpdateRowSource.None;
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

        public async Task<int> ExecuteNonQueryAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

#if !NETFRAMEWORK
            await
#endif
            using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            command.Connection = connection;
            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<int> ExecuteNonQueryAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task<int> ExecuteNonQueryAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteNonQueryAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteNonQueryAsync(command, transaction).ConfigureAwait(false);
        }

        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteNonQueryAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteNonQueryAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteNonQueryAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region ExecuteReaderAsync

        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            command.Connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection, cancellationToken).ConfigureAwait(false);
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteReaderAsync(cancellationToken);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteReaderAsync(command).ConfigureAwait(false);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteReaderAsync(command, transaction).ConfigureAwait(false);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteReaderAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteReaderAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteReaderAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

        #region ExecuteScalarAsync

        public async Task<object?> ExecuteScalarAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

#if !NETFRAMEWORK
            await
#endif
            using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            command.Connection = connection;
            return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<object?> ExecuteScalarAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(transaction);

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteScalarAsync(cancellationToken);
        }

        public async Task<object?> ExecuteScalarAsync(string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteScalarAsync(command).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteScalarAsync(string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteScalarAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteScalarAsync(command, transaction).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, CancellationToken cancellationToken, params object[] parameterValues)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return await ExecuteScalarAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteScalarAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteScalarAsync(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
#if !NETFRAMEWORK
            await
#endif
            using var command = CreateCommand(commandType, commandText);
            return await ExecuteScalarAsync(command, transaction, cancellationToken).ConfigureAwait(false);
        }

        #endregion

#endif
    }
}
