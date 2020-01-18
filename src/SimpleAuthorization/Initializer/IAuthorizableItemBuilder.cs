using System.Collections.Generic;

namespace SimpleAuthorization.Initializer
{
    public interface IAuthorizableItemBuilder
    {
        IStoreBuilder Add(ISecurityIdentity securityIdentity);
        IStoreBuilder Add(IEnumerable<ISecurityIdentity> securityIdentities);

    }
}