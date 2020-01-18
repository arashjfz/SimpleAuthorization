using System;

namespace SimpleAuthorization.Storage
{
    public interface IAuthorizableItemHierarchy
    {
        Guid SecurityIdentityKey { get; }
        Guid AuthorizableItemKey { get; }
    }
}