using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate;
using SimpleAuthorization.Nhibernate.Models;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Nhibernate
{
    public class Storage:ISecurityStorage
    {
        private readonly Func<ISession> _sessionFactory;

        public Storage(Func<ISession> sessionFactory)
        {
            if(sessionFactory == null)
                throw new ArgumentException("sessionFactory is required",nameof(sessionFactory));
            _sessionFactory = sessionFactory;
        }
        
        #region Implementation of ISecurityStorage

        public void AddItems(IEnumerable<IStorageEntity> items)
        {
            ISession session = _sessionFactory();
            foreach (IStorageEntity storageEntity in items)
                session.Save(GetStorageItem(storageEntity));
            session.Flush();
        }

        public void RemoveItems(IEnumerable<IStorageEntity> items)
        {
            ISession session = _sessionFactory();
            foreach (IStorageEntity storageEntity in items)
                session.Delete(new StorageItem {Id = storageEntity.GetIdentity()});
            session.Flush();
        }

        public void UpdateItems(IEnumerable<IStorageEntity> items)
        {
            ISession session = _sessionFactory();
            foreach (IStorageEntity storageEntity in items)
                session.Update(GetStorageItem(storageEntity));
            session.Flush();

        }

        public IEnumerable GetItems(Type entityType)
        {
            ISession session = _sessionFactory();
            StorageType storageType = GetStorageType(entityType);
            return session.QueryOver<StorageItem>().Where(item => item.Type == storageType).List()
                .Select(GetStorageEntity);
        }

        public void ApplyBulkActions(IEnumerable<IStorageAction> actions)
        {
            foreach (IStorageAction storageAction in actions)
            {
                switch (storageAction.StorageActionType)
                {
                    case StorageActionType.Add:
                        AddItems(storageAction.Entities);
                        break;
                    case StorageActionType.Remove:
                        RemoveItems(storageAction.Entities);
                        break;
                    case StorageActionType.Update:
                        UpdateItems(storageAction.Entities);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        private StorageItem GetStorageItem(IStorageEntity storageEntity)
        {
            
            return GetStorageItem(storageEntity, storageEntity.GetIdentity(),
                GetStorageType(storageEntity.GetType()));

        }

        private StorageType GetStorageType(Type storageEntityType)
        {
            if (typeof(IStorageAccessAuthorization).IsAssignableFrom(storageEntityType))
                return StorageType.AccessAuthorization;

            if (typeof(IStorageBag).IsAssignableFrom(storageEntityType))
                return StorageType.Bag;

            if (typeof(IStorageConditionAuthorization).IsAssignableFrom(storageEntityType))
                return StorageType.ConditionAuthorization;

            if (typeof(IStorageSecurityIdentity).IsAssignableFrom(storageEntityType))
                return StorageType.SecurityIdentity;

            if (typeof(IStorageSecurityIdentityRelation).IsAssignableFrom(storageEntityType))
                return StorageType.SecurityIdentityRelation;

            if (typeof(IStorageSecurityItem).IsAssignableFrom(storageEntityType))
                return StorageType.SecurityItem;

            if (typeof(IStorageSecurityItemRelation).IsAssignableFrom(storageEntityType))
                return StorageType.SecurityItemRelation;
            throw new ArgumentOutOfRangeException();
        }
        
        private StorageItem GetStorageItem(IStorageEntity storageEntity, string id, StorageType storageType)
        {
            return new StorageItem{Id = id,Type = storageType,Data = JsonConvert.SerializeObject(storageEntity) };
        }

        private IStorageEntity GetStorageEntity(StorageItem storageItem)
        {
            Type storageEntityType;
            switch (storageItem.Type)
            {
                case StorageType.AccessAuthorization:
                    storageEntityType = typeof(StorageAccessAuthorization);
                    break;
                case StorageType.ConditionAuthorization:
                    storageEntityType = typeof(StorageConditionAuthorization);
                    break;
                case StorageType.SecurityIdentity:
                    storageEntityType = typeof(StorageSecurityIdentity);
                    break;
                case StorageType.SecurityIdentityRelation:
                    storageEntityType = typeof(StorageSecurityIdentityRelation);
                    break;
                case StorageType.SecurityItem:
                    storageEntityType = typeof(StorageSecurityItem);
                    break;
                case StorageType.SecurityItemRelation:
                    storageEntityType = typeof(StorageSecurityItemRelation);
                    break;
                case StorageType.Bag:
                    storageEntityType = typeof(StorageBag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (IStorageEntity) JsonConvert.DeserializeObject(storageItem.Data, storageEntityType);
        }
        
        private class StorageAccessAuthorization:IStorageAccessAuthorization
        {
            #region Implementation of IStorageAccessAuthorization

            public string Id { get; set; }
            public string SecurityIdentityId { get; set; }
            public string SecurityItemId { get; set; }
            public byte[] LifeTime { get; set; }
            public AccessType AccessType { get; set; }

            #endregion
        }
        private class StorageConditionAuthorization: IStorageConditionAuthorization
        {
            #region Implementation of IStorageConditionAuthorization

            public string Id { get; set; }
            public string SecurityIdentityId { get; set; }
            public string SecurityItemId { get; set; }
            public byte[] LifeTime { get; set; }
            public byte[] Conditions { get; set; }

            #endregion
        }
        private class StorageSecurityIdentity : IStorageSecurityIdentity
        {
            #region Implementation of IStorageSecurityIdentity

            public string Id { get; set; }

            #endregion
        }
        private class StorageSecurityIdentityRelation : IStorageSecurityIdentityRelation
        {
            #region Implementation of IStorageSecurityIdentityRelation

            public string SecurityIdentityId { get; set; }
            public string ParentId { get; set; }

            #endregion
        }
        private class StorageSecurityItem : IStorageSecurityItem
        {
            #region Implementation of IStorageSecurityItem

            public string Id { get; set; }

            #endregion
        }
        private class StorageSecurityItemRelation : IStorageSecurityItemRelation
        {
            #region Implementation of IStorageSecurityItemRelation

            public string ParentId { get; set; }
            public string SecurityItemId { get; set; }

            #endregion
        }
        private class StorageBag : IStorageBag
        {
            #region Implementation of IStorageBag

            public string TargetId { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }

            #endregion
        }
    }
}
