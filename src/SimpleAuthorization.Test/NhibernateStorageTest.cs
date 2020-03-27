using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using SimpleAuthorization.Engine;
using SimpleAuthorization.Nhibernate;
using Xunit;

namespace SimpleAuthorization.Test
{
    public class NhibernateStorageTest
    {
        private StorageProvider GetStorageProvider()
        {
            Configuration configuration = Fluently.Configure().
                Database(MsSqlConfiguration.MsSql2012.ConnectionString("SERVER=localhost;DATABASE=SimpleAuthorizationTest;Integrated Security=False;Persist Security Info=True;UID=sa;PWD=@rashof123456;Connect Timeout=1200")
                    .Driver<Sql2008ClientDriver>()
                    .Dialect<MsSql2012Dialect>()).BuildConfiguration().AddSimpleAuthorizationMapping();
            ISessionFactory sessionFactory = configuration.BuildSessionFactory();
            ISession session = sessionFactory.OpenSession();
            return new StorageProvider(new Nhibernate.Storage(() => session), session);
        }
        [Fact]
        public void Add_SecurityItem()
        {
            EngineFactory factory = new EngineFactory();
            ISecurityEngine firstEngine = factory.CreateEngine();
            using (StorageProvider storageProvider = GetStorageProvider())
            {
                firstEngine.Store.AttachToStorage(storageProvider.Storage);
                ISecurityItem addSecurityItem = firstEngine.Store.AddSecurityItem();
            }
            ISecurityEngine secondEngine = factory.CreateEngine();
            using (StorageProvider storageProvider = GetStorageProvider())
            {
                secondEngine.Store.AttachToStorage(storageProvider.Storage);
            }

        }
    }

    internal class StorageProvider:IDisposable
    {
        private readonly ISession _session;
        public Nhibernate.Storage Storage { get; }

        public StorageProvider(Nhibernate.Storage storage,ISession session)
        {
            _session = session;
            Storage = storage;
        }
        #region IDisposable

        public void Dispose()
        {
            _session.Dispose();
        }

        #endregion
    }
}
