using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleAuthorization.Engine
{
    internal class AccessAuthorization:IAccessAuthorization,INotifyPropertyChanged
    {
        public string Id { get; }
        private ISecurityItem _securityItem;
        private ISecurityIdentity _securityIdentity;
        private IAuthorizationLifeTime _lifeTime;
        private AccessType _accessType;
        private readonly SecurityStore _store;
        private readonly SecurityBag _bag;

        public AccessAuthorization(SecurityStore store,string id)
        {
            Id = id;
            _store = store;
            _bag = new SecurityBag();
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

        #region Implementation of IAccessAuthorization

        public AccessType AccessType
        {
            get => _accessType;
            set
            {
                if (value == _accessType) return;
                _accessType = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}