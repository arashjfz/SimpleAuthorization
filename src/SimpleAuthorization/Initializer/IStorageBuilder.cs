using SimpleAuthorization.Storage;

namespace SimpleAuthorization.Initializer
{
    public interface IStorageBuilder
    {
        ISecurityStorage Build();
    }
}