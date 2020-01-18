using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface IAuthorization
    {
        IAuthorisableItem AuthorisableItem { get; }
        ISecurityItem SecurityItem { get; }
        IAuthorizationLifeCycle LifeCycle { get; }
        ISecurityIdentity DelegatedBy { get; }
        AuthorizationType Type { get; }
        IEnumerable<ICondition> Conditions { get; }
    }

    public enum AuthorizationType
    {
        Allow,
        Deny,
        Nutral
    }
}