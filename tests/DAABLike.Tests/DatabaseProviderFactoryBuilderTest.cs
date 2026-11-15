using System.Data.Common;
#if !NET35
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace DAABLike.Tests
{
    public class DatabaseProviderFactoryBuilderTest
    {
        [Fact]
        public void Build()
        {
            var builder = new DatabaseProviderFactoryBuilder();
            var factory = builder
                .RegisterDefault("default", SqlClientFactory.Instance)
                .Register("Named1", "Named1Connection", SqlClientFactory.Instance)
                .Register("Named2", "Named2Connection", "System.Data.Odbc")
                .Build();

#if NET
            DbProviderFactories.RegisterFactory("System.Data.Odbc", SqlClientFactory.Instance);
#endif

            Database db;
            db = factory.CreateDefault();
            Assert.Equal("default", db.ConnectionString);
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
            db = factory.Create("Named1");
            Assert.Equal("Named1Connection", db.ConnectionString);
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
            db = factory.Create("Named2");
            Assert.Equal("Named2Connection", db.ConnectionString);
#if NET
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
#else
            Assert.Equal(System.Data.Odbc.OdbcFactory.Instance, db.DbProviderFactory);
#endif

            builder = new DatabaseProviderFactoryBuilder();
            factory = builder
                .RegisterDefault("default", "System.Data.Odbc")
                .Register("Named1", "Named1Connection", SqlClientFactory.Instance)
                .Build();
            db = factory.CreateDefault();
            Assert.Equal("default", db.ConnectionString);
#if NET
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
#else
            Assert.Equal(System.Data.Odbc.OdbcFactory.Instance, db.DbProviderFactory);
#endif
            db = factory.Create("Named1");
            Assert.Equal("Named1Connection", db.ConnectionString);
            Assert.Equal(SqlClientFactory.Instance, db.DbProviderFactory);
        }

        [Fact]
        public void RegisterException()
        {
            var builder = new DatabaseProviderFactoryBuilder();
            DbProviderFactory nullFactory = null!;
            string nullText = null!;
            Assert.Throws<ArgumentNullException>("connectionString", () => builder.RegisterDefault(null!, nullFactory));
            Assert.Throws<ArgumentException>("connectionString", () => builder.RegisterDefault("", nullFactory));
            Assert.Throws<ArgumentNullException>("dbProviderFactory", () => builder.RegisterDefault("test", nullFactory));
            Assert.Throws<ArgumentNullException>("connectionString", () => builder.RegisterDefault(null!, nullText));
            Assert.Throws<ArgumentException>("connectionString", () => builder.RegisterDefault("", nullText));
            Assert.Throws<ArgumentNullException>("providerInvariantName", () => builder.RegisterDefault("test", nullText));
            Assert.Throws<ArgumentException>("providerInvariantName", () => builder.RegisterDefault("test", ""));

            Assert.Throws<ArgumentNullException>("name", () => builder.Register(null!, null!, nullFactory));
            Assert.Throws<ArgumentNullException>("connectionString", () => builder.Register("", null!, nullFactory));
            Assert.Throws<ArgumentException>("connectionString", () => builder.Register("", "", nullFactory));
            Assert.Throws<ArgumentNullException>("dbProviderFactory", () => builder.Register("", "test", nullFactory));
            Assert.Throws<ArgumentNullException>("connectionString", () => builder.Register("", null!, nullText));
            Assert.Throws<ArgumentException>("connectionString", () => builder.Register("", "", nullText));
            Assert.Throws<ArgumentNullException>("providerInvariantName", () => builder.Register("", "test", nullText));
            Assert.Throws<ArgumentException>("providerInvariantName", () => builder.Register("", "test", ""));
        }
    }
}
