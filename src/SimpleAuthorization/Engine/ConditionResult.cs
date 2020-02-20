using System.Collections.Generic;

namespace SimpleAuthorization.Engine
{
    internal class ConditionResult: IConditionResult
    {
        public ConditionResult(Dictionary<IConditionalAuthorization, IEnumerable<IAccessCondition>> conditions)
        {
            Conditions = conditions;
        }

        #region Implementation of IConditionResult

        public IReadOnlyDictionary<IConditionalAuthorization, IEnumerable<IAccessCondition>> Conditions { get; }

        #endregion
    }
}