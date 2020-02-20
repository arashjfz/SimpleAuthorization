using System.Collections.Generic;
using SimpleAuthorization.Activator;

namespace SimpleAuthorization
{
    public interface ISecurityIdentity: IBagObject
    {
        ISecurityStore Store { get; }
        bool IsActive { get; set; }
        ICollection<ISecurityIdentity> Children { get; set; }
        ICollection<ISecurityIdentity> Parents { get; set; }
    }
}