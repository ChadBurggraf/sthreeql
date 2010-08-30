//-----------------------------------------------------------------------
// <copyright file="DatabaseTargetConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Represents a backup database target's configuration element.
    /// </summary>
    public class DatabaseTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name of this target's associated AWS bucket.
        /// This bucket name must be configured in the awsTargets collection.
        /// </summary>
        [ConfigurationProperty("awsBucketName", IsRequired = true)]
        public string AWSBucketName
        {
            get { return (string)this["awsBucketName"]; }
            set { this["awsBucketName"] = value; }
        }

        /// <summary>
        /// Gets or sets the optional prefix to use when when storing this target on AWS.
        /// </summary>
        [ConfigurationProperty("awsPrefix", IsRequired = false)]
        public string AWSPrefix
        {
            get { return (string)this["awsPrefix"]; }
            set { this["awsBucketName"] = value; }
        }

        /// <summary>
        /// Gets or sets the target's catalog name.
        /// </summary>
        [ConfigurationProperty("catalogName", IsRequired = true)]
        public string CatalogName
        {
            get { return (string)this["catalogName"]; }
            set { this["catalogName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the target's datasource.
        /// </summary>
        [ConfigurationProperty("dataSource", IsRequired = true)]
        public string DataSource
        {
            get { return (string)this["dataSource"]; }
            set { this["dataSource"] = value; }
        }

        /// <summary>
        /// Gets the configured datasource element for this target.
        /// </summary>
        public DataSourceConfigurationElement DataSourceElement
        {
            get { return SThreeQLConfiguration.Section.DataSources[this.DataSource]; }
        }

        /// <summary>
        /// Gets or sets the target's name.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["dataSource"] = value; }
        }

        /// <summary>
        /// Creates a database connection string from this instance's values.
        /// </summary>
        /// <returns>A database connection string.</returns>
        public string CreateConnectionString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat(
                CultureInfo.InvariantCulture, 
                "data source={0};connection timeout={1};", 
                this.DataSource, 
                SThreeQLConfiguration.Section.DatabaseTimeout);

            if (!String.IsNullOrEmpty(this.DataSourceElement.UserId) && !String.IsNullOrEmpty(this.DataSourceElement.Password))
            {
                sb.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    "user id={0};password={1};",
                    this.DataSourceElement.UserId,
                    this.DataSourceElement.Password);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether this element is read-only.
        /// </summary>
        /// <returns>A value indicating whether this element is read-only.</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
