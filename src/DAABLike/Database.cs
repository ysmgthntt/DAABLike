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
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _dbProviderFactory = dbProviderFactory ?? throw new ArgumentNullException(nameof(dbProviderFactory));
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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

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

        public bool SupportsParameterDiscovery => true;

        private void DeriveParameters(DbCommand discoveryCommand)
        {
            var cb = _dbProviderFactory.CreateCommandBuilder()!;
            var t = cb.GetType();
            var mi = t.GetMethod("DeriveParameters", BindingFlags.Public | BindingFlags.Static);
            if (mi is null)
                throw new NotSupportedException($"{t.Name} has no DeriveParameters method.");
            mi.Invoke(cb, new[] { discoveryCommand });
        }

        public void DiscoverParameters(DbCommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(DbCommand command, DbTransaction transaction)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            command.Connection = OpenConnection();
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        public IDataReader ExecuteReader(DbCommand command, DbTransaction transaction)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using var connection = OpenConnection();
            command.Connection = connection;
            return command.ExecuteScalar();
        }

        public object? ExecuteScalar(DbCommand command, DbTransaction transaction)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            return command.Parameters[name].Value;
        }

        public DbCommand GetSqlStringCommand(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentNullException(nameof(query));

            return CreateCommand(CommandType.Text, query);
        }

        public DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));

            return CreateCommand(CommandType.StoredProcedure, storedProcedureName);
        }

        public DbCommand GetStoredProcCommand(string storedProcedureName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));

            var command = CreateCommand(CommandType.StoredProcedure, storedProcedureName);
            AssignParameters(command, parameterValues);
            return command;
        }

        public void AssignParameters(DbCommand command, object[] parameterValues)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (parameterValues is null)
                throw new ArgumentNullException(nameof(parameterValues));

            var parameters = command.Parameters;
            int startIndex = (parameters.Count > 0 && parameters[0].Direction == ParameterDirection.ReturnValue) ? 1 : 0;
            if (parameters.Count - startIndex != parameterValues.Length)
                throw new InvalidOperationException("Number of parameters do not match.");

            for (int i = 0; i < parameterValues.Length; i++)
                parameters[startIndex + i].Value = parameterValues[i] ?? DBNull.Value;
        }

        public DbCommand GetStoredProcCommandWithSourceColumns(string storedProcedureName, params string[] sourceColumns)
        {
            if (string.IsNullOrEmpty(storedProcedureName))
                throw new ArgumentNullException(nameof(storedProcedureName));
            if (sourceColumns is null)
                throw new ArgumentNullException(nameof(sourceColumns));

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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            using var connection = OpenConnection();
            command.Connection = connection;
            DoLoadDataSet(command, dataSet, tableNames);
        }

        public void LoadDataSet(DbCommand command, DataSet dataSet, string[] tableNames, DbTransaction transaction)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

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
            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));
            if (tableNames is null)
                throw new ArgumentNullException(nameof(tableNames));
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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

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
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));
            if (dataSet is null)
                throw new ArgumentNullException(nameof(dataSet));
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
                onRowUpdatedEventHandler = CreateEventHandler(evt, handler);
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
        private static Delegate CreateEventHandler(EventInfo evt, MethodInfo handler)
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
            if (command is null)
                throw new ArgumentNullException(nameof(command));

#if !NETFRAMEWORK
            await
#endif
            using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            command.Connection = connection;
            return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<int> ExecuteNonQueryAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteNonQueryAsync(cancellationToken);
        }

        public Task<int> ExecuteNonQueryAsync(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQueryAsync(command);
        }

        public Task<int> ExecuteNonQueryAsync(string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQueryAsync(command, transaction);
        }

        public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteNonQueryAsync(command, transaction, cancellationToken);
        }

        public Task<int> ExecuteNonQueryAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQueryAsync(command, cancellationToken);
        }

        public Task<int> ExecuteNonQueryAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteNonQueryAsync(command, transaction, cancellationToken);
        }

        #endregion

        #region ExecuteReaderAsync

        public async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            command.Connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteReaderAsync(cancellationToken);
        }

        public Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReaderAsync(command);
        }

        public Task<DbDataReader> ExecuteReaderAsync(string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReaderAsync(command, cancellationToken);
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReaderAsync(command, transaction);
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteReaderAsync(command, transaction, cancellationToken);
        }

        public Task<DbDataReader> ExecuteReaderAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReaderAsync(command, cancellationToken);
        }

        public Task<DbDataReader> ExecuteReaderAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteReaderAsync(command, transaction, cancellationToken);
        }

        #endregion

        #region ExecuteScalarAsync

        public async Task<object?> ExecuteScalarAsync(DbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

#if !NETFRAMEWORK
            await
#endif
            using var connection = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            command.Connection = connection;
            return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<object?> ExecuteScalarAsync(DbCommand command, DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            command.Connection = transaction.Connection;
            command.Transaction = transaction;
            return command.ExecuteScalarAsync(cancellationToken);
        }

        public Task<object?> ExecuteScalarAsync(string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalarAsync(command);
        }

        public Task<object?> ExecuteScalarAsync(string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalarAsync(command, cancellationToken);
        }

        public Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, params object[] parameterValues)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalarAsync(command, transaction);
        }

        public Task<object?> ExecuteScalarAsync(DbTransaction transaction, string storedProcedureName, object[] parameterValues, CancellationToken cancellationToken)
        {
            using var command = GetStoredProcCommand(storedProcedureName, parameterValues);
            return ExecuteScalarAsync(command, transaction, cancellationToken);
        }

        public Task<object?> ExecuteScalarAsync(CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalarAsync(command, cancellationToken);
        }

        public Task<object?> ExecuteScalarAsync(DbTransaction transaction, CommandType commandType, string commandText, CancellationToken cancellationToken = default)
        {
            using var command = CreateCommand(commandType, commandText);
            return ExecuteScalarAsync(command, transaction, cancellationToken);
        }

        #endregion

#endif
    }
}
