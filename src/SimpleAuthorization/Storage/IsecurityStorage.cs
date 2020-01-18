using System;
using System.Collections.Generic;

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


        IEnumerable<IAuthorizableItemHierarchy> AuthorizableHierarchies { get; }
        void RemoveAuthorizableHierarchy(IEnumerable<IAuthorizableItemHierarchy> hierarchies);
        void AddAuthorizableHierarchy(IEnumerable<IAuthorizableItemHierarchy> hierarchies);


        IEnumerable<Guid> AuthorizableItemKeys { get; }
        void RemoveAuthorizableItemKeys(IEnumerable<Guid> authorizableItemKeys);
        void AddAuthorizableItemKeys(IEnumerable<Guid> authorizableItemKeys);

        IEnumerable<Guid> SecurityIdentityKeys { get; }
        void RemoveSecurityIdentityKeys(IEnumerable<Guid> securityIdentityKeys);
        void AddSecurityIdentityKeys(IEnumerable<Guid> securityIdentityKeys);


        event EventHandler Changed;

    }
}
