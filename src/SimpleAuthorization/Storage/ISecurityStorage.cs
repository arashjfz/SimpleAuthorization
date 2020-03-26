using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthorization.Storage
{
    public interface ISecurityStorage
    {
        void AddItems(IEnumerable<IStorageEntity> items);
        void RemoveItems(IEnumerable<IStorageEntity> items);
        void UpdateItems(IEnumerable<IStorageEntity> items);
        IEnumerable GetItems(Type entityType);
        void ApplyBulkActions(IEnumerable<IStorageAction> actions);
    }

    public static class SecurityStorageExtensions
    {
        public static IEnumerable<T> GetItems<T>(this ISecurityStorage storage) where T : IStorageEntity
        {
            return storage.GetItems(typeof(T)).Cast<T>();
        }

        public static string GetIdentity(this IStorageEntity storageEntity)
        {
            switch (storageEntity)
            {
                case IStorageAccessAuthorization storageAccessAuthorization:
                    return storageAccessAuthorization.Id;
                case IStorageBag storageBag:
                    return $"{storageBag.TargetId}_{storageBag.Key}";
                case IStorageConditionAuthorization storageConditionAuthorization:
                    return storageConditionAuthorization.Id;
                case IStorageSecurityIdentity storageSecurityIdentity:
                    return storageSecurityIdentity.Id;
                case IStorageSecurityIdentityRelation storageSecurityIdentityRelation:
                    return
                        $"{storageSecurityIdentityRelation.ParentId}_{storageSecurityIdentityRelation.SecurityIdentityId}";
                case IStorageSecurityItem storageSecurityItem:
                    return storageSecurityItem.Id;
                case IStorageSecurityItemRelation storageSecurityItemRelation:
                    return $"{storageSecurityItemRelation.ParentId}_{storageSecurityItemRelation.SecurityItemId}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public enum StorageActionType
    {
        Add,
        Remove,
        Update
    }
    public interface IStorageAction
    {
        StorageActionType StorageActionType { get; }
        IEnumerable<IStorageEntity> Entities { get; }
    }
}
