using System;
using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface ISecurityItem
    {
        Guid Key { get; }
        IEnumerable<ISecurityItem> Parents { get; }
        IEnumerable<ISecurityItem> Children { get; }
    }

    public static class SecurityItemExtensions
    {
        public static IEnumerable<ISecurityItem> GetAllParents(this ISecurityItem securityItem)
        {
            if(securityItem == null)
                yield break;
            foreach (ISecurityItem parent in securityItem.Parents)
            {
                yield return parent;
                foreach (ISecurityItem ancestor in parent.GetAllParents())
                    yield return ancestor;
            }
        }
        public static IEnumerable<ISecurityItem> GetAllChildren(this ISecurityItem securityItem)
        {
            if (securityItem == null)
                yield break;
            foreach (ISecurityItem child in securityItem.Children)
            {
                yield return child;
                foreach (ISecurityItem grandChild in child.GetAllChildren())
                    yield return grandChild;
            }
        }

    }
}