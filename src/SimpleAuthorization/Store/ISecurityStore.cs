using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public interface ISecurityStore
    {
        IEnumerable<ISecurityItem> GetSecurityItems();
        IEnumerable<IAuthorisableItem> GetAuthorisableItems();
        IEnumerable<ISecurityIdentity> GetSecurityIdentities();
        IEnumerable<IAuthorization> GetAuthorizations();
        event EventHandler Changed;
    }
}