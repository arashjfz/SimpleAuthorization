using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public interface ISecurityIdentityProvider
    {
        IEnumerable<ISecurityIdentity> Provide();
        event EventHandler Changed;
    }
}