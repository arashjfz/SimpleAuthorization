using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageSecurityItem: IStorageEntity
    {
        string Id { get; }
    }
}