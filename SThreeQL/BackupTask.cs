using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Represents a task for executing the backup procedure on a backup target.
    /// </summary>
    public class BackupTask : AWSTask
    {
        #region Member Variables

        private string awsKey, tempPath;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration element identifying the backup target to execute.</param>
        public BackupTask(DatabaseTargetConfigurationElement config)
            : this(config, null, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration element identifying the backup target to execute.</param>
        /// <param name="progressAction">The action to call for network progress notifications.</param>
        /// <param name="completeAction">The action to call when the network activity is complete.</param>
        public BackupTask(DatabaseTargetConfigurationElement config, Action<int> progressAction, Action completeAction)
            : base(SThreeQLConfiguration.Section.AWSTargets[config.AWSBucketName], progressAction, completeAction)
        {
            Config = config;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the generated AWS key to use for the backup.
        /// </summary>
        private string AWSKey
        {
            get
            {
                if (awsKey == null)
                {
                    awsKey = Config.CatalogName.ToCatalogNameAWSKey();
                }

                return awsKey;
            }
        }

        /// <summary>
        /// Gets the configuration element identifying the backup target to execute.
        /// </summary>
        public DatabaseTargetConfigurationElement Config { get; protected set; }

        /// <summary>
        /// Gets the fully qualified temporary file path.
        /// </summary>
        private string TempPath
        {
            get
            {
                if (tempPath == null)
                {
                    tempPath = Path.Combine(
                        SThreeQLConfiguration.Section.BackupTargets.TempDir, 
                        Path.GetRandomFileName()
                    );
                }

                return tempPath;
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Runs the backup script to create a fresh backup file.
        /// </summary>
        private void BackupDatabase()
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }

            using (SqlConnection connection = new SqlConnection(Common.CreateConnectionString(Config)))
            {
                connection.Open();

                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;
                        command.CommandText = Common.GetEmbeddedResourceText("SThreeQL.Backup.sql");

                        command.Parameters.Add(new SqlParameter("@Catalog", Config.CatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", TempPath));
                        command.Parameters.Add(new SqlParameter("@Name", Config.CatalogName + " - Full Database Backup"));

                        command.ExecuteNonQuery();
                    }

                    Common.CompressFile(TempPath);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The result of the execution.</returns>
        public override TaskExecutionResult Execute()
        {
            TaskExecutionResult result = new TaskExecutionResult();

            try
            {
                BackupDatabase();
                UploadBackup();
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.Success = false;
            }
            finally
            {
                try
                {
                    if (File.Exists(TempPath))
                    {
                        File.Delete(TempPath);
                    }
                }
                catch
                {
                    // Eat it.
                }
            }

            return result;
        }

        /// <summary>
        /// Uploads the backup set to AWS.
        /// </summary>
        private void UploadBackup()
        {
            string redirectUrl = Service.GetRedirectUrl(AWSConfig.BucketName, null);

            using (ObjectAddRequest request = new ObjectAddRequest(AWSConfig.BucketName, AWSKey))
            {
                request.Headers.Add("x-amz-acl", "private");
                request.LoadStreamWithFile(TempPath);

                if (redirectUrl != null)
                {
                    request.RedirectUrl = redirectUrl + AWSKey;
                }

                if (ProgressAction != null)
                {
                    request.Progress += new EventHandler<TransferInfoProgressEventArgs>(delegate(object sender, TransferInfoProgressEventArgs e)
                    {
                        ProgressAction((int)((double)e.Info.BytesTransferred / e.Info.BytesTotal * 100));
                    });
                }

                using (ObjectAddResponse response = Service.ObjectAdd(request)) { }

                if (CompleteAction != null)
                {
                    CompleteAction();
                }
            }
        }

        #endregion
    }
}
