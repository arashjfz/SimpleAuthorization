namespace SimpleAuthorization.Storage
{
    class StorageAccessAuthorization : IStorageAccessAuthorization
    {
        public StorageAccessAuthorization(string id, string securityIdentityId, string securityItemId, byte[] lifeTimeConfig, AccessType accessType)
        {
            Id = id;
            SecurityIdentityId = securityIdentityId;
            SecurityItemId = securityItemId;
            LifeTime = lifeTimeConfig;
            AccessType = accessType;
        }

        #region Implementation of IStorageAccessAuthorization

        public string Id { get; }
        public string SecurityIdentityId { get; }
        public string SecurityItemId { get; }
        public byte[] LifeTime { get; }
        public AccessType AccessType { get; }

        #endregion
    }
}