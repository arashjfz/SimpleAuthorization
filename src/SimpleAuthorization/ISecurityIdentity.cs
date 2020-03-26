using System.Collections.Generic;

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