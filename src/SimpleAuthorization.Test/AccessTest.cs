using SimpleAuthorization.Engine;
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
        }

        [Fact]
        public void Operation_Access_Operation_Direct_To_User()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            ISecurityItem operation = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation");
            ISecurityItem hasNoAccessFromUserOperation = engine.Store.AddSecurityItem().AddBagItem(Name, "HasNoAccessFromUserOperation");
            ISecurityIdentity user = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user");
            engine.Store.AccessAuthorize(user, operation);

            ICheckAccessResult operationAccessResult = engine.CheckAccess(user, operation);
            ICheckAccessResult hasNoAccessFromUserOperationAccessResult = engine.CheckAccess(user, hasNoAccessFromUserOperation);


            Assert.True(operationAccessResult.HasAccess());
            Assert.False(hasNoAccessFromUserOperationAccessResult.HasAccess());
        }

        [Fact]
        public void Operation_Access_Task_Direct_To_User()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            ISecurityItem operation1 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation1");
            ISecurityItem operation2 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation2");
            ISecurityItem task = engine.Store.AddSecurityItem().AddBagItem(Name, "Task");
            task.Children.Add(operation1);
            task.Children.Add(operation2);
            ISecurityItem hasNoAccessFromUserOperation = engine.Store.AddSecurityItem().AddBagItem(Name, "HasNoAccessFromUserOperation");
            ISecurityIdentity user = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user");
            engine.Store.AccessAuthorize(user, task);

            ICheckAccessResult operation1AccessResult = engine.CheckAccess(user, operation1);
            ICheckAccessResult operation2AccessResult = engine.CheckAccess(user, operation2);
            ICheckAccessResult taskAccessResult = engine.CheckAccess(user, task);
            ICheckAccessResult hasNoAccessFromUserOperationAccessResult = engine.CheckAccess(user, hasNoAccessFromUserOperation);



            Assert.True(operation1AccessResult.HasAccess());
            Assert.True(operation2AccessResult.HasAccess());
            Assert.True(taskAccessResult.HasAccess());
            Assert.False(hasNoAccessFromUserOperationAccessResult.HasAccess());
        }

        [Fact]
        public void Operation_Deny_Direct_To_User()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            ISecurityItem operation1 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation1");
            ISecurityItem operation2 = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation2");
            ISecurityItem task = engine.Store.AddSecurityItem().AddBagItem(Name, "Task");
            task.Children.Add(operation1);
            task.Children.Add(operation2);
            ISecurityItem hasNoAccessFromUserOperation = engine.Store.AddSecurityItem().AddBagItem(Name, "HasNoAccessFromUserOperation");
            ISecurityIdentity user = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user");
            engine.Store.AccessAuthorize(user, task);
            engine.Store.AccessAuthorize(user, operation1).Deny();

            ICheckAccessResult operation1AccessResult = engine.CheckAccess(user, operation1);
            ICheckAccessResult operation2AccessResult = engine.CheckAccess(user, operation2);
            ICheckAccessResult taskAccessResult = engine.CheckAccess(user, task);
            ICheckAccessResult hasNoAccessFromUserOperationAccessResult = engine.CheckAccess(user, hasNoAccessFromUserOperation);


            Assert.False(operation1AccessResult.HasAccess());
            Assert.True(operation2AccessResult.HasAccess());
            Assert.True(taskAccessResult.HasAccess());
            Assert.False(hasNoAccessFromUserOperationAccessResult.HasAccess());
        }

        [Fact]
        public void Operation_Access_To_Group()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            ISecurityItem operation = engine.Store.AddSecurityItem().AddBagItem(Name, "Operation");
            ISecurityIdentity user1 = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user1");
            ISecurityIdentity user2 = engine.Store.AddSecurityIdentity().AddBagItem(Name, "user2");
            ISecurityIdentity adminGroup = engine.Store.AddSecurityIdentity().AddBagItem(Name, "adminGroup");
            adminGroup.Children.Add(user1);
            engine.Store.AccessAuthorize(adminGroup, operation);


            ICheckAccessResult user1AccessResult = engine.CheckAccess(user1, operation);
            ICheckAccessResult user2AccessResult = engine.CheckAccess(user2, operation);

            Assert.True(user1AccessResult.HasAccess());
            Assert.False(user2AccessResult.HasAccess());
        }
    }
}
