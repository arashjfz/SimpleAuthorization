using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SimpleAuthorization.Activator
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

    internal interface ISecurityIdentityAuthorizer
    {
        ICheckAccessResult CheckAccess(ISecurityItem securityItem);
        IConditionResult GetConditions(ISecurityItem securityItem);
    }
    internal class SecurityItemAuthorizationsResolver : ISecurityItemAuthorizationsResolver
    {
        private readonly SecurityStore _store;
        private readonly ISecurityItem _securityItem;

        public SecurityItemAuthorizationsResolver(SecurityStore store, ISecurityItem securityItem)
        {
            _store = store;
            _securityItem = securityItem;
        }
        #region Implementation of ISecurityItemAuthorizationsResolver

        public IEnumerable<IAuthorization> GetAuthorizations()
        {
            foreach (ISecurityItem item in Enumerable.Repeat(_securityItem, 1).Concat(_securityItem.GetAllAncestors()))
                foreach (IAuthorization authorization in _store.GetSecurityItemAuthorizations(item))
                    yield return authorization;
        }

        #endregion
    }

    internal class SecurityIdentityAuthorizer : ISecurityIdentityAuthorizer
    {
        private readonly ISecurityIdentity _securityIdentity;
        private readonly ISecurityIdentityAuthorizerFactory _securityIdentityAuthorizerFactory;
        private readonly ISecurityItemAuthorizationsResolverFactory _securityItemAuthorizationsResolverFactory;

        public SecurityIdentityAuthorizer(ISecurityIdentity securityIdentity, ISecurityIdentityAuthorizerFactory securityIdentityAuthorizerFactory, ISecurityItemAuthorizationsResolverFactory securityItemAuthorizationsResolverFactory)
        {
            _securityIdentity = securityIdentity;
            _securityIdentityAuthorizerFactory = securityIdentityAuthorizerFactory;
            _securityItemAuthorizationsResolverFactory = securityItemAuthorizationsResolverFactory;
        }

        #region Implementation of ISecurityIdentityAuthorizer

        public ICheckAccessResult CheckAccess(ISecurityItem securityItem)
        {
            ICheckAccessResult checkAccessResult = OnlyCheckAccessSelf(securityItem);
            if (checkAccessResult.AccessType == AccessType.Deny)
                return checkAccessResult;
            IList<ICheckAccessResult> allowedParentCheckAccessResults = new List<ICheckAccessResult>();
            foreach (ISecurityIdentity parent in _securityIdentity.Parents)
            {
                ISecurityIdentityAuthorizer securityIdentityAuthorizer = _securityIdentityAuthorizerFactory.CreateCache(parent);
                ICheckAccessResult parentCheckAccessResult = securityIdentityAuthorizer.CheckAccess(securityItem);
                if (parentCheckAccessResult.AccessType == AccessType.Deny)
                    return parentCheckAccessResult;
                if (parentCheckAccessResult.AccessType == AccessType.Allow)
                    allowedParentCheckAccessResults.Add(parentCheckAccessResult);
            }

            if (checkAccessResult.AccessType == AccessType.Allow)
                return checkAccessResult;
            ICheckAccessResult allowedParentAccessResult = allowedParentCheckAccessResults.FirstOrDefault();
            if (allowedParentAccessResult != null)
                return allowedParentAccessResult;
            return new CheckAccessResult(AccessType.Neutral);
        }

        public IConditionResult GetConditions(ISecurityItem securityItem)
        {
            ConditionResult conditionResult = (ConditionResult) OnlySelfGetConditions(securityItem);
            foreach (ISecurityIdentity parent in _securityIdentity.Parents)
            {
                ISecurityIdentityAuthorizer securityIdentityAuthorizer = _securityIdentityAuthorizerFactory.CreateCache(parent);
                IConditionResult parentConditions = securityIdentityAuthorizer.GetConditions(securityItem);
                MergeConditionResult(conditionResult,parentConditions);
            }
            return conditionResult;
        }

        private void MergeConditionResult(ConditionResult conditionResult, IConditionResult mergeFromConditionResult)
        {
            foreach (var pair in mergeFromConditionResult.DelegatedConditions)
            {
                if (!conditionResult.DelegatedConditions.TryGetValue(pair.Key, out IEnumerable<IAccessCondition> result))
                {
                    result = new List<IAccessCondition>();
                    conditionResult.DelegatedConditions[pair.Key] = result;
                }

                foreach (IAccessCondition accessCondition in pair.Value)
                    ((List<IAccessCondition>) result).Add(accessCondition);
            }

            foreach (IAccessCondition accessCondition in mergeFromConditionResult.SelfConditions) 
                conditionResult.SelfConditions.Add(accessCondition);
        }
        private IConditionResult OnlySelfGetConditions(ISecurityItem securityItem)
        {
            DateTime now = DateTime.Now;
            ISecurityItemAuthorizationsResolver securityItemAuthorizationsResolver = _securityItemAuthorizationsResolverFactory.CreateResolver(securityItem);
            ConditionResult result = new ConditionResult();
            foreach (IConditionalAuthorization conditionalAuthorization in securityItemAuthorizationsResolver.GetAuthorizations()
                .Where(a => a.SecurityIdentity.Equals(_securityIdentity)).OfType<IConditionalAuthorization>())
            {
                if (conditionalAuthorization.LifeTime != null && !conditionalAuthorization.LifeTime.IsActive(now))
                    continue;
                if(conditionalAuthorization.DelegatedBy == null)
                    foreach (IAccessCondition conditionalAuthorizationCondition in conditionalAuthorization.Conditions)
                        result.SelfConditions.Add(conditionalAuthorizationCondition);
                else
                {
                    if (!result.DelegatedConditions.TryGetValue(conditionalAuthorization.DelegatedBy,
                        out IEnumerable<IAccessCondition> accessConditions))
                    {
                        accessConditions = new List<IAccessCondition>();
                        result.DelegatedConditions[conditionalAuthorization.DelegatedBy] = accessConditions;
                    }
                    foreach (IAccessCondition conditionalAuthorizationCondition in conditionalAuthorization.Conditions)
                        ((List<IAccessCondition>)accessConditions).Add(conditionalAuthorizationCondition);
                }
            }
            return result;
        }
        private ICheckAccessResult OnlyCheckAccessSelf(ISecurityItem securityItem)
        {
            DateTime now = DateTime.Now;
            ISecurityItemAuthorizationsResolver securityItemAuthorizationsResolver = _securityItemAuthorizationsResolverFactory.CreateResolver(securityItem);
            IAuthorization allowdAuthorization = null;
            foreach (IAccessAuthorization accessAuthorization in securityItemAuthorizationsResolver.GetAuthorizations()
                .Where(a => a.SecurityIdentity.Equals(_securityIdentity)).OfType<IAccessAuthorization>())
            {
                if(accessAuthorization.LifeTime != null && !accessAuthorization.LifeTime.IsActive(now))
                    continue;
                if(accessAuthorization.AccessType == AccessType.Deny)
                    return new CheckAccessResult(AccessType.Deny){DelegatedFrom = { accessAuthorization.DelegatedBy}};
                if (accessAuthorization.AccessType == AccessType.Allow)
                {
                    if(allowdAuthorization == null || allowdAuthorization.DelegatedBy != null)
                        allowdAuthorization = accessAuthorization;

                }
            }
            if(allowdAuthorization != null)
                return new CheckAccessResult(AccessType.Allow) { DelegatedFrom = { allowdAuthorization.DelegatedBy } };
            return new CheckAccessResult(AccessType.Neutral);
        }
        #endregion
    }
    internal class ConditionResult: IConditionResult
    {
        public ConditionResult()
        {
            DelegatedConditions =new Dictionary<ISecurityIdentity, IEnumerable<IAccessCondition>>();
            SelfConditions = new List<IAccessCondition>();
        }
        #region Implementation of IConditionResult

        public Dictionary<ISecurityIdentity,IEnumerable<IAccessCondition>> DelegatedConditions { get; }
        public IList<IAccessCondition> SelfConditions { get; }
        IReadOnlyDictionary<ISecurityIdentity, IEnumerable<IAccessCondition>> IConditionResult.DelegatedConditions =>
            new ReadOnlyDictionary<ISecurityIdentity, IEnumerable<IAccessCondition>>(DelegatedConditions);
        IEnumerable<IAccessCondition> IConditionResult.SelfConditions => SelfConditions;

        #endregion
    }

    internal interface ISecurityItemAuthorizationsResolver
    {
        IEnumerable<IAuthorization> GetAuthorizations();
    }
    internal class CheckAccessResult : ICheckAccessResult
    {
        public CheckAccessResult(AccessType accessType)
        {
            AccessType = accessType;
            DelegatedFrom = new List<ISecurityIdentity>();
        }

        #region Implementation of ICheckAccessResult

        public AccessType AccessType { get; }
        public IList<ISecurityIdentity> DelegatedFrom { get; }
        IEnumerable<ISecurityIdentity> ICheckAccessResult.DelegatedFrom => DelegatedFrom;

        #endregion
    }

    internal interface ISecurityIdentityAuthorizerFactory
    {
        ISecurityIdentityAuthorizer CreateCache(ISecurityIdentity securityIdentity);
    }

    internal interface ISecurityItemAuthorizationsResolverFactory
    {
        ISecurityItemAuthorizationsResolver CreateResolver(ISecurityItem securityItem);
    }
}