using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Represents a task for executing the restore procedure on a restore target.
    /// </summary>
    public class RestoreTask : AWSTask
    {
        #region Member Variables

        private string awsPrefix, tempPath;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration element identifying the backup target to execute.</param>
        public RestoreTask(DatabaseRestoreTargetConfigurationElement config)
            : this(config, null, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration element identifying the restore target to execute.</param>
        /// <param name="progressAction">The action to call for network progress notifications.</param>
        /// <param name="completeAction">The action to call when the network activity is complete.</param>
        public RestoreTask(DatabaseRestoreTargetConfigurationElement config, Action<int> progressAction, Action completeAction)
            : base(SThreeQLConfiguration.Section.AWSTargets[config.AWSBucketName], progressAction, completeAction)
        {
            Config = config;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the AWS prefix to use when searching for a backup set to restore.
        /// </summary>
        private string AWSPrefix
        {
            get
            {
                if (awsPrefix == null)
                {
                    awsPrefix = Config.CatalogName.ToCatalogNamePrefix();
                }

                return awsPrefix;
            }
        }

        /// <summary>
        /// Gets the configuration element identifying the restore target to execute.
        /// </summary>
        public DatabaseRestoreTargetConfigurationElement Config { get; protected set; }

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
                        SThreeQLConfiguration.Section.RestoreTargets.TempDir,
                        Path.GetRandomFileName()
                    );
                }

                return tempPath;
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Downloads the latest backup set from the AWS service.
        /// </summary>
        protected void DownloadDatabase()
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }

            string redirectUrl = Service.GetRedirectUrl(AWSConfig.BucketName, AWSPrefix);
            BucketListResponseItem item = GetLatestBackupItem(redirectUrl);

            using (ObjectGetRequest request = new ObjectGetRequest(AWSConfig.BucketName, item.Key))
            {
                if (!String.IsNullOrEmpty(redirectUrl))
                {
                    request.RedirectUrl = redirectUrl + item.Key;
                }

                using (ObjectGetResponse response = Service.ObjectGet(request))
                {
                    if (ProgressAction != null)
                    {
                        response.Progress += new EventHandler<TransferInfoProgressEventArgs>(delegate(object sender, TransferInfoProgressEventArgs e)
                        {
                            ProgressAction((int)((double)e.Info.BytesTransferred / item.Size * 100));
                        });
                    }

                    response.StreamResponseToFile(TempPath);
                }
            }

            if (CompleteAction != null)
            {
                CompleteAction();
            }

            Common.DecompressFile(TempPath);
        }

        /// <summary>
        /// Gets the latest backup set item by last modified date.
        /// </summary>
        /// <param name="redirectUrl">The redirect URL to use if the service is currently redirecting the bucket.</param>
        /// <returns>The latest backup set.</returns>
        private BucketListResponseItem GetLatestBackupItem(string redirectUrl)
        {
            List<BucketListResponseItem> items = new List<BucketListResponseItem>();
            string marker = String.Empty;
            bool truncated = true;

            while (truncated)
            {
                using (BucketListRequest request = new BucketListRequest(AWSConfig.BucketName, AWSPrefix))
                {
                    if (!String.IsNullOrEmpty(redirectUrl))
                    {
                        request.RedirectUrl = redirectUrl;
                    }

                    if (!String.IsNullOrEmpty(marker))
                    {
                        request.QueryList.Add("marker", marker);
                    }

                    using (BucketListResponse response = Service.BucketList(request))
                    {
                        XmlDocument doc = response.StreamResponseToXmlDocument();

                        foreach (XmlNode node in doc.SelectNodes("//*[local-name()='Contents']"))
                        {
                            items.Add(BucketListResponseItem.Create(node));
                        }

                        truncated = Boolean.Parse(doc.SelectSingleNode("//*[local-name()='IsTruncated']").InnerXml);
                    }
                }
            }

            if (items.Count == 0)
            {
                throw new InvalidOperationException(String.Concat("There was no backup set found for catalog \"", Config.CatalogName, "\"."));
            }

            return items.OrderByDescending(i => i.LastModified).First();
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
                DownloadDatabase();
                RestoreDatabase();
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
        /// Runs the restore script on the downloaded backup set.
        /// </summary>
        private void RestoreDatabase()
        {
            string restoreCatalogPath = Path.Combine(Config.RestorePath, String.Concat(Config.RestoreCatalogName, ".mdf"));
            string restoreLogPath = Path.Combine(Config.RestorePath, String.Concat(Config.RestoreCatalogName, "_log.ldf"));
            string connectionString = String.Concat(
                "data source=", Config.DataSource, ";",
                "user id=", Config.UserId, ";",
                "password=", Config.Password, ";"
            );

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(
                            Common.GetEmbeddedResourceText("SThreeQL.Drop.sql"),
                            Config.RestoreCatalogName);

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            SqlConnection.ClearAllPools();

            if (File.Exists(restoreCatalogPath))
            {
                File.Delete(restoreCatalogPath);
            }

            if (File.Exists(restoreLogPath))
            {
                File.Delete(restoreLogPath);
            }

            if (!Directory.Exists(Config.RestorePath))
            {
                Directory.CreateDirectory(Config.RestorePath);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = Common.GetEmbeddedResourceText("SThreeQL.Restore.sql");

                        command.Parameters.Add(new SqlParameter("@RestoreCatalog", Config.RestoreCatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", TempPath));
                        command.Parameters.Add(new SqlParameter("@Name", Config.CatalogName));
                        command.Parameters.Add(new SqlParameter("@LogName", Config.LogName));
                        command.Parameters.Add(new SqlParameter("@RestoreCatalogPath", restoreCatalogPath));
                        command.Parameters.Add(new SqlParameter("@RestoreLogPath", restoreLogPath));

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        #endregion
    }
}
