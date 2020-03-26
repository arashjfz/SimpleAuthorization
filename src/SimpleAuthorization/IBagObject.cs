namespace SimpleAuthorization
{
    public interface IBagObject
    {
        string Id { get; }
        ISecurityBag Bag { get;  }
    }
}