using System.Collections.Generic;
using SimpleAuthorization.Store;

namespace SimpleAuthorization.Engine
{
    internal class AuthorizationEngine : IAuthorizationEngine
    {
        readonly SecurityCache _cache;
        public AuthorizationEngine(ISecurityStore store)
        {
            _cache = new SecurityCache(store);
        }

        #region Implementation of IAuthorizationEngine

        public bool CheckAccess(ISecurityIdentity securityIdentity, ISecurityItem securityItem)
        {
            return _cache.GetSidCache(securityIdentity).CheckAccess(securityItem);
        }

        public IEnumerable<ICondition> GetConditions(ISecurityIdentity securityIdentity, ISecurityItem securityItem)
        {
            return _cache.GetSidCache(securityIdentity).GetConditions(securityItem);
        }

        #endregion
    }
}