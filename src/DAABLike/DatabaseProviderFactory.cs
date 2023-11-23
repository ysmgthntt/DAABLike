using System.Data.Common;

namespace DAABLike
{
    public sealed class DatabaseProviderFactory
    {
        private readonly Dictionary<string, DatabaseRegistration> _databaseRegistrations;

        internal DatabaseProviderFactory(Dictionary<string, DatabaseRegistration> databaseRegistrations)
        {
            _databaseRegistrations = databaseRegistrations;
        }

        public Database CreateDefault() => Create("");

        public Database Create(string name)
        {
            ANE.ThrowIfNull(name);

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
