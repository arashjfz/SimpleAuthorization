using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    public interface IAuthorizationEngine
    {
        bool CheckAccess(ISecurityIdentity securityIdentity, ISecurityItem securityItem);
        IEnumerable<ICondition> GetConditions(ISecurityIdentity securityIdentity, ISecurityItem securityItem);
    }
}