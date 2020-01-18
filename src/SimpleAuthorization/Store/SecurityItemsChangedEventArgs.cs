using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public class SecurityItemsChangedEventArgs : EventArgs
    {
        public IEnumerable<ISecurityItem> AddedItems { get; }
        public IEnumerable<ISecurityItem> RemovedItems { get; }

        public SecurityItemsChangedEventArgs(IEnumerable<ISecurityItem> addedItems, IEnumerable<ISecurityItem> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}