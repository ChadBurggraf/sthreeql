using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents an Amazon S3 storage target configuration element.
    /// </summary>
    public class AWSTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the target's access key ID.
        /// </summary>
        [ConfigurationProperty("awsAccessKeyId", IsRequired = true)]
        public string AWSAccessKeyId
        {
            get { return (string)this["awsAccessKeyId"]; }
        }

        /// <summary>
        /// Gets the target's secret access key ID.
        /// </summary>
        [ConfigurationProperty("awsSecretAccessKeyId", IsRequired = true)]
        public string AWSSecretAccessKeyId
        {
            get { return (string)this["awsSecretAccessKeyId"]; }
        }

        /// <summary>
        /// Gets the target's bucket name.
        /// </summary>
        [ConfigurationProperty("bucketName", IsKey = true, IsRequired = true)]
        public string BucketName
        {
            get { return (string)this["bucketName"]; }
        }
    }
}
