using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public interface ISecurityStore
    {
        ISecurityItem GetSecurityItem(Guid key);
        IEnumerable<ISecurityItem> GetSecurityItems();
        IAuthorisableItem GetAuthorizableItem(Guid key);
        IEnumerable<IAuthorisableItem> GetAuthorisableItems();
        ISecurityIdentity GetSecurityIdentity(Guid key);
        IEnumerable<ISecurityIdentity> GetSecurityIdentities();
        IAuthorization GetAuthorization(Guid key);
        IEnumerable<IAuthorization> GetAuthorizations();
        event EventHandler Changed;
    }
}