using System.Data.Common;

namespace DAABLike
{
    /// <summary>
    /// Creates <see cref="Database"/> instances using configured database providers and connection strings.
    /// </summary>
    /// <remarks>
    /// On .NET, instances must be configured using <see cref="DatabaseProviderFactoryBuilder"/>.
    /// On .NET Framework, instances can also be created directly using the parameterless constructor or the constructor that specifies the default connection string name.
    /// In this case, the connection strings are read from the application's configuration file (<c>app.config</c>).
    /// If no default connection string name is specified, <c>DefaultConnection</c> is used.
    /// </remarks>
    public sealed class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly string _defaultName;
        private readonly Dictionary<string, DatabaseRegistration> _databaseRegistrations;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class.
        /// </summary>
        /// <remarks>
        /// On .NET Framework, the connection strings are read from the application's configuration file (<c>app.config</c>),
        /// and <c>DefaultConnection</c> is used as the default connection string name.
        /// </remarks>
        public DatabaseProviderFactory() : this("DefaultConnection") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class with the specified default connection string name.
        /// </summary>
        /// <param name="defaultName">The name of the connection string to use when creating a default database.</param>
        /// <remarks>
        /// On .NET Framework, the connection strings are read from the application's configuration file (<c>app.config</c>).
        /// </remarks>
        public DatabaseProviderFactory(string defaultName)
        {
            ArgumentException.ThrowIfNullOrEmpty(defaultName);

            _defaultName = defaultName;
            _databaseRegistrations = new();
#if NETFRAMEWORK
            foreach (System.Configuration.ConnectionStringSettings css in System.Configuration.ConfigurationManager.ConnectionStrings)
                _databaseRegistrations.Add(css.Name, new DatabaseRegistration(css.ConnectionString, css.ProviderName));
#endif
        }

        internal DatabaseProviderFactory(string defaultName, Dictionary<string, DatabaseRegistration> databaseRegistrations)
        {
            _defaultName = defaultName;
            _databaseRegistrations = databaseRegistrations;
        }

        /// <summary>
        /// Creates a <see cref="Database"/> instance using the default database configuration.
        /// </summary>
        /// <returns>A database instance configured with the default database configuration.</returns>
        public Database CreateDefault() => Create(_defaultName);

        /// <summary>
        /// Creates a <see cref="Database"/> instance using the specified name.
        /// </summary>
        /// <param name="name">The name of the database configuration to use.</param>
        /// <returns>A database instance configured with the specified name.</returns>
        public Database Create(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            var registration = _databaseRegistrations[name];
            if (registration.DatabaseInstance is not null)
                return registration.DatabaseInstance;
            var dbProviderFactory = DbProviderFactories.GetFactory(registration.ProviderInvariantName!);
            var database = new Database(registration.ConnectionString!, dbProviderFactory);
            registration.DatabaseInstance = database;
            return database;
        }

        /// <summary>
        /// Creates a builder for configuring a <see cref="DatabaseProviderFactory"/> instance.
        /// </summary>
        /// <returns>A new <see cref="DatabaseProviderFactoryBuilder"/> instance.</returns>
        public static DatabaseProviderFactoryBuilder Builder() => new();
    }
}
