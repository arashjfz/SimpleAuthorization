using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public class SecurityIdentitiesChangedEventArgs : EventArgs
    {
        public IEnumerable<ISecurityIdentity> AddedItems { get; }
        public IEnumerable<ISecurityIdentity> RemovedItems { get; }

        public SecurityIdentitiesChangedEventArgs(IEnumerable<ISecurityIdentity> addedItems, IEnumerable<ISecurityIdentity> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}