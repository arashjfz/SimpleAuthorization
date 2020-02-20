using System.Collections.Generic;
using SimpleAuthorization.Engine;

namespace SimpleAuthorization
{
    public interface IConditionalAuthorization : IAuthorization
    {
        IList<IAccessCondition> Conditions { get; }
    }
}