using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthorization.Engine
{
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
                {
                    deniedItems.Add(authorization.SecurityItem);
                    foreach (ISecurityItem securityItem in authorization.SecurityItem.GetAllChildren())
                        deniedItems.Add(securityItem);
                }
                else
                    acceptedItems.Add(authorization.SecurityItem);
            }

            foreach (ISecurityItem securityItem in acceptedItems)
                if (!deniedItems.Contains(securityItem) &&
                    !securityItem.GetAllParents().Any(p => deniedItems.Contains(p)))
                {
                    _allowedSecurityItems.Add(securityItem, null);
                    foreach (ISecurityItem child in securityItem.GetAllChildren())
                        if(!deniedItems.Contains(child))
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
}