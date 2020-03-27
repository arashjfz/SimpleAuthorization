using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Engine
{
    internal class StorageSynchronizer
    {
        private readonly SecurityStore _store;
        private readonly ISecurityStorage _securityStorage;
        private bool _isSuspended;
        private bool _ignoreEvents;
        readonly Queue<StorageAction> _storageActions = new Queue<StorageAction>();
        public StorageSynchronizer(SecurityStore store, ISecurityStorage securityStorage)
        {
            _store = store;
            _securityStorage = securityStorage;
            Sync();
        }

        private object DeserializeObject(byte[] serializedData)
        {
            if (serializedData == null)
                return null;
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            return binaryFormatter.Deserialize(new MemoryStream(serializedData));
        }
        private void Sync()
        {
            IEnumerable<IStorageSecurityItem> storageSecurityItems = _securityStorage.GetItems<IStorageSecurityItem>();
            IEnumerable<IStorageSecurityItemRelation> storageSecurityItemRelations = _securityStorage.GetItems<IStorageSecurityItemRelation>();
            IEnumerable<IStorageSecurityIdentity> storageSecurityIdentities = _securityStorage.GetItems<IStorageSecurityIdentity>();
            IEnumerable<IStorageSecurityIdentityRelation> storageSecurityIdentityRelations = _securityStorage.GetItems<IStorageSecurityIdentityRelation>();
            IEnumerable<IStorageAccessAuthorization> storageAccessAuthorizations = _securityStorage.GetItems<IStorageAccessAuthorization>();
            IEnumerable<IStorageConditionAuthorization> storageConditionAuthorizations = _securityStorage.GetItems<IStorageConditionAuthorization>();
            IEnumerable<IStorageBag> storageBags = _securityStorage.GetItems<IStorageBag>();
            _ignoreEvents = true;
            Suspend();
            try
            {
                Dictionary<string, ISecurityItem> securityItems = SyncSecurityItems(storageSecurityItems);
                SyncSecurityItemRelations(storageSecurityItemRelations, securityItems);
                Dictionary<string, ISecurityIdentity> securityIdentities = SyncSecurityIdentity(storageSecurityIdentities);
                SyncSecurityIdentityRelations(storageSecurityIdentityRelations, securityIdentities);
                Dictionary<string, IAuthorization> accessAuthorizations = SyncAccessAuthorizations(storageAccessAuthorizations, securityItems, securityIdentities);
                Dictionary<string, IAuthorization> conditionAuthorizations = SyncConditionAuthorizations(storageConditionAuthorizations, securityItems, securityIdentities);
                HashSet<IStorageBag> bags = new HashSet<IStorageBag>(storageBags, new StorageBagComparer());
                foreach (IStorageBag storageBag in bags)
                {
                    if (securityItems.TryGetValue(storageBag.TargetId, out ISecurityItem securityItem))
                        securityItem.Bag[storageBag.Key] = storageBag.Value;
                    if (securityIdentities.TryGetValue(storageBag.TargetId, out ISecurityIdentity securityIdentity))
                        securityIdentity.Bag[storageBag.Key] = storageBag.Value;
                    if (accessAuthorizations.TryGetValue(storageBag.TargetId, out IAuthorization accessAuthorization))
                        accessAuthorization.Bag[storageBag.Key] = storageBag.Value;
                    if (conditionAuthorizations.TryGetValue(storageBag.TargetId, out IAuthorization conditionAuthorization))
                        conditionAuthorization.Bag[storageBag.Key] = storageBag.Value;
                }

                HashSet<IStorageBag> newBags = new HashSet<IStorageBag>(new StorageBagComparer());
                foreach (IBagObject bagObject in _store.SecurityItems.Cast<IBagObject>().Concat(_store.Authorizations).Concat(_store.SecurityIdentities))
                    foreach (string bagKey in bagObject.Bag.Keys)
                    {
                        StorageBag storageBag = new StorageBag(bagObject.Id, bagKey, bagObject.Bag[bagKey]);
                        if (!bags.Contains(storageBag))
                            newBags.Add(storageBag);
                    }
                if (newBags.Count > 0)
                    AddNewAction(StorageActionType.Add, newBags);
            }
            finally
            {
                _ignoreEvents = false;
                Resume();
            }
        }

        private Dictionary<string, IAuthorization> SyncAccessAuthorizations(IEnumerable<IStorageAccessAuthorization> storageAccessAuthorizations, Dictionary<string, ISecurityItem> securityItems,
            Dictionary<string, ISecurityIdentity> securityIdentities)
        {
            Dictionary<string, IStorageAccessAuthorization> accessAuthorizations =
                storageAccessAuthorizations.ToDictionary(a => a.Id);
            Dictionary<string, IAuthorization> authorizations = _store.Authorizations.ToDictionary(a => a.Id);
            foreach (string accessAuthorizationId in accessAuthorizations.Keys)
                if (!authorizations.ContainsKey(accessAuthorizationId))
                {
                    IStorageAccessAuthorization storageAccessAuthorization = accessAuthorizations[accessAuthorizationId];
                    if (!securityItems.TryGetValue(storageAccessAuthorization.SecurityItemId, out ISecurityItem securityItem))
                        continue;
                    if (!securityIdentities.TryGetValue(storageAccessAuthorization.SecurityIdentityId,
                        out ISecurityIdentity securityIdentity))
                        continue;

                    IAccessAuthorization accessAuthorization =
                        _store.AccessAuthorize(securityIdentity, securityItem, accessAuthorizationId);
                    accessAuthorization.LifeTime =
                        (IAuthorizationLifeTime)DeserializeObject(storageAccessAuthorization.LifeTime);
                    authorizations.Add(accessAuthorizationId, accessAuthorization);
                }

            foreach (string accessAuthorizationId in authorizations.Keys)
            {
                if (!accessAuthorizations.ContainsKey(accessAuthorizationId))
                {
                    AccessAuthorization accessAuthorization = (AccessAuthorization)authorizations[accessAuthorizationId];
                    AddNewAction(StorageActionType.Add,
                        new StorageAccessAuthorization(accessAuthorizationId, accessAuthorization.SecurityIdentity.Id,
                            accessAuthorization.SecurityItem.Id, ToByteArray(accessAuthorization.LifeTime),
                            accessAuthorization.AccessType));
                }
            }

            return authorizations;
        }
        private Dictionary<string, IAuthorization> SyncConditionAuthorizations(IEnumerable<IStorageConditionAuthorization> storageConditionAuthorizations, Dictionary<string, ISecurityItem> securityItems,
    Dictionary<string, ISecurityIdentity> securityIdentities)
        {
            Dictionary<string, IStorageConditionAuthorization> conditionAuthorizations =
                storageConditionAuthorizations.ToDictionary(a => a.Id);
            Dictionary<string, IAuthorization> authorizations = _store.Authorizations.ToDictionary(a => a.Id);
            foreach (string conditionAuthorizationId in conditionAuthorizations.Keys)
                if (!authorizations.ContainsKey(conditionAuthorizationId))
                {
                    IStorageConditionAuthorization conditionAuthorization = conditionAuthorizations[conditionAuthorizationId];
                    if (!securityItems.TryGetValue(conditionAuthorization.SecurityItemId, out ISecurityItem securityItem))
                        continue;
                    if (!securityIdentities.TryGetValue(conditionAuthorization.SecurityIdentityId,
                        out ISecurityIdentity securityIdentity))
                        continue;

                    IConditionalAuthorization conditionalAuthorization =
                        _store.ConditionalAuthorize(securityIdentity, securityItem, conditionAuthorizationId);
                    conditionalAuthorization.LifeTime =
                        (IAuthorizationLifeTime)DeserializeObject(conditionAuthorization.LifeTime);
                    authorizations.Add(conditionAuthorizationId, conditionalAuthorization);
                }

            foreach (string conditionAuthorizationId in authorizations.Keys)
            {
                if (!conditionAuthorizations.ContainsKey(conditionAuthorizationId))
                {
                    ConditionalAuthorization conditionalAuthorization = (ConditionalAuthorization)authorizations[conditionAuthorizationId];
                    AddNewAction(StorageActionType.Add,
                        new StorageConditionAuthorization(conditionAuthorizationId, conditionalAuthorization.SecurityIdentity.Id,
                            conditionalAuthorization.SecurityItem.Id, ToByteArray(conditionalAuthorization.LifeTime),
                            ToByteArray(conditionalAuthorization.Conditions)));
                }
            }

            return authorizations;
        }
        private void SyncSecurityItemRelations(IEnumerable<IStorageSecurityItemRelation> storageSecurityItemRelations, Dictionary<string, ISecurityItem> securityItems)
        {
            HashSet<IStorageSecurityItemRelation> securityItemRelations = new HashSet<IStorageSecurityItemRelation>(
                storageSecurityItemRelations,
                new StorageSecurityItemRelationComparer());
            foreach (IStorageSecurityItemRelation storageSecurityItemRelation in securityItemRelations)
            {
                if (securityItems.TryGetValue(storageSecurityItemRelation.ParentId, out ISecurityItem parent) &&
                    securityItems.TryGetValue(storageSecurityItemRelation.SecurityItemId, out ISecurityItem child))
                {
                    if (!parent.Children.Contains(child))
                        parent.Children.Add(child);
                }
            }

            foreach (ISecurityItem parent in _store.SecurityItems)
            {
                foreach (ISecurityItem child in parent.Children)
                {
                    IStorageSecurityItemRelation storageSecurityItemRelation =
                        new StorageSecurityItemRelation(parent.Id, child.Id);
                    if (!securityItemRelations.Contains(storageSecurityItemRelation))
                        AddNewAction(StorageActionType.Add, storageSecurityItemRelation);
                }
            }
        }
        private Dictionary<string, ISecurityItem> SyncSecurityItems(IEnumerable<IStorageSecurityItem> storageSecurityItems)
        {
            Dictionary<string, ISecurityItem> securityItems = _store.SecurityItems.ToDictionary(i => i.Id);
            HashSet<string> storageSecurityItemIds = new HashSet<string>(storageSecurityItems.Select(i => i.Id));
            foreach (string storageId in storageSecurityItemIds)
                if (!securityItems.ContainsKey(storageId))
                    securityItems.Add(storageId, _store.AddSecurityItem(storageId));
            foreach (string securityItemId in securityItems.Keys)
                if (!storageSecurityItemIds.Contains(securityItemId))
                    AddNewAction(StorageActionType.Add, new StorageSecurityItem(securityItemId));
            return securityItems;
        }

        private void SyncSecurityIdentityRelations(IEnumerable<IStorageSecurityIdentityRelation> storageSecurityItemRelations, Dictionary<string, ISecurityIdentity> securityIdentities)
        {
            HashSet<IStorageSecurityIdentityRelation> securityIdentityRelations = new HashSet<IStorageSecurityIdentityRelation>(
                storageSecurityItemRelations,
                new StorageSecurityIdentityRelationComparer());
            foreach (IStorageSecurityIdentityRelation storageSecurityIdentityRelation in securityIdentityRelations)
            {
                if (securityIdentities.TryGetValue(storageSecurityIdentityRelation.ParentId, out ISecurityIdentity parent) &&
                    securityIdentities.TryGetValue(storageSecurityIdentityRelation.SecurityIdentityId, out ISecurityIdentity child))
                {
                    if (!parent.Children.Contains(child))
                        parent.Children.Add(child);
                }
            }

            foreach (ISecurityIdentity parent in _store.SecurityIdentities)
            {
                foreach (ISecurityIdentity child in parent.Children)
                {
                    IStorageSecurityIdentityRelation securityIdentityRelation =
                        new StorageSecurityIdentityRelation(parent.Id, child.Id);
                    if (!securityIdentityRelations.Contains(securityIdentityRelation))
                        AddNewAction(StorageActionType.Add, securityIdentityRelation);
                }
            }
        }
        private Dictionary<string, ISecurityIdentity> SyncSecurityIdentity(IEnumerable<IStorageSecurityIdentity> storageSecurityIdentities)
        {
            Dictionary<string, ISecurityIdentity> securityIdentities = _store.SecurityIdentities.ToDictionary(i => i.Id);
            HashSet<string> storageSecurityIdentityIds = new HashSet<string>(storageSecurityIdentities.Select(i => i.Id));
            foreach (string storageId in storageSecurityIdentityIds)
                if (!securityIdentities.ContainsKey(storageId))
                    securityIdentities.Add(storageId, _store.AddSecurityIdentity(storageId));
            foreach (string securityIdentityId in securityIdentities.Keys)
                if (!storageSecurityIdentityIds.Contains(securityIdentityId))
                    AddNewAction(StorageActionType.Add, new StorageSecurityIdentity(securityIdentityId));
            return securityIdentities;
        }


        public void OnSecurityItemRelationAdded(ISecurityItem parent, ISecurityItem child)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Add, new StorageSecurityItemRelation(parent.Id, child.Id));
        }
        public void OnSecurityItemRelationRemoved(ISecurityItem parent, ISecurityItem child)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Remove, new StorageSecurityItemRelation(parent.Id, child.Id));
        }
        public void OnSecurityIdentityRelationAdded(ISecurityIdentity parent, ISecurityIdentity child)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Add, new StorageSecurityIdentityRelation(parent.Id, child.Id));
        }
        public void OnSecurityIdentityRelationRemoved(ISecurityIdentity parent, ISecurityIdentity child)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Remove, new StorageSecurityIdentityRelation(parent.Id, child.Id));
        }

        private byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.GetBuffer();
            }


        }
        public void OnAuthorizationAdded(IAuthorization authorization)
        {
            if (_ignoreEvents)
                return;
            switch (authorization)
            {
                case AccessAuthorization accessAuthorization:
                    AddNewAction(StorageActionType.Add,
                        new StorageAccessAuthorization(
                            accessAuthorization.Id.ToString(),
                            accessAuthorization.SecurityIdentity.Id,
                            accessAuthorization.SecurityItem.Id,
                            ToByteArray(accessAuthorization.LifeTime),
                            accessAuthorization.AccessType));
                    break;
                case ConditionalAuthorization conditionalAuthorization:
                    AddNewAction(StorageActionType.Add,
                        new StorageConditionAuthorization(
                            conditionalAuthorization.Id.ToString(),
                            conditionalAuthorization.SecurityIdentity.Id,
                            conditionalAuthorization.SecurityItem.Id,
                            ToByteArray(conditionalAuthorization.LifeTime),
                            ToByteArray(conditionalAuthorization.Conditions)));
                    break;
            }
        }
        public void OnAuthorizationRemoved(IAuthorization authorization)
        {
            if (_ignoreEvents)
                return;
            switch (authorization)
            {
                case AccessAuthorization accessAuthorization:
                    AddNewAction(StorageActionType.Remove,
                        new StorageAccessAuthorization(
                            accessAuthorization.Id.ToString(),
                            accessAuthorization.SecurityIdentity.Id,
                            accessAuthorization.SecurityItem.Id,
                            ToByteArray(accessAuthorization.LifeTime),
                            accessAuthorization.AccessType));
                    break;
                case ConditionalAuthorization conditionalAuthorization:
                    AddNewAction(StorageActionType.Remove,
                        new StorageConditionAuthorization(
                            conditionalAuthorization.Id.ToString(),
                            conditionalAuthorization.SecurityIdentity.Id,
                            conditionalAuthorization.SecurityItem.Id,
                            ToByteArray(conditionalAuthorization.LifeTime),
                            ToByteArray(conditionalAuthorization.Conditions)));
                    break;
            }
        }
        public void OnSecurityItemAdded(ISecurityItem securityItem)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Add, new StorageSecurityItem(securityItem.Id));
        }
        public void OnSecurityItemRemoved(ISecurityItem securityItem)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Remove, new StorageSecurityItem(securityItem.Id));
        }
        public void OnSecurityIdentityAdded(ISecurityIdentity securityIdentity)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Add, new StorageSecurityIdentity(securityIdentity.Id));
        }
        public void OnSecurityIdentityRemoved(ISecurityIdentity securityIdentity)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Remove, new StorageSecurityIdentity(securityIdentity.Id));
        }
        public void OnBagAdded(IBagObject bagObject, string key, string value)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Add, new StorageBag(bagObject.Id, key, value));

        }
        public void OnBagRemoved(IBagObject bagObject, string key, string value)
        {
            if (_ignoreEvents)
                return;
            AddNewAction(StorageActionType.Remove, new StorageBag(bagObject.Id, key, value));
        }


        public void Suspend()
        {
            _isSuspended = true;
        }
        public void Resume()
        {
            _isSuspended = false;

            if (_storageActions.Count > 0)
            {
                ApplyActions(_storageActions);
                _storageActions.Clear();
            }
        }

        private void AddNewAction(StorageActionType actionType, IStorageEntity entity)
        {
            AddNewAction(actionType, Enumerable.Repeat(entity, 1));
        }
        private void AddNewAction(StorageActionType actionType, IEnumerable<IStorageEntity> entities)
        {
            StorageAction storageAction = new StorageAction(actionType, entities);
            if (_isSuspended)
                _storageActions.Enqueue(storageAction);
            else
                ApplyAction(storageAction);
        }

        private void ApplyActions(IEnumerable<StorageAction> storageActions)
        {
            _securityStorage.ApplyBulkActions(storageActions);
        }
        private void ApplyAction(StorageAction storageAction)
        {
            switch (storageAction.StorageActionType)
            {
                case StorageActionType.Add:
                    _securityStorage.AddItems(storageAction.Entities);
                    break;
                case StorageActionType.Remove:
                    _securityStorage.RemoveItems(storageAction.Entities);
                    break;
                case StorageActionType.Update:
                    _securityStorage.UpdateItems(storageAction.Entities);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

    }

    internal class StorageAction : IStorageAction
    {
        public StorageAction(StorageActionType storageActionType, IEnumerable<IStorageEntity> entities)
        {
            StorageActionType = storageActionType;
            Entities = entities;
        }

        public StorageActionType StorageActionType { get; }
        public IEnumerable<IStorageEntity> Entities { get; }
    }
    internal class StorageSecurityItemRelationComparer : IEqualityComparer<IStorageSecurityItemRelation>
    {
        #region Implementation of IEqualityComparer<in IStorageSecurityItemRelation>

        public bool Equals(IStorageSecurityItemRelation x, IStorageSecurityItemRelation y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.ParentId == y.ParentId && x.SecurityItemId == y.SecurityItemId;
        }

        public int GetHashCode(IStorageSecurityItemRelation obj)
        {
            return obj.ParentId.GetHashCode() ^ obj.SecurityItemId.GetHashCode();
        }

        #endregion
    }
    internal class StorageSecurityIdentityRelationComparer : IEqualityComparer<IStorageSecurityIdentityRelation>
    {
        #region Implementation of IEqualityComparer<in IStorageSecurityItemRelation>

        public bool Equals(IStorageSecurityIdentityRelation x, IStorageSecurityIdentityRelation y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.ParentId == y.ParentId && x.SecurityIdentityId == y.SecurityIdentityId;
        }

        public int GetHashCode(IStorageSecurityIdentityRelation obj)
        {
            return obj.ParentId.GetHashCode() ^ obj.SecurityIdentityId.GetHashCode();
        }

        #endregion
    }
    internal class StorageBagComparer : IEqualityComparer<IStorageBag>
    {
        #region Implementation of IEqualityComparer<in IStorageSecurityItemRelation>

        public bool Equals(IStorageBag x, IStorageBag y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return x.TargetId == y.TargetId && x.Key == y.Key;
        }

        public int GetHashCode(IStorageBag obj)
        {
            return obj.TargetId.GetHashCode() ^ obj.Key.GetHashCode();
        }

        #endregion
    }

}