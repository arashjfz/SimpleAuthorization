using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageSecurityItemRelation : IStorageEntity
    {
        string SecurityItemId { get; }
        string ParentId { get; }

    }
}