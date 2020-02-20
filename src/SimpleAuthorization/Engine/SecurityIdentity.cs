using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleAuthorization.Engine
{
    internal class SecurityIdentity:ISecurityIdentity
    {
        public SecurityIdentity(ISecurityStore store)
        {
            Store = store;
            Bag = new SecurityBag();
            Children = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(ChildrenChanged);
            Parents = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(ParentsChanged);
        }

        private void ParentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityIdentity parent = (ISecurityIdentity)e.NewItems[0];
                parent.Children.Add(this);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityIdentity parent = (ISecurityIdentity)e.OldItems[0];
                parent.Children.Remove(this);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityIdentity parent in e.OldItems)
                {
                    parent.Children.Remove(this);
                }
            }
        }

        private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityIdentity child = (ISecurityIdentity)e.NewItems[0];
                child.Parents.Add(this);
            }
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityIdentity child = (ISecurityIdentity)e.OldItems[0];
                child.Parents.Remove(this);
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityIdentity child in e.OldItems)
                {
                    child.Parents.Remove(this);
                }
            }

        }

        #region Implementation of IBagObject

        public ISecurityBag Bag { get; }

        #endregion

        #region Implementation of ISecurityIdentity

        public ISecurityStore Store { get; }
        public bool IsActive { get; set; }
        public ICollection<ISecurityIdentity> Children { get; set; }
        public ICollection<ISecurityIdentity> Parents { get; set; }

        #endregion
    }
}