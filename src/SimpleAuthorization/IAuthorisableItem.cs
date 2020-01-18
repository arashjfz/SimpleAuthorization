using System;
using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface IAuthorisableItem
    {
        Guid Key { get; }
        IEnumerable<ISecurityIdentity> GetSecurityIdentities();
        bool ContainSecurityIdentity(ISecurityIdentity securityIdentity);
    }
}