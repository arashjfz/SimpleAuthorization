namespace SimpleAuthorization.Storage
{
    class StorageSecurityItem : IStorageSecurityItem
    {
        public StorageSecurityItem(string id)
        {
            Id = id;
        }

        #region Implementation of IStorageSecurityItem

        public string Id { get; }

        #endregion
    }
}