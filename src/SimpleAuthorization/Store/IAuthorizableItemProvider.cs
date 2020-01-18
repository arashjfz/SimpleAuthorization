using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public interface IAuthorizableItemProvider
    {
        IEnumerable<IAuthorisableItem> Provide();
        event EventHandler Changed;
    }
}