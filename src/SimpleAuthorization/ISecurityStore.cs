using System.Collections.Generic;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization
{
    public interface ISecurityStore
    {
        ISecurityEngine Engine { get; }
        IReadOnlyCollection<ISecurityIdentity> SecurityIdentities { get; }
        IReadOnlyCollection<ISecurityItem> SecurityItems { get; }
        IReadOnlyCollection<IAuthorization> Authorizations { get; }
        void AttachToStorage(ISecurityStorage storage);
    }
}
