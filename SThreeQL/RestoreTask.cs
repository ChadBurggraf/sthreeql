using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Represents a task for executing the restore procedure on a restore target.
    /// </summary>
    public class RestoreTask : AWSTask, IRestoreDelegate
    {
        #region Member Variables

        private static readonly object locker = new object();
        private string awsPrefix;
        private IRestoreDelegate restoreDelegate;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The restore target to execute.</param>
        public RestoreTask(DatabaseRestoreTargetConfigurationElement target)
            : base(SThreeQLConfiguration.Section.AWSTargets[target.AWSBucketName]) 
        {
            Target = target;
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
                    awsPrefix = EscapeCatalogName(Target.CatalogName);
                }

                return awsPrefix;
            }
        }

        /// <summary>
        /// Gets the configuration-defined temporary directory to use for this task type.
        /// </summary>
        protected override string ConfiguredTempDir 
        { 
            get { return SThreeQLConfiguration.Section.RestoreTargets.TempDir; } 
        }

        /// <summary>
        /// Gets or sets the restore delegate.
        /// </summary>
        public IRestoreDelegate RestoreDelegate
        {
            get
            {
                lock (locker)
                {
                    if (restoreDelegate == null)
                    {
                        restoreDelegate = this;
                    }

                    return restoreDelegate;
                }
            }
            set
            {
                lock (locker)
                {
                    restoreDelegate = value;
                }
            }
        }

        /// <summary>
        /// Gets the restore target to execute.
        /// </summary>
        public DatabaseRestoreTargetConfigurationElement Target { get; protected set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Downloads the latest backup set from the AWS service.
        /// </summary>
        /// <returns>The path to the downloaded and decompressed backup file.</returns>
        public string DownloadBackup()
        {
            return DownloadBackup(new GZipCompressor());
        }

        /// <summary>
        /// Downloads the latest backup set from the AWS service.
        /// </summary>
        /// <param name="compressor">The compresor to use when decompressing the downloaded file.</param>
        /// <returns>The path to the downloaded and decompressed backup file.</returns>
        public string DownloadBackup(ICompressor compressor)
        {
            S3Object latest = GetLatestBackupItem();
            string path = Path.Combine(TempDir, latest.Key);
            string fileName = Path.GetFileName(path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            GetObjectRequest request = new GetObjectRequest()
                .WithBucketName(AWSConfig.BucketName)
                .WithKey(latest.Key);

            TransferInfo info = new TransferInfo()
            {
                BytesTransferred = 0,
                FileName = fileName,
                FileSize = 0,
                Target = Target
            };

            TransferDelegate.OnTransferStart(info);

            using (FileStream file = File.Create(path))
            {
                using (GetObjectResponse response = S3Client.GetObject(request))
                {
                    byte[] buffer = new byte[4096];
                    int count = 0;

                    while (0 < (count = response.ResponseStream.Read(buffer, 0, buffer.Length)))
                    {
                        info.BytesTransferred += count;
                        info.FileSize = response.ContentLength;
                        file.Write(buffer, 0, count);

                        TransferDelegate.OnTransferProgress(info);
                    }
                }
            }

            TransferDelegate.OnTransferComplete(info);

            RestoreDelegate.OnDeompressStart(Target);
            string decompressedPath = compressor.Decompress(path);
            RestoreDelegate.OnDecompressComplete(Target);

            File.Delete(path);

            return decompressedPath;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The result of the execution.</returns>
        public override TaskExecutionResult Execute()
        {
            TaskExecutionResult result = new TaskExecutionResult() { Target = Target };
            string path = String.Empty;

            try
            {
                path = DownloadBackup();
                RestoreDatabase(path);
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
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Gets the latest backup set item by last modified date.
        /// </summary>
        /// <returns>The latest backup set.</returns>
        private S3Object GetLatestBackupItem()
        {
            List<S3Object> objects = new List<S3Object>();
            string marker = String.Empty;
            bool truncated = true;

            while (truncated)
            {
                ListObjectsRequest request = new ListObjectsRequest()
                    .WithBucketName(AWSConfig.BucketName)
                    .WithPrefix(AWSPrefix)
                    .WithMarker(marker);

                using (ListObjectsResponse response = S3Client.ListObjects(request))
                {
                    objects.AddRange(response.S3Objects);

                    if (response.IsTruncated)
                    {
                        marker = objects[objects.Count - 1].Key;
                    }
                    else
                    {
                        truncated = false;
                    }
                }
            }

            if (objects.Count == 0)
            {
                throw new InvalidOperationException(String.Concat("There was no backup set found for catalog \"", Target.CatalogName, "\"."));
            }

            return objects.OrderByDescending(o => DateTime.Parse(o.LastModified, CultureInfo.InvariantCulture)).First();
        }

        /// <summary>
        /// Runs the restore script on the downloaded backup set.
        /// </summary>
        /// <param name="path">The path of the backup file to restore.</param>
        public void RestoreDatabase(string path)
        {
            string connectionString = Target.ConnectionString;
            RestoreDelegate.OnRestoreStart(Target);

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
                            new SqlScript("Drop.sql").Text,
                            Target.RestoreCatalogName);

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            SqlConnection.ClearAllPools();

            if (!Directory.Exists(Target.RestorePath))
            {
                Directory.CreateDirectory(Target.RestorePath);
            }

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
                        command.CommandText = new SqlScript("GetFiles.sql").Text;

                        command.Parameters.Add(new SqlParameter("@Path", path));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(files);
                        }
                    }

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;

                        command.Parameters.Add(new SqlParameter("@RestoreCatalog", Target.RestoreCatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", path));

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < files.Rows.Count; i++)
                        {
                            string fileName = files.Rows[i]["LogicalName"].ToString();
                            string filePath = Path.Combine(Target.RestorePath, Path.GetFileName(files.Rows[i]["PhysicalName"].ToString()));

                            command.Parameters.Add(new SqlParameter("@FileName" + i, fileName));
                            command.Parameters.Add(new SqlParameter("@FilePath" + i, filePath));

                            sb.Append(String.Concat("\tMOVE @FileName", i, " TO @FilePath", i, ",\n"));
                        }

                        command.CommandText = String.Format(new SqlScript("Restore.sql").Text, sb.ToString());
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            RestoreDelegate.OnRestoreComplete(Target);
        }

        #endregion

        #region IRestoreDelegate Members

        /// <summary>
        /// Called when a database restore is complete.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreComplete(DatabaseRestoreTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database restore begins.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreStart(DatabaseRestoreTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database backup file has been decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDecompressComplete(DatabaseRestoreTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database backup file is about to be decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDeompressStart(DatabaseRestoreTargetConfigurationElement target) { }

        #endregion
    }
}
