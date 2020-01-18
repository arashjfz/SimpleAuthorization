using SimpleAuthorization.Store;

namespace SimpleAuthorization.Engine
{
    public static class AuthorizationEngineExtensions
    {
        public static IAuthorizationEngine GetEngine(this ISecurityStore store)
        {
            return new AuthorizationEngine(store);
        }
    }
}