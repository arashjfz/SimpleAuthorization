using System;
using System.Collections.Generic;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Store
{
    public interface ISecurityItemProvider
    {
        IEnumerable<Guid> ProvideSecurityItemKeys();
        IEnumerable<ISecurityHierarchy> ProvideSecurityHierarchies();
        event EventHandler Changed;
    }
}