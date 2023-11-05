namespace DAABLike
{
    public static class DatabaseFactory
    {
        private static Func<Database>? _createDefaultDatabase;
        private static Func<string, Database>? _createNamedDatabase;

        public static Database CreateDatabase()
            => _createDefaultDatabase!();

        public static Database CreateDatabase(string name)
            => _createNamedDatabase!(name);

        public static void SetDatabaseProviderFactory(DatabaseProviderFactory factory, bool throwIfSet = true)
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            SetDatabases(factory.CreateDefault, factory.Create, throwIfSet);
        }

        public static void SetDatabases(Func<Database> createDefaultDatabase, Func<string, Database> createNamedDatabase, bool throwIfSet = true)
        {
            if (createDefaultDatabase is null)
                throw new ArgumentNullException(nameof(createDefaultDatabase));
            if (createNamedDatabase is null)
                throw new ArgumentNullException(nameof(createNamedDatabase));

            if (throwIfSet && _createDefaultDatabase is not null && _createNamedDatabase is not null)
                throw new InvalidOperationException("Already set.");

            _createDefaultDatabase = createDefaultDatabase;
            _createNamedDatabase = createNamedDatabase;
        }

        public static void ClearDatabaseProviderFactory()
        {
            _createDefaultDatabase = null;
            _createNamedDatabase = null;
        }
    }
}
