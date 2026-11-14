using Microsoft.Data.SqlClient;

namespace DAABLike.Tests
{
    public class DatabaseFactoryTest
    {
        [Fact]
        public void CreateDatabase()
        {
            Database db;
#if NET
            Assert.ThrowsAny<Exception>(DatabaseFactory.CreateDatabase);
            Assert.Throws<ArgumentNullException>("name", () => DatabaseFactory.CreateDatabase(null!));
            Assert.ThrowsAny<Exception>(() => DatabaseFactory.CreateDatabase(""));
            DatabaseFactory.ClearDatabaseProviderFactory();
            Assert.ThrowsAny<Exception>(() => DatabaseFactory.CreateDatabase("name"));
#else
            db = DatabaseFactory.CreateDatabase();
            Assert.Equal("default", db.ConnectionString);
            Assert.Equal(System.Data.SqlClient.SqlClientFactory.Instance, db.DbProviderFactory);
            DatabaseFactory.ClearDatabaseProviderFactory();
            db = DatabaseFactory.CreateDatabase("NamedConnection");
            Assert.Equal("named", db.ConnectionString);
            Assert.Equal(System.Data.Odbc.OdbcFactory.Instance, db.DbProviderFactory);
#endif

            Database CreateDefaultDatabase()
                => new Database("test", SqlClientFactory.Instance);

            Database CreateNamedDatabase(string name)
                => name switch
                {
                    "Test1" => new Database("Test1Connection", SqlClientFactory.Instance),
                    "Test2" => new Database("Test2Connection", SqlClientFactory.Instance),
                    _ => throw new ArgumentException("name", nameof(name))
                };

            DatabaseFactory.ClearDatabaseProviderFactory();
            Assert.Throws<ArgumentNullException>("createDefaultDatabase", () => DatabaseFactory.SetDatabases(null!, null!));
            Assert.Throws<ArgumentNullException>("createNamedDatabase", () => DatabaseFactory.SetDatabases(CreateDefaultDatabase, null!));
            DatabaseFactory.SetDatabases(CreateDefaultDatabase, CreateNamedDatabase);
            Assert.Throws<InvalidOperationException>(() => DatabaseFactory.SetDatabases(CreateDefaultDatabase, CreateNamedDatabase));
            DatabaseFactory.SetDatabases(CreateDefaultDatabase, CreateNamedDatabase, false);

            db = DatabaseFactory.CreateDatabase();
            Assert.Equal("test", db.ConnectionString);
            db = DatabaseFactory.CreateDatabase("Test1");
            Assert.Equal("Test1Connection", db.ConnectionString);
            db = DatabaseFactory.CreateDatabase("Test2");
            Assert.Equal("Test2Connection", db.ConnectionString);

            DatabaseFactory.ClearDatabaseProviderFactory();
            var factory = DatabaseProviderFactory.Builder()
                .RegisterDefault("testtest", SqlClientFactory.Instance)
                .Register("Test3", "Test3Connection", SqlClientFactory.Instance)
                .Register("Test4", "Test4Connection", SqlClientFactory.Instance)
                .Build();
            Assert.Throws<ArgumentNullException>("factory", () => DatabaseFactory.SetDatabaseProviderFactory(null!));
            DatabaseFactory.SetDatabaseProviderFactory(factory);
            Assert.Throws<InvalidOperationException>(() => DatabaseFactory.SetDatabaseProviderFactory(factory));
            DatabaseFactory.SetDatabaseProviderFactory(factory, false);

            db = DatabaseFactory.CreateDatabase();
            Assert.Equal("testtest", db.ConnectionString);
            db = DatabaseFactory.CreateDatabase("Test3");
            Assert.Equal("Test3Connection", db.ConnectionString);
            db = DatabaseFactory.CreateDatabase("Test4");
            Assert.Equal("Test4Connection", db.ConnectionString);
        }
    }
}
