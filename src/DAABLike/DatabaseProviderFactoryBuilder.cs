using System.Data.Common;

namespace DAABLike
{
    public sealed class DatabaseProviderFactoryBuilder
    {
        private readonly Dictionary<string, DatabaseRegistration> _databaseRegistrations = new();

        public DatabaseProviderFactoryBuilder RegisterDefault(string connectionString, string providerInvariantName)
            => Register("", connectionString, providerInvariantName);

        public DatabaseProviderFactoryBuilder RegisterDefault(string connectionString, DbProviderFactory dbProviderFactory)
            => Register("", connectionString, dbProviderFactory);

        public DatabaseProviderFactoryBuilder Register(string name, string connectionString, string providerInvariantName)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(providerInvariantName))
                throw new ArgumentNullException(nameof(providerInvariantName));

            _databaseRegistrations.Add(name, new DatabaseRegistration(connectionString, providerInvariantName));
            return this;
        }

        public DatabaseProviderFactoryBuilder Register(string name, string connectionString, DbProviderFactory dbProviderFactory)
        {
            _databaseRegistrations.Add(name, new DatabaseRegistration(connectionString, dbProviderFactory));
            return this;
        }

        public DatabaseProviderFactory Build()
            => new DatabaseProviderFactory(_databaseRegistrations);
    }
}
