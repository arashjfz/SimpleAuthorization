using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleAuthorization.Activator
{
    internal class SecurityCollection<T>:ICollection<T>, INotifyCollectionChanged,IReadOnlyCollection<T>
    {
        private HashSet<T> _set = new HashSet<T>();
        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            if(_set.Contains(item))
                return;
            _set.Add(item);
            T[] newItems = {item};
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
        }

        public void Clear()
        {
            IList<T> oldItems = new List<T>(_set);
            _set.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, new T[0], oldItems));
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _set.CopyTo(array,arrayIndex);
        }

        public bool Remove(T item)
        {
            T[] oldItems = { item };

            bool remove = _set.Remove(item);
            if (remove)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new T[0], oldItems));
            return remove;
        }

        public int Count => _set.Count;
        public bool IsReadOnly => false;

        #endregion

        #region Implementation of INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public SecurityCollection<T> RegisterCollectionNotifyChanged(
            NotifyCollectionChangedEventHandler notifyCollectionChanged)
        {
            CollectionChanged += notifyCollectionChanged;
            return this;
        }
    }
}