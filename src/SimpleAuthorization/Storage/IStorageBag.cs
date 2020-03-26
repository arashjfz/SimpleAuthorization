using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageBag : IStorageEntity
    {
        string TargetId { get; }
        string Key { get; }
        string Value { get; }
    }
}