using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Store
{
    public class AuthorizationsChangedEventArgs : EventArgs
    {
        public IEnumerable<IAuthorization> AddedItems { get; }
        public IEnumerable<IAuthorization> RemovedItems { get; }

        public AuthorizationsChangedEventArgs(IEnumerable<IAuthorization> addedItems, IEnumerable<IAuthorization> removedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
        }
    }
}