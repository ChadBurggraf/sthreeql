using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Implements <see cref="IBackupDelegate"/>, <see cref="IRestoreDelegate"/>, <see cref="IScheduleDelegate"/> and <see cref="ITransferDelegate"/>
    /// for logging to the system console.
    /// </summary>
    public class ConsoleDelegate : IBackupDelegate, IRestoreDelegate, IScheduleDelegate, ITransferDelegate
    {
        #region Helpers

        /// <summary>
        /// Common schedule delegate error handler.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        static void OnScheduleError(ScheduleConfigurationElement schedule, DatabaseTargetConfigurationElement target, Exception ex)
        {
            string targetType = target is DatabaseRestoreTargetConfigurationElement ? "restore" : "backup";
            string message = target != null ?
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing {0} target {1} for schedule {2}", targetType, target.Name, schedule.Name) :
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing the schedule {0}", schedule.Name);

            if (ex != null)
            {
                message += ":\n";
                message += "   " + ex.Message + "\n";
                message += "   " + ex.StackTrace;
            }
            else
            {
                message += ".";
            }

            Console.Error.WriteLine(message);
        }

        #endregion

        #region IBackupDelegate Members

        /// <summary>
        /// Called when a database backup is complete.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupComplete(DatabaseTargetConfigurationElement target)
        {
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Called when a database backup begins.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupStart(DatabaseTargetConfigurationElement target)
        {
            Console.Write("Backing up database {0}... ", target.CatalogName);
        }

        /// <summary>
        /// Called when a database backup file has been compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressComplete(DatabaseTargetConfigurationElement target)
        {
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Called when a database backup file is about to be compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressStart(DatabaseTargetConfigurationElement target)
        {
            Console.Write("Compressing the backup file... ");
        }

        #endregion

        #region IRestoreDelegate Members

        /// <summary>
        /// Called when a database restore is complete.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreComplete(DatabaseRestoreTargetConfigurationElement target)
        {
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Called when a database restore begins.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreStart(DatabaseRestoreTargetConfigurationElement target)
        {
            Console.Write("Restoring catalog {0}... ", target.RestoreCatalogName);
        }

        /// <summary>
        /// Called when a database backup file has been decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDecompressComplete(DatabaseRestoreTargetConfigurationElement target)
        {
            Console.WriteLine("Done.");
        }

        /// <summary>
        /// Called when a database backup file is about to be decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDeompressStart(DatabaseRestoreTargetConfigurationElement target)
        {
            Console.Write("Decompressing the backup file... ");
        }

        #endregion

        #region IScheduleDelegate Members

        /// <summary>
        /// Gets or sets the backup delegate.
        /// </summary>
        public IBackupDelegate BackupDelegate
        {
            get { return this; }
            set { }
        }

        /// <summary>
        /// Gets or sets the restore delegate.
        /// </summary>
        public IRestoreDelegate RestoreDelegate
        {
            get { return this; }
            set { }
        }

        /// <summary>
        /// Gets or sets the transfer delegate.
        /// </summary>
        public ITransferDelegate TransferDelegate
        {
            get { return this; }
            set { }
        }

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a scheduled backup target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The backup target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        public void OnBackupError(ScheduleConfigurationElement schedule, DatabaseTargetConfigurationElement target, Exception ex)
        {
            OnScheduleError(schedule, target, ex);
        }

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a schedule restore target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The restore target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        public void OnRestoreError(ScheduleConfigurationElement schedule, DatabaseRestoreTargetConfigurationElement target, Exception ex)
        {
            OnScheduleError(schedule, target, ex);
        }

        /// <summary>
        /// Called when a schedule finishes.
        /// </summary>
        /// <param name="schedule">The schedule that is finishing.</param>
        public void OnScheduleFinish(ScheduleConfigurationElement schedule)
        {
            Console.WriteLine("Schedule {0} is complete at {1:F}.\n", schedule.Name, DateTime.Now);
        }

        /// <summary>
        /// Called when a schedule starts.
        /// </summary>
        /// <param name="schedule">The schedule that is starting.</param>
        public void OnScheduleStart(ScheduleConfigurationElement schedule)
        {
            Console.WriteLine("Executing schedule {0} at {1:F}.\n", schedule.Name, DateTime.Now);
        }

        #endregion

        #region ITransferDelegate Members

        /// <summary>
        /// Called when a transfer is complete.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferComplete(TransferInfo info)
        {
            Console.WriteLine();

            if (info.Target is DatabaseRestoreTargetConfigurationElement)
            {
                Console.WriteLine("Download complete.");   
            }
            else
            {
                Console.WriteLine("Upload complete.");
            }
        }

        /// <summary>
        /// Called when a transfer's progress has been updated..
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferProgress(TransferInfo info)
        {
            if (info.FileSize > 0 && info.BytesTransferred > 0)
            {
                Console.CursorLeft = 0;
                Console.Write("{0} of {1} ({2}%)          ",
                    info.BytesTransferred.ToFileSize(),
                    info.FileSize.ToFileSize(),
                    (int)((double)info.BytesTransferred / info.FileSize * 100));
            }
        }

        /// <summary>
        /// Called when a transfer begins.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferStart(TransferInfo info)
        {
            if (info.Target is DatabaseRestoreTargetConfigurationElement)
            {
                Console.WriteLine("Downloading file {0}...", info.FileName);
            }
            else
            {
                Console.WriteLine("Uploading file {0} ({1})...", info.FileName, info.FileSize.ToFileSize());
            }
        }

        #endregion
    }
}
