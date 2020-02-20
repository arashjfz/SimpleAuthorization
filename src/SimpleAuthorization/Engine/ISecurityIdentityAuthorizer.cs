namespace SimpleAuthorization.Engine
{
    internal interface ISecurityIdentityAuthorizer
    {
        ICheckAccessResult CheckAccess(ISecurityItem securityItem);
        IConditionResult GetConditions(ISecurityItem securityItem);
    }
}