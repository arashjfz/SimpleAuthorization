using System;
using System.Collections.Generic;
using System.Linq;
using SimpleAuthorization.Engine;
using SimpleAuthorization.Initializer;
using SimpleAuthorization.Storage;
using SimpleAuthorization.Store;
using Xunit;

namespace SimpleAuthorization.Test
{
    public class AccessTest
    {
        private readonly User _testUser = new User(Guid.NewGuid(), "test");
        private readonly User _adminUser = new User(Guid.NewGuid(), "admin");
        private readonly Group _group;
        private readonly User[] _users;
        private readonly Guid _operation1_1 = Guid.NewGuid();
        private readonly Guid _operation1_2 = Guid.NewGuid();
        private readonly Guid _operation2_1 = Guid.NewGuid();
        private readonly Guid _operation2_2 = Guid.NewGuid();
        private readonly Guid _task1 = Guid.NewGuid();
        private readonly Guid _task2 = Guid.NewGuid();
        private readonly Guid _role = Guid.NewGuid();
        private readonly IAuthorizationEngine _authorizationEngine;
        private readonly AuthorizationProvider _authorizationProvider;
        private readonly ISecurityStore _securityStore;


        public AccessTest()
        {
            _users = new[] { _testUser, _adminUser };
            _group = new Group(new[] { _testUser, _adminUser });
            _authorizationProvider = new AuthorizationProvider();
            StoreBuilder storeBuilder = new StoreBuilder();
            _securityStore = storeBuilder
                .AddSecurityIdentities(_users)
                .AddAuthorizableItems(_users)
                .AddAuthorizableItem(_group)
                .AddAuthrizationProvider(_authorizationProvider)
                .AddSecurityItem(_operation1_1)
                .AddParent(_task1)
                .AddSecurityItem(_operation1_2)
                .AddParent(_task1)
                .AddSecurityItem(_operation2_1)
                .AddParent(_task2)
                .AddSecurityItem(_operation2_2)
                .AddParent(_task2)
                .AddSecurityItem(_task1)
                .AddParent(_role)
                .AddSecurityItem(_task2)
                .AddParent(_role)
                .AddSecurityItem(_role)
                .NoParent().Build();
            _authorizationEngine = _securityStore.GetEngine();
        }

        [Fact]
        public void Operation_Access_Operation_Direct_To_User()
        {
            _authorizationProvider.Add(new StorageAuthorization(_testUser.Key, _operation1_1, AuthorizationType.Allow));

            bool operation1_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_1));
            bool operation1_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_2));
            bool operation2_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_1));
            bool operation2_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_2));

            Assert.True(operation1_1);
            Assert.False(operation1_2);
            Assert.False(operation2_1);
            Assert.False(operation2_2);
        }
        [Fact]
        public void Operation_Access_Task_Direct_To_User()
        {
            _authorizationProvider.Add(new StorageAuthorization(_testUser.Key, _task1, AuthorizationType.Allow));

            bool operation1_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_1));
            bool operation1_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_2));
            bool operation2_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_1));
            bool operation2_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_2));

            Assert.True(operation1_1);
            Assert.True(operation1_2);
            Assert.False(operation2_1);
            Assert.False(operation2_2);
        }
        [Fact]
        public void Operation_Deny_Direct_To_User()
        {
            _authorizationProvider.Add(new StorageAuthorization(_testUser.Key, _role, AuthorizationType.Allow));
            _authorizationProvider.Add(new StorageAuthorization(_testUser.Key, _operation1_1, AuthorizationType.Deny));

            bool operation1_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_1));
            bool operation1_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_2));
            bool operation2_1 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_1));
            bool operation2_2 = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation2_2));

            Assert.False(operation1_1);
            Assert.True(operation1_2);
            Assert.True(operation2_1);
            Assert.True(operation2_2);
        }
        [Fact]
        public void Operation_Access_To_Group()
        {
            _authorizationProvider.Add(new StorageAuthorization(_group.Key, _operation1_1, AuthorizationType.Allow));

            bool testUser = _authorizationEngine.CheckAccess(_testUser, _securityStore.GetSecurityItem(_operation1_1));
            bool admin = _authorizationEngine.CheckAccess(_adminUser, _securityStore.GetSecurityItem(_operation1_1));

            Assert.True(testUser);
            Assert.True(admin);
        }
    }
    public class User : ISecurityIdentity, IAuthorisableItem
    {
        public User(Guid userId, string username)
        {
            Key = userId;
            Username = username;
        }
        #region Implementation of ISecurityIdentity

        public Guid Key { get; }
        public IEnumerable<ISecurityIdentity> GetSecurityIdentities()
        {
            return Enumerable.Repeat(this, 1);
        }

        public bool ContainSecurityIdentity(ISecurityIdentity securityIdentity)
        {
            return securityIdentity.Key == Key;
        }

        public string Username { get; }

        #endregion
    }
    public class Group : IAuthorisableItem
    {
        private Dictionary<Guid, User> _users;

        public Group(IEnumerable<User> users)
        {
            Key = Guid.NewGuid();
            _users = users.ToDictionary(user => user.Key);

        }
        #region Implementation of IAuthorisableItem

        public Guid Key { get; }
        public IEnumerable<ISecurityIdentity> GetSecurityIdentities()
        {
            return _users.Values;
        }

        public bool ContainSecurityIdentity(ISecurityIdentity securityIdentity)
        {
            return _users.ContainsKey(securityIdentity.Key);
        }

        #endregion
    }
    public class AuthorizationProvider : IAuthorizationProvider
    {
        List<IStorageAuthorization> _authorizations = new List<IStorageAuthorization>();
        #region Implementation of IAuthorizationProvider

        public IEnumerable<IStorageAuthorization> Provide()
        {
            return _authorizations;
        }

        public void Add(IStorageAuthorization authorization)
        {
            _authorizations.Add(authorization);
            OnChanged();
        }

        public event EventHandler Changed;

        #endregion

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
    public class StorageAuthorization : IStorageAuthorization
    {
        public StorageAuthorization(Guid authorisableItemKey, Guid securityItemKey, AuthorizationType type)
        {
            AuthorisableItemKey = authorisableItemKey;
            SecurityItemKey = securityItemKey;
            Type = type;
            Key = Guid.NewGuid();
            Conditions = new List<byte[]>();
        }

        #region Implementation of IStorageAuthorization

        public Guid Key { get; }
        public Guid AuthorisableItemKey { get; }
        public Guid SecurityItemKey { get; }
        public byte[] LifeCycle { get; }
        public Guid? DelegatedByKey { get; }
        public AuthorizationType Type { get; }
        public IEnumerable<byte[]> Conditions { get; }

        #endregion
    }
}
