using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
            get { return (string)this["awsBucketName"]; }
        }

        /// <summary>
        /// Gets the optional prefix to use when when storing this target on AWS.
        /// </summary>
        [ConfigurationProperty("awsPrefix", IsRequired = false)]
        public string AWSPrefix
        {
            get { return (string)this["awsPrefix"]; }
        }

        /// <summary>
        /// Gets the target's catalog name.
        /// </summary>
        [ConfigurationProperty("catalogName", IsRequired = true)]
        public string CatalogName
        {
            get { return (string)this["catalogName"]; }
        }

        /// <summary>
        /// Gets a connection string built from this instance's values.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "data source={0};connection timeout={1};", DataSource, SThreeQLConfiguration.Section.DatabaseTimeout);

                if (!String.IsNullOrEmpty(DataSourceElement.UserId) && !String.IsNullOrEmpty(DataSourceElement.Password))
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "user id={0};password={1};", DataSourceElement.UserId, DataSourceElement.Password);
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the name of the target's datasource.
        /// </summary>
        [ConfigurationProperty("dataSource", IsRequired = true)]
        public string DataSource
        {
            get { return (string)this["dataSource"]; }
        }

        /// <summary>
        /// Gets the configured datasource element for this target.
        /// </summary>
        public DataSourceConfigurationElement DataSourceElement
        {
            get { return SThreeQLConfiguration.Section.DataSources[DataSource]; }
        }

        /// <summary>
        /// Gets the target's name.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }
    }
}
