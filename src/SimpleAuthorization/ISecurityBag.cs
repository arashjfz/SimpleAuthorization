using System.Collections.Generic;

namespace SimpleAuthorization
{
    public interface ISecurityBag 
    {
        string this[string key] { get; set; }
        ICollection<string> Keys { get; }

        ICollection<string> Values { get; }

        void Add(string key, string value);

        bool ContainsKey(string key);

        bool Remove(string key);

        bool TryGetValue(string key, out string value);
    }
}