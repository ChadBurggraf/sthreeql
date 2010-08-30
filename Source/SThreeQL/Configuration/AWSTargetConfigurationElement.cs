//-----------------------------------------------------------------------
// <copyright file="AWSTargetConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents an Amazon S3 storage target configuration element.
    /// </summary>
    public class AWSTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the target's access key ID.
        /// </summary>
        [ConfigurationProperty("awsAccessKeyId", IsRequired = true)]
        public string AWSAccessKeyId
        {
            get { return (string)this["awsAccessKeyId"]; }
            set { this["awsAccessKeyId"] = value; }
        }

        /// <summary>
        /// Gets or sets the target's secret access key ID.
        /// </summary>
        [ConfigurationProperty("awsSecretAccessKeyId", IsRequired = true)]
        public string AWSSecretAccessKeyId
        {
            get { return (string)this["awsSecretAccessKeyId"]; }
            set { this["awsSecretAccessKeyId"] = value; }
        }

        /// <summary>
        /// Gets or sets the target's bucket name.
        /// </summary>
        [ConfigurationProperty("bucketName", IsKey = true, IsRequired = true)]
        public string BucketName
        {
            get { return (string)this["bucketName"]; }
            set { this["bucketName"] = value; }
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
