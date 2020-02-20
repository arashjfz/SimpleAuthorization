using System;
using System.Collections.Generic;
using System.Reflection;

namespace SimpleAuthorization.DynamicObjectGraph
{
    public interface IObjectConnection
    {
        object Host { get;  }
        PropertyInfo Property { get; }
         object ConnectedObject { get; }
         
    }


    public interface IObjectChainChangeAware
    {
        void Changed(IObjectChangeContext context);
    }

    public interface IObjectChangeContext
    {
        IEnumerable<IObjectConnection> ChainSet { get; }
        ChangeType ChangeType { get; }
        IEnumerable<IObjectConnection> Connections { get; }
    }

    public enum ChangeType
    {
        Connected,
        Disconnected
    }
    public class ObjectConnectionAddedEventArgs : EventArgs
    {
        public IObjectConnection Connection { get; }

        public ObjectConnectionAddedEventArgs(IObjectConnection connection)
        {
            Connection = connection;
        }
    }
    public class ObjectConnectionRemovedEventArgs : EventArgs
    {
        public IObjectConnection Connection { get; }

        public ObjectConnectionRemovedEventArgs(IObjectConnection connection)
        {
            Connection = connection;
        }
    }

    public class AttachableObjectAttribute : Attribute
    {

    }
}