namespace SimpleAuthorization.Storage
{
    class StorageSecurityIdentity : IStorageSecurityIdentity
    {
        public StorageSecurityIdentity(string id)
        {
            Id = id;
        }

        #region Implementation of IStorageSecurityIdentity

        public string Id { get; }

        #endregion

    }
}