namespace SimpleAuthorization.Storage
{
    internal class StorageBag : IStorageBag
    {
        public StorageBag(string targetId, string key, string value)
        {
            TargetId = targetId;
            Key = key;
            Value = value;
        }

        #region Implementation of IStorageBag

        public string TargetId { get; }
        public string Key { get; }
        public string Value { get; }

        #endregion

    }
}