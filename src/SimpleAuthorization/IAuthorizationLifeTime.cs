using System;

namespace SimpleAuthorization
{
    public interface IAuthorizationLifeTime
    {
        bool IsActive(DateTime inTime);
    }
}