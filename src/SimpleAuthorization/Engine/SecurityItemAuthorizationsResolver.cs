using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthorization.Engine
{
    internal class SecurityItemAuthorizationsResolver : ISecurityItemAuthorizationsResolver
    {
        private readonly SecurityStore _store;
        private readonly ISecurityItem _securityItem;

        public SecurityItemAuthorizationsResolver(SecurityStore store, ISecurityItem securityItem)
        {
            _store = store;
            _securityItem = securityItem;
        }
        #region Implementation of ISecurityItemAuthorizationsResolver

        public IEnumerable<IAuthorization> GetAuthorizations()
        {
            foreach (ISecurityItem item in Enumerable.Repeat(_securityItem, 1).Concat(_securityItem.GetAllAncestors()))
            foreach (IAuthorization authorization in _store.GetSecurityItemAuthorizations(item))
                yield return authorization;
        }

        #endregion
    }
}