using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Base class for tasks.
    /// </summary>
    public abstract class Task
    {
        private static readonly object locker = new object();
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
                if (tempDir == null)
                {
                    tempDir = !String.IsNullOrEmpty(ConfiguredTempDir) ?
                        ConfiguredTempDir :
                        Path.GetTempPath();
                }

                return tempDir;
            }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The result of the execution.</returns>
        public abstract TaskExecutionResult Execute();

        protected virtual void RunOnComplete(DatabaseTargetConfigurationElement target)
        {
            if (target.OnComplete != null)
            {
                Process process = new Process();
                process.StartInfo.FileName = target.OnComplete.Executable;
                process.StartInfo.UseShellExecute = false;

                if (!String.IsNullOrEmpty(target.OnComplete.Arguments))
                {
                    process.StartInfo.Arguments = target.OnComplete.Arguments;
                }

                if (!String.IsNullOrEmpty(target.OnComplete.WorkingDirectory))
                {
                    process.StartInfo.WorkingDirectory = target.OnComplete.WorkingDirectory;
                }

                
            }
        }

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
                "_"
            );
        }
    }
}
