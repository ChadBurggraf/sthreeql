//-----------------------------------------------------------------------
// <copyright file="SThreeQLProcessor.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Service
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceProcess;

    /// <summary>
    /// Runs SThreeQL as a polling service.
    /// </summary>
    public partial class SThreeQLProcessor : ServiceBase
    {
        #region Private Fields

        private static readonly object SyncRoot = new object();
        private Scheduler scheduler;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the SThreeQLProcessor class.
        /// </summary>
        public SThreeQLProcessor()
        {
            this.InitializeComponent();

            this.scheduler = new Scheduler();
            this.scheduler.ScheduleStart += new EventHandler<ScheduleEventArgs>(this.ScheduleStart);
            this.scheduler.ScheduleError += new EventHandler<ScheduleEventArgs>(this.ScheduleError);
            this.scheduler.ScheduleComplete += new EventHandler<ScheduleEventArgs>(this.ScheduleComplete);
            this.scheduler.RestoreTransferStart += new EventHandler<RestoreDatabaseTargetEventArgs>(this.RestoreTransferStart);
            this.scheduler.RestoreComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(this.RestoreComplete);
            this.scheduler.BackupTransferComplete += new EventHandler<DatabaseTargetEventArgs>(this.BackupTransferComplete);
            this.scheduler.BackupStart += new EventHandler<DatabaseTargetEventArgs>(this.BackupStart);
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Raises the service's Continue event.
        /// </summary>
        protected override void OnContinue()
        {
            this.scheduler.Start();
        }

        /// <summary>
        /// Raises the service's Pause event.
        /// </summary>
        protected override void OnPause()
        {
            this.scheduler.Stop();
        }

        /// <summary>
        /// Raises the service's Start event.
        /// </summary>
        /// <param name="args">The start arguments.</param>
        protected override void OnStart(string[] args)
        {
            this.scheduler.Start();
        }

        /// <summary>
        /// Raises the service's Stop event.
        /// </summary>
        protected override void OnStop()
        {
            this.scheduler.Stop();
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Logs a message to the system event log.
        /// </summary>
        /// <param name="isError">A value indicating whether the message is an error.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">The message format arguments.</param>
        private static void LogMessage(bool isError, string message, params string[] args)
        {
            const string Source = "SThreeQL Service";

            lock (SThreeQLProcessor.SyncRoot)
            {
                if (!EventLog.SourceExists(Source))
                {
                    EventLog.CreateEventSource(Source, "Application");
                }

                EventLog.WriteEntry(
                    Source,
                    String.Format(CultureInfo.InvariantCulture, message, args),
                    isError ? EventLogEntryType.Error : EventLogEntryType.Information);
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Raises a BackupStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BackupStart(object sender, DatabaseTargetEventArgs e)
        {
            LogMessage(false, "Executing backup of '{0}' for target '{1}'.", e.CatalogName, e.Name);
        }

        /// <summary>
        /// Raises a BackupTransferComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void BackupTransferComplete(object sender, DatabaseTargetEventArgs e)
        {
            LogMessage(false, "Completed backup of '{0}' for target '{1}' successfully.", e.CatalogName, e.Name);
        }

        /// <summary>
        /// Raises a RestoreComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RestoreComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            LogMessage(false, "Completed restore of '{0}' for target '{1}' successfully.", e.RestoreCatalogName, e.Name);
        }

        /// <summary>
        /// Raises a RestoreTransferStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RestoreTransferStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            LogMessage(false, "Executing restore of '{0}' for target '{1}'.", e.RestoreCatalogName, e.Name);
        }

        /// <summary>
        /// Raises a ScheduleComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ScheduleComplete(object sender, ScheduleEventArgs e)
        {
            LogMessage(false, "Finished all targets in schedule '{0}'.", e.Name);
        }

        /// <summary>
        /// Raises a ScheduleError event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ScheduleError(object sender, ScheduleEventArgs e)
        {
            string target = e.OperationType == ScheduleOperationType.Restore ? "restore" : "backup";
            string message = !String.IsNullOrEmpty(e.TargetName) ?
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing {0} target '{1}' for schedule '{2}'", target, e.TargetName, e.Name) :
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing schedule '{0}'", e.Name);

            if (e.ErrorException != null)
            {
                message += String.Format(CultureInfo.InvariantCulture, ":\n   {0}\n   {1}", e.ErrorException.Message, e.ErrorException.StackTrace);
            }
            else
            {
                message += ".";
            }

            LogMessage(true, message, null);
        }

        /// <summary>
        /// Raises a ScheduleStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ScheduleStart(object sender, ScheduleEventArgs e)
        {
            LogMessage(false, "Executing schedule '{0}'.", e.Name);
        }

        #endregion
    }
}
