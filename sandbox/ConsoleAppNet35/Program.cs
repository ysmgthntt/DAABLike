using DAABLike;

Console.WriteLine("Hello, World!");


bool entlib6Style = false;
if (entlib6Style)
    DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());

var db = DatabaseFactory.CreateDatabase();
Console.WriteLine(db.ConnectionString);

var result = db.ExecuteDataSet("sp_help", "sys.objects");
Console.WriteLine(result);
Console.WriteLine(Environment.Version);
