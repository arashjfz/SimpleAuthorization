using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface IConditionResult
    {
        IReadOnlyDictionary<ISecurityIdentity, IEnumerable<IAccessCondition>> DelegatedConditions { get; }
        IEnumerable<IAccessCondition> SelfConditions { get; }
    }

    public static class ConditionResultExtensions
    {
        public static IEnumerable<IAccessCondition> Conditions(this IConditionResult conditionResult)
        {
            foreach (var pair in conditionResult.DelegatedConditions)
                foreach (IAccessCondition accessCondition in pair.Value)
                    yield return accessCondition;
            foreach (IAccessCondition accessCondition in conditionResult.SelfConditions)
                yield return accessCondition;
        }
    }
}