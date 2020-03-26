using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageAccessAuthorization : IStorageEntity
    {
        string Id { get; }
        string SecurityIdentityId { get; }
        string SecurityItemId { get; }
        byte[] LifeTime { get; }
        AccessType AccessType { get; }
    }
}