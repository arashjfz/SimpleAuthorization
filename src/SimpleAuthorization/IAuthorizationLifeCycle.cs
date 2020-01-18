using System;

namespace SimpleAuthorization
{
    public interface IAuthorizationLifeCycle
    {
        bool IsValidNow();
    }
}
