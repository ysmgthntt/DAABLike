namespace DAABLike
{
    /// <summary>
    /// Provides static methods for creating <see cref="Database"/> instances.
    /// </summary>
    /// <remarks>
    /// A <see cref="DatabaseProviderFactory"/> can be registered using
    /// <see cref="SetDatabaseProviderFactory"/>.
    /// If no factory has been registered, a <see cref="DatabaseProviderFactory"/> is created internally.
    /// On .NET Framework, this allows database instances to be created directly from the connection strings
    /// configured in the application's configuration file (<c>app.config</c>).
    /// </remarks>
    public static class DatabaseFactory
    {
        private static IDatabaseProviderFactory? _factory;

        /// <summary>
        /// Creates a <see cref="Database"/> instance using the default database configuration.
        /// </summary>
        /// <returns>A database instance.</returns>
        public static Database CreateDatabase()
        {
            _factory ??= new DatabaseProviderFactory();
            return _factory.CreateDefault();
        }

        /// <summary>
        /// Creates a <see cref="Database"/> instance using the specified database configuration.
        /// </summary>
        /// <param name="name">The name of the database configuration to use.</param>
        /// <returns>A database instance.</returns>
        public static Database CreateDatabase(string name)
        {
            _factory ??= new DatabaseProviderFactory();
            return _factory.Create(name);
        }

        /// <summary>
        /// Sets the database provider factory to use when creating database instances.
        /// </summary>
        /// <param name="factory">The database provider factory to use.</param>
        /// <param name="throwIfSet">
        /// <see langword="true"/> to throw an exception if a factory has already been set;
        /// otherwise, <see langword="false"/> to replace the existing factory.
        /// </param>
        public static void SetDatabaseProviderFactory(DatabaseProviderFactory factory, bool throwIfSet = true)
        {
            ArgumentNullException.ThrowIfNull(factory);

            if (throwIfSet && _factory is not null)
                throw new InvalidOperationException("Already set.");

            _factory = factory;
        }

        /// <summary>
        /// Sets the delegates used to create default and named database instances.
        /// </summary>
        /// <param name="createDefaultDatabase">The delegate used to create the default database instance.</param>
        /// <param name="createNamedDatabase">The delegate used to create a database instance with the specified name.</param>
        /// <param name="throwIfSet">
        /// <see langword="true"/> to throw an exception if the database creation delegates have already been set;
        /// otherwise, <see langword="false"/> to replace the existing delegates.
        /// </param>
        public static void SetDatabases(Func<Database> createDefaultDatabase, Func<string, Database> createNamedDatabase, bool throwIfSet = true)
        {
            ArgumentNullException.ThrowIfNull(createDefaultDatabase);
            ArgumentNullException.ThrowIfNull(createNamedDatabase);

            if (throwIfSet && _factory is not null)
                throw new InvalidOperationException("Already set.");

            _factory = new FuncFactory(createDefaultDatabase, createNamedDatabase);
        }

        private sealed class FuncFactory(Func<Database> createDefaultDatabase, Func<string, Database> createNamedDatabase) : IDatabaseProviderFactory
        {
            public Database CreateDefault() => createDefaultDatabase();
            public Database Create(string name) => createNamedDatabase(name);
        }

        /// <summary>
        /// Clears the configured database creation factory or delegates.
        /// </summary>
        /// <remarks>
        /// Clears any <see cref="DatabaseProviderFactory"/> or database creation delegates
        /// previously configured through <see cref="SetDatabaseProviderFactory"/> or <see cref="SetDatabases"/>.
        /// </remarks>
        public static void ClearDatabaseProviderFactory()
        {
            _factory = null;
        }
    }
}
