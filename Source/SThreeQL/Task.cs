//-----------------------------------------------------------------------
// <copyright file="Task.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <summary>
    /// Represents the base class for tasks.
    /// </summary>
    public abstract class Task : IDisposable
    {
        private string tempDir;
        private Control control;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the Task class.
        /// </summary>
        protected Task()
        {
            this.control = new Control();
            this.control.CreateControl();
        }

        /// <summary>
        /// Finalizes an instance of the Task class.
        /// </summary>
        ~Task()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the configuration-defined temporary directory to use for this task type.
        /// </summary>
        protected abstract string ConfiguredTempDir { get; }

        /// <summary>
        /// Gets the directory to write temporary files to.
        /// </summary>
        protected string TempDir
        {
            get
            {
                if (this.tempDir == null)
                {
                    this.tempDir = !String.IsNullOrEmpty(this.ConfiguredTempDir) ?
                        this.ConfiguredTempDir :
                        Path.GetTempPath();
                }

                return this.tempDir;
            }
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The result of the execution.</returns>
        public abstract TaskExecutionResult Execute();

        /// <summary>
        /// Escapes a catalog name to be pretty and URL-safe.
        /// </summary>
        /// <param name="catalogName">The catalog name to escape.</param>
        /// <returns>The escaped catalog name.</returns>
        protected static string EscapeCatalogName(string catalogName)
        {
            return Regex.Replace(
                Regex.Replace(catalogName, @"[^a-z0-9]", "_", RegexOptions.IgnoreCase),
                @"_+",
                "_");
        }

        /// <summary>
        /// Disposes of resources used by this instance.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose of managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.control != null)
                    {
                        this.control.Dispose();
                        this.control = null;
                    }
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Invokes the given <see cref="Action"/> on the main thread.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to invoke.</param>
        protected void InvokeOnMainThread(Action action)
        {
            if (action != null)
            {
                this.control.Invoke(action);
            }
        }
    }
}
