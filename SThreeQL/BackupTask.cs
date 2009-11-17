using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Model;
using Affirma.ThreeSharp.Statistics;
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
            : base(SThreeQLConfiguration.Section.AWSTargets[config.AWSBucketName]) 
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
                        !String.IsNullOrEmpty(SThreeQLConfiguration.Section.BackupTargets.TempDir) ? SThreeQLConfiguration.Section.BackupTargets.TempDir : Path.GetTempPath(), 
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
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        private void BackupDatabase(TextWriter stdOut, TextWriter stdError)
        {
            stdOut.WriteLine(String.Format("   Backing up catalog {0}.", Config.CatalogName));
            
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

                    stdOut.WriteLine("   Compressing the backup file.");
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
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        /// <returns>The result of the execution.</returns>
        public override TaskExecutionResult Execute(TextWriter stdOut, TextWriter stdError)
        {
            TaskExecutionResult result = new TaskExecutionResult();
            stdOut.WriteLine(String.Format("Executing backup target {0}:", Config.Name));

            try
            {
                BackupDatabase(stdOut, stdError);
                UploadBackup(stdOut, stdError);

                stdOut.WriteLine("   Backup complete.");
            }
            catch (Exception ex)
            {
                stdError.WriteLine(String.Format("   Failed to execute backup task {0}:", Config.Name));
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
        /// Uploads the backup set to AWS.
        /// </summary>
        /// <param name="stdOut">A text writer to write standard output messages to.</param>
        /// <param name="stdError">A text writer to write standard error messages to.</param>
        private void UploadBackup(TextWriter stdOut, TextWriter stdError)
        {
            string redirectUrl = Service.GetRedirectUrl(AWSConfig.BucketName, null);
            stdOut.WriteLine("   Uploading the backup file (" + new FileInfo(TempPath).Length.ToFileSize() + "):");

            using (ObjectAddRequest request = new ObjectAddRequest(AWSConfig.BucketName, AWSKey))
            {
                request.Headers.Add("x-amz-acl", "private");
                request.LoadStreamWithFile(TempPath);

                if (redirectUrl != null)
                {
                    request.RedirectUrl = redirectUrl + AWSKey;
                }

                bool statusRunning = true;
                ObjectAddResponse response = null;

                Thread statusThread = new Thread(new ThreadStart(delegate()
                {
                    DateTime lastTime = DateTime.Now;
                    long lastTransferred = 0;

                    while (statusRunning)
                    {
                        try
                        {
                            TransferInfo info = Service.GetTransferInfo(request.ID);
                            long transferred = info.BytesTransferred;
                            int percent = (int)((double)transferred / request.BytesTotal * 100d);

                            if (percent > 0)
                            {
                                TimeSpan duration = DateTime.Now.Subtract(lastTime);
                                double rate = ((transferred - lastTransferred) / 1024) / duration.TotalSeconds;
                                stdOut.WriteLine(String.Format("      {0:###}% uploaded ({1:N0} KB/S).", percent, rate));

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

                using (response = Service.ObjectAdd(request)) 
                {
                    statusRunning = false;
                    stdOut.WriteLine("      Upload complete.");
                }
            }
        }

        #endregion
    }
}
