using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface ISecurityStore
    {
        ISecurityEngine Engine { get; }
        IReadOnlyCollection<ISecurityIdentity> SecurityIdentities { get; }
        IReadOnlyCollection<ISecurityItem> SecurityItems { get; }
        IReadOnlyCollection<IAuthorization> Authorizations { get; }
    }
}
