using System;
using System.Collections.Generic;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Store
{
    public interface IAuthorizationProvider
    {
        IEnumerable<IStorageAuthorization> Provide();
        event EventHandler Changed;
    }
}