using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuthorization.Nhibernate.Models
{
    public class StorageItem
    {
        public virtual string Id { get; set; }
        public virtual StorageType Type { get; set; }
        public virtual string Data { get; set; }
    }

    public enum StorageType
    {
        AccessAuthorization,
        ConditionAuthorization,
        SecurityIdentity,
        SecurityIdentityRelation,
        SecurityItem,
        SecurityItemRelation,
        Bag
    }
}
