using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal class SecurityEngine : ISecurityEngine, ISecurityIdentityAuthorizerFactory, ISecurityItemAuthorizationsResolverFactory
    {
        private readonly SecurityStore _store;
        private readonly Dictionary<ISecurityIdentity, ISecurityIdentityAuthorizer> _securityIdentitiesCache = new Dictionary<ISecurityIdentity, ISecurityIdentityAuthorizer>();
        private readonly Dictionary<ISecurityItem, ISecurityItemAuthorizationsResolver> _securityItemsCache = new Dictionary<ISecurityItem, ISecurityItemAuthorizationsResolver>();
        public SecurityEngine()
        {
            _store = new SecurityStore(this);
        }
        #region Implementation of ISecurityEngine

        public ISecurityStore Store => _store;

        public ICheckAccessResult CheckAccess(ISecurityIdentity securityIdentity, ISecurityItem securityItem)
        {
            ISecurityIdentityAuthorizer securityIdentityAuthorizer = CreateCache(securityIdentity);
            return securityIdentityAuthorizer.CheckAccess(securityItem);
        }

        public IConditionResult GetConditions(ISecurityIdentity securityIdentity, ISecurityItem securityItem)
        {
            ISecurityIdentityAuthorizer securityIdentityAuthorizer = CreateCache(securityIdentity);
            return securityIdentityAuthorizer.GetConditions(securityItem);
        }

        #endregion

        #region Implementation of ISecurityIdentityAuthorizerFactory

        public ISecurityIdentityAuthorizer CreateCache(ISecurityIdentity securityIdentity)
        {
            lock (this)
            {
                if (!_securityIdentitiesCache.TryGetValue(securityIdentity, out ISecurityIdentityAuthorizer result))
                {
                    result = new SecurityIdentityAuthorizer(securityIdentity, this, this);
                    _securityIdentitiesCache[securityIdentity] = result;
                }
                return result;
            }
        }

        #endregion

        #region Implementation of ISecurityItemAuthorizationsResolverFactory

        public ISecurityItemAuthorizationsResolver CreateResolver(ISecurityItem securityItem)
        {
            lock (this)
            {
                if (!_securityItemsCache.TryGetValue(securityItem, out ISecurityItemAuthorizationsResolver result))
                {
                    result = new SecurityItemAuthorizationsResolver(_store, securityItem);
                    _securityItemsCache[securityItem] = result;
                }
                return result;
            }
        }

        #endregion
    }
}