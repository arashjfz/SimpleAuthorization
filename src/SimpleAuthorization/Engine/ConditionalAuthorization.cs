using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleAuthorization.Engine
{
    internal class ConditionalAuthorization:IConditionalAuthorization,INotifyPropertyChanged
    {
        private ISecurityItem _securityItem;
        private ISecurityIdentity _securityIdentity;
        private IAuthorizationLifeTime _lifeTime;
        private ISecurityIdentity _delegatedBy;

        public ConditionalAuthorization(ISecurityStore store)
        {
            Store = store;
            Bag = new SecurityBag();
            Conditions = new ObservableCollection<IAccessCondition>();
            ((INotifyCollectionChanged)Conditions).CollectionChanged+=ConditionsChanged;
        }

        private void ConditionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
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

        public ISecurityIdentity DelegatedBy
        {
            get => _delegatedBy;
            set
            {
                if (Equals(value, _delegatedBy)) return;
                _delegatedBy = value;
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