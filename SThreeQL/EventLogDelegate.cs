using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Implements <see cref="IBackupDelegate"/>, <see cref="IRestoreDelegate"/>, <see cref="IScheduleDelegate"/> and <see cref="ITransferDelegate"/>
    /// for logging to the system event log.
    /// </summary>
    public class EventLogDelegate : IBackupDelegate, IRestoreDelegate, IScheduleDelegate, ITransferDelegate
    {
        #region Helpers

        /// <summary>
        /// Logs a message to the system event log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="isError">A value indicating whether the message is an error.</param>
        static void LogMessage(string message, bool isError)
        {
            const string source = "SThreeQL Service";

            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, "Application");
            }

            EventLog.WriteEntry(source, message, isError ? EventLogEntryType.Error : EventLogEntryType.Information);
        }

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

            LogMessage(message, true);
        }

        #endregion

        #region IBackupDelegate Members

        /// <summary>
        /// Called when a database backup is complete.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupComplete(DatabaseTargetConfigurationElement target)
        {
        }

        /// <summary>
        /// Called when a database backup begins.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnBackupStart(DatabaseTargetConfigurationElement target)
        {
            LogMessage(String.Format(CultureInfo.InvariantCulture, "Executing backup of {0} for target {1}.", target.CatalogName, target.Name), false);
        }

        /// <summary>
        /// Called when a database backup file has been compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressComplete(DatabaseTargetConfigurationElement target)
        {
        }

        /// <summary>
        /// Called when a database backup file is about to be compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        public void OnCompressStart(DatabaseTargetConfigurationElement target)
        {
        }

        #endregion

        #region IRestoreDelegateMembers

        /// <summary>
        /// Called when a database restore is complete.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreComplete(DatabaseRestoreTargetConfigurationElement target)
        {
            LogMessage(String.Format(CultureInfo.InvariantCulture, "Completed restore of {0} for target {1} successfully.", target.RestoreCatalogName, target.Name), false);
        }

        /// <summary>
        /// Called when a database restore begins.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnRestoreStart(DatabaseRestoreTargetConfigurationElement target)
        {
        }

        /// <summary>
        /// Called when a database backup file has been decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDecompressComplete(DatabaseRestoreTargetConfigurationElement target)
        {
        }

        /// <summary>
        /// Called when a database backup file is about to be decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        public void OnDeompressStart(DatabaseRestoreTargetConfigurationElement target)
        {
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
            LogMessage(String.Format(CultureInfo.InvariantCulture, "Schedule {0} is complete.", schedule.Name), false);
        }

        /// <summary>
        /// Called when a schedule starts.
        /// </summary>
        /// <param name="schedule">The schedule that is starting.</param>
        public void OnScheduleStart(ScheduleConfigurationElement schedule)
        {
            LogMessage(String.Format(CultureInfo.InvariantCulture, "Executing schedule {0}.", schedule.Name), false);
        }

        #endregion

        #region ITransferDelegate Members

        /// <summary>
        /// Called when a transfer is complete.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferComplete(TransferInfo info)
        {
            if (!(info.Target is DatabaseRestoreTargetConfigurationElement))
            {
                LogMessage(String.Format(CultureInfo.InvariantCulture, "Completed backup of {0} for target {1} successfully.", info.Target.CatalogName, info.Target.Name), false);
            }
        }

        /// <summary>
        /// Called when a transfer's progress has been updated..
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferProgress(TransferInfo info) { }

        /// <summary>
        /// Called when a transfer begins.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        public void OnTransferStart(TransferInfo info)
        {
            if (info.Target is DatabaseRestoreTargetConfigurationElement)
            {
                LogMessage(String.Format(CultureInfo.InvariantCulture, "Downloading restore {0} for target {1}.", info.FileName, info.Target.Name), false);
            }
        }

        #endregion
    }
}
