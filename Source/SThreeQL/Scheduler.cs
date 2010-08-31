//-----------------------------------------------------------------------
// <copyright file="Scheduler.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using SThreeQL.Configuration;

    /// <summary>
    /// Schedules targets based on the configuration.
    /// </summary>
    public class Scheduler
    {
        #region Private Fields

        private Dictionary<string, DateTime> inProgress;
        private Dictionary<string, DateTime> pending;
        private Thread god;
        private bool running;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        public Scheduler() : 
            this(SThreeQLConfiguration.Section.Schedules) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the Scheduler class.
        /// </summary>
        /// <param name="schedules">The schedules collection this instance should manage.</param>
        public Scheduler(ScheduleConfigurationElementCollection schedules)
        {
            this.inProgress = new Dictionary<string, DateTime>();
            this.pending = new Dictionary<string, DateTime>();

            foreach (ScheduleConfigurationElement schedule in schedules)
            {
                this.pending.Add(schedule.Name, GetNextExecuteDate(schedule));
            }

            this.god = new Thread(new ThreadStart(this.OnGodStart));
            this.god.Start();
        }

        #endregion

        #region Events

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
        public event EventHandler<DatabaseTargetEventArgs> BackupCompressComplete;

        /// <summary>
        /// Event raised when the compress operation is starting.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupCompressStart;

        /// <summary>
        /// Event fired when the task's network transfer is complete.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupTransferComplete;

        /// <summary>
        /// Event fired when the task's network transfer raises a progress tick.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupTransferProgress;

        /// <summary>
        /// Event fired when the task's network transfer starts.
        /// </summary>
        public event EventHandler<DatabaseTargetEventArgs> BackupTransferStart;

        /// <summary>
        /// Event fired when the decompress operation is complete.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreDecompressComplete;

        /// <summary>
        /// Event fired when the decompress operation is starting.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreDecompressStart;

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
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreTransferComplete;

        /// <summary>
        /// Event fired when the task's network transfer raises a progress tick.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreTransferProgress;

        /// <summary>
        /// Event fired when the task's network transfer starts.
        /// </summary>
        public event EventHandler<RestoreDatabaseTargetEventArgs> RestoreTransferStart;

        /// <summary>
        /// Event raised when a schedule has completed execution.
        /// </summary>
        public event EventHandler<ScheduleEventArgs> ScheduleComplete;
        
        /// <summary>
        /// Event raised when an error occurrs while executing a schedule.
        /// </summary>
        public event EventHandler<ScheduleEventArgs> ScheduleError;

        /// <summary>
        /// Event raised when a schedule is starting execution.
        /// </summary>
        public event EventHandler<ScheduleEventArgs> ScheduleStart;

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the next execution date for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to get the next execution date for.</param>
        /// <returns>The schedule's next execution date.</returns>
        public static DateTime GetNextExecuteDate(ScheduleConfigurationElement schedule)
        {
            return GetNextExecuteDate(schedule, DateTime.Now);
        }

        /// <summary>
        /// Gets the next execution date for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to get the next execution date for.</param>
        /// <param name="now">The reference time to compare schedule dates to.</param>
        /// <returns>The schedule's next execution date.</returns>
        public static DateTime GetNextExecuteDate(ScheduleConfigurationElement schedule, DateTime now)
        {
            if (now < schedule.StartDate)
            {
                return schedule.StartDate;
            }

            /*
             * TODO: The only repeat type is Daily right now.
             */

            int days = (int)Math.Ceiling(now.Subtract(schedule.StartDate).TotalDays);
            return schedule.StartDate.AddDays(days);
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        public void Start()
        {
            this.running = true;
        }

        /// <summary>
        /// Stops the scheduler. Does not stop any tasks that are already in progress.
        /// </summary>
        public void Stop()
        {
            this.running = false;
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Executes the given schedule.
        /// </summary>
        /// <param name="schd">The schedule object to execute.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log exceptions rather than bail.")]
        private void ExecuteSchedule(object schd)
        {
            ScheduleConfigurationElement schedule = (ScheduleConfigurationElement)schd;
            this.Fire(this.ScheduleStart, schedule, null, null);

            foreach (ScheduleTargetConfigurationElement target in schedule.BackupTargets)
            {
                DatabaseTargetConfigurationElement config = null;

                try
                {
                    config = SThreeQLConfiguration.Section.BackupTargets[target.Name];

                    BackupTask task = new BackupTask(config);
                    task.BackupComplete += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupComplete);
                    task.BackupStart += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupStart);
                    task.CompressComplete += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupCompressComplete);
                    task.CompressStart += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupCompressStart);
                    task.TransferComplete += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupTransferComplete);
                    task.TransferProgress += new EventHandler<DatabaseTargetEventArgs>(this.OnBackupTransferProgress);
                    task.TransferStart += new EventHandler<DatabaseTargetEventArgs>(this.OnTransferStart);

                    task.Execute();
                }
                catch (Exception ex)
                {
                    this.Fire(this.ScheduleError, schedule, config, ex);
                }
            }

            foreach (ScheduleTargetConfigurationElement target in schedule.RestoreTargets)
            {
                DatabaseRestoreTargetConfigurationElement config = null;

                try
                {
                    config = SThreeQLConfiguration.Section.RestoreTargets[target.Name];

                    RestoreTask task = new RestoreTask(config);
                    task.DecompressComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreDecompressComplete);
                    task.DecompressStart += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreDecompressStart);
                    task.RestoreComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreComplete);
                    task.RestoreStart += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreStart);
                    task.TransferComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreTransferComplete);
                    task.TransferProgress += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreTransferProgress);
                    task.TransferStart += new EventHandler<RestoreDatabaseTargetEventArgs>(this.OnRestoreTransferStart);
                    
                    task.Execute();
                }
                catch (Exception ex)
                {
                    this.Fire(this.ScheduleError, schedule, config, ex);
                }
            }

            this.Fire(this.ScheduleComplete, schedule, null, null);

            lock (this)
            {
                this.inProgress.Remove(schedule.Name);
                this.pending.Add(schedule.Name, GetNextExecuteDate(schedule));
            }
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        /// <param name="args">The arguments to fire the event with.</param>
        private void Fire(EventHandler<DatabaseTargetEventArgs> handler, DatabaseTargetEventArgs args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        /// <param name="args">The arguments to fire the event with.</param>
        private void Fire(EventHandler<RestoreDatabaseTargetEventArgs> handler, RestoreDatabaseTargetEventArgs args)
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Fires an event for this instance.
        /// </summary>
        /// <param name="handler">The event handler to fire.</param>
        /// <param name="schedule">The schedule to fire the event for.</param>
        /// <param name="target">The target to fire the event for, if applicable.</param>
        /// <param name="exception">The exeption to fire the event for, if applicable.</param>
        private void Fire(EventHandler<ScheduleEventArgs> handler, ScheduleConfigurationElement schedule, DatabaseTargetConfigurationElement target, Exception exception)
        {
            if (handler != null)
            {
                ScheduleEventArgs args = new ScheduleEventArgs()
                {
                    ErrorException = exception,
                    Name = schedule.Name,
                    RepeatType = schedule.Repeat,
                    StartDate = schedule.StartDate
                };

                if (target != null)
                {
                    args.OperationType = target is DatabaseRestoreTargetConfigurationElement ? ScheduleOperationType.Restore : ScheduleOperationType.Backup;
                    args.TargetName = target.Name;
                }

                handler(this, args);
            }
        }

        /// <summary>
        /// Raises a backup task's BackupStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupStart(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupStart, e);
        }

        /// <summary>
        /// Raises a backup task's BackupComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupComplete(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupComplete, e);
        }

        /// <summary>
        /// Raises a backup task's CompressComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupCompressComplete(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupCompressComplete, e);
        }

        /// <summary>
        /// Raises a backup task's CompressStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupCompressStart(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupCompressStart, e);
        }

        /// <summary>
        /// Raises a backup task's TransferComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupTransferComplete(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupTransferComplete, e);
        }

        /// <summary>
        /// Raises a backup task's TransferProgress event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnBackupTransferProgress(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupTransferProgress, e);
        }

        /// <summary>
        /// Raises a backup task's TransferStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnTransferStart(object sender, DatabaseTargetEventArgs e)
        {
            this.Fire(this.BackupTransferStart, e);
        }

        /// <summary>
        /// Raised in the god thread's ThreadStart.
        /// </summary>
        private void OnGodStart()
        {
            while (true)
            {
                lock (this)
                {
                    if (this.running)
                    {
                        var q = (from kvp in this.pending
                                 where kvp.Value <= DateTime.Now
                                 select kvp).ToArray();

                        foreach (KeyValuePair<string, DateTime> kvp in q)
                        {
                            this.pending.Remove(kvp.Key);
                            this.inProgress.Add(kvp.Key, kvp.Value);
                            new Thread(this.ExecuteSchedule).Start(SThreeQLConfiguration.Section.Schedules[kvp.Key]);
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Raises a restore target's RestoreComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreComplete, e);
        }

        /// <summary>
        /// Raises a restore target's DecompressComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreDecompressComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreDecompressComplete, e);
        }

        /// <summary>
        /// Raises a restore target's DecompressStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreDecompressStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreDecompressStart, e);
        }

        /// <summary>
        /// Raises a restore target's RestoreStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreStart, e);
        }

        /// <summary>
        /// Raises a restore target's TransferComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreTransferComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreTransferComplete, e);
        }

        /// <summary>
        /// Raises a restore target's TransferProgress event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreTransferProgress(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreTransferProgress, e);
        }

        /// <summary>
        /// Raises a restore target's TransferStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRestoreTransferStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            this.Fire(this.RestoreTransferStart, e);
        }

        #endregion
    }
}
