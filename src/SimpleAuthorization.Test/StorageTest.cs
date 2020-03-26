using System;
using System.Collections;
using System.Collections.Generic;
using SimpleAuthorization.Engine;
using SimpleAuthorization.Storage;
using Xunit;

namespace SimpleAuthorization.Test
{
    public class StorageTest
    {
        [Fact]
        public void Add_SecurityItem()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            engine.Store.AddSecurityItem("BD4335A8-D067-429F-B2EE-70DD5E3C214B");

            Assert.Contains("BD4335A8-D067-429F-B2EE-70DD5E3C214B", securityStorage.StorageEntities.Keys);
        }
        [Fact]
        public void Add_SecurityItem_Relation()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            ISecurityItem parentSecurityItem = engine.Store.AddSecurityItem("3F028123-7EDA-42F7-80B9-09A9DE954047");
            ISecurityItem childSecurityItem = engine.Store.AddSecurityItem("13F1C233-90DC-4904-9EAA-C1A912D2B0A7");
            parentSecurityItem.Children.Add(childSecurityItem);

            Assert.Contains("3F028123-7EDA-42F7-80B9-09A9DE954047_13F1C233-90DC-4904-9EAA-C1A912D2B0A7", securityStorage.StorageEntities.Keys);
        }
        [Fact]
        public void Add_SecurityItem_Bag()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            ISecurityItem securityItem = engine.Store.AddSecurityItem("07325155-574E-4D89-B680-C0F1B90569E5");
            securityItem.Bag["Name"] = "Test";
            Assert.Contains("07325155-574E-4D89-B680-C0F1B90569E5_Name", securityStorage.StorageEntities.Keys);
        }

        [Fact]
        public void Add_SecurityIdentity()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            engine.Store.AddSecurityIdentity("6B46E638-69B6-4C2B-A1D6-6B3A514FBFDE");

            Assert.Contains("6B46E638-69B6-4C2B-A1D6-6B3A514FBFDE", securityStorage.StorageEntities.Keys);
        }
        [Fact]
        public void Add_SecurityIdentity_Relation()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            ISecurityIdentity parentSecurityIdentity = engine.Store.AddSecurityIdentity("77A018B1-EC20-41F1-951D-549193486D4B");
            ISecurityIdentity childSecurityIdentity = engine.Store.AddSecurityIdentity("4B64042A-B5DE-4BDF-8A5D-60112F65275B");
            parentSecurityIdentity.Children.Add(childSecurityIdentity);

            Assert.Contains("77A018B1-EC20-41F1-951D-549193486D4B_4B64042A-B5DE-4BDF-8A5D-60112F65275B", securityStorage.StorageEntities.Keys);
        }

        [Fact]
        public void Add_SecurityIdentity_Bag()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine engine = factory.CreateEngine();
            TestSecurityStorage securityStorage = new TestSecurityStorage();
            engine.Store.AttachToStorage(securityStorage);

            ISecurityIdentity securityIdentity = engine.Store.AddSecurityIdentity("AC2ECB1E-AFE1-46EA-B9A7-F1593E2C92C6");
            securityIdentity.Bag["Name"] = "Test";
            Assert.Contains("AC2ECB1E-AFE1-46EA-B9A7-F1593E2C92C6", securityStorage.StorageEntities.Keys);
        }
    }

    internal class TestSecurityStorage:ISecurityStorage
    {
        internal readonly Dictionary<string,IStorageEntity> StorageEntities = new Dictionary<string, IStorageEntity>();
        #region Implementation of ISecurityStorage

        public void AddItems(IEnumerable<IStorageEntity> items)
        {
            foreach (IStorageEntity storageEntity in items)
                StorageEntities.Add(storageEntity.GetIdentity(), storageEntity);
        }

        public void RemoveItems(IEnumerable<IStorageEntity> items)
        {
            foreach (IStorageEntity storageEntity in items)
                StorageEntities.Remove(storageEntity.GetIdentity());

        }

        public void UpdateItems(IEnumerable<IStorageEntity> items)
        {
            foreach (IStorageEntity storageEntity in items)
                StorageEntities[storageEntity.GetIdentity()] = storageEntity;
        }

        public IEnumerable GetItems(Type entityType)
        {
            foreach (IStorageEntity storageEntitiesValue in StorageEntities.Values)
                if (entityType.IsInstanceOfType(storageEntitiesValue))
                    yield return entityType;
        }

        public void ApplyBulkActions(IEnumerable<IStorageAction> actions)
        {
            foreach (IStorageAction storageAction in actions)
            {
                switch (storageAction.StorageActionType)
                {
                    case StorageActionType.Add:
                        AddItems(storageAction.Entities);
                        break;
                    case StorageActionType.Remove:
                        RemoveItems(storageAction.Entities);
                        break;
                    case StorageActionType.Update:
                        UpdateItems(storageAction.Entities);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion
    }
}
