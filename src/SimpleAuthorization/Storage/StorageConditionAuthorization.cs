namespace SimpleAuthorization.Storage
{
    class StorageConditionAuthorization : IStorageConditionAuthorization
    {
        public StorageConditionAuthorization(string id, string securityIdentityId, string securityItemId, byte[] lifeTime, byte[] conditions)
        {
            Id = id;
            SecurityIdentityId = securityIdentityId;
            SecurityItemId = securityItemId;
            LifeTime = lifeTime;
            Conditions = conditions;
        }

        #region Implementation of IStorageConditionAuthorization

        public string Id { get; }
        public string SecurityIdentityId { get; }
        public string SecurityItemId { get; }
        public byte[] LifeTime { get; }
        public byte[] Conditions { get; }

        #endregion
    }
}