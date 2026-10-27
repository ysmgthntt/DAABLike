using System.Data.Common;

namespace DAABLike
{
    public sealed class DatabaseProviderFactory : IDatabaseProviderFactory
    {
        private readonly string _defaultName;
        private readonly Dictionary<string, DatabaseRegistration> _databaseRegistrations;

        public DatabaseProviderFactory() : this("DefaultConnection") { }

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

        public Database CreateDefault() => Create(_defaultName);

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

        public static DatabaseProviderFactoryBuilder Builder() => new();
    }
}
