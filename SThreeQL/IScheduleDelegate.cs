using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Defines a schedule delegate.
    /// </summary>
    public interface IScheduleDelegate
    {
        /// <summary>
        /// Gets or sets the backup delegate.
        /// </summary>
        IBackupDelegate BackupDelegate { get; set; }

        /// <summary>
        /// Gets or sets the restore delegate.
        /// </summary>
        IRestoreDelegate RestoreDelegate { get; set; }

        /// <summary>
        /// Gets or sets the transfer delegate.
        /// </summary>
        ITransferDelegate TransferDelegate { get; set; }

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a scheduled backup target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The backup target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        void OnBackupError(ScheduleConfigurationElement schedule, DatabaseTargetConfigurationElement target, Exception ex);

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a schedule restore target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The restore target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        void OnRestoreError(ScheduleConfigurationElement schedule, DatabaseRestoreTargetConfigurationElement target, Exception ex);

        /// <summary>
        /// Called when a schedule finishes.
        /// </summary>
        /// <param name="schedule">The schedule that is finishing.</param>
        void OnScheduleFinish(ScheduleConfigurationElement schedule);

        /// <summary>
        /// Called when a schedule starts.
        /// </summary>
        /// <param name="schedule">The schedule that is starting.</param>
        void OnScheduleStart(ScheduleConfigurationElement schedule);
    }
}
