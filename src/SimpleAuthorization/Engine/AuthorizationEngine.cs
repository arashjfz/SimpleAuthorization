using System;
using System.Collections.Generic;
using System.Linq;
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
    internal class SidAuthorizationCache
    {
        private readonly ISecurityIdentity _securityIdentity;
        private readonly ISecurityIdentityAuthorizationResolver _securityIdentityAuthorizationResolver;
        private bool _initialized;
        private readonly Dictionary<IAuthorizationLifeCycle, bool> _lifeCycles = new Dictionary<IAuthorizationLifeCycle, bool>();
        private readonly Dictionary<ISecurityItem, List<ICondition>> _allowedSecurityItems = new Dictionary<ISecurityItem, List<ICondition>>();
        public SidAuthorizationCache(ISecurityIdentity securityIdentity, ISecurityIdentityAuthorizationResolver securityIdentityAuthorizationResolver)
        {
            _securityIdentity = securityIdentity;
            _securityIdentityAuthorizationResolver = securityIdentityAuthorizationResolver;
        }

        public bool CheckAccess(ISecurityItem securityItem)
        {
            TryInitialize();
            return _allowedSecurityItems.ContainsKey(securityItem);
        }
        public IEnumerable<ICondition> GetConditions(ISecurityItem securityItem)
        {
            TryInitialize();
            return InternalGetConditions(securityItem);
        }

        private IEnumerable<ICondition> InternalGetConditions(ISecurityItem securityItem)
        {
            if (_allowedSecurityItems.TryGetValue(securityItem, out List<ICondition> currentSecurityItemConditions) && currentSecurityItemConditions != null)
                foreach (ICondition currentSecurityItemCondition in currentSecurityItemConditions)
                    yield return currentSecurityItemCondition;
            foreach (ISecurityItem parent in securityItem.GetAllParents())
                foreach (ICondition parentCondition in InternalGetConditions(parent))
                    yield return parentCondition;
        }
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
            HashSet<ISecurityItem> deniedItems = new HashSet<ISecurityItem>();
            HashSet<ISecurityItem> acceptedItems = new HashSet<ISecurityItem>();
            foreach (IAuthorization authorization in _securityIdentityAuthorizationResolver.Resolve(_securityIdentity))
            {
                if (authorization.Type == AuthorizationType.Nutral)
                    continue;
                bool lifeCycleIsValidNow = false;
                if (authorization.LifeCycle != null)
                    _lifeCycles.Add(authorization.LifeCycle, lifeCycleIsValidNow = authorization.LifeCycle.IsValidNow());
                if (authorization.LifeCycle != null && !lifeCycleIsValidNow)
                    continue;

                if (authorization.Type == AuthorizationType.Deny)
                    deniedItems.Add(authorization.SecurityItem);
                else
                    acceptedItems.Add(authorization.SecurityItem);
            }

            foreach (ISecurityItem securityItem in acceptedItems)
                if (!deniedItems.Contains(securityItem) &&
                    !securityItem.GetAllParents().Any(p => deniedItems.Contains(p)))
                {
                    _allowedSecurityItems.Add(securityItem, null);
                    foreach (ISecurityItem child in securityItem.GetAllChildren())
                        _allowedSecurityItems.Add(child, null);
                }

            foreach (IAuthorization authorization in _securityIdentityAuthorizationResolver.Resolve(_securityIdentity))
            {
                if (authorization.Conditions == null)
                    continue;
                if (!_allowedSecurityItems.TryGetValue(authorization.SecurityItem, out List<ICondition> conditions))
                    continue;
                if (conditions == null)
                    _allowedSecurityItems[authorization.SecurityItem] = conditions = new List<ICondition>();
                conditions.AddRange(authorization.Conditions);
            }
        }

        public bool LifeCyclesIsValid()
        {
            foreach (var pair in _lifeCycles)
                if (pair.Key.IsValidNow() != pair.Value)
                    return false;
            return true;
        }
    }

    internal interface ISecurityIdentityAuthorizationResolver
    {
        IEnumerable<IAuthorization> Resolve(ISecurityIdentity securityIdentity);
    }
}