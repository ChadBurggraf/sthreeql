using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Model;
using Affirma.ThreeSharp.Query;
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
        /// <param name="progressAction">The action to call for progress notifications.</param>
        /// <param name="completeAction">The action to call when the network activity is complete.</param>
        protected AWSTask(AWSTargetConfigurationElement awsConfig, Action<int> progressAction, Action completeAction)
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

            ProgressAction = progressAction;
            CompleteAction = completeAction;
        }

        /// <summary>
        /// Gets the AWS configuration element used to build this instance's AWS service.
        /// </summary>
        protected AWSTargetConfigurationElement AWSConfig { get; private set; }

        /// <summary>
        /// Gets the action to call when the network activity is complete.
        /// </summary>
        public Action CompleteAction { get; protected set; }

        /// <summary>
        /// Gets the action to call for progress notifications.
        /// Perceent complete is passed as the only parameter.
        /// </summary>
        public Action<int> ProgressAction { get; protected set; }

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
        /// <returns>The result of the execution.</returns>
        public abstract TaskExecutionResult Execute();
    }
}
