using System.Data.Common;

namespace DAABLike
{
    internal sealed class DatabaseRegistration
    {
        internal string? ConnectionString { get; }
        internal string? ProviderInvariantName { get; }
        internal Database? DatabaseInstance { get; set; }

        public DatabaseRegistration(string connectionString, string providerInvariantName)
        {
            ConnectionString = connectionString;
            ProviderInvariantName = providerInvariantName;
        }

        public DatabaseRegistration(string connectionString, DbProviderFactory dbProviderFactory)
        {
            DatabaseInstance = new Database(connectionString, dbProviderFactory);
        }
    }
}
