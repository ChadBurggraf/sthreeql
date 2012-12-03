//-----------------------------------------------------------------------
// <copyright file="SThreeQLConsole.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Console
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using NDesk.Options;
    using SThreeQL.Configuration;
    
    /// <summary>
    /// Provides console-based execution of SThreeQL tasks.
    /// </summary>
    public sealed class SThreeQLConsole
    {
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Prevents a default instance of the SThreeQLConsole class from being created.
        /// </summary>
        private SThreeQLConsole()
        {
        }

        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">The execution arguments.</param>
        /// <returns>The application's exit code.</returns>
        public static int Main(string[] args)
        {
            int all = 0, backup = 0, man = 0, schedules = 0, restore = 0;
            string target = null;

            var options = new OptionSet()
            {
                { "a|all", "execute all backup and restore targets in the configuration.", v => { ++all; } },
                { "b|backup", "execute backup targets.", v => { ++backup; } },
                { "r|restore", "execute restore targets.", v => { ++restore; } },
                { "s|schedules", "execute schedules.", v => { ++schedules; } },
                { "t|target=", "the name of the specific backup/restore/schedule to execute.", v => target = v },
                { "m|man", "show this message", v => { ++man; } }
            };

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                ParseError(options, ex);
                return 1;
            }

            if (man > 0)
            {
                Help(options);
                return 0;
            }

            if (all > 0)
            {
                ExecuteBackup(String.Empty);
                ExecuteRestore(String.Empty);
            }
            else if (backup > 0)
            {
                if (!ExecuteBackup(target))
                {
                    return 1;
                }
            }
            else if (restore > 0)
            {
                if (!ExecuteRestore(target))
                {
                    return 1;
                }
            }
            else if (schedules > 0)
            {
                ScheduleConfigurationElementCollection schd = new ScheduleConfigurationElementCollection();

                if (!String.IsNullOrEmpty(target))
                {
                    var schedule = SThreeQLConfiguration.Section.Schedules[target];

                    if (schedule != null)
                    {
                        schd.Add(schedule);
                    }
                    else
                    {
                        WriteError("There is no schedule defined for \"{0}\".", target);
                        return 1;
                    }
                }
                else
                {
                    foreach (var schedule in SThreeQLConfiguration.Section.Schedules)
                    {
                        schd.Add(schedule);
                    }
                }

                Scheduler scheduler = new Scheduler(schd);
                scheduler.BackupComplete += new EventHandler<DatabaseTargetEventArgs>(BackupComplete);
                scheduler.BackupCompressComplete += new EventHandler<DatabaseTargetEventArgs>(BackupCompressComplete);
                scheduler.BackupCompressStart += new EventHandler<DatabaseTargetEventArgs>(BackupCompressStart);
                scheduler.BackupStart += new EventHandler<DatabaseTargetEventArgs>(BackupStart);
                scheduler.BackupTransferComplete += new EventHandler<DatabaseTargetEventArgs>(BackupTransferComplete);
                scheduler.BackupTransferProgress += new EventHandler<DatabaseTargetEventArgs>(BackupTransferProgress);
                scheduler.BackupTransferStart += new EventHandler<DatabaseTargetEventArgs>(BackupTransferStart);
                scheduler.RestoreComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreComplete);
                scheduler.RestoreDecompressComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreDecompressComplete);
                scheduler.RestoreDecompressStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreDecompressStart);
                scheduler.RestoreStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreStart);
                scheduler.RestoreTransferComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferComplete);
                scheduler.RestoreTransferProgress += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferProgress);
                scheduler.RestoreTransferStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferStart);
                scheduler.ScheduleComplete += new EventHandler<ScheduleEventArgs>(ScheduleComplete);
                scheduler.ScheduleError += new EventHandler<ScheduleEventArgs>(ScheduleError);
                scheduler.ScheduleStart += new EventHandler<ScheduleEventArgs>(ScheduleStart);
                scheduler.Start();
            }
            else
            {
                Help(options);
                return 0;
            }

            return 0;
        }

        /// <summary>
        /// Raises a BackupComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupComplete(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a BackupCompressComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupCompressComplete(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a BackupCompressStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupCompressStart(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Compressing the backup file... ");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a BackupStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupStart(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Backing up database ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.CatalogName);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("... ");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a BackupTransferComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupTransferComplete(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                Console.WriteLine("Upload complete.");
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a BackupTransferProgress event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupTransferProgress(object sender, DatabaseTargetEventArgs e)
        {
            OnTransferProgress(e.Transfer);
        }

        /// <summary>
        /// Raises a BackupTransferStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void BackupTransferStart(object sender, DatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Uploading file ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.Transfer.FileName);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" ({0})... ", e.Transfer.FileSize.ToFileSize());
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Executes backup operations, optionally restricted to the given target name.
        /// </summary>
        /// <param name="target">The name of the target to backup, or null to execute all configured backups.</param>
        /// <returns>True if the given target is valid, false otherwise.</returns>
        private static bool ExecuteBackup(string target)
        {
            if (!String.IsNullOrEmpty(target))
            {
                var element = SThreeQLConfiguration.Section.BackupTargets[target];

                if (element != null)
                {
                    ExecuteBackup(element);
                }
                else
                {
                    WriteError("There is no backup target defined for '{0}'.", target);
                    return false;
                }
            }
            else
            {
                foreach (var element in SThreeQLConfiguration.Section.BackupTargets)
                {
                    ExecuteBackup(element);
                }
            }

            return true;
        }

        /// <summary>
        /// Executes a backup operation.
        /// </summary>
        /// <param name="target">The target to execute.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log exceptions rather than bail.")]
        private static void ExecuteBackup(DatabaseTargetConfigurationElement target)
        {
            try
            {
                BackupTask task = new BackupTask(target);
                task.BackupComplete += new EventHandler<DatabaseTargetEventArgs>(BackupComplete);
                task.BackupStart += new EventHandler<DatabaseTargetEventArgs>(BackupStart);
                task.CompressComplete += new EventHandler<DatabaseTargetEventArgs>(BackupCompressComplete);
                task.CompressStart += new EventHandler<DatabaseTargetEventArgs>(BackupCompressStart);
                task.TransferComplete += new EventHandler<DatabaseTargetEventArgs>(BackupTransferComplete);
                task.TransferProgress += new EventHandler<DatabaseTargetEventArgs>(BackupTransferProgress);
                task.TransferStart += new EventHandler<DatabaseTargetEventArgs>(BackupTransferStart);

                var result = task.Execute();

                if (!result.Success)
                {
                    WriteError(result.Exception);
                }
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        /// <summary>
        /// Executes restore operations, optionally restricted to the given target name.
        /// </summary>
        /// <param name="target">The name of the target to restore, or null to execute all configured restores.</param>
        /// <returns>True if the given target is valid, false otherwise.</returns>
        private static bool ExecuteRestore(string target)
        {
            if (!String.IsNullOrEmpty(target))
            {
                var element = SThreeQLConfiguration.Section.RestoreTargets[target];

                if (element != null)
                {
                    ExecuteRestore(element);
                }
                else
                {
                    WriteError("There is no restore target defined for '{0}'.", target);
                    return false;
                }
            }
            else
            {
                foreach (var element in SThreeQLConfiguration.Section.RestoreTargets)
                {
                    ExecuteRestore(element);
                }
            }

            return true;
        }

        /// <summary>
        /// Executes a restore operation.
        /// </summary>
        /// <param name="target">The target to execute.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to log exceptions rather than bail.")]
        private static void ExecuteRestore(DatabaseRestoreTargetConfigurationElement target)
        {
            try
            {
                RestoreTask task = new RestoreTask(target);
                task.DecompressComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreDecompressComplete);
                task.DecompressStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreDecompressStart);
                task.RestoreComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreComplete);
                task.RestoreStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreStart);
                task.TransferComplete += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferComplete);
                task.TransferProgress += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferProgress);
                task.TransferStart += new EventHandler<RestoreDatabaseTargetEventArgs>(RestoreTransferStart);

                var result = task.Execute();

                if (!result.Success)
                {
                    WriteError(result.Exception);
                }
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        /// <summary>
        /// Prints help to the console.
        /// </summary>
        /// <param name="options">The option set to print.</param>
        private static void Help(OptionSet options)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.WriteLine("Usage: s3ql -a | -b [-t TARGET] | -r [-t TARGET] | -s [-t TARGET]");
                Console.WriteLine();
                options.WriteOptionDescriptions(Console.Out);
            }
        }

        /// <summary>
        /// Handles transfer progress events.
        /// </summary>
        /// <param name="info">The transfer information.</param>
        private static void OnTransferProgress(TransferInfo info)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;

                if (info.FileSize > 0 && info.BytesTransferred > 0)
                {
                    Console.CursorLeft = 0;

                    Console.Write(
                        "{0} of {1} ({2}%)          ",
                        info.BytesTransferred.ToFileSize(),
                        info.FileSize.ToFileSize(),
                        (int)((double)info.BytesTransferred / info.FileSize * 100));
                }

                Console.ResetColor();
            }
        }

        /// <summary>
        /// Prints argument parse error information to the console.
        /// </summary>
        /// <param name="options">The option set that generated the error.</param>
        /// <param name="exception">The exception that represents the error.</param>
        private static void ParseError(OptionSet options, OptionException exception)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.Error.WriteLine("s3ql: ", exception.Message);
                Console.Error.WriteLine();
                options.WriteOptionDescriptions(Console.Error);
            }
        }

        /// <summary>
        /// Raises a RestoreComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done.");
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a RestoreDecompressComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreDecompressComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("done.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a RestoreDecompressStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreDecompressStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Decompressing the backup file... ");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a RestoreStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Restoring catalog ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.RestoreCatalogName);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("... ");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a RestoreTransferComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreTransferComplete(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
                Console.WriteLine("Download complete.");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a RestoreTransferProgress event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreTransferProgress(object sender, RestoreDatabaseTargetEventArgs e)
        {
            OnTransferProgress(e.Transfer);
        }

        /// <summary>
        /// Raises a RestoreTransferStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void RestoreTransferStart(object sender, RestoreDatabaseTargetEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Downloading file ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.Transfer.FileName);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("... ");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a ScheduleComplete event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void ScheduleComplete(object sender, ScheduleEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Schedule ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.Name);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" is complete at {0:F}.", DateTime.Now);
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Raises a ScheduleError event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void ScheduleError(object sender, ScheduleEventArgs e)
        {
            string type = e.OperationType == ScheduleOperationType.Restore ? "restore" : "backup";
            string message = !String.IsNullOrEmpty(e.TargetName) ?
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing {0} target '{1}' for schedule '{2}'", type, e.TargetName, e.Name) :
                String.Format(CultureInfo.InvariantCulture, "An unhandled exception occurred while executing schedule '{0}'", e.Name);

            if (e.ErrorException != null)
            {
                message += String.Format(CultureInfo.InvariantCulture, ":\n   {0}\n   {1}", e.ErrorException.Message, e.ErrorException.StackTrace);
            }
            else
            {
                message += ".";
            }

            WriteError(message);
        }

        /// <summary>
        /// Raises a ScheduleStart event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void ScheduleStart(object sender, ScheduleEventArgs e)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("Executing schedule ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(e.Name);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" at {0:F}.", DateTime.Now);
                Console.WriteLine();
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Writes an error message to the console.
        /// </summary>
        /// <param name="ex">The exception that represents the error.</param>
        private static void WriteError(Exception ex)
        {
            WriteError("An unhandled exception occurred:\n   {0}\n   {1}", ex.Message, ex.StackTrace);
        }

        /// <summary>
        /// Writes an error message to the console.
        /// </summary>
        /// <param name="format">The format string to write.</param>
        /// <param name="args">The arguments to format the message with.</param>
        private static void WriteError(string format, params string[] args)
        {
            lock (SThreeQLConsole.SyncRoot)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }
        }
    }
}
