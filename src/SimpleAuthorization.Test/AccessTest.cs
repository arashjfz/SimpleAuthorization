using System.Diagnostics;
using SimpleAuthorization.Activator;
using Xunit;

namespace SimpleAuthorization.Test
{
    public class AccessTest
    {
        private const string Name = "name";
        [Fact]
        public void Engine_Instantiate()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            ISecurityItem operation1 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation1");
            ISecurityItem operation2 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation2");
            ISecurityItem task = engine.Store.AddSecurityItem().AddBagItem(Name, "Task");
            task.Children.Add(operation1);
            task.Children.Add(operation2);
            ISecurityIdentity user1 = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user1");
            ISecurityIdentity user2 = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user2");
            ISecurityIdentity group = engine.Store.AddSecurityIdentity().AddBagItem(Name, "group");
            group.Children.Add(user1);
            group.Children.Add(user2);
            IAccessAuthorization user1ToOperationAccessAuthorization = engine.Store.AccessAuthorize(user1, operation1);
            user1ToOperationAccessAuthorization.DelegatedBy = user2;
            ICheckAccessResult checkAccessResult = engine.CheckAccess(user1, operation1);
        }
    }
}
