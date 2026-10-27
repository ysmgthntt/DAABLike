namespace DAABLike
{
    public static class DatabaseFactory
    {
        private static IDatabaseProviderFactory? _factory;

        public static Database CreateDatabase()
        {
            _factory ??= new DatabaseProviderFactory();
            return _factory.CreateDefault();
        }

        public static Database CreateDatabase(string name)
        {
            _factory ??= new DatabaseProviderFactory();
            return _factory.Create(name);
        }

        public static void SetDatabaseProviderFactory(DatabaseProviderFactory factory, bool throwIfSet = true)
        {
            ArgumentNullException.ThrowIfNull(factory);

            if (throwIfSet && _factory is not null)
                throw new InvalidOperationException("Already set.");

            _factory = factory;
        }

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

        public static void ClearDatabaseProviderFactory()
        {
            _factory = null;
        }
    }
}
