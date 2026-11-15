using System.Data.Common;

namespace DAABLike
{
    /// <summary>
    /// Builds a <see cref="DatabaseProviderFactory"/> by registering database configurations.
    /// </summary>
    public sealed class DatabaseProviderFactoryBuilder
    {
        private readonly Dictionary<string, DatabaseRegistration> _databaseRegistrations = new();

        /// <summary>
        /// Registers the default database configuration with the specified connection string and provider invariant name.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="providerInvariantName">The invariant name of the database provider.</param>
        /// <returns>This builder instance.</returns>
        public DatabaseProviderFactoryBuilder RegisterDefault(string connectionString, string providerInvariantName)
            => Register("", connectionString, providerInvariantName);

        /// <summary>
        /// Registers the default database configuration with the specified connection string and provider factory.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="dbProviderFactory">The provider factory to use.</param>
        /// <returns>This builder instance.</returns>
        public DatabaseProviderFactoryBuilder RegisterDefault(string connectionString, DbProviderFactory dbProviderFactory)
            => Register("", connectionString, dbProviderFactory);

        /// <summary>
        /// Registers a database configuration with the specified name, connection string, and provider invariant name.
        /// </summary>
        /// <param name="name">The name of the database configuration.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="providerInvariantName">The invariant name of the database provider.</param>
        /// <returns>This builder instance.</returns>
        public DatabaseProviderFactoryBuilder Register(string name, string connectionString, string providerInvariantName)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentException.ThrowIfNullOrEmpty(providerInvariantName);

            _databaseRegistrations.Add(name, new DatabaseRegistration(connectionString, providerInvariantName));
            return this;
        }

        /// <summary>
        /// Registers a database configuration with the specified name, connection string, and provider factory.
        /// </summary>
        /// <param name="name">The name of the database configuration.</param>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="dbProviderFactory">The provider factory to use.</param>
        /// <returns>This builder instance.</returns>
        public DatabaseProviderFactoryBuilder Register(string name, string connectionString, DbProviderFactory dbProviderFactory)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentNullException.ThrowIfNull(dbProviderFactory);

            _databaseRegistrations.Add(name, new DatabaseRegistration(connectionString, dbProviderFactory));
            return this;
        }

        /// <summary>
        /// Builds a <see cref="DatabaseProviderFactory"/> using the registered database configurations.
        /// </summary>
        /// <returns>A database provider factory configured with the registered database configurations.</returns>
        public DatabaseProviderFactory Build()
            => new DatabaseProviderFactory("", _databaseRegistrations);
    }
}
