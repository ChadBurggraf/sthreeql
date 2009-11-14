using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Model;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Statistics;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Abstract task for tasks needing an AWS service.
    /// </summary>
    public abstract class AWSTask
    {
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

            ServiceConfig = new ThreeSharpConfig();
            ServiceConfig.AwsAccessKeyID = AWSConfig.AWSAccessKeyId;
            ServiceConfig.AwsSecretAccessKey = AWSConfig.AWSSecretAccessKeyId;
            ServiceConfig.ConnectionLimit = 40;
            ServiceConfig.IsSecure = SThreeQLConfiguration.Section.UseSSL;
            ServiceConfig.Format = CallingFormat.SUBDOMAIN;

            Service = new ThreeSharpQuery(ServiceConfig);
        }

        /// <summary>
        /// Gets the AWS configuration element used to build this instance's AWS service.
        /// </summary>
        protected AWSTargetConfigurationElement AWSConfig { get; private set; }

        /// <summary>
        /// Gets the AWS service.
        /// </summary>
        protected IThreeSharp Service { get; private set; }

        /// <summary>
        /// Gets the AWS service configuration.
        /// </summary>
        protected ThreeSharpConfig ServiceConfig { get; private set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        /// <returns>The result of the execution.</returns>
        public abstract TaskExecutionResult Execute(TextWriter stdOut, TextWriter stdError);
    }
}
