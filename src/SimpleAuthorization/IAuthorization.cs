using SimpleAuthorization.Activator;

namespace SimpleAuthorization
{
    public interface IAuthorization: IBagObject
    {
        ISecurityStore Store { get; }
        ISecurityItem SecurityItem { get; set; }
        ISecurityIdentity SecurityIdentity { get; set; }
        IAuthorizationLifeTime LifeTime { get; set; }
        ISecurityIdentity DelegatedBy { get; set; }
    }
}