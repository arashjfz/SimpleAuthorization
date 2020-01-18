using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuthorization.Storage;
using SimpleAuthorization.Store;

namespace SimpleAuthorization.Initializer
{
    public class StoreBuilder : IStoreBuilder
    {
        private readonly List<ISecurityIdentityProvider> _securityIdentityProviders = new List<ISecurityIdentityProvider>();
        private readonly List<IAuthorizableItemProvider> _authorizableItemProviders = new List<IAuthorizableItemProvider>();
        private List<IAuthorizationProvider> _authorizationProviders = new List<IAuthorizationProvider>();
        private readonly List<ISecurityItemProvider> _securityItemProviders = new List<ISecurityItemProvider>();
        #region Implementation of IStoreBuilder

        public IStorageBuilder Storage { get; }
        public IStoreBuilder AddSecurityIdentity(ISecurityIdentity securityIdentity)
        {
            _securityIdentityProviders.Add(new SecurityIdentityProvider(Enumerable.Repeat(securityIdentity, 1)));
            return this;
        }
        public IStoreBuilder AddSecurityIdentities(IEnumerable<ISecurityIdentity> securityIdentities)
        {
            _securityIdentityProviders.Add(new SecurityIdentityProvider(securityIdentities));
            return this;
        }
        public IAuthorizableItemBuilder AddAuthorizableItem(Guid key)
        {
            return new AuthorizableItemBuilder(this, key);
        }

        public IStoreBuilder AddAuthorizableItem(IAuthorisableItem authorisableItem)
        {
            _authorizableItemProviders.Add(new AuthorizableItemProvider(Enumerable.Repeat(authorisableItem,1)));
            return this;
        }

        public IStoreBuilder AddAuthorizableItems(IEnumerable<IAuthorisableItem> authorisableItems)
        {
            _authorizableItemProviders.Add(new AuthorizableItemProvider(authorisableItems));
            return this;

        }

        public ISecurityItemBuilder AddSecurityItem(Guid key)
        {
            return new SecurityItemBuilder(this, key);
        }
        public IStoreBuilder AddSecurityIdentityProvider(ISecurityIdentityProvider securityIdentityProvider)
        {
            _securityIdentityProviders.Add(securityIdentityProvider);
            return this;
        }
        public IStoreBuilder AddAuthorizableItemProvider(IAuthorizableItemProvider authorizableItemProvider)
        {
            _authorizableItemProviders.Add(authorizableItemProvider);
            return this;
        }
        public IStoreBuilder AddSecurityItemProvider(ISecurityItemProvider securityItemProvider)
        {
            _securityItemProviders.Add(securityItemProvider);
            return this;
        }
        public IStoreBuilder AddAuthrizationProvider(IAuthorizationProvider authorizationProvider)
        {
            _authorizationProviders.Add(authorizationProvider);
            return this;
        }

        public ISecurityStore Build()
        {
            return new SecurityStore(_securityIdentityProviders, _authorizableItemProviders, _securityItemProviders, _authorizationProviders);
        }
        #endregion
        private class SecurityIdentityProvider : ISecurityIdentityProvider
        {
            private readonly IEnumerable<ISecurityIdentity> _securityIdentities;

            public SecurityIdentityProvider(IEnumerable<ISecurityIdentity> securityIdentities)
            {
                _securityIdentities = securityIdentities;
            }
            #region Implementation of ISecurityIdentityProvider

            public IEnumerable<ISecurityIdentity> Provide()
            {
                return _securityIdentities;
            }

            public event EventHandler Changed;

            #endregion
        }
        private class AuthorizableItemBuilder : IAuthorizableItemBuilder
        {
            private readonly StoreBuilder _storeBuilder;
            private readonly Guid _key;

            public AuthorizableItemBuilder(StoreBuilder storeBuilder, Guid key)
            {
                _storeBuilder = storeBuilder;
                _key = key;
            }
            #region Implementation of IAuthorizableItemBuilder

            public IStoreBuilder Add(ISecurityIdentity securityIdentity)
            {
                Dictionary<Guid, ISecurityIdentity> securityIdentities = new Dictionary<Guid, ISecurityIdentity>();
                securityIdentities.Add(securityIdentity.Key, securityIdentity);
                _storeBuilder._authorizableItemProviders.Add(new AuthorizableItemProvider(Enumerable.Repeat(new AuthorizableItem(securityIdentities, _key), 1)));
                return _storeBuilder;
            }

