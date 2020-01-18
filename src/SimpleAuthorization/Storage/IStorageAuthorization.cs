using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Storage
{
    public interface IStorageAuthorization
    {
        Guid Key { get; }
        Guid AuthorisableItemKey { get; }
        Guid SecurityItemKey { get; }
        byte[] LifeCycle { get; }
        Guid? DelegatedByKey { get; }
        AuthorizationType Type { get; }
        IEnumerable<byte[]> Conditions { get; }

    }
}