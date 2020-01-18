using System;
using System.Collections.Generic;
using SimpleAuthorization.Storage;
using SimpleAuthorization.Store;

namespace SimpleAuthorization.Initializer
{
    public interface IStoreBuilder
    {
        IStoreBuilder AddSecurityIdentity(ISecurityIdentity securityIdentity);
        IStoreBuilder AddSecurityIdentities(IEnumerable<ISecurityIdentity> securityIdentities);
        IAuthorizableItemBuilder AddAuthorizableItem(Guid key);
        IStoreBuilder AddAuthorizableItem(IAuthorisableItem authorisableItem);
        IStoreBuilder AddAuthorizableItems(IEnumerable<IAuthorisableItem> authorisableItems);
        ISecurityItemBuilder AddSecurityItem(Guid key);
        IStoreBuilder AddSecurityIdentityProvider(ISecurityIdentityProvider securityIdentityProvider);
        IStoreBuilder AddAuthorizableItemProvider(IAuthorizableItemProvider authorizableItemProvider);
        IStoreBuilder AddSecurityItemProvider(ISecurityItemProvider securityItemProvider);
        IStoreBuilder AddAuthrizationProvider(IAuthorizationProvider authorizationProvider);
        IStoreBuilder AddStorage(ISecurityStorage storage);
        ISecurityStore Build();
    }
}
