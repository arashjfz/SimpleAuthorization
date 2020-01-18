using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuthorization.Store;

namespace SimpleAuthorization.Engine
{
    internal class SecurityCache : ISecurityIdentityAuthorizationResolver
    {
        private readonly ISecurityStore _store;
        private readonly Dictionary<ISecurityIdentity, SidAuthorizationCache> _cache = new Dictionary<ISecurityIdentity, SidAuthorizationCache>();
        private readonly Dictionary<ISecurityIdentity,HashSet<IAuthorization>> _authorizations = new Dictionary<ISecurityIdentity, HashSet<IAuthorization>>();
        private bool _initialized;

        public SecurityCache(ISecurityStore store)
        {
            _store = store;
            store.Changed += StoreOnChanged;
        }

        private void StoreOnChanged(object sender, EventArgs e)
        {
            Purge();
        }

        public SidAuthorizationCache GetSidCache(ISecurityIdentity securityIdentity)
        {
            lock (this)
            {
                if (_cache.TryGetValue(securityIdentity, out SidAuthorizationCache cache) && cache.LifeCyclesIsValid())
                    return cache;
                cache = new SidAuthorizationCache(securityIdentity, this);
                _cache[securityIdentity] = cache;
                return cache;
            }
        }

        private void Purge()
        {
            lock (this)
            {
                _cache.Clear();
                _authorizations.Clear();
                _initialized = false;
            }
        }

        #region Implementation of ISecurityIdentityAuthorizationResolver

        public IEnumerable<IAuthorization> Resolve(ISecurityIdentity securityIdentity)
        {
            TryInitialize();
            if(!_authorizations.TryGetValue(securityIdentity,out HashSet<IAuthorization> authorizations))
                return Enumerable.Empty<IAuthorization>();
            return authorizations;
        }

        #endregion

        private void TryInitialize()
        {
            lock (this)
            {
                if (_initialized)
                    return;
                _initialized = true;
                Initialize();
            }
        }

        private void Initialize()
        {
            foreach (IAuthorization authorization in _store.GetAuthorizations())
            {
                foreach (ISecurityIdentity securityIdentity in authorization.AuthorisableItem.GetSecurityIdentities())
                {
                    if (!_authorizations.TryGetValue(securityIdentity, out HashSet<IAuthorization> authorizations))
                        _authorizations[securityIdentity] = authorizations = new HashSet<IAuthorization>();
                    authorizations.Add(authorization);
                }
            }
        }
    }
}