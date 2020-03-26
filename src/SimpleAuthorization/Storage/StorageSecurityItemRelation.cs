namespace SimpleAuthorization.Storage
{
    internal class StorageSecurityItemRelation : IStorageSecurityItemRelation
    {
        public StorageSecurityItemRelation(string parentId, string securityItemId)
        {
            ParentId = parentId;
            SecurityItemId = securityItemId;
        }

        #region Implementation of IStorageSecurityItemRelation

        public string ParentId { get; }
        public string SecurityItemId { get; }

        #endregion
    }
}