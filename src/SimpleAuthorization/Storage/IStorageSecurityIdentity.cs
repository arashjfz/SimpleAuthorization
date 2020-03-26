using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageSecurityIdentity : IStorageEntity
    {
        string Id { get; }
    }
}