using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Schedules targets based on the configuration.
    /// </summary>
    public class Scheduler : IScheduleDelegate
    {
        #region Private Members

        private IDictionary<string, DateTime> inProgress;
        private IDictionary<string, DateTime> pending;
        private IScheduleDelegate scheduleDelegate;
        private Thread god;
        private bool running;

        #endregion

        #region Construction

        /// <summary>
        /// Constructor.
        /// </summary>
        public Scheduler() : this(SThreeQLConfiguration.Section.Schedules) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schedules">The schedules collection this instance should manage.</param>
        public Scheduler(ScheduleConfigurationElementCollection schedules) : this(schedules, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schedules">The schedules collection this instance should manage.</param>
        /// <param name="scheduleDelegate">The delegate to use.</param>
        public Scheduler(ScheduleConfigurationElementCollection schedules, IScheduleDelegate scheduleDelegate)
        {
            this.scheduleDelegate = scheduleDelegate;

            foreach (ScheduleConfigurationElement schedule in schedules)
            {
                Pending.Add(schedule.Name, GetNextExecuteDate(schedule));
            }

            god = new Thread(new ThreadStart(OnGodStart));
            god.Start();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a collection of schedules that are currently in progress.
        /// </summary>
        protected IDictionary<string, DateTime> InProgress
        {
            get
            {
                lock (this)
                {
                    if (inProgress == null)
                    {
                        inProgress = new Dictionary<string, DateTime>();
                    }

                    return inProgress;
                }
            }
        }

        /// <summary>
        /// Gets a collection of schedules that are currently pending.
        /// </summary>
        protected IDictionary<string, DateTime> Pending
        {
            get
            {
                lock (this)
                {
                    if (pending == null)
                    {
                        pending = new Dictionary<string, DateTime>();
                    }

                    return pending;
                }
            }
        }

        /// <summary>
        /// Gets the schedule's delegate.
        /// </summary>
        public IScheduleDelegate ScheduleDelegate
        {
            get
            {
                lock (this)
                {
                    if (scheduleDelegate == null)
                    {
                        scheduleDelegate = this;
                    }

                    return scheduleDelegate;
                }
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Executes the given schedule.
        /// </summary>
        /// <param name="schd">The schedule object to execute.</param>
        private void ExecuteSchedule(object schd)
        {
            ScheduleConfigurationElement schedule = (ScheduleConfigurationElement)schd;
            ScheduleDelegate.OnScheduleStart(schedule);

            foreach (ScheduleTargetConfigurationElement target in schedule.BackupTagets)
            {
                DatabaseTargetConfigurationElement config = null;

                try
                {
                    config = SThreeQLConfiguration.Section.BackupTargets[target.Name];
                    BackupTask task = new BackupTask(config);
                    task.BackupDelegate = ScheduleDelegate.BackupDelegate;
                    task.TransferDelegate = ScheduleDelegate.TransferDelegate;
                    task.Execute();
                }
                catch (Exception ex)
                {
                    ScheduleDelegate.OnBackupError(schedule, config, ex);
                }
            }

            foreach (ScheduleTargetConfigurationElement target in schedule.RestoreTargets)
            {
                DatabaseRestoreTargetConfigurationElement config = null;

                try
                {
                    config = SThreeQLConfiguration.Section.RestoreTargets[target.Name];
                    RestoreTask task = new RestoreTask(config);
                    task.RestoreDelegate = ScheduleDelegate.RestoreDelegate;
                    task.TransferDelegate = ScheduleDelegate.TransferDelegate;
                    task.Execute();
                }
                catch (Exception ex)
                {
                    ScheduleDelegate.OnRestoreError(schedule, config, ex);
                }
            }

            ScheduleDelegate.OnScheduleFinish(schedule);

            lock (this)
            {
                InProgress.Remove(schedule.Name);
                Pending.Add(schedule.Name, GetNextExecuteDate(schedule));
            }
        }

        /// <summary>
        /// Raised in the god thread's ThreadStart.
        /// </summary>
        private void OnGodStart()
        {
            while (true)
            {
                if (running)
                {
                    lock (this)
                    {
                        var q = (from kvp in Pending
                                 where kvp.Value <= DateTime.Now
                                 select kvp).ToArray();

                        foreach (KeyValuePair<string, DateTime> kvp in q)
                        {
                            Pending.Remove(kvp.Key);
                            InProgress.Add(kvp);
                            new Thread(ExecuteSchedule).Start(SThreeQLConfiguration.Section.Schedules[kvp.Key]);
                        }
                    }
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Starts the scheduler.
        /// </summary>
        public void Start()
        {
            running = true;
        }

        /// <summary>
        /// Stops the scheduler. Does not stop any tasks that are already in progress.
        /// </summary>
        public void Stop()
        {
            running = false;
        }

        #endregion

        #region Static Methods

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

            //
            // TODO: The only repeat type is Daily right now.
            //

            int days = (int)Math.Ceiling(now.Subtract(schedule.StartDate).TotalDays);
            return schedule.StartDate.AddDays(days);
        }

        #endregion

        #region IScheduleDelegate Members

        /// <summary>
        /// Gets or sets the backup delegate.
        /// </summary>
        public IBackupDelegate BackupDelegate { get; set; }

        /// <summary>
        /// Gets or sets the restore delegate.
        /// </summary>
        public IRestoreDelegate RestoreDelegate { get; set; }

        /// <summary>
        /// Gets or sets the transfer delegate.
        /// </summary>
        public ITransferDelegate TransferDelegate { get; set; }

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a scheduled backup target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The backup target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        public void OnBackupError(ScheduleConfigurationElement schedule, DatabaseTargetConfigurationElement target, Exception ex) { }

        /// <summary>
        /// Called when an uncaught exception occurs during the execution of a schedule restore target.
        /// </summary>
        /// <param name="schedule">The schedule that was executing when the error occurred.</param>
        /// <param name="target">The restore target that caused the error.</param>
        /// <param name="ex">The exception that occurred.</param>
        public void OnRestoreError(ScheduleConfigurationElement schedule, DatabaseRestoreTargetConfigurationElement target, Exception ex) { }

        /// <summary>
        /// Called when a schedule finishes.
        /// </summary>
        /// <param name="schedule">The schedule that is finishing.</param>
        public void OnScheduleFinish(ScheduleConfigurationElement schedule) { }

        /// <summary>
        /// Called when a schedule starts.
        /// </summary>
        /// <param name="schedule">The schedule that is starting.</param>
        public void OnScheduleStart(ScheduleConfigurationElement schedule) { }

        #endregion
    }
}
