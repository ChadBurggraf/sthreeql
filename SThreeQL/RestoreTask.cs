using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Model;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Statistics;
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
            : base(SThreeQLConfiguration.Section.AWSTargets[config.AWSBucketName]) 
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
                        !String.IsNullOrEmpty(SThreeQLConfiguration.Section.RestoreTargets.TempDir) ? SThreeQLConfiguration.Section.RestoreTargets.TempDir : Path.GetTempPath(),
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
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        /// </summary>
        protected void DownloadDatabase(TextWriter stdOut, TextWriter stdError)
        {
            if (File.Exists(TempPath))
            {
                File.Delete(TempPath);
            }

            string redirectUrl = Service.GetRedirectUrl(AWSConfig.BucketName, AWSPrefix);
            BucketListResponseItem item = GetLatestBackupItem(redirectUrl);

            stdOut.WriteLine("   Downloading the backup file (" + item.Size.ToFileSize() + "):");

            using (ObjectGetRequest request = new ObjectGetRequest(AWSConfig.BucketName, item.Key))
            {
                if (!String.IsNullOrEmpty(redirectUrl))
                {
                    request.RedirectUrl = redirectUrl + item.Key;
                }

                using (ObjectGetResponse response = Service.ObjectGet(request))
                {
                    bool statusRunning = true;
                    
                    Thread statusThread = new Thread(new ThreadStart(delegate()
                    {
                        DateTime lastTime = DateTime.Now;
                        long lastTransferred = 0;

                        while (statusRunning)
                        {
                            try
                            {
                                TransferInfo info = Service.GetTransferInfo(response.ID);
                                long transferred = info.BytesTransferred;
                                int percent = (int)((double)transferred / item.Size * 100d);

                                if (percent > 0)
                                {
                                    TimeSpan duration = DateTime.Now.Subtract(lastTime);
                                    double rate = ((transferred - lastTransferred) / 1024) / duration.TotalSeconds;
                                    stdOut.WriteLine(String.Format("      {0:###}% downloaded ({1:N0} KB/S).", percent, rate));

                                    lastTime = DateTime.Now;
                                    lastTransferred = transferred;

                                    if (percent == 100)
                                    {
                                        statusRunning = false;
                                    }
                                }
                            }
                            catch
                            {
                                // The transfer hasn't started yet.
                            }

                            Thread.Sleep(1000);
                        }
                    }));

                    statusThread.Start();
                    response.StreamResponseToFile(TempPath);
                    statusRunning = false;
                    stdOut.WriteLine("      Download complete.");
                }
            }

            stdOut.WriteLine("   Decompressing the downloaded file.");
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
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        /// <returns>The result of the execution.</returns>
        public override TaskExecutionResult Execute(TextWriter stdOut, TextWriter stdError)
        {
            TaskExecutionResult result = new TaskExecutionResult();
            stdOut.WriteLine(String.Format("Executing restore target {0}:", Config.Name));

            try
            {
                DownloadDatabase(stdOut, stdError);
                RestoreDatabase(stdOut, stdError);

                stdOut.WriteLine("   Restore complete.");
            }
            catch (Exception ex)
            {
                stdError.WriteLine(String.Format("   Failed to execute restore task {0}:", Config.Name));
                stdError.WriteLine("      " + ex.Message);
                stdError.WriteLine("      " + ex.StackTrace);
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
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        private void RestoreDatabase(TextWriter stdOut, TextWriter stdError)
        {
            string connectionString = Common.CreateConnectionString(Config);
            stdOut.WriteLine(String.Format("   Dropping catalog {0} if it exists.", Config.RestoreCatalogName));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
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

            if (!Directory.Exists(Config.RestorePath))
            {
                Directory.CreateDirectory(Config.RestorePath);
            }

            stdOut.WriteLine(String.Format("   Restoring catalog {0}.", Config.RestoreCatalogName));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    DataTable files = new DataTable();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;
                        command.CommandText = Common.GetEmbeddedResourceText("SThreeQL.GetFiles.sql");

                        command.Parameters.Add(new SqlParameter("@Path", TempPath));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(files);
                        }
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add(new SqlParameter("@RestoreCatalog", Config.RestoreCatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", TempPath));

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < files.Rows.Count; i++)
                        {
                            string name = files.Rows[i]["LogicalName"].ToString();
                            string path = Path.Combine(Config.RestorePath, Path.GetFileName(files.Rows[i]["PhysicalName"].ToString()));

                            command.Parameters.Add(new SqlParameter("@FileName" + i, name));
                            command.Parameters.Add(new SqlParameter("@FilePath" + i, path));

                            sb.Append(String.Concat("\tMOVE @FileName", i, " TO @FilePath", i, ",\n"));
                        }

                        command.CommandText = String.Format(Common.GetEmbeddedResourceText("SThreeQL.Restore.sql"), sb.ToString());
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
