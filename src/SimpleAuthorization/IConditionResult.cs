using System.Collections.Generic;
using System.Linq;

namespace SimpleAuthorization
{
    public interface IConditionResult
    {
        IReadOnlyDictionary<IConditionalAuthorization, IEnumerable<IAccessCondition>> Conditions { get; }
    }

    public static class ConditionResultExtensions
    {
        public static IEnumerable<IAccessCondition> GetAllConditions(this IConditionResult conditionResult)
        {
            return conditionResult.Conditions.Values.SelectMany(c => c);
        }
    }
}