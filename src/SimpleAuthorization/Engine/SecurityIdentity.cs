using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleAuthorization.Engine
{
    internal class SecurityIdentity:ISecurityIdentity
    {
        private readonly SecurityStore _store;
        private readonly SecurityBag _bag;

        public SecurityIdentity(SecurityStore store, string id)
        {
            _store = store;
            Id = id;
            _bag = new SecurityBag();
            Children = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(ChildrenChanged);
            Parents = new SecurityCollection<ISecurityIdentity>().RegisterCollectionNotifyChanged(ParentsChanged);
            _bag.Added += BagOnAdded;
            _bag.Removed += BagOnRemoved;
        }
        private void BagOnRemoved(object sender, SecurityBagEventArgs e)
        {
            _store.OnBagRemoved(this, e.Key, e.Value);
        }

        private void BagOnAdded(object sender, SecurityBagEventArgs e)
        {
            _store.OnBagAdded(this, e.Key, e.Value);
        }
        private void ParentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityIdentity parent = (ISecurityIdentity)e.NewItems[0];
                parent.Children.Add(this);
                _store.OnSecurityIdentityRelationAdded(parent,this);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityIdentity parent = (ISecurityIdentity)e.OldItems[0];
                parent.Children.Remove(this);
                _store.OnSecurityIdentityRelationRemoved(parent, this);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityIdentity parent in e.OldItems)
                {
                    parent.Children.Remove(this);
                    _store.OnSecurityIdentityRelationRemoved(parent, this);
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

        public ISecurityBag Bag => _bag;

        #endregion

        #region Implementation of ISecurityIdentity

        public string Id { get; }
        public ISecurityStore Store => _store;

        public bool IsActive { get; set; }
        public ICollection<ISecurityIdentity> Children { get; set; }
        public ICollection<ISecurityIdentity> Parents { get; set; }

        #endregion
    }
}