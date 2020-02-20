using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface ICheckAccessResult
    {
        AccessType AccessType { get; }
        IEnumerable<ISecurityIdentity> DelegatedFrom { get; }
    }

    public static class CheckAccessResultExtensions
    {
        public static bool HasAccess(this ICheckAccessResult checkAccessResult)
        {
            return checkAccessResult.AccessType == AccessType.Allow;
        }
    }
}