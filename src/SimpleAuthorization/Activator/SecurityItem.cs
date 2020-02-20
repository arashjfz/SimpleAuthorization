using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleAuthorization.Activator
{
    internal class SecurityItem:ISecurityItem
    {
        public SecurityItem(ISecurityStore store)
        {
            Store = store;
            Bag = new SecurityBag();
            Children = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(ChildrenChanged);
            Parents = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(ParentsChanged);
        }

        private void ParentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityItem parent = (ISecurityItem)e.NewItems[0];
                parent.Children.Add(this);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityItem parent = (ISecurityItem)e.OldItems[0];
                parent.Children.Remove(this);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityItem parent in e.OldItems)
                {
                    parent.Children.Remove(this);
                }
            }
        }

        private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityItem child = (ISecurityItem)e.NewItems[0];
                child.Parents.Add(this);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityItem child = (ISecurityItem)e.OldItems[0];
                child.Parents.Remove(this);
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityItem child in e.OldItems)
                {
                    child.Parents.Remove(this);
                }
            }

        }

        #region Implementation of ISecurityItem

        public ISecurityStore Store { get; }
        public ICollection<ISecurityItem> Children { get; }
        public ICollection<ISecurityItem> Parents { get; }

        #endregion

        #region Implementation of IBagObject

        public ISecurityBag Bag { get; }

        #endregion
    }
}