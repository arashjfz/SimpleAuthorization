using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuthorization.Storage
{
    public interface ISecurityStorage
    {
        IEnumerable<ISecurityHierarchy> SecurityHierarchies { get; }
        void RemoveSecurityHierarchy(IEnumerable<ISecurityHierarchy> hierarchies);
        void AddSecurityHierarchy(IEnumerable<ISecurityHierarchy> hierarchies);
        IEnumerable<Guid> SecurityItemKeys { get; }
        void RemoveSecurityItemKeys(IEnumerable<Guid> securityItemKeys);
        void AddSecurityItemKeys(IEnumerable<Guid> securityItemKeys);
        IEnumerable<IStorageAuthorization> Authorizations { get; }
        void UpdateAuthorizations(IEnumerable<IStorageAuthorization> authorizations);
        void RemoveAuthorizations(IEnumerable<IStorageAuthorization> authorizations);
        void AddAuthorizations(IEnumerable<IStorageAuthorization> authorizations);
    }

    public interface ISecurityHierarchy
    {
        Guid SecurityItemKey { get; }
        Guid SecurityItemParentKey { get; }
    }

    public interface IStorageAuthorization
    {
        Guid Key { get; }
        Guid AuthorisableItemKey { get; }
        Guid SecurityItemKey { get; }
        byte[] LifeCycle { get; }
        Guid? DelegatedByKey { get; }
        AuthorizationType Type { get; }
        IEnumerable<byte[]> Conditions { get; }

    }
}
