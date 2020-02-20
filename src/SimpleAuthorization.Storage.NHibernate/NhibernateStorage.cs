using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using SimpleAuthorization.Storage.NHibernate.Models;

namespace SimpleAuthorization.Storage.NHibernate
{
    public class NhibernateStorage:ISecurityStorage
    {
        private readonly ISessionFactory _sessionFactory;

        public NhibernateStorage(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }
        #region Implementation of ISecurityStorage

        private ISession GetSession()
        {
            return _sessionFactory.OpenSession();
        }
        public IEnumerable<ISecurityHierarchy> SecurityHierarchies
        {
            get
            {
                using (ISession session = GetSession())
                {
                    return session.QueryOver<SecurityHierarchy>().List();
                }
            }
        }

        public void RemoveSecurityHierarchy(IEnumerable<ISecurityHierarchy> hierarchies)
        {
            List<string> ids = hierarchies.Select(h => $"{h.SecurityItemKey}{h.SecurityItemParentKey}").ToList();
            using (ISession session = GetSession())
            {
                session.Query<SecurityHierarchy>().Where(hierarchy => ids.Contains(hierarchy.Id)).Delete();
            }
        }

        public void AddSecurityHierarchy(IEnumerable<ISecurityHierarchy> hierarchies)
        {
            using (ISession session = GetSession())
            {
                foreach (ISecurityHierarchy securityHierarchy in hierarchies)
                {
                    session.Save(new SecurityHierarchy(securityHierarchy));
                }
                session.Flush();
            }
        }

        public IEnumerable<Guid> SecurityItemKeys
        {
            get
            {
                using (ISession session = GetSession())
                {
                    return session.QueryOver<SecurityItem>().Select(item => item.Id).List<Guid>();
                }
            }
        }

        public void RemoveSecurityItemKeys(IEnumerable<Guid> securityItemKeys)
        {
            using (ISession session = GetSession())
            {
                session.Query<SecurityItem>().Where(item => securityItemKeys.Contains(item.Id)).Delete();
            }
        }

        public void AddSecurityItemKeys(IEnumerable<Guid> securityItemKeys)
        {
            using (ISession session = GetSession())
            {
                foreach (Guid securityItemKey in securityItemKeys)
                {
                    session.Save(new SecurityItem {Id = securityItemKey});
                }
            }
        }

        public IEnumerable<IStorageAuthorization> Authorizations
        {
            get
            {
                using (ISession session = GetSession())
                {
                    return session.QueryOver<StorageAuthorization>().List();
                }
            }
        }

        public void UpdateAuthorizations(IEnumerable<IStorageAuthorization> authorizations)
        {
            using (ISession session = GetSession())
            {
                foreach (IStorageAuthorization storageAuthorization in authorizations)
                {
                    session.Update(new StorageAuthorization(storageAuthorization));
                }
                session.Flush();
            }
        }

        public void RemoveAuthorizations(IEnumerable<IStorageAuthorization> authorizations)
        {
            List<Guid> authorizationKeys = authorizations.Select(authorization => authorization.Key).ToList();
            using (ISession session = GetSession())
            {
                session.Query<StorageAuthorization>().Where(authorization => authorizationKeys.Contains(authorization.Key))
                    .Delete();
            }
        }

        public void AddAuthorizations(IEnumerable<IStorageAuthorization> authorizations)
        {
            using (ISession session = GetSession())
            {
                foreach (IStorageAuthorization storageAuthorization in authorizations)
                {
                    session.Save(new StorageAuthorization(storageAuthorization));
                }
                session.Flush();
            }

        }

        public IEnumerable<IAuthorizableItemHierarchy> AuthorizableHierarchies { get; }
        public void RemoveAuthorizableHierarchy(IEnumerable<IAuthorizableItemHierarchy> hierarchies)
        {
            throw new NotImplementedException();
        }

        public void AddAuthorizableHierarchy(IEnumerable<IAuthorizableItemHierarchy> hierarchies)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> AuthorizableItemKeys { get; }
        public void RemoveAuthorizableItemKeys(IEnumerable<Guid> authorizableItemKeys)
        {
            throw new NotImplementedException();
        }

        public void AddAuthorizableItemKeys(IEnumerable<Guid> authorizableItemKeys)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Guid> SecurityIdentityKeys { get; }
        public void RemoveSecurityIdentityKeys(IEnumerable<Guid> securityIdentityKeys)
        {
            throw new NotImplementedException();
        }

        public void AddSecurityIdentityKeys(IEnumerable<Guid> securityIdentityKeys)
        {
            throw new NotImplementedException();
        }

        public event EventHandler Changed;

        #endregion
    }
}
