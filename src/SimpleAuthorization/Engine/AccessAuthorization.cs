using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleAuthorization.Engine
{
    internal class AccessAuthorization:IAccessAuthorization,INotifyPropertyChanged
    {
        private ISecurityItem _securityItem;
        private ISecurityIdentity _securityIdentity;
        private IAuthorizationLifeTime _lifeTime;
        private AccessType _accessType;

        public AccessAuthorization(ISecurityStore store)
        {
            Store = store;
            Bag = new SecurityBag();
        }
        #region Implementation of IBagObject

        public ISecurityBag Bag { get; }

        #endregion

        #region Implementation of IAuthorization

        public ISecurityStore Store { get; }

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

        public ISecurityIdentity DelegatedBy { get; set; }

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