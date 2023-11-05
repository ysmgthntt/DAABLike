using System.Data.Common;

namespace DAABLike
{
    public sealed class DatabaseProviderFactory
    {
        private readonly IDictionary<string, DatabaseRegistration> _databaseRegistrations;

        public DatabaseProviderFactory(IDictionary<string, DatabaseRegistration> databaseRegistrations)
        {
            _databaseRegistrations = databaseRegistrations ?? throw new ArgumentNullException(nameof(databaseRegistrations));
        }

        public Database CreateDefault() => Create("");

        public Database Create(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var registration = _databaseRegistrations[name];
            if (registration.DatabaseInstance is not null)
                return registration.DatabaseInstance;
            var dbProviderFactory = DbProviderFactories.GetFactory(registration.ProviderInvariantName);
            var database = new Database(registration.ConnectionString, dbProviderFactory);
            registration.DatabaseInstance = database;
            return database;
        }
    }
}
