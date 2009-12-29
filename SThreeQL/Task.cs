using System;
using System.Collections.Generic;
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
        private static string tempDir;

        /// <summary>
        /// Gets the directory to write temporary files to.
        /// </summary>
        protected static string TempDir
        {
            get
            {
                lock (locker)
                {
                    if (tempDir == null)
                    {
                        tempDir = !String.IsNullOrEmpty(SThreeQLConfiguration.Section.BackupTargets.TempDir) ?
                            SThreeQLConfiguration.Section.BackupTargets.TempDir :
                            Path.GetTempPath();
                    }

                    return tempDir;
                }
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
                "_"
            );
        }

        /// <summary>
        /// Gets a random file name located in the configured temporary directory.
        /// </summary>
        /// <returns>A random, temporary file name.</returns>
        protected static string GetTemporaryPath()
        {
            return Path.Combine(TempDir, Path.GetRandomFileName());
        }
    }
}
