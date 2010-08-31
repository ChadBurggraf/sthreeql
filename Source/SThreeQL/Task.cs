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

    /// <summary>
    /// Represents the base class for tasks.
    /// </summary>
    public abstract class Task
    {
        private string tempDir;

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
    }
}
