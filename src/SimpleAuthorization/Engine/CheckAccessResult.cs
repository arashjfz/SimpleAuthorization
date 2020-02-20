using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal class CheckAccessResult : ICheckAccessResult
    {
        public CheckAccessResult(AccessType accessType, IEnumerable<IAccessAuthorization> affectedAuthorizations)
        {
            AccessType = accessType;
            AffectedAuthorizations = affectedAuthorizations;
        }

        #region Implementation of ICheckAccessResult

        public AccessType AccessType { get; }
        public IEnumerable<IAccessAuthorization> AffectedAuthorizations { get; }

        #endregion
    }
}