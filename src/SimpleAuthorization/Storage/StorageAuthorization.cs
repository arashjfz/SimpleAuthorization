using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Storage
{
    class StorageAuthorization : IStorageAuthorization
    {
        #region Implementation of IStorageAuthorization

        public Guid Key { get; set; }
        public Guid AuthorisableItemKey { get; set; }
        public Guid SecurityItemKey { get; set; }
        public byte[] LifeCycle { get; set; }
        public Guid? DelegatedByKey { get; set; }
        public AuthorizationType Type { get; set; }
        public IEnumerable<byte[]> Conditions { get; set; }

        #endregion
    }
}