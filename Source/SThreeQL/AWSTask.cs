//-----------------------------------------------------------------------
// <copyright file="AWSTask.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using SThreeQL.Configuration;

    /// <summary>
    /// Base <see cref="Task"/> implementation for tasks needing an AWS service.
    /// </summary>
    public abstract class AWSTask : Task
    {
        /// <summary>
        /// Gets the number of seconds to use for SQL connection timeouts.
        /// </summary>
        protected const int DatabaseConnectionTimeout = 600;

        private static readonly object locker = new object();
        private static bool servicePointInitialized;

        /// <summary>
        /// Initializes a new instance of the AWSTask class.
        /// </summary>
        /// <param name="awsConfig">The <see cref="AWSTargetConfigurationElement"/> to use when building the AWS service.</param>
        protected AWSTask(AWSTargetConfigurationElement awsConfig)
        {
            if (awsConfig == null)
            {
                throw new ArgumentNullException("awsConfig cannot be null.");
            }

            EnsureServicePointInitialized();
            this.AWSConfig = awsConfig;

            AmazonS3Config sthreeConfig = new AmazonS3Config()
            {
                CommunicationProtocol = SThreeQLConfiguration.Section.UseSSL ? Protocol.HTTPS : Protocol.HTTP
            };

            this.S3Client = AWSClientFactory.CreateAmazonS3Client(awsConfig.AWSAccessKeyId, awsConfig.AWSSecretAccessKeyId, sthreeConfig);
        }

        /// <summary>
        /// Gets the AWS configuration element used to build this instance's AWS service.
        /// </summary>
        protected AWSTargetConfigurationElement AWSConfig { get; private set; }

        /// <summary>
        /// Gets the AWS S3 client;
        /// </summary>
        protected AmazonS3 S3Client { get; private set; }

        /// <summary>
        /// Ensures the <see cref="ServicePointManager"/> is initialized to accept all SSL certificates.
        /// We're only making AWS calls, so I think we can blindly trust their certificates.
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
