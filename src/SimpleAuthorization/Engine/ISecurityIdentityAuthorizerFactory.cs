namespace SimpleAuthorization.Engine
{
    internal interface ISecurityIdentityAuthorizerFactory
    {
        ISecurityIdentityAuthorizer CreateCache(ISecurityIdentity securityIdentity);
    }
}