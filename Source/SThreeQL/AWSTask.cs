//-----------------------------------------------------------------------
// <copyright file="AwsTask.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using SThreeQL.Configuration;

    /// <summary>
    /// Base <see cref="Task"/> implementation for tasks needing an Aws service.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
    public abstract class AwsTask : Task
    {
        /// <summary>
        /// Gets the number of seconds to use for SQL connection timeouts.
        /// </summary>
        protected const int DatabaseConnectionTimeout = 600;

        private static readonly object locker = new object();
        private static bool servicePointInitialized;

        /// <summary>
        /// Initializes a new instance of the AwsTask class.
        /// </summary>
        /// <param name="awsConfig">The <see cref="AwsTargetConfigurationElement"/> to use when building the Aws service.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        protected AwsTask(AwsTargetConfigurationElement awsConfig)
        {
            if (awsConfig == null)
            {
                throw new ArgumentNullException("awsConfig", "awsConfig cannot be null.");
            }

            EnsureServicePointInitialized();
            this.AwsConfig = awsConfig;

            AmazonS3Config sthreeConfig = new AmazonS3Config()
            {
                CommunicationProtocol = SThreeQLConfiguration.Section.UseSsl ? Protocol.HTTPS : Protocol.HTTP
            };

            this.S3Client = AWSClientFactory.CreateAmazonS3Client(awsConfig.AwsAccessKeyId, awsConfig.AwsSecretAccessKeyId, sthreeConfig);
        }

        /// <summary>
        /// Gets the Aws configuration element used to build this instance's Aws service.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        protected AwsTargetConfigurationElement AwsConfig { get; private set; }

        /// <summary>
        /// Gets the Aws S3 client;
        /// </summary>
        protected AmazonS3 S3Client { get; private set; }

        /// <summary>
        /// Ensures the <see cref="ServicePointManager"/> is initialized to accept all SSL certificates.
        /// We're only making Aws calls, so I think we can blindly trust their certificates.
        /// </summary>
        private static void EnsureServicePointInitialized()
        {
            lock (locker)
            {
                if (!servicePointInitialized)
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    });

                    servicePointInitialized = true;
                }
            }
        }
    }
}
