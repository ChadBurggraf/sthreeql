//-----------------------------------------------------------------------
// <copyright file="AwsTargetConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an Amazon S3 storage target configuration element.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
    public class AwsTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the target's access key ID.
        /// </summary>
        [ConfigurationProperty("awsAccessKeyId", IsRequired = true)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        public string AwsAccessKeyId
        {
            get { return (string)this["awsAccessKeyId"]; }
            set { this["awsAccessKeyId"] = value; }
        }

        /// <summary>
        /// Gets or sets the target's secret access key ID.
        /// </summary>
        [ConfigurationProperty("awsSecretAccessKeyId", IsRequired = true)]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        public string AwsSecretAccessKeyId
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
