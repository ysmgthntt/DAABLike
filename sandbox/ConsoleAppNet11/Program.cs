using DAABLike;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

#pragma warning disable CA2007

Console.WriteLine("Hello, World!");

const string dbProviderName = "Microsoft.Data.SqlClient";
const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=DataAccessTracingTestDB;Integrated Security=true";

Approach approach = Approach.DatabaseFactory;
Database db;

if (approach == Approach.NoFactory)
{
    db = new Database(connectionString, SqlClientFactory.Instance);
}
else if (approach == Approach.DatabaseProviderFactory)
{
    var factory = DatabaseProviderFactory.Builder()
        .RegisterDefault(connectionString, SqlClientFactory.Instance)
        .Build();
    db = factory.CreateDefault();
}
else if (approach == Approach.DatabaseProviderFactoryWithDbProviderFactories)
{
    DbProviderFactories.RegisterFactory(dbProviderName, SqlClientFactory.Instance);
    var factory = DatabaseProviderFactory.Builder()
        .RegisterDefault(connectionString, dbProviderName)
        .Build();
    db = factory.CreateDefault();
}
else if (approach == Approach.DatabaseFactory)
{
    DbProviderFactories.RegisterFactory(dbProviderName, SqlClientFactory.Instance);
    var factory = DatabaseProviderFactory.Builder()
        .RegisterDefault(connectionString, dbProviderName)
        .Build();
    DatabaseFactory.SetDatabaseProviderFactory(factory);
    db = DatabaseFactory.CreateDatabase();
}
else
{
    return;
}


//var result = db.ExecuteNonQuery("sp_help", "sys.objects");
await db.ExecuteNonQueryAsync("sp_help", "sys.objects");
await db.ExecuteNonQueryAsync("sp_help", CancellationToken.None, "sys.objects");

await using (var dr = await db.ExecuteReaderAsync("sp_help", ["sys.objects"]))
{
    while (await dr.ReadAsync())
        Console.WriteLine($"{await dr.GetFieldValueAsync<string>(0)}");
}
await using (var dr = await db.ExecuteReaderAsync("sp_help", CancellationToken.None, "sys.objects"))
{
    while (await dr.ReadAsync())
        Console.WriteLine($"{await dr.GetFieldValueAsync<string>(0)}");
}
await using (var dr = await db.ExecuteReaderAsync(CommandType.StoredProcedure, "sp_who"))
{
    while (await dr.ReadAsync())
        Console.WriteLine($"{await dr.GetFieldValueAsync<short>(0)}");
}

await db.ExecuteScalarAsync("sp_help", "sys.objects");
await db.ExecuteScalarAsync("sp_help", CancellationToken.None, "sys.objects");
await db.ExecuteScalarAsync(CommandType.StoredProcedure, "sp_who");

