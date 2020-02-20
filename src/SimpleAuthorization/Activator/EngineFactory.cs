﻿namespace SimpleAuthorization.Activator
{
    public class EngineFactory
    {
        public ISecurityEngine CreateEngine()
        {
            return new SecurityEngine();
        }
    }

    public static class ActivationExtensions
    {
        public static ISecurityItem AddSecurityItem(this ISecurityStore store)
        {
            SecurityStore securityStore = (SecurityStore) store;
            SecurityItem securityItem = new SecurityItem(store);
            securityStore.SecurityItems.Add(securityItem);
            return securityItem;
        }
        public static ISecurityIdentity AddSecurityIdentity(this ISecurityStore store)
        {
            SecurityStore securityStore = (SecurityStore)store;
            SecurityIdentity securityIdentity = new SecurityIdentity(store);
            securityStore.SecurityIdentities.Add(securityIdentity);
            return securityIdentity;
        }

        public static IAccessAuthorization AccessAuthorize(this ISecurityStore store,ISecurityIdentity securityIdentity,ISecurityItem securityItem)
        {
            SecurityStore securityStore = (SecurityStore)store;
            AccessAuthorization accessAuthorization = new AccessAuthorization(store)
                {SecurityIdentity = securityIdentity, SecurityItem = securityItem};
            securityStore.Authorizations.Add(accessAuthorization);
            return accessAuthorization;
        }
        public static IConditionalAuthorization ConditionalAuthorize(this ISecurityStore store,ISecurityIdentity securityIdentity,ISecurityItem securityItem)
        {
            SecurityStore securityStore = (SecurityStore)store;
            ConditionalAuthorization conditionalAuthorization = new ConditionalAuthorization(store)
                {SecurityIdentity = securityIdentity, SecurityItem = securityItem};
            securityStore.Authorizations.Add(conditionalAuthorization);
            return conditionalAuthorization;
        }

        public static T AddBagItem<T>(this T bagObject, string key, object value) where T : IBagObject
        {
            bagObject.Bag.Add(key,value);
            return bagObject;
        }
    }
}
