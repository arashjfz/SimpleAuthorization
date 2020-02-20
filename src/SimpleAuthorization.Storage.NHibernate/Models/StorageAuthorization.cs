using System;

namespace SimpleAuthorization.Storage.NHibernate.Models
{
    internal class StorageAuthorization: IStorageAuthorization
    {
        public StorageAuthorization()
        {
            
        }

        public StorageAuthorization(IStorageAuthorization storageAuthorization)
        {
            Key = storageAuthorization.Key;
            AuthorisableItemKey = storageAuthorization.AuthorisableItemKey;
            SecurityItemKey = storageAuthorization.SecurityItemKey;
            LifeCycle = storageAuthorization.LifeCycle;
            DelegatedByKey = storageAuthorization.DelegatedByKey;
            Type = storageAuthorization.Type;
            Conditions = storageAuthorization.Conditions;
        }
        #region Implementation of IStorageAuthorization
        public virtual Guid Key { get; set; }
        public virtual Guid AuthorisableItemKey { get; set; }
        public virtual Guid SecurityItemKey { get; set; }
        public virtual byte[] LifeCycle { get; set; }
        public virtual Guid? DelegatedByKey { get; set; }
        public virtual AuthorizationType Type { get; set; }
        public virtual byte[] Conditions { get; set; }

        #endregion
    }
}
