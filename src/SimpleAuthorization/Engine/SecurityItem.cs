using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleAuthorization.Engine
{
    internal class SecurityItem:ISecurityItem
    {
        private readonly SecurityStore _store;
        private readonly SecurityBag _bag;

        public SecurityItem(SecurityStore store, string id)
        {
            _store = store;
            Id = id;
            _bag = new SecurityBag();
            Children = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(ChildrenChanged);
            Parents = new SecurityCollection<ISecurityItem>().RegisterCollectionNotifyChanged(ParentsChanged);
            _bag.Added += BagOnAdded;
            _bag.Removed += BagOnRemoved;
        }

        private void BagOnRemoved(object sender, SecurityBagEventArgs e)
        {
            _store.OnBagRemoved(this, e.Key, e.Value);
        }

        private void BagOnAdded(object sender, SecurityBagEventArgs e)
        {
            _store.OnBagAdded(this,e.Key,e.Value);
        }

        private void ParentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ISecurityItem parent = (ISecurityItem)e.NewItems[0];
                parent.Children.Add(this);
                _store.OnSecurityItemRelationAdded(parent,this);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ISecurityItem parent = (ISecurityItem)e.OldItems[0];
                parent.Children.Remove(this);
                _store.OnSecurityItemRelationRemoved(parent, this);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (ISecurityItem parent in e.OldItems)
                {
                    parent.Children.Remove(this);
                    _store.OnSecurityItemRelationRemoved(parent, this);
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

        public string Id { get; }
        public ISecurityStore Store => _store;

        public ICollection<ISecurityItem> Children { get; }
        public ICollection<ISecurityItem> Parents { get; }

        #endregion

        #region Implementation of IBagObject

        public ISecurityBag Bag => _bag;

        #endregion
    }
}