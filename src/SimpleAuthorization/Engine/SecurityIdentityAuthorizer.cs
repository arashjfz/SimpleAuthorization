using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthorization.Engine
{
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
            if (allowedParentCheckAccessResults.Count>0)
                return new CheckAccessResult(AccessType.Allow, allowedParentCheckAccessResults.SelectMany(r => r.AffectedAuthorizations));
            return new CheckAccessResult(AccessType.Neutral,Enumerable.Empty<IAccessAuthorization>());
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
            foreach (var pair in mergeFromConditionResult.Conditions) 
                ((Dictionary<IConditionalAuthorization, IEnumerable<IAccessCondition>>)conditionResult.Conditions).Add(pair.Key,pair.Value);
        }
        private IConditionResult OnlySelfGetConditions(ISecurityItem securityItem)
        {
            DateTime now = DateTime.Now;
            ISecurityItemAuthorizationsResolver securityItemAuthorizationsResolver = _securityItemAuthorizationsResolverFactory.CreateResolver(securityItem);
            Dictionary < IConditionalAuthorization, IEnumerable < IAccessCondition >> result = new Dictionary<IConditionalAuthorization, IEnumerable<IAccessCondition>>();
            foreach (IConditionalAuthorization conditionalAuthorization in securityItemAuthorizationsResolver.GetAuthorizations()
                .Where(a => a.SecurityIdentity.Equals(_securityIdentity)).OfType<IConditionalAuthorization>())
            {
                if (conditionalAuthorization.LifeTime != null && !conditionalAuthorization.LifeTime.IsActive(now))
                    continue;
                result.Add(conditionalAuthorization,conditionalAuthorization.Conditions);
            }
            return new ConditionResult(result);
        }
        private ICheckAccessResult OnlyCheckAccessSelf(ISecurityItem securityItem)
        {
            DateTime now = DateTime.Now;
            ISecurityItemAuthorizationsResolver securityItemAuthorizationsResolver = _securityItemAuthorizationsResolverFactory.CreateResolver(securityItem);
            List<IAccessAuthorization> affectedAuthorizations = new List<IAccessAuthorization>();
            bool deniedFound = false;
            bool allowedFound = false;
            foreach (IAccessAuthorization accessAuthorization in securityItemAuthorizationsResolver.GetAuthorizations()
                .Where(a => a.SecurityIdentity.Equals(_securityIdentity)).OfType<IAccessAuthorization>())
            {
                if(accessAuthorization.LifeTime != null && !accessAuthorization.LifeTime.IsActive(now))
                    continue;
                switch (accessAuthorization.AccessType)
                {
                    case AccessType.Allow:
                        if(!deniedFound)
                            affectedAuthorizations.Add(accessAuthorization);
                        allowedFound = true;
                        break;
                    case AccessType.Deny:
                        if(allowedFound && !deniedFound)
                            affectedAuthorizations.Clear();
                        affectedAuthorizations.Add(accessAuthorization);
                        deniedFound = true;
                        break;
                    case AccessType.Neutral:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            if(deniedFound)
                return new CheckAccessResult(AccessType.Deny,affectedAuthorizations);
            if(allowedFound)
                return new CheckAccessResult(AccessType.Allow, affectedAuthorizations);
            return new CheckAccessResult(AccessType.Neutral,Enumerable.Empty<IAccessAuthorization>());
        }
        #endregion
    }
}