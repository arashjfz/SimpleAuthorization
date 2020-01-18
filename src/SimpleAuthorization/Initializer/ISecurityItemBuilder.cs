using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Initializer
{
    public interface ISecurityItemBuilder
    {
        IStoreBuilder NoParent();
        IStoreBuilder AddParent(Guid parent);
        IStoreBuilder AddParents(IEnumerable<Guid> parents);
    }
}