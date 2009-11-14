using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using SThreeQL.Configuration;

namespace SThreeQL.Service
{
    /// <summary>
    /// Runs SThreeQL as a polling service.
    /// </summary>
    partial class SThreeQLProcessor : ServiceBase
    {
        private bool running;
        int interval;

        /*/// <summary>
        /// Constructor.
        /// </summary>
        public SThreeQLProcessor()
        {
            InitializeComponent();

            // Convert from hours to miliseconds.
            interval = (int)(SThreeQLConfiguration.Section.ServiceInterval * 60 * 60 * 1000);
        }

        /// <summary>
        /// Delegate used to set a timeout for repeated executions of a method.
        /// </summary>
        protected delegate void SetTimeoutDelegate();

        /// <summary>
        /// Processes all of the configured tasks.
        /// </summary>
        protected void Process()
        {
            if (running)
            {
                ProcessBackups();
                ProcessRestores();
            }

            // Set a timeout and call Process() again after the poll time has elapsed.
            new SetTimeoutDelegate(delegate()
            {
                Thread.Sleep(interval);
            }).BeginInvoke(new AsyncCallback(delegate(IAsyncResult result)
            {
                Process();
            }), new object());
        }

        /// <summary>
        /// Processes all of the configured backup tasks.
        /// </summary>
        protected void ProcessBackups()
        {
            /*foreach (DatabaseTargetConfigurationElement config in SThreeQLConfiguration.Section.BackupTargets)
            {
                try
                {
                    TaskExecutionResult result = new BackupTask(config).Execute();

                    if (!result.Success)
                    {
                        WriteTaskFailureApplicationEvent(config, result.Exception);
                    }
                }
                catch (Exception ex)
                {
                    WriteTaskFailureApplicationEvent(config, ex);
                }
            }
        }

        /// <summary>
        /// Processes all of the configured restore tasks.
        /// </summary>
        protected void ProcessRestores()
        {
            /*foreach (DatabaseRestoreTargetConfigurationElement config in SThreeQLConfiguration.Section.RestoreTargets)
            {
                try
                {
                    TaskExecutionResult result = new RestoreTask(config).Execute();

                    if (!result.Success)
                    {
                        WriteTaskFailureApplicationEvent(config, result.Exception);
                    }
                }
                catch (Exception ex)
                {
                    WriteTaskFailureApplicationEvent(config, ex);
                }
            }
        }

        /// <summary>
        /// Writes a task failure event to the system event log.
        /// </summary>
        /// <param name="config">The task that failed.</param>
        /// <param name="ex">The exception that was thrown by the task.</param>
        protected void WriteTaskFailureApplicationEvent(DatabaseTargetConfigurationElement config, Exception ex)
        {
            const string name = "SThreeQL Service";

            try
            {
                if (!EventLog.SourceExists(name))
                {
                    EventLog.CreateEventSource(name, "Application");
                }

                EventLog.WriteEntry(name, String.Concat("An exception of type \"",
                    ex.GetType().ToString(),
                    "\" was thrown while processing task \"",
                    config.Name,
                    "\".\n\n",
                    ex.Message,
                    "\n\n",
                    ex.StackTrace), EventLogEntryType.Error);
            }
            catch
            {
                // Whatever.
            }
        }

        #region Base Overrides

        /// <summary>
        /// Raises the service's Continue event.
        /// </summary>
        protected override void OnContinue()
        {
            running = true;

            new SetTimeoutDelegate(delegate()
            {
                Thread.Sleep(1000);
            }).BeginInvoke(new AsyncCallback(delegate(IAsyncResult result)
            {
                Process();
            }), new object());
        }

        /// <summary>
        /// Raises the service's Pause event.
        /// </summary>
        protected override void OnPause()
        {
            running = false;
        }

        /// <summary>
        /// Raises the service's Start event.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            running = true;

            new SetTimeoutDelegate(delegate()
            {
                Thread.Sleep(1000);
            }).BeginInvoke(new AsyncCallback(delegate(IAsyncResult result)
            {
                Process();
            }), new object());
        }

        /// <summary>
        /// Raises the service's Stop event.
        /// </summary>
        protected override void OnStop()
        {
            running = false;
        }

        #endregion*/
    }
}
