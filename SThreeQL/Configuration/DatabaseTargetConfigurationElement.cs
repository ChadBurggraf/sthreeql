using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a backup database target's configuration element.
    /// </summary>
    public class DatabaseTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the name of this target's associated AWS bucket.
        /// This bucket name must be configured in the awsTargets collection.
        /// </summary>
        [ConfigurationProperty("awsBucketName", IsRequired = true)]
        public string AWSBucketName
        {
            get { return (string)(this["awsBucketName"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the target's catalog name.
        /// </summary>
        [ConfigurationProperty("catalogName", IsRequired = true)]
        public string CatalogName
        {
            get { return (string)(this["catalogName"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the target's data source (i.e., server name or IP address).
        /// </summary>
        [ConfigurationProperty("dataSource", IsRequired = true)]
        public string DataSource
        {
            get { return (string)(this["dataSource"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the target's name.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)(this["name"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the password to use when connecting to the target's datasource.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)(this["password"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the user ID to use when connecting to the target's datasource.
        /// The user must have access to the master database, as well as
        /// drop/restore priviledges on the server.
        /// </summary>
        [ConfigurationProperty("userId", IsRequired = true)]
        public string UserId
        {
            get { return (string)(this["userId"] ?? String.Empty); }
        }
    }
}
