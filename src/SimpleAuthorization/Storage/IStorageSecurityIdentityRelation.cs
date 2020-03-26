using System;

namespace SimpleAuthorization.Storage
{
    public interface IStorageSecurityIdentityRelation : IStorageEntity
    {
        string SecurityIdentityId { get; }
        string ParentId { get; }
    }
}