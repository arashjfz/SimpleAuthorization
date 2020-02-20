namespace SimpleAuthorization.Engine
{
    internal interface ISecurityItemAuthorizationsResolverFactory
    {
        ISecurityItemAuthorizationsResolver CreateResolver(ISecurityItem securityItem);
    }
}