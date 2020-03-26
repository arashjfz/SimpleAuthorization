using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleAuthorization.Engine
{
    internal class ConditionalAuthorization:IConditionalAuthorization,INotifyPropertyChanged
    {
        public string Id { get; }
        private ISecurityItem _securityItem;
        private ISecurityIdentity _securityIdentity;
        private IAuthorizationLifeTime _lifeTime;
        private readonly SecurityStore _store;
        private readonly SecurityBag _bag;

        public ConditionalAuthorization(SecurityStore store,string id)
        {
            Id = id;
            _store = store;
            _bag = new SecurityBag();
            Conditions = new ObservableCollection<IAccessCondition>();
            ((INotifyCollectionChanged)Conditions).CollectionChanged+=ConditionsChanged;
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


        private void ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        #region Implementation of IBagObject

        public ISecurityBag Bag => _bag;

        #endregion

        #region Implementation of IAuthorization

        public ISecurityStore Store => _store;

        public ISecurityItem SecurityItem
        {
            get => _securityItem;
            set
            {
                if (Equals(value, _securityItem)) return;
                _securityItem = value;
                OnPropertyChanged();
            }
        }

        public ISecurityIdentity SecurityIdentity
        {
            get => _securityIdentity;
            set
            {
                if (Equals(value, _securityIdentity)) return;
                _securityIdentity = value;
                OnPropertyChanged();
            }
        }

        public IAuthorizationLifeTime LifeTime
        {
            get => _lifeTime;
            set
            {
                if (Equals(value, _lifeTime)) return;
                _lifeTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Implementation of IConditionalAuthorization

        public IList<IAccessCondition> Conditions { get; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}