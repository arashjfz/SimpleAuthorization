using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleAuthorization
{
    public static class Extensions
    {
        public static IEnumerable<ISecurityIdentity> GetAllAncestors(this ISecurityIdentity securityIdentity)
        {
            return InternalGetAllAncestors(securityIdentity).Distinct();
        }

        private static IEnumerable<ISecurityIdentity> InternalGetAllAncestors(this ISecurityIdentity securityIdentity)
        {
            foreach (ISecurityIdentity securityIdentityParent in securityIdentity.Parents)
            {
                yield return securityIdentityParent;
                foreach (ISecurityIdentity parentOfParent in securityIdentityParent.InternalGetAllAncestors())
                    yield return parentOfParent;
            }
        }
        public static IEnumerable<ISecurityItem> GetAllAncestors(this ISecurityItem securityItem)
        {
            return InternalGetAllAncestors(securityItem).Distinct();
        }

        private static IEnumerable<ISecurityItem> InternalGetAllAncestors(this ISecurityItem securityItem)
        {
            foreach (ISecurityItem parent in securityItem.Parents)
            {
                yield return parent;
                foreach (ISecurityItem parentOfParent in parent.InternalGetAllAncestors())
                    yield return parentOfParent;
            }
        }
    }
}
