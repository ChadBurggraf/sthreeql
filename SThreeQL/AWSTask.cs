using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Abstract task for tasks needing an AWS service.
    /// </summary>
    public abstract class AWSTask : Task, ITransferDelegate
    {
        private static readonly object locker = new object();
        private ITransferDelegate transferDelegate;

        /// <summary>
        /// Gets the number of seconds to use for SQL connection timeouts.
        /// </summary>
        protected const int CONNECTION_TIMEOUT = 600;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="awsConfig">The <see cref="AWSTargetConfigurationElement"/> to use when building the AWS service.</param>
        protected AWSTask(AWSTargetConfigurationElement awsConfig)
        {
            if (awsConfig == null)
            {
                throw new ArgumentNullException("awsConfig cannot be null.");
            }

            AWSConfig = awsConfig;

            AmazonS3Config s3Config = new AmazonS3Config()
            {
                CommunicationProtocol = SThreeQLConfiguration.Section.UseSSL ? Protocol.HTTPS : Protocol.HTTP
            };

            S3Client = AWSClientFactory.CreateAmazonS3Client(awsConfig.AWSAccessKeyId, awsConfig.AWSSecretAccessKeyId, s3Config);
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
        /// Gets or sets the transfer delegate.
        /// </summary>
        public ITransferDelegate TransferDelegate
        {
            get
            {
                lock (locker)
                {
                    if (transferDelegate == null)
                    {
                        transferDelegate = this;
                    }

                    return transferDelegate;
                }
            }
            set
            {
                lock (locker)
                {
                    transferDelegate = value;
                }
            }
        }

        #region ITransferDelegate Members

        /// <summary>
        /// Called when a transfer is complete.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferComplete(TransferInfo info) { }

        /// <summary>
        /// Called when a transfer's progress has been updated..
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferProgress(TransferInfo info) { }

        /// <summary>
        /// Called when a transfer begins.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferStart(TransferInfo info) { }

        #endregion
    }
}
