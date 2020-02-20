using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Castle.DynamicProxy;

namespace SimpleAuthorization.DynamicObjectGraph
{
    public static class DynamicInterfaceActivator
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();
        private static readonly InterfaceInterceptor InterfaceInterceptor = new InterfaceInterceptor();
        public static object Activate(Type interfaceType)
        {
            CustomInstantiateAttribute customInstantiateAttribute = interfaceType.GetCustomAttribute<CustomInstantiateAttribute>();
            if (customInstantiateAttribute != null)
                return customInstantiateAttribute.Instantiate();
            return ProxyGenerator.CreateClassProxy(typeof(ClassToProxy), new []{ interfaceType }, InterfaceInterceptor);
        }
    }
    [AttributeUsage(AttributeTargets.Interface|AttributeTargets.Property,AllowMultiple = false)]
    public abstract class CustomInstantiateAttribute : Attribute
    {
        public abstract object Instantiate();
    }
    public class ClassToProxy
    {
        private  Dictionary<string, object> _properties;
        internal Dictionary<string, object> Properties => _properties??(_properties = new Dictionary<string, object>());
    }
    public class InterfaceInterceptor:IInterceptor
    {
        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name.StartsWith("get_"))
            {
                PropertyInfo propertyInfo = invocation.Method.DeclaringType?.GetProperty(invocation.Method.Name.Remove(0, 4));
                if (propertyInfo != null)
                {
                    ClassToProxy classToProxy = (ClassToProxy)invocation.Proxy;
                    if (classToProxy.Properties.TryGetValue(propertyInfo.Name, out object result))
                    {
                        invocation.ReturnValue = result;
                    }
                    else
                    {
                        if (!propertyInfo.CanWrite && propertyInfo.PropertyType.IsInterface)
                        {
                            CustomInstantiateAttribute customInstantiateAttribute = propertyInfo.GetCustomAttribute<CustomInstantiateAttribute>();
                            if (customInstantiateAttribute != null)
                                result =  customInstantiateAttribute.Instantiate();
                            else
                                result = DynamicInterfaceActivator.Activate(propertyInfo.PropertyType);
                            classToProxy.Properties[propertyInfo.Name] = result;
                            invocation.ReturnValue = result;
                        }
                    }
                    
                }
            }
            else if (invocation.Method.Name.StartsWith("set_"))
            {
                PropertyInfo propertyInfo = invocation.Method.DeclaringType?.GetProperty(invocation.Method.Name.Remove(0, 4));
                if (propertyInfo != null)
                {
                    ClassToProxy classToProxy = (ClassToProxy)invocation.InvocationTarget;
                    classToProxy.Properties[propertyInfo.Name] = invocation.Arguments[0];
                }
            }
        }

        #endregion
    }

}
