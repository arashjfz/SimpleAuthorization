using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Store
{
    internal class SecurityStore:ISecurityStore
    {
        private readonly IEnumerable<ISecurityIdentityProvider> _securityIdentityProviders;
        private readonly IEnumerable<IAuthorizableItemProvider> _authorizableItemProviders;
        private readonly IEnumerable<ISecurityItemProvider> _securityItemProviders;
        private readonly IEnumerable<IAuthorizationProvider> _authorizationProviders;
        private readonly Dictionary<Guid,ISecurityItem> _securityItems = new Dictionary<Guid, ISecurityItem>();
        private readonly Dictionary<Guid,IAuthorisableItem> _authorizableItems = new Dictionary<Guid, IAuthorisableItem>();
        private readonly Dictionary<Guid,ISecurityIdentity> _securityIdentities = new Dictionary<Guid, ISecurityIdentity>();
        private readonly Dictionary<Guid,IAuthorization> _authorizations = new Dictionary<Guid, IAuthorization>();

        public SecurityStore(IEnumerable<ISecurityIdentityProvider> securityIdentityProviders,
            IEnumerable<IAuthorizableItemProvider> authorizableItemProviders,
            IEnumerable<ISecurityItemProvider> securityItemProviders,
            IEnumerable<IAuthorizationProvider> authorizationProviders)
        {
            _securityIdentityProviders = securityIdentityProviders??Enumerable.Empty<ISecurityIdentityProvider>();
            _authorizableItemProviders = authorizableItemProviders??Enumerable.Empty<IAuthorizableItemProvider>();
            _securityItemProviders = securityItemProviders??Enumerable.Empty<ISecurityItemProvider>();
            _authorizationProviders = authorizationProviders??Enumerable.Empty<IAuthorizationProvider>();

            PopulateSecurityItems();
            PopulateAuthorizableItems();
            PopulateSecurityIdentity();
            PopulateAuthorizations();

            foreach (ISecurityIdentityProvider securityIdentityProvider in _securityIdentityProviders)
                securityIdentityProvider.Changed += (sender, args) =>
                {
                    PopulateSecurityIdentity();
                    OnChanged();
                };
            foreach (IAuthorizableItemProvider authorizableItemProvider in _authorizableItemProviders)
                authorizableItemProvider.Changed += (sender, args) =>
                {
                    PopulateAuthorizableItems();
                    OnChanged();
                };
            foreach (ISecurityItemProvider securityItemProvider in _securityItemProviders)
                securityItemProvider.Changed += (sender, args) =>
                {
                    PopulateSecurityItems();
                    OnChanged();
                };
            foreach (IAuthorizationProvider authorizationProvider in _authorizationProviders)
                authorizationProvider.Changed += (sender, args) =>
                {
                    PopulateAuthorizations();
                    OnChanged();
                };
        }

        private void PopulateSecurityItems()
        {
            _securityItems.Clear();
            IEnumerable<Guid> securityItemKeys = _securityItemProviders.SelectMany(p => p.ProvideSecurityItemKeys());
            IEnumerable<ISecurityHierarchy> securityHierarchies = _securityItemProviders.SelectMany(p => p.ProvideSecurityHierarchies());
            Dictionary<Guid, IEnumerable<Guid>> parentMap = securityHierarchies.GroupBy(h => h.SecurityItemKey).ToDictionary(g => g.Key,g => g.Select(h => h.SecurityItemParentKey));
            Dictionary<Guid, IEnumerable<Guid>> childMap = securityHierarchies.GroupBy(h => h.SecurityItemParentKey).ToDictionary(g => g.Key,g => g.Select(h => h.SecurityItemKey));
            foreach (Guid key in securityItemKeys)
                _securityItems.Add(key, new StoreSecurityItem(key, parentMap, childMap, _securityItems));
        }

        private void PopulateAuthorizableItems()
        {
            _authorizableItems.Clear();
            foreach (IAuthorisableItem authorisableItem in _authorizableItemProviders.SelectMany(p => p.Provide())) 
                _authorizableItems.Add(authorisableItem.Key,authorisableItem);
        }

        private void PopulateSecurityIdentity()
        {
            _securityIdentities.Clear();
            foreach (ISecurityIdentity securityIdentity in _securityIdentityProviders.SelectMany(p => p.Provide()))
                _securityIdentities.Add(securityIdentity.Key,securityIdentity);
        }
        private void PopulateAuthorizations()
        {
            _authorizations.Clear();
            foreach (IStorageAuthorization storageAuthorization in _authorizationProviders.SelectMany(p => p.Provide()))
            {
                _authorizations.Add(storageAuthorization.Key, new StoreAuthorization(
                    _authorizableItems[storageAuthorization.AuthorisableItemKey],
                    _securityItems[storageAuthorization.SecurityItemKey],
                    BinaryFormat<IAuthorizationLifeCycle>(storageAuthorization.LifeCycle),
                    storageAuthorization.DelegatedByKey == null
                        ? null
                        : _securityIdentities[storageAuthorization.DelegatedByKey.Value], storageAuthorization.Type,
                    storageAuthorization.Conditions == null
                        ? null
                        : storageAuthorization.Conditions.Select(values => BinaryFormat<ICondition>(values))));
            }
                
        }

        private T BinaryFormat<T>(byte[] values)
        {
            if (values == null)
                return default(T);
            BinaryFormatter formatter = new BinaryFormatter();
            return (T) formatter.Deserialize(new MemoryStream(values));
        }
        #region Implementation of ISecurityStore

        public ISecurityItem GetSecurityItem(Guid key)
        {
            _securityItems.TryGetValue(key, out ISecurityItem securityItem);
            return securityItem;
        }

        public IEnumerable<ISecurityItem> GetSecurityItems()
        {
            return _securityItems.Values;
        }

        public IAuthorisableItem GetAuthorizableItem(Guid key)
        {
            _authorizableItems.TryGetValue(key, out IAuthorisableItem authorisableItem);
            return authorisableItem;
        }

        public IEnumerable<IAuthorisableItem> GetAuthorisableItems()
        {
            return _authorizableItems.Values;
        }

        public ISecurityIdentity GetSecurityIdentity(Guid key)
        {
            _securityIdentities.TryGetValue(key, out ISecurityIdentity securityIdentity);
            return securityIdentity;
        }

        public IEnumerable<ISecurityIdentity> GetSecurityIdentities()
        {
            return _securityIdentities.Values;
        }

        public IAuthorization GetAuthorization(Guid key)
        {
            _authorizations.TryGetValue(key, out IAuthorization authorization);
            return authorization;
        }

        public IEnumerable<IAuthorization> GetAuthorizations()
        {

            return _authorizations.Values;
        }

        public event EventHandler Changed;

        #endregion

        private class StoreSecurityItem : ISecurityItem
        {
            private readonly Dictionary<Guid, IEnumerable<Guid>> _parentMap;
            private readonly Dictionary<Guid, IEnumerable<Guid>> _childMap;
            private readonly Dictionary<Guid, ISecurityItem> _securityItemMap;

            public StoreSecurityItem(Guid key, Dictionary<Guid, IEnumerable<Guid>> parentMap, Dictionary<Guid, IEnumerable<Guid>> childMap,Dictionary<Guid,ISecurityItem> securityItemMap)
            {
                _parentMap = parentMap;
                _childMap = childMap;
                _securityItemMap = securityItemMap;
                Key = key;
            }
            #region Implementation of ISecurityItem

            public Guid Key { get; }

            public IEnumerable<ISecurityItem> Parents
            {
                get
                {
                    if (!_parentMap.TryGetValue(Key, out IEnumerable<Guid> parents))
                        return Enumerable.Empty<ISecurityItem>();
                    return parents.Select(k => _securityItemMap[k]);
                }
            }

            public IEnumerable<ISecurityItem> Children
            {
                get
                {
                    if (!_childMap.TryGetValue(Key, out IEnumerable<Guid> children))
                        return Enumerable.Empty<ISecurityItem>();
                    return children.Select(k => _securityItemMap[k]);

                }
            }

            #endregion
        }
        private class StoreAuthorization:IAuthorization
        {
            public StoreAuthorization(IAuthorisableItem authorisableItem, ISecurityItem securityItem, IAuthorizationLifeCycle lifeCycle, ISecurityIdentity delegatedBy, AuthorizationType type, IEnumerable<ICondition> conditions)
            {
                AuthorisableItem = authorisableItem;
                SecurityItem = securityItem;
                LifeCycle = lifeCycle;
                DelegatedBy = delegatedBy;
                Type = type;
                Conditions = conditions;
            }

            #region Implementation of IAuthorization

            public IAuthorisableItem AuthorisableItem { get; }
            public ISecurityItem SecurityItem { get; }
            public IAuthorizationLifeCycle LifeCycle { get; }
            public ISecurityIdentity DelegatedBy { get; }
            public AuthorizationType Type { get; }
            public IEnumerable<ICondition> Conditions { get; }

            #endregion
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}