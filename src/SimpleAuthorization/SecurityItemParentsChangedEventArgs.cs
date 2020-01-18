using System;
using System.Collections.Generic;

namespace SimpleAuthorization
{
    public class SecurityItemParentsChangedEventArgs : EventArgs
    {
        public IEnumerable<ISecurityItem> AddedParents { get; }
        public IEnumerable<ISecurityItem> RemovedParents { get; }

        public SecurityItemParentsChangedEventArgs(IEnumerable<ISecurityItem> addedParents,IEnumerable<ISecurityItem> removedParents)
        {
            AddedParents = addedParents;
            RemovedParents = removedParents;
        }
    }
}