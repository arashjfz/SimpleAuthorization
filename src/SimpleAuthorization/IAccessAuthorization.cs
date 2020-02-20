using SimpleAuthorization.Engine;

namespace SimpleAuthorization
{
    public interface IAccessAuthorization:IAuthorization
    {
        AccessType AccessType { get; set; }
    }
}