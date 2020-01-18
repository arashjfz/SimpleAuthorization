using System;

namespace SimpleAuthorization
{
    public interface ISecurityIdentity
    {
        Guid Key { get;  }
    }
}