await using (var command = db.GetSqlStringCommand("SELECT * FROM TestTable"))
{
    db.AddInParameter(command, "@test", DbType.AnsiString, "test");

    {
        using (var dr = db.ExecuteReader(command))
        {
            while (dr.Read())
                Console.WriteLine($"{dr["Id"]}: {dr["Value"]}");
        }
        await using (var dr = await db.ExecuteReaderAsync(command))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<int>(0)}: {await dr.GetFieldValueAsync<string>(1)}");
        }
        Console.WriteLine(db.ExecuteScalar(command));
        Console.WriteLine(await db.ExecuteScalarAsync(command));
        db.ExecuteNonQuery(command);
        await db.ExecuteNonQueryAsync(command);
        using var ds = db.ExecuteDataSet(command);
    }

    await using var connection = db.CreateConnection();
    await connection.OpenAsync();
    await using (var trans = await connection.BeginTransactionAsync())
    {
        db.ExecuteNonQuery(trans, "sp_help", "sys.objects");
        await db.ExecuteNonQueryAsync(trans, "sp_help", "sys.objects");
        await db.ExecuteNonQueryAsync(trans, "sp_help", CancellationToken.None, "sys.objects");

        await db.ExecuteNonQueryAsync(CommandType.StoredProcedure, "sp_who");
        await db.ExecuteNonQueryAsync(trans, CommandType.StoredProcedure, "sp_who");

        using (var dr = db.ExecuteReader(command, trans))
        {
            while (dr.Read())
                Console.WriteLine($"{dr["Id"]}: {dr["Value"]}");
        }
        await using (var dr = await db.ExecuteReaderAsync(command, trans))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<int>(0)}: {await dr.GetFieldValueAsync<string>(1)}");
        }
        await using (var dr = await db.ExecuteReaderAsync(trans, "sp_help", ["sys.objects"]))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<string>(0)}");
        }
        await using (var dr = await db.ExecuteReaderAsync(trans, "sp_help", CancellationToken.None, "sys.objects"))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<string>(0)}");
        }
        await using (var dr = await db.ExecuteReaderAsync(trans, CommandType.StoredProcedure, "sp_who"))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<short>(0)}");
        }

        await db.ExecuteScalarAsync(trans, "sp_help", "sys.objects");
        await db.ExecuteScalarAsync(trans, "sp_help", CancellationToken.None, "sys.objects");
        await db.ExecuteScalarAsync(trans, CommandType.StoredProcedure, "sp_who");

        Console.WriteLine(db.ExecuteScalar(command, trans));
        Console.WriteLine(await db.ExecuteScalarAsync(command, trans));
        db.ExecuteNonQuery(command, trans);
        await db.ExecuteNonQueryAsync(command, trans);

        using var ds = db.ExecuteDataSet(command, trans);
        using var cmdInsert = db.GetSqlStringCommand("INSERT INTO TestTable VALUES (@Id, @Value)");
        using var cmdUpdate = db.GetSqlStringCommand("UPDATE TestTable SET Id = @Id, Value = @Value WHERE Id = @Old_Id");
        using var cmdDelete = db.GetSqlStringCommand("DELETE FROM TestTable WHERE Id = @Old_Id");
        db.AddInParameter(cmdInsert, "@Id", DbType.Int32, "Id", DataRowVersion.Current);
        db.AddInParameter(cmdInsert, "@Value", DbType.String, "Value", DataRowVersion.Current);

        db.AddInParameter(cmdUpdate, "@Id", DbType.Int32, "Id", DataRowVersion.Current);
        db.AddInParameter(cmdUpdate, "@Value", DbType.String, "Value", DataRowVersion.Current);
        db.AddInParameter(cmdUpdate, "@Old_Id", DbType.Int32, "Id", DataRowVersion.Original);

        db.AddInParameter(cmdDelete, "@Old_Id", DbType.Int32, "Id", DataRowVersion.Original);

        var dt = ds.Tables[0];
        var newrow = dt.NewRow();
        newrow["Id"] = 4;
        newrow["Value"] = "Val4";
        dt.Rows.Add(newrow);

        if (dt.Select("Id = 2") is [var row2, ..])
            row2["Value"] = "Updated";

        if (dt.Select("Id = 3") is [var row3, ..])
            row3.Delete();

        db.UpdateDataSet(ds, dt.TableName, cmdInsert, cmdUpdate, cmdDelete, trans);
        Console.WriteLine();
        await using (var dr = await db.ExecuteReaderAsync(command, trans))
        {
            while (await dr.ReadAsync())
                Console.WriteLine($"{await dr.GetFieldValueAsync<int>(0)}: {await dr.GetFieldValueAsync<string>(1)}");
        }
        await trans.RollbackAsync();

        if (dt.Select("Id = 2") is [var row22, ..])
            row22["Value"] = "12345678901";

        db.UpdateDataSet(ds, dt.TableName, cmdInsert, cmdUpdate, cmdDelete, UpdateBehavior.Continue);
    }
}

enum Approach
{
    NoFactory,
    DatabaseFactory,
    DatabaseProviderFactory,
    DatabaseProviderFactoryWithDbProviderFactories,
}
