namespace SimpleAuthorization.Storage
{
    internal class StorageSecurityIdentityRelation : IStorageSecurityIdentityRelation
    {
        public StorageSecurityIdentityRelation(string parentId, string securityIdentityId)
        {
            SecurityIdentityId = securityIdentityId;
            ParentId = parentId;
        }

        #region Implementation of IStorageSecurityIdentityRelation

        public string SecurityIdentityId { get; }
        public string ParentId { get; }

        #endregion

    }
}