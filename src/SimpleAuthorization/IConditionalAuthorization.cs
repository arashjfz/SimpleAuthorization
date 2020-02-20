using System.Collections.Generic;
using SimpleAuthorization.Activator;

namespace SimpleAuthorization
{
    public interface IConditionalAuthorization : IAuthorization
    {
        IList<IAccessCondition> Conditions { get; }
    }
}