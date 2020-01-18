using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal interface ISecurityIdentityAuthorizationResolver
    {
        IEnumerable<IAuthorization> Resolve(ISecurityIdentity securityIdentity);
    }
}