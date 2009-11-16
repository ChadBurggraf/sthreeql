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
    public class Scheduler
    {
        #region Private Members

        private IDictionary<string, DateTime> inProgress;
        private IDictionary<string, DateTime> pending;
        private TextWriter stdOut, stdError;
        private readonly object locker = new object();
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
        public Scheduler(ScheduleConfigurationElementCollection schedules) : this(schedules, null, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="schedules">The schedules collection this instance should manage.</param>
        /// <param name="stdOut">The text writer to write the standard output stream to.</param>
        /// <param name="stdError">The text writer to write the standard error stream to.</param>
        public Scheduler(ScheduleConfigurationElementCollection schedules, TextWriter stdOut, TextWriter stdError)
        {
            this.stdOut = stdOut;
            this.stdError = stdError;

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
                lock (locker)
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
                lock (locker)
                {
                    if (pending == null)
                    {
                        pending = new Dictionary<string, DateTime>();
                    }

                    return pending;
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

            if (stdOut != null && stdError != null)
            {
                ExecuteSchedule(schedule, stdOut, stdError);
            }
            else
            {
                StringBuilder output = new StringBuilder();
                StringBuilder error = new StringBuilder();

                using (TextWriter outWriter = new StringWriter(output))
                {
                    using (TextWriter errorWriter = new StringWriter(error))
                    {
                        ExecuteSchedule(schedule, outWriter, errorWriter);
                    }
                }

                const string source = "SThreeQL Service";

                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }

                if (output.Length > 0)
                {
                    EventLog.WriteEntry(source, output.ToString(), EventLogEntryType.Information);
                }

                if (error.Length > 0)
                {
                    EventLog.WriteEntry(source, error.ToString(), EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// Concrete schedule executer.
        /// </summary>
        /// <param name="schedule">The schedule to execute.</param>
        /// <param name="stdOut">The text writer to write standard output to.</param>
        /// <param name="stdError">The text writer to write standard error to.</param>
        private void ExecuteSchedule(ScheduleConfigurationElement schedule, TextWriter stdOut, TextWriter stdError)
        {
            foreach (ScheduleTargetConfigurationElement target in schedule.BackupTagets)
            {
                try
                {
                    DatabaseTargetConfigurationElement config = SThreeQLConfiguration.Section.BackupTargets[target.Name];
                    new BackupTask(config).Execute(stdOut, stdError);
                }
                catch (Exception ex)
                {
                    stdError.WriteLine("An unhandled exception occurred when running backup task " + target.Name + ":");
                    stdError.WriteLine(ex.Message);
                    stdError.WriteLine(ex.StackTrace);
                }
            }

            foreach (ScheduleTargetConfigurationElement target in schedule.RestoreTargets)
            {
                try
                {
                    DatabaseRestoreTargetConfigurationElement config = SThreeQLConfiguration.Section.RestoreTargets[target.Name];
                    new RestoreTask(config).Execute(stdOut, stdError);
                }
                catch (Exception ex)
                {
                    stdError.WriteLine("An unhandled exception occurred when running restore task " + target.Name + ":");
                    stdError.WriteLine(ex.Message);
                    stdError.WriteLine(ex.StackTrace);
                }
            }

            lock (locker)
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
                    lock (locker)
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
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static DateTime GetNextExecuteDate(ScheduleConfigurationElement schedule)
        {
            if (DateTime.Now < schedule.StartDate)
            {
                return schedule.StartDate;
            }

            //
            // TODO: The only repeat type is Daily right now.
            //

            int days = (int)Math.Ceiling(DateTime.Now.Subtract(schedule.StartDate).TotalDays);
            return schedule.StartDate.AddDays(days);
        }

        #endregion
    }
}
