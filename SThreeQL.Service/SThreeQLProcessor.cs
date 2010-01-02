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
        private Scheduler scheduler;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SThreeQLProcessor()
        {
            InitializeComponent();
            scheduler = new Scheduler(SThreeQLConfiguration.Section.Schedules, new EventLogDelegate());
        }

        #region Base Overrides

        /// <summary>
        /// Raises the service's Continue event.
        /// </summary>
        protected override void OnContinue()
        {
            scheduler.Start();
        }

        /// <summary>
        /// Raises the service's Pause event.
        /// </summary>
        protected override void OnPause()
        {
            scheduler.Stop();
        }

        /// <summary>
        /// Raises the service's Start event.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            scheduler.Start();
        }

        /// <summary>
        /// Raises the service's Stop event.
        /// </summary>
        protected override void OnStop()
        {
            scheduler.Stop();
        }

        #endregion
    }
}
