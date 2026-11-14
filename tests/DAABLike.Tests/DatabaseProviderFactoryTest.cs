namespace DAABLike.Tests
{
    public class DatabaseProviderFactoryTest
    {
        [Fact]
        public void Constructor()
        {
            Assert.Throws<ArgumentNullException>("defaultName", () => new DatabaseProviderFactory(null!));
            Assert.Throws<ArgumentException>("defaultName", () => new DatabaseProviderFactory(""));
        }

        [Fact]
        public void CreateDefault()
        {
            DatabaseProviderFactory factory;
            factory = new DatabaseProviderFactory();
#if NET
            Assert.ThrowsAny<Exception>(factory.CreateDefault);
            factory = new DatabaseProviderFactory("NamedConnection");
            Assert.ThrowsAny<Exception>(factory.CreateDefault);
#else
            Database db;
            db = factory.CreateDefault();
            Assert.Equal("default", db.ConnectionString);
            Assert.Equal(System.Data.SqlClient.SqlClientFactory.Instance, db.DbProviderFactory);

            factory = new DatabaseProviderFactory("NamedConnection");
            db = factory.CreateDefault();
            Assert.Equal("named", db.ConnectionString);
            Assert.Equal(System.Data.Odbc.OdbcFactory.Instance, db.DbProviderFactory);
#endif
        }

        [Fact]
        public void Create()
        {
            var factory = new DatabaseProviderFactory();
#if NET
            Assert.ThrowsAny<Exception>(() => factory.Create("NamedConnection"));
            Assert.ThrowsAny<Exception>(() => factory.Create("DefaultConnection"));
#else
            Database db;
            db = factory.Create("NamedConnection");
            Assert.Equal("named", db.ConnectionString);
            Assert.Equal(System.Data.Odbc.OdbcFactory.Instance, db.DbProviderFactory);
            db = factory.Create("DefaultConnection");
            Assert.Equal("default", db.ConnectionString);
            Assert.Equal(System.Data.SqlClient.SqlClientFactory.Instance, db.DbProviderFactory);
#endif
        }

        [Fact]
        public void CreateException()
        {
            var factory = new DatabaseProviderFactory();
            Assert.Throws<ArgumentNullException>("name", () => factory.Create(null!));
            Assert.ThrowsAny<Exception>(() => factory.Create("test"));
        }
    }
}
