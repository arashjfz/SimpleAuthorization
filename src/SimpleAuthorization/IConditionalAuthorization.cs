using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface IConditionalAuthorization : IAuthorization
    {
        IEnumerable<ICondition> Conditions { get; }
    }
}