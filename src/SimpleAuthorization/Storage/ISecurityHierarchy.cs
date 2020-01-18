using System;

namespace SimpleAuthorization.Storage
{
    public interface ISecurityHierarchy
    {
        Guid SecurityItemKey { get; }
        Guid SecurityItemParentKey { get; }
    }
}