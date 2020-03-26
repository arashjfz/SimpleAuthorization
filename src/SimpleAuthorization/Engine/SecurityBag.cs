using System;
using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal class SecurityBag : ISecurityBag
    {
        private readonly Dictionary<string, string> _map = new Dictionary<string, string>();
        #region Implementation of ISecurityBag

        public string this[string key]
        {
            get => _map[key];
            set
            {
                bool contains = _map.TryGetValue(key,out string oldValue);
                _map[key] = value;
                if(contains)
                    OnRemoved(new SecurityBagEventArgs(key, oldValue));
                OnAdded(new SecurityBagEventArgs(key,value));
            }
        }

        public ICollection<string> Keys => _map.Keys;

        public ICollection<string> Values => _map.Values;

        public void Add(string key, string value)
        {
            _map.Add(key, value);
            OnAdded(new SecurityBagEventArgs(key,value));
        }

        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            _map.TryGetValue(key, out string value);
            bool result = _map.Remove(key);
            if(result)
                OnRemoved(new SecurityBagEventArgs(key,value));
            return result;
        }

        public bool TryGetValue(string key, out string value)
        {
            return _map.TryGetValue(key, out value);
        }

        #endregion

        public event EventHandler<SecurityBagEventArgs> Added;
        public event EventHandler<SecurityBagEventArgs> Removed;

        private void OnAdded(SecurityBagEventArgs e)
        {
            Added?.Invoke(this, e);
        }

        private void OnRemoved(SecurityBagEventArgs e)
        {
            Removed?.Invoke(this, e);
        }
    }

    public class SecurityBagEventArgs : EventArgs
    {
        public string Key { get; }
        public string Value { get; }

        public SecurityBagEventArgs(string key,string value)
        {
            Key = key;
            Value = value;
        }
    }

}