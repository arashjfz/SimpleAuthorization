using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal interface ISecurityItemAuthorizationsResolver
    {
        IEnumerable<IAuthorization> GetAuthorizations();
    }
}