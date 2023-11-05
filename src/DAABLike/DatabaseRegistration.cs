using System.Data.Common;

namespace DAABLike
{
    public sealed class DatabaseRegistration
    {
        internal string? ConnectionString { get; }
        internal string? ProviderInvariantName { get; }
        internal Database? DatabaseInstance { get; set; }

        public DatabaseRegistration(string connectionString, string providerInvariantName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrEmpty(providerInvariantName))
                throw new ArgumentNullException(nameof(providerInvariantName));

            ConnectionString = connectionString;
            ProviderInvariantName = providerInvariantName;
        }

        public DatabaseRegistration(string connectionString, DbProviderFactory dbProviderFactory)
        {
            DatabaseInstance = new Database(connectionString, dbProviderFactory);
        }
    }
}
