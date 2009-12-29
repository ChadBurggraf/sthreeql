using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a datasource definition.
    /// </summary>
    public class DataSourceConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the datasource's name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }
        /// <summary>
        /// Gets the password to use when connecting to the datasource.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)(this["password"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the user ID to use when connecting to the datasource.
        /// </summary>
        [ConfigurationProperty("userId", IsRequired = false)]
        public string UserId
        {
            get { return (string)(this["userId"] ?? String.Empty); }
        }
    }
}
