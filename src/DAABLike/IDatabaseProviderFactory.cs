namespace DAABLike
{
    internal interface IDatabaseProviderFactory
    {
        public Database CreateDefault();
        public Database Create(string name);
    }
}
