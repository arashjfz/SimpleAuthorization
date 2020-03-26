using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageConditionAuthorization : IStorageEntity
    {
        string Id { get; }
        string SecurityIdentityId { get; }
        string SecurityItemId { get; }
        byte[] LifeTime { get; }
        byte[] Conditions { get;}
    }
}