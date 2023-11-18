// See https://aka.ms/new-console-template for more information
using DAABLike;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;

Console.WriteLine("Hello, World!");


const string DbProviderName = "Microsoft.Data.SqlClient";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddIniFile("appsettings.ini")
    .Build();

var connectionString = configuration.GetConnectionString("TestDB")!;
DbProviderFactories.RegisterFactory(DbProviderName, SqlClientFactory.Instance);

DatabaseFactory.SetDatabaseProviderFactory(DatabaseProviderFactory.Builder()
    .RegisterDefault(connectionString, DbProviderName)
    .Register("NamedDB", connectionString, SqlClientFactory.Instance)
    .Build()
);

var db = DatabaseFactory.CreateDatabase();
db = DatabaseFactory.CreateDatabase();
db = DatabaseFactory.CreateDatabase("NamedDB");
db = DatabaseFactory.CreateDatabase("NamedDB");

using (var command = db.GetSqlStringCommand("SELECT * FROM TestTable"))
{
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
