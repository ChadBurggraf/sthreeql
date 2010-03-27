using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Represents a task for executing the backup procedure on a backup target.
    /// </summary>
    public class BackupTask : AWSTask, IBackupDelegate
    {
        #region Member Variables

        private string backupFileName;
        private IBackupDelegate backupDelegate;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The backup target to execute.</param>
        public BackupTask(DatabaseTargetConfigurationElement target)
            : base(SThreeQLConfiguration.Section.AWSTargets[target.AWSBucketName]) 
        {
            Target = target;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the backup delegate.
        /// </summary>
        public IBackupDelegate BackupDelegate
        {
            get
            {
                lock (this)
                {
                    if (backupDelegate == null)
                    {
                        backupDelegate = this;
                    }

                    return backupDelegate;
                }
            }
            set
            {
                lock (this)
                {
                    backupDelegate = value;
                }
            }
        }

        /// <summary>
        /// Gets the name to use for the backup file.
        /// </summary>
        protected string BackupFileName
        {
            get
            {
                if (backupFileName == null)
                {
                    backupFileName = String.Concat(
                        EscapeCatalogName(Target.CatalogName),
                        "_",
                        DateTime.Now.ToISO8601UTCPathSafeString(),
                        ".bak"
                    );
                }

                return backupFileName;
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
        /// Gets the configuration element identifying the backup target to execute.
        /// </summary>
        public DatabaseTargetConfigurationElement Target { get; protected set; }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Runs the backup script to create a fresh backup file.
        /// </summary>
        /// <returns>The path to the compressed backup file.</returns>
        public string BackupDatabase()
        {
            return BackupDatabase(new GZipCompressor());
        }

        /// <summary>
        /// Runs the backup script to create a fresh backup file.
        /// </summary>
        /// <param name="compressor">The compressor to use when compressing the backup file.</param>
        /// <returns>The path to the compressed backup file.</returns>
        public string BackupDatabase(ICompressor compressor)
        {
            string path = Path.Combine(TempDir, Path.GetRandomFileName());

            using (SqlConnection connection = new SqlConnection(Target.ConnectionString))
            {
                connection.Open();

                try
                {
                    BackupDelegate.OnBackupStart(Target);

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;
                        command.CommandText = new SqlScript("Backup.sql").Text;

                        command.Parameters.Add(new SqlParameter("@Catalog", Target.CatalogName));
                        command.Parameters.Add(new SqlParameter("@Path", path));
                        command.Parameters.Add(new SqlParameter("@Name", Target.CatalogName + " - Full Database Backup"));

                        command.ExecuteNonQuery();
                    }

                    BackupDelegate.OnBackupComplete(Target);

                    string namedPath = Path.Combine(TempDir, BackupFileName);

                    if (File.Exists(namedPath))
                    {
                        File.Delete(namedPath);
                    }

                    File.Move(path, namedPath);

                    BackupDelegate.OnCompressStart(Target);
                    string compressedPath = compressor.Compress(namedPath);
                    BackupDelegate.OnCompressComplete(Target);

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
        public override TaskExecutionResult Execute()
        {
            TaskExecutionResult result = new TaskExecutionResult() { Target = Target };
            string path = String.Empty;

            try
            {
                path = BackupDatabase();
                UploadBackup(path);
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
                catch { }
            }

            return result;
        }

        /// <summary>
        /// Uploads the backup set to AWS.
        /// </summary>
        /// <param name="path">The path of the compressed backup to upload.</param>
        public void UploadBackup(string path)
        {
            string fileName = Path.GetFileName(path);

            if (!String.IsNullOrEmpty(Target.AWSPrefix))
            {
                fileName = Target.AWSPrefix.EndsWith("/", StringComparison.Ordinal) ? 
                    Target.AWSPrefix + fileName : 
                    Target.AWSPrefix + "/" + fileName;
            }

            long fileSize = new FileInfo(path).Length;

            using (FileStream file = File.OpenRead(path))
            {
                PutObjectRequest request = new PutObjectRequest()
                    .WithCannedACL(S3CannedACL.Private)
                    .WithBucketName(AWSConfig.BucketName)
                    .WithKey(fileName);

                request.InputStream = file;

                TransferInfo info = new TransferInfo()
                {
                    BytesTransferred = 0,
                    FileName = fileName,
                    FileSize = fileSize,
                    Target = Target
                };

                bool uploading = true;

                Thread uploadThread = new Thread(new ThreadStart(delegate()
                {
                    while (uploading)
                    {
                        try
                        {
                            info.BytesTransferred = file.Position;

                            if (info.BytesTransferred < info.FileSize)
                            {
                                TransferDelegate.OnTransferProgress(info);
                            }

                            Thread.Sleep(500);
                        }
                        catch (ObjectDisposedException) { }
                    }
                }));

                TransferDelegate.OnTransferStart(info);
                uploadThread.Start();

                using (S3Response response = S3Client.PutObject(request))
                {
                    uploading = false;
                    info.BytesTransferred = info.FileSize;
                    TransferDelegate.OnTransferComplete(info);
                }
            }
        }

        #endregion

        #region IBackupDelegate Members

        /// <summary>
        /// Called when a database backup is complete.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupComplete(DatabaseTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database backup begins.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupStart(DatabaseTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database backup file has been compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressComplete(DatabaseTargetConfigurationElement target) { }

        /// <summary>
        /// Called when a database backup file is about to be compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressStart(DatabaseTargetConfigurationElement target) { }

        #endregion
    }
}