            public IStoreBuilder Add(IEnumerable<ISecurityIdentity> securityIdentities)
            {
                Dictionary<Guid, ISecurityIdentity> securityIdentitiesMap = securityIdentities.ToDictionary(s => s.Key);
                _storeBuilder._authorizableItemProviders.Add(new AuthorizableItemProvider(Enumerable.Repeat(new AuthorizableItem(securityIdentitiesMap, _key), 1)));
                return _storeBuilder;
            }

            #endregion
            private class AuthorizableItem : IAuthorisableItem
            {
                private readonly IDictionary<Guid, ISecurityIdentity> _securityIdentities;

                public AuthorizableItem(IDictionary<Guid, ISecurityIdentity> securityIdentities, Guid key)
                {
                    Key = key;
                    _securityIdentities = securityIdentities;
                }
                #region Implementation of IAuthorisableItem

                public Guid Key { get; }
                public IEnumerable<ISecurityIdentity> GetSecurityIdentities()
                {
                    return _securityIdentities.Values;
                }

                public bool ContainSecurityIdentity(ISecurityIdentity securityIdentity)
                {
                    return _securityIdentities.ContainsKey(securityIdentity.Key);
                }

                #endregion
            }
        }
        private class AuthorizableItemProvider : IAuthorizableItemProvider
        {
            private readonly IEnumerable<IAuthorisableItem> _authorisableItems;

            public AuthorizableItemProvider(IEnumerable<IAuthorisableItem> authorisableItems)
            {
                _authorisableItems = authorisableItems;
            }
            #region Implementation of IAuthorizableItemProvider

            public IEnumerable<IAuthorisableItem> Provide()
            {
                return _authorisableItems;
            }

            public event EventHandler Changed;

            #endregion
        }
        private class SecurityItemProvider : ISecurityItemProvider
        {
            private readonly IEnumerable<Guid> _securityItemKeys;
            private readonly IEnumerable<ISecurityHierarchy> _hierarchies;

            public SecurityItemProvider(IEnumerable<Guid> securityItemKeys, IEnumerable<ISecurityHierarchy> hierarchies)
            {
                _securityItemKeys = securityItemKeys;
                _hierarchies = hierarchies;
            }
            #region Implementation of ISecurityItemProvider

            public IEnumerable<Guid> ProvideSecurityItemKeys()
            {
                return _securityItemKeys;
            }

            public IEnumerable<ISecurityHierarchy> ProvideSecurityHierarchies()
            {
                return _hierarchies;
            }

            public event EventHandler Changed;

            #endregion
        }
        private class SecurityHierarchy : ISecurityHierarchy
        {
            public SecurityHierarchy(Guid securityItemKey, Guid securityItemParentKey)
            {
                SecurityItemKey = securityItemKey;
                SecurityItemParentKey = securityItemParentKey;
            }

            #region Implementation of ISecurityHierarchy

            public Guid SecurityItemKey { get; }
            public Guid SecurityItemParentKey { get; }

            #endregion
        }
        private class SecurityItemBuilder : ISecurityItemBuilder
        {
            private readonly StoreBuilder _storeBuilder;
            private readonly Guid _key;

            public SecurityItemBuilder(StoreBuilder storeBuilder, Guid key)
            {
                _storeBuilder = storeBuilder;
                _key = key;
            }
            #region Implementation of ISecurityItemBuilder

            public IStoreBuilder NoParent()
            {
                _storeBuilder._securityItemProviders.Add(new SecurityItemProvider(Enumerable.Repeat(_key, 1),
                    Enumerable.Empty<ISecurityHierarchy>()));
                return _storeBuilder;
            }

            public IStoreBuilder AddParent(Guid parent)
            {
                IEnumerable<ISecurityHierarchy> securityHierarchies = Enumerable.Repeat(new SecurityHierarchy(_key, parent), 1);
                _storeBuilder._securityItemProviders.Add(new SecurityItemProvider(Enumerable.Repeat(_key, 1),
                    securityHierarchies));
                return _storeBuilder;
            }

            public IStoreBuilder AddParents(IEnumerable<Guid> parents)
            {
                _storeBuilder._securityItemProviders.Add(new SecurityItemProvider(Enumerable.Repeat(_key, 1),
                    parents.Select(p => new SecurityHierarchy(_key, p))));
                return _storeBuilder;
            }

            #endregion
        }
    }
}