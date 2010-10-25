//-----------------------------------------------------------------------
// <copyright file="BackupTask.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using Amazon.S3.Model;
    using SThreeQL.Configuration;

    /// <summary>
    /// Represents a task for executing the backup procedure on a backup target.
    /// </summary>
    public class BackupTask : AwsTask
    {
        private string backupFileName;

        /// <summary>
        /// Initializes a new instance of the BackupTask class.
        /// </summary>
        /// <param name="target">The backup target to execute.</param>
        public BackupTask(DatabaseTargetConfigurationElement target)
            : base(SThreeQLConfiguration.Section.AwsTargets[target.AwsBucketName]) 
        {
            this.Target = target;
        }

        /// <summary>
        /// Event raised when the backup operation is complete.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupComplete;

        /// <summary>
        /// Event raised when the backup operation is starting.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupStart;

        /// <summary>
        /// Event raised when the compress operation is complete.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> CompressComplete;

        /// <summary>
        /// Event raised when the compress operation is starting.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> CompressStart;

        /// <summary>
        /// Event fired when the task's network transfer is complete.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> TransferComplete;

        /// <summary>
        /// Event fired when the task's network transfer raises a progress tick.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> TransferProgress;

        /// <summary>
        /// Event fired when the task's network transfer starts.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> TransferStart;

        /// <summary>
        /// Gets or sets the configuration element identifying the backup target to execute.
        /// </summary>
        public DatabaseTargetConfigurationElement Target { get; protected set; }

        /// <summary>
        /// Gets the name to use for the backup file.
        /// </summary>
        protected string BackupFileName
        {
            get
            {
                if (this.backupFileName == null)
                {
                    this.backupFileName = String.Concat(
                        EscapeCatalogName(this.Target.CatalogName),
                        "_",
                        DateTime.Now.ToIso8601UtcPathSafeString(),
                        ".bak");
                }

                return this.backupFileName;
            }
        }

        /// <summary>
        /// Gets the configuration-defined temporary directory to use for this task type.
        /// </summary>
        protected override string ConfiguredTempDir 
        { 
            get { return SThreeQLConfiguration.Section.BackupTargets.TempDir; } 
        }

        /// <summary>
        /// Creates a new <see cref="DatabaseTargetEventArgs"/> instance.
        /// </summary>
        /// <param name="task">The <see cref="BackupTask"/> to create the arguments with.</param>
        /// <param name="info">The <see cref="TransferInfo"/> to create the arguments with, if applicable.</param>
        /// <returns>The created event arguments.</returns>
        public static DatabaseTargetEventArgs CreateEventArgs(BackupTask task, TransferInfo info)
        {
            return new DatabaseTargetEventArgs()
            {
                CatalogName = task.Target.CatalogName,
                Name = task.Target.Name,
                Transfer = info
            };
        }

        /// <summary>
        /// Runs the backup script to create a fresh backup file.
        /// </summary>
        /// <returns>The path to the compressed backup file.</returns>
        public string BackupDatabase()
        {
            return this.BackupDatabase(new GZipCompressor());
        }

        /// <summary>
        /// Runs the backup script to create a fresh backup file.
        /// </summary>
        /// <param name="compressor">The compressor to use when compressing the backup file.</param>
        /// <returns>The path to the compressed backup file.</returns>
        public string BackupDatabase(ICompressor compressor)
        {
            string path = Path.Combine(TempDir, Path.GetRandomFileName());

            using (SqlConnection connection = new SqlConnection(this.Target.CreateConnectionString()))
            {
                connection.Open();

                try
                {
                    this.Fire(this.BackupStart);

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;
                        command.CommandText = new SqlScript("Backup.sql").Text;

                        command.Parameters.Add(new SqlParameter("@Catalog", this.Target.CatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", path));
                        command.Parameters.Add(new SqlParameter("@Name", this.Target.CatalogName + " - Full Database Backup"));

                        command.ExecuteNonQuery();
                    }

                    this.Fire(this.BackupComplete);

                    string namedPath = Path.Combine(TempDir, this.BackupFileName);

                    if (File.Exists(namedPath))
                    {
                        File.Delete(namedPath);
                    }

                    File.Move(path, namedPath);

                    this.Fire(this.CompressStart);
                    string compressedPath = compressor.Compress(namedPath);
                    this.Fire(this.CompressComplete);

                    File.Delete(namedPath);
                    return compressedPath;
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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log exceptions rather than bail.")]
        public override TaskExecutionResult Execute()
        {
            TaskExecutionResult result = new TaskExecutionResult() { Target = this.Target };
            string path = String.Empty;

            try
            {
                path = this.BackupDatabase();
                this.UploadBackup(path);
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
                    if (!String.IsNullOrEmpty(path) && File.Exists(path))
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
        /// Uploads the backup set to Aws.
        /// </summary>
        /// <param name="path">The path of the compressed backup to upload.</param>
        public void UploadBackup(string path)
        {
            string fileName = Path.GetFileName(path);

            if (!String.IsNullOrEmpty(this.Target.AwsPrefix))
            {
                fileName = this.Target.AwsPrefix.EndsWith("/", StringComparison.Ordinal) ?
                    this.Target.AwsPrefix + fileName :
                    this.Target.AwsPrefix + "/" + fileName;
            }

            long fileSize = new FileInfo(path).Length;

            using (FileStream file = File.OpenRead(path))
            {
                PutObjectRequest request = new PutObjectRequest()
                        .WithCannedACL(S3CannedACL.Private)
                        .WithBucketName(AwsConfig.BucketName)
                        .WithKey(fileName)
                        .WithTimeout(-1);

                request.InputStream = file;

                TransferInfo info = new TransferInfo()
                {
                    BytesTransferred = 0,
                    FileName = fileName,
                    FileSize = fileSize
                };

                bool uploading = true;

                Thread statusThread = new Thread(new ThreadStart(delegate
                {
                    while (uploading)
                    {
                        try
                        {
                            info.BytesTransferred = file.Position;

                            if (info.BytesTransferred < info.FileSize)
                            {
                                this.Fire(this.TransferProgress, new TransferInfo(info));
                            }

                            Thread.Sleep(250);
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }));

                this.Fire(this.TransferStart, new TransferInfo(info));
                statusThread.Start();

                using (S3Response response = S3Client.PutObject(request))
                {
                    uploading = false;
                    info.BytesTransferred = info.FileSize;
                    this.Fire(this.TransferComplete, new TransferInfo(info));
                }
            }
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        private void Fire(EventHandler<DatabaseTargetEventArgs> handler)
        {
            this.Fire(handler, null);
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        /// <param name="info">The <see cref="TransferInfo"/> to create the event arguments with, if applicable.</param>
        private void Fire(EventHandler<DatabaseTargetEventArgs> handler, TransferInfo info)
        {
            if (handler != null)
            {
                handler(this, CreateEventArgs(this, info));
            }
        }
    }
}
