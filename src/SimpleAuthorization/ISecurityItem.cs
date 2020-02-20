using System.Collections.Generic;
using SimpleAuthorization.Engine;

namespace SimpleAuthorization
{
    public interface ISecurityItem:  IBagObject
    {
        ISecurityStore Store { get; }
        ICollection<ISecurityItem> Children { get; }
        ICollection<ISecurityItem> Parents { get; }

    }
}