using SimpleAuthorization.Activator;

namespace SimpleAuthorization
{
    public interface IAccessAuthorization:IAuthorization
    {
        AccessType AccessType { get; set; }
    }
}