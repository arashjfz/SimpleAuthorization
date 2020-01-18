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
}
