using System.Collections.Generic;
using SimpleAuthorization.Engine;

namespace SimpleAuthorization
{
    public interface ISecurityEngine
    {
        ISecurityStore Store { get; }
        ICheckAccessResult CheckAccess(ISecurityIdentity securityIdentity, ISecurityItem securityItem);
        IConditionResult GetConditions(ISecurityIdentity securityIdentity, ISecurityItem securityItem);

    }
}