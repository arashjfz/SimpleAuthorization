using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface ISecurityItem:  IBagObject
    {
        ISecurityStore Store { get; }
        ICollection<ISecurityItem> Children { get; }
        ICollection<ISecurityItem> Parents { get; }

    }
}