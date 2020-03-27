using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.Cfg;

namespace SimpleAuthorization.Nhibernate
{
    public static class ConfigurationExtensions
    {
        public static Configuration AddSimpleAuthorizationMapping(this Configuration configuration)
        {
            return configuration.AddAssembly(typeof(ConfigurationExtensions).Assembly);
        }
    }
}
