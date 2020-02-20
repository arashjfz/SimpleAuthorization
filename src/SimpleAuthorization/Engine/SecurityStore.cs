using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SimpleAuthorization.Engine
{
    internal class SecurityStore : ISecurityStore
    {
        readonly Dictionary<ISecurityIdentity, HashSet<IAuthorization>> _securityIdentityAuthorizations = new Dictionary<ISecurityIdentity, HashSet<IAuthorization>>();
        readonly Dictionary<ISecurityItem, HashSet<IAuthorization>> _securityItemAuthorizations = new Dictionary<ISecurityItem, HashSet<IAuthorization>>();
        public SecurityStore(ISecurityEngine engine)
        {
            Engine = engine;
            SecurityIdentities = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(SecurityIdentitiesChanged);
            SecurityItems = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(SecurityItemsChanged);
            Authorizations = new SecurityCollection<IAuthorization>().RegisterCollectionNotifyChanged(AuthorizationsChanged);
        }

        private void AddAuthorization(IAuthorization authorization)
        {
            if (!_securityIdentityAuthorizations.TryGetValue(authorization.SecurityIdentity,
                out HashSet<IAuthorization> identityAuthorizations))
            {
                identityAuthorizations = new HashSet<IAuthorization>();
                _securityIdentityAuthorizations[authorization.SecurityIdentity] = identityAuthorizations;
            }

            identityAuthorizations.Add(authorization);
            if (!_securityItemAuthorizations.TryGetValue(authorization.SecurityItem,
                out HashSet<IAuthorization> securityItemAuthorizations))
            {
                securityItemAuthorizations = new HashSet<IAuthorization>();
                _securityItemAuthorizations[authorization.SecurityItem] = securityItemAuthorizations;
            }

            securityItemAuthorizations.Add(authorization);
        }

        private void RemoveAuthorization(IAuthorization authorization)
        {
            if (_securityIdentityAuthorizations.TryGetValue(authorization.SecurityIdentity,
                out HashSet<IAuthorization> identityAuthorizations))
            {
                identityAuthorizations.Remove(authorization);
                if (identityAuthorizations.Count == 0)
                    _securityIdentityAuthorizations.Remove(authorization.SecurityIdentity);
            }
            if (_securityItemAuthorizations.TryGetValue(authorization.SecurityItem,
                out HashSet<IAuthorization> securityItemAuthorizations))
            {
                securityItemAuthorizations.Remove(authorization);
                if (securityItemAuthorizations.Count == 0)
                    _securityItemAuthorizations.Remove(authorization.SecurityItem);
            }


        }
        private void AuthorizationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IAuthorization authorization in e.NewItems)
                        AddAuthorization(authorization);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IAuthorization authorization in e.OldItems)
                        RemoveAuthorization(authorization);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _securityIdentityAuthorizations.Clear();
                    _securityItemAuthorizations.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SecurityItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private void SecurityIdentitiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        public ICollection<ISecurityIdentity> SecurityIdentities { get; }
        public ICollection<ISecurityItem> SecurityItems { get; }
        public ICollection<IAuthorization> Authorizations { get; }
        #region Implementation of ISecurityStore

        public ISecurityEngine Engine { get; }
        IReadOnlyCollection<ISecurityIdentity> ISecurityStore.SecurityIdentities => (IReadOnlyCollection<ISecurityIdentity>)SecurityIdentities;
        IReadOnlyCollection<ISecurityItem> ISecurityStore.SecurityItems => (IReadOnlyCollection<ISecurityItem>)SecurityItems;
        IReadOnlyCollection<IAuthorization> ISecurityStore.Authorizations => (IReadOnlyCollection<IAuthorization>)Authorizations;

        #endregion

        public IEnumerable<IAuthorization> GetSecurityIdentityAuthorizations(ISecurityIdentity securityIdentity)
        {
            if (_securityIdentityAuthorizations.TryGetValue(securityIdentity,
                out HashSet<IAuthorization> authorizations))
                return authorizations;
            return Enumerable.Empty<IAuthorization>();
        }

        public IEnumerable<IAuthorization> GetSecurityItemAuthorizations(ISecurityItem securityItem)
        {
            if (_securityItemAuthorizations.TryGetValue(securityItem, out HashSet<IAuthorization> authorizations))
                return authorizations;
            return Enumerable.Empty<IAuthorization>();
        }
    }
}