using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public class AuthorisableItemsChangedEventArgs : EventArgs
    {
        public IEnumerable<IAuthorisableItem> AddedItems { get; }
        public IEnumerable<IAuthorisableItem> RemovedItems { get; }

        public AuthorisableItemsChangedEventArgs(IEnumerable<IAuthorisableItem> addedItems, IEnumerable<IAuthorisableItem> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}