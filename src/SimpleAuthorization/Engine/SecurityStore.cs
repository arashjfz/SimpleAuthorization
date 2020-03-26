using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Engine
{
    internal class SecurityStore : ISecurityStore
    {
        private StorageSynchronizer _storageSynchronizer;
        readonly Dictionary<ISecurityIdentity, HashSet<IAuthorization>> _securityIdentityAuthorizations = new Dictionary<ISecurityIdentity, HashSet<IAuthorization>>();
        readonly Dictionary<ISecurityItem, HashSet<IAuthorization>> _securityItemAuthorizations = new Dictionary<ISecurityItem, HashSet<IAuthorization>>();
        public SecurityStore(ISecurityEngine engine)
        {
            Engine = engine;
            SecurityIdentities = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(SecurityIdentitiesChanged);
            SecurityItems = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(SecurityItemsChanged);
            Authorizations = new SecurityCollection<IAuthorization>().RegisterCollectionNotifyChanged(AuthorizationsChanged);
        }

        private void AuthorizationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IAuthorization authorization in e.NewItems)
                        OnAuthorizationAdded(authorization);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IAuthorization authorization in e.OldItems)
                        OnAuthorizationRemoved(authorization);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (IAuthorization authorization in e.OldItems)
                        OnAuthorizationRemoved(authorization);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SecurityItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ISecurityItem securityItem in e.NewItems)
                        OnSecurityItemAdded(securityItem);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ISecurityItem securityItem in e.OldItems)
                        OnSecurityItemRemoved(securityItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (ISecurityItem securityItem in e.OldItems)
                        OnSecurityItemRemoved(securityItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SecurityIdentitiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ISecurityIdentity securityIdentity in e.NewItems)
                        OnSecurityIdentityAdded(securityIdentity);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ISecurityIdentity securityIdentity in e.OldItems)
                        OnSecurityIdentityRemoved(securityIdentity);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    foreach (ISecurityIdentity securityIdentity in e.OldItems)
                        OnSecurityIdentityRemoved(securityIdentity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ICollection<ISecurityIdentity> SecurityIdentities { get; }
        public ICollection<ISecurityItem> SecurityItems { get; }
        public ICollection<IAuthorization> Authorizations { get; }

        #region Implementation of ISecurityStore

        public ISecurityEngine Engine { get; }
        IReadOnlyCollection<ISecurityIdentity> ISecurityStore.SecurityIdentities => (IReadOnlyCollection<ISecurityIdentity>)SecurityIdentities;
        IReadOnlyCollection<ISecurityItem> ISecurityStore.SecurityItems => (IReadOnlyCollection<ISecurityItem>)SecurityItems;
        IReadOnlyCollection<IAuthorization> ISecurityStore.Authorizations => (IReadOnlyCollection<IAuthorization>)Authorizations;
        public void AttachToStorage(ISecurityStorage storage)
        {
            if (storage != null)
                _storageSynchronizer = new StorageSynchronizer(this, storage);
            else
                _storageSynchronizer = null;
        }

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

        public void OnSecurityItemRelationAdded(ISecurityItem parent, ISecurityItem child)
        {
            _storageSynchronizer?.OnSecurityItemRelationAdded(parent,child);
        }
        public void OnSecurityItemRelationRemoved(ISecurityItem parent, ISecurityItem child)
        {
            _storageSynchronizer?.OnSecurityItemRelationRemoved(parent, child);
        }
        public void OnSecurityIdentityRelationAdded(ISecurityIdentity parent, ISecurityIdentity child)
        {
            _storageSynchronizer?.OnSecurityIdentityRelationAdded(parent, child);
        }
        public void OnSecurityIdentityRelationRemoved(ISecurityIdentity parent, ISecurityIdentity child)
        {
            _storageSynchronizer?.OnSecurityIdentityRelationRemoved(parent, child);
        }
        private void OnAuthorizationAdded(IAuthorization authorization)
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
            _storageSynchronizer?.OnAuthorizationAdded(authorization);
        }
        private void OnAuthorizationRemoved(IAuthorization authorization)
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
            _storageSynchronizer?.OnAuthorizationRemoved(authorization);

        }
        private void OnSecurityItemAdded(ISecurityItem securityItem)
        {
            _storageSynchronizer?.OnSecurityItemAdded(securityItem);
        }
        private void OnSecurityItemRemoved(ISecurityItem securityItem)
        {
            _storageSynchronizer?.OnSecurityItemRemoved(securityItem);
        }
        private void OnSecurityIdentityAdded(ISecurityIdentity securityIdentity)
        {
            _storageSynchronizer?.OnSecurityIdentityAdded(securityIdentity);
        }
        private void OnSecurityIdentityRemoved(ISecurityIdentity securityIdentity)
        {
            _storageSynchronizer?.OnSecurityIdentityRemoved(securityIdentity);
        }
        public void OnBagAdded(IBagObject bagObject, string key, string value)
        {
            _storageSynchronizer?.OnBagAdded(bagObject,key,value);
        }
        public void OnBagRemoved(IBagObject bagObject, string key, string value)
        {
            _storageSynchronizer?.OnBagRemoved(bagObject, key, value);
        }
    }
}