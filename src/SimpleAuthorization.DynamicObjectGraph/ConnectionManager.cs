using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SimpleAuthorization.DynamicObjectGraph
{
    public class ConnectionManager
    {
        private readonly Bag _bag;
        private readonly HashSet<object> _attachedObject = new HashSet<object>();
        public ConnectionManager(object rootObject)
        {
            _bag = new Bag(this);
            AttachToObject(rootObject, null);
        }
        private void AttachToObject(object obj, PropertyInfo byProperty)
        {
            if (_attachedObject.Contains(obj))
                return;
            _attachedObject.Add(obj);
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
                TryConnectOrDisconnect(obj, propertyInfo, propertyInfo.GetValue(obj));
            if (obj is INotifyPropertyChanged notifyPropertyChanged)
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChangedOnPropertyChanged;
            if (obj is INotifyCollectionChanged notifyCollectionChanged && byProperty != null)
                notifyCollectionChanged.CollectionChanged += (sender, e) => NotifyCollectionChangedOnCollectionChanged(sender, e, byProperty);
        }
        private void NotifyCollectionChangedOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, PropertyInfo byProperty)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object newItem in e.NewItems)
                        TryConnectOrDisconnect(sender, byProperty, newItem);
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                    foreach (object oldItem in e.OldItems)
                        TryDisconnect(sender, byProperty, oldItem);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Reset:
                    foreach (object oldItem in e.OldItems)
                        TryDisconnect(sender, byProperty, oldItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void NotifyPropertyChangedOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo propertyInfo = sender.GetType().GetProperty(e.PropertyName);
            if (propertyInfo == null)
                return;
            TryConnectOrDisconnect(sender, propertyInfo, propertyInfo.GetValue(sender));
        }
        private void TryDisconnect(object host, PropertyInfo property, object connectedObject)
        {
            ObjectConnection objectConnection = new ObjectConnection(host, property, connectedObject);
            if (_bag.RemoveByConnectionObject(objectConnection))
                NotifyConnectionChange(objectConnection, ChangeType.Disconnected, Enumerable.Empty<IObjectConnection>(), new HashSet<object>());
        }
        private void TryConnectOrDisconnect(object host, PropertyInfo property, object connectedObject)
        {
            if (connectedObject == null)
            {
                foreach (IObjectConnection objectConnection in _bag.RemoveByHostAndProperty(host, property))
                    NotifyConnectionChange(objectConnection, ChangeType.Disconnected, Enumerable.Empty<IObjectConnection>(), new HashSet<object>());
            }
            else
            {
                if (connectedObject is INotifyCollectionChanged)
                    return;
                ObjectConnection connection = new ObjectConnection(host, property, connectedObject);
                if (!_bag.FindConnectionsByHostAndProperty(host, property).Contains(connection))
                {
                    _bag.AddConnection(connection);
                    NotifyConnectionChange(connection, ChangeType.Connected, Enumerable.Empty<IObjectConnection>(), new HashSet<object>());
                }

                if (connectedObject.GetType().GetCustomAttribute<AttachableObjectAttribute>(true) != null)
                    AttachToObject(connectedObject, property);
            }
        }
        private void NotifyConnectionChange(IObjectConnection objectConnection, ChangeType changeType, IEnumerable<IObjectConnection> chainSet, HashSet<object> traversedHosts)
        {
            if (traversedHosts.Contains(objectConnection.Host))
                return;
            traversedHosts.Add(objectConnection.Host);
            if (objectConnection.Host is IObjectChainChangeAware objectChainChangeAware)
                objectChainChangeAware.Changed(new ObjectChangeContext(chainSet, changeType, Enumerable.Repeat(objectConnection,1)));
            foreach (IObjectConnection connection in _bag.FindConnectionsByConnectedObject(objectConnection.Host))
                NotifyConnectionChange(objectConnection, changeType,chainSet.Concat(Enumerable.Repeat(connection,1)),traversedHosts);
        }
        private class ObjectChangeContext : IObjectChangeContext
        {
            public ObjectChangeContext(IEnumerable<IObjectConnection> chainSet, ChangeType changeType, IEnumerable<IObjectConnection> connections)
            {
                ChainSet = chainSet;
                ChangeType = changeType;
                Connections = connections;
            }

            #region Implementation of IObjectChangeContext

            public IEnumerable<IObjectConnection> ChainSet { get; }
            public ChangeType ChangeType { get; }
            public IEnumerable<IObjectConnection> Connections { get; }

            #endregion
        }
        private class ObjectConnection : IObjectConnection, IEquatable<IObjectConnection>
        {
            public ObjectConnection(object host, PropertyInfo property, object connectedObject)
            {
                Host = host;
                Property = property;
                ConnectedObject = connectedObject;
            }

            #region Implementation of IObjectConnection

            public object Host { get; }
            public PropertyInfo Property { get; }
            public object ConnectedObject { get; }

            #endregion

            #region Equality members

            protected bool Equals(ObjectConnection other)
            {
                return Equals(Host, other.Host) && Equals(Property, other.Property) && Equals(ConnectedObject, other.ConnectedObject);
            }

            public bool Equals(IObjectConnection other)
            {
                if (other == null)
                    return false;
                return Equals(Host, other.Host) && Equals(Property, other.Property) && Equals(ConnectedObject, other.ConnectedObject);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ObjectConnection)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Host != null ? Host.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Property != null ? Property.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (ConnectedObject != null ? ConnectedObject.GetHashCode() : 0);
                    return hashCode;
                }
            }

            #endregion
        }
        private class Bag
        {
            private readonly ConnectionManager _connectionManager;
            private readonly ConnectedObjectBag _connectedObjectBag = new ConnectedObjectBag();
            private readonly HostBag _hostBag = new HostBag();

            public Bag(ConnectionManager connectionManager)
            {
                _connectionManager = connectionManager;
            }
            public ICollection<IObjectConnection> FindConnectionsByConnectedObject(object connectedObject)
            {
                _connectedObjectBag.TryGetValue(connectedObject, out ICollection<IObjectConnection> result);
                if (result == null)
                    return new List<IObjectConnection>();
                return result;
            }
            public IEnumerable<IObjectConnection> FindConnectionsByHost(object hostObject)
            {
                _hostBag.TryGetValue(hostObject, out IDictionary<PropertyInfo, ICollection<IObjectConnection>> result);
                if (result == null)
                    return Enumerable.Empty<IObjectConnection>();
                return result.Values.SelectMany(c => c);
            }
            public ICollection<IObjectConnection> FindConnectionsByHostAndProperty(object hostObject, PropertyInfo property)
            {
                _hostBag.TryGetValue(hostObject, out IDictionary<PropertyInfo, ICollection<IObjectConnection>> result);
                if (result == null)
                    return new List<IObjectConnection>();
                result.TryGetValue(property, out ICollection<IObjectConnection> objectCollections);
                if (objectCollections == null)
                    return new List<IObjectConnection>();
                return objectCollections;
            }

            public IEnumerable<IObjectConnection> RemoveByHostAndProperty(object hostObject, PropertyInfo property)
            {
                foreach (IObjectConnection objectConnection in FindConnectionsByHostAndProperty(hostObject, property))
                {
                    _connectedObjectBag.TryGetValue(objectConnection.ConnectedObject, out ICollection<IObjectConnection> objectConnections);
                    objectConnections?.Remove(objectConnection);
                    yield return objectConnection;
                }
                _hostBag.TryGetValue(hostObject,
                    out IDictionary<PropertyInfo, ICollection<IObjectConnection>> items);
                items?.Remove(property);
            }

            public bool RemoveByConnectionObject(IObjectConnection connectionObject)
            {
                FindConnectionsByHostAndProperty(connectionObject.Host, connectionObject.Property)
                    .Remove(connectionObject);
                return FindConnectionsByConnectedObject(connectionObject.ConnectedObject).Remove(connectionObject);
            }
            public void AddConnection(IObjectConnection connection)
            {
                AddConnectionToConnectionBag(connection);
                AddConnectionToHostBag(connection);
            }

            private void AddConnectionToHostBag(IObjectConnection connection)
            {
                _hostBag.TryGetValue(connection.Host,
                    out IDictionary<PropertyInfo, ICollection<IObjectConnection>> items);
                if (items == null)
                {
                    items = new Dictionary<PropertyInfo, ICollection<IObjectConnection>>();
                    _hostBag[connection.Host] = items;
                }

                items.TryGetValue(connection.Property, out ICollection<IObjectConnection> objectConnections);
                if (objectConnections == null)
                {
                    objectConnections = new HashSet<IObjectConnection>();
                    items[connection.Property] = objectConnections;
                }

                objectConnections.Add(connection);
            }

            private void AddConnectionToConnectionBag(IObjectConnection connection)
            {
                _connectedObjectBag.TryGetValue(connection.ConnectedObject,
                    out ICollection<IObjectConnection> objectConnections);
                if (objectConnections == null)
                {
                    objectConnections = new HashSet<IObjectConnection>();
                    _connectedObjectBag[connection.ConnectedObject] = objectConnections;
                }

                objectConnections.Add(connection);
            }

            private class ConnectedObjectBag : Dictionary<object, ICollection<IObjectConnection>>
            {
            }

            private class HostBag : Dictionary<object, IDictionary<PropertyInfo, ICollection<IObjectConnection>>>
            {
            }
        }

    }
}