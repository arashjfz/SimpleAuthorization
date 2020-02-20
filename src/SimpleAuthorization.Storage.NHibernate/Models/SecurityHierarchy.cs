using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAuthorization.Storage.NHibernate.Models
{
    internal class SecurityHierarchy: ISecurityHierarchy
    {
        public SecurityHierarchy()
        {
            
        }

        public SecurityHierarchy(ISecurityHierarchy securityHierarchy)
        {
            SecurityItemKey = securityHierarchy.SecurityItemKey;
            SecurityItemParentKey = securityHierarchy.SecurityItemParentKey;
            Id = $"{SecurityItemKey}{SecurityItemParentKey}";
        }
        #region Implementation of ISecurityHierarchy

        public virtual string Id { get; set; }
        public virtual Guid SecurityItemKey { get; set; }
        public virtual Guid SecurityItemParentKey { get; set; }

        #endregion
    }
}
