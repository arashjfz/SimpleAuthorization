using SimpleAuthorization.Engine;

namespace SimpleAuthorization
{
    public interface IAuthorization: IBagObject
    {
        ISecurityStore Store { get; }
        ISecurityItem SecurityItem { get;  }
        ISecurityIdentity SecurityIdentity { get; }
        IAuthorizationLifeTime LifeTime { get; set; }
    }
}