//-----------------------------------------------------------------------
// <copyright file="RestoreTask.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Amazon.S3.Model;
    using SThreeQL.Configuration;

    /// <summary>
    /// Represents a task for executing the restore procedure on a restore target.
    /// </summary>
    public class RestoreTask : AwsTask
    {
        private string awsPrefix;

        /// <summary>
        /// Initializes a new instance of the RestoreTask class.
        /// </summary>
        /// <param name="target">The restore target to execute.</param>
        public RestoreTask(DatabaseRestoreTargetConfigurationElement target)
            : base(SThreeQLConfiguration.Section.AwsTargets[target.AwsBucketName]) 
        {
            this.Target = target;
        }

        /// <summary>
        /// Event fired when the decompress operation is complete.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> DecompressComplete;

        /// <summary>
        /// Event fired when the decompress operation is starting.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> DecompressStart;

        /// <summary>
        /// Event fired when the restore operation is complete.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreComplete;

        /// <summary>
        /// Event fired when the restore operation is starting.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreStart;

        /// <summary>
        /// Event fired when the task's network transfer is complete.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> TransferComplete;

        /// <summary>
        /// Event fired when the task's network transfer raises a progress tick.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> TransferProgress;

        /// <summary>
        /// Event fired when the task's network transfer starts.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> TransferStart;

        /// <summary>
        /// Gets or sets the restore target to execute.
        /// </summary>
        public DatabaseRestoreTargetConfigurationElement Target { get; protected set; }

        /// <summary>
        /// Gets the Aws prefix to use when searching for a backup set to restore.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        protected string AwsPrefix
        {
            get
            {
                if (this.awsPrefix == null)
                {
                    if (!String.IsNullOrEmpty(this.Target.AwsPrefix))
                    {
                        this.awsPrefix = this.Target.AwsPrefix;

                        if (!this.awsPrefix.EndsWith("/", StringComparison.Ordinal))
                        {
                            this.awsPrefix += "/";
                        }
                    }

                    this.awsPrefix += EscapeCatalogName(this.Target.CatalogName);
                }

                return this.awsPrefix;
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
        /// Creates a new <see cref="RestoreDatabaseTargetEventArgs"/> instance.
        /// </summary>
        /// <param name="task">The <see cref="RestoreTask"/> to create the arguments with.</param>
        /// <param name="info">The <see cref="TransferInfo"/> to create the arguments with, if applicable.</param>
        /// <returns>The created event arguments.</returns>
        public static RestoreDatabaseTargetEventArgs CreateEventArgs(RestoreTask task, TransferInfo info)
        {
            return new RestoreDatabaseTargetEventArgs()
            {
                CatalogName = task.Target.CatalogName,
                Name = task.Target.Name,
                RestoreCatalogName = task.Target.RestoreCatalogName,
                RestorePath = task.Target.RestorePath,
                Transfer = info
            };
        }

        /// <summary>
        /// Downloads the latest backup set from the Aws service.
        /// </summary>
        /// <returns>The path to the downloaded and decompressed backup file.</returns>
        public string DownloadBackup()
        {
            return this.DownloadBackup(new GZipCompressor());
        }

        /// <summary>
        /// Downloads the latest backup set from the Aws service.
        /// </summary>
        /// <param name="compressor">The compresor to use when decompressing the downloaded file.</param>
        /// <returns>The path to the downloaded and decompressed backup file.</returns>
        public string DownloadBackup(ICompressor compressor)
        {
            S3Object latest = this.GetLatestBackupItem();
            string path = latest.Key;

            if (!String.IsNullOrEmpty(this.Target.AwsPrefix) && path.StartsWith(this.Target.AwsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(this.Target.AwsPrefix.Length);

                if (path.StartsWith("/", StringComparison.Ordinal)) 
                {
                    path = path.Substring(1);
                }
            }

            path = Path.Combine(TempDir, path);
            string fileName = Path.GetFileName(path);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            GetObjectRequest request = new GetObjectRequest()
                .WithBucketName(AwsConfig.BucketName)
                .WithKey(latest.Key);

            TransferInfo info = new TransferInfo()
            {
                BytesTransferred = 0,
                FileName = fileName,
                FileSize = 0
            };

            this.Fire(this.TransferStart, info);

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

                        this.Fire(this.TransferProgress, info);
                    }
                }
            }

            this.Fire(this.TransferComplete, info);

            this.Fire(this.DecompressStart);
            string decompressedPath = compressor.Decompress(path);
            this.Fire(this.DecompressComplete);

            File.Delete(path);

            return decompressedPath;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The result of the execution.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log exceptions rather than bail.")]
        public override TaskExecutionResult Execute()
        {
            TaskExecutionResult result = new TaskExecutionResult() { Target = this.Target };
            string path = String.Empty;

            try
            {
                path = this.DownloadBackup();
                this.RestoreDatabase(path);
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
                catch 
                { 
                }
            }

            return result;
        }

        /// <summary>
        /// Runs the restore script on the downloaded backup set.
        /// </summary>
        /// <param name="path">The path of the backup file to restore.</param>
        public void RestoreDatabase(string path)
        {
            string connectionString = this.Target.CreateConnectionString();
            this.Fire(this.RestoreStart);

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
                            CultureInfo.InvariantCulture,
                            new SqlScript("Drop.sql").Text,
                            this.Target.RestoreCatalogName);

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            SqlConnection.ClearAllPools();

            if (!Directory.Exists(this.Target.RestorePath))
            {
                Directory.CreateDirectory(this.Target.RestorePath);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                try
                {
                    DataTable files = new DataTable() { Locale = CultureInfo.InvariantCulture };

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

                        command.Parameters.Add(new SqlParameter("@RestoreCatalog", this.Target.RestoreCatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", path));

                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < files.Rows.Count; i++)
                        {
                            string fileName = files.Rows[i]["LogicalName"].ToString();
                            string filePath = Path.Combine(this.Target.RestorePath, Path.GetFileName(files.Rows[i]["PhysicalName"].ToString()));

                            command.Parameters.Add(new SqlParameter("@FileName" + i, fileName));
                            command.Parameters.Add(new SqlParameter("@FilePath" + i, filePath));

                            sb.Append(String.Concat("\tMOVE @FileName", i, " TO @FilePath", i, ",\n"));
                        }

                        command.CommandText = String.Format(CultureInfo.InvariantCulture, new SqlScript("Restore.sql").Text, sb.ToString());
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            this.Fire(this.RestoreComplete);
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        private void Fire(EventHandler<RestoreDatabaseTargetEventArgs> handler)
        {
            this.Fire(handler, null);
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        /// <param name="info">The <see cref="TransferInfo"/> to create the event arguments with, if applicable.</param>
        private void Fire(EventHandler<RestoreDatabaseTargetEventArgs> handler, TransferInfo info)
        {
            if (handler != null)
            {
                handler(this, CreateEventArgs(this, info));
            }
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
                    .WithBucketName(AwsConfig.BucketName)
                    .WithPrefix(this.AwsPrefix)
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
                throw new InvalidOperationException(String.Concat("There was no backup set found for catalog \"", this.Target.CatalogName, "\"."));
            }

            return objects.OrderByDescending(o => DateTime.Parse(o.LastModified, CultureInfo.InvariantCulture)).First();
        }
    }
}
