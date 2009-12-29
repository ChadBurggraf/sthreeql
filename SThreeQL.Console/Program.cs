using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SThreeQL.Configuration;

namespace SThreeQL.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            bool schedule = false;
            bool backup = true;
            bool restore = true;
            string targetName = null;

            if (args.Length > 0)
            {
                string qualifier = args[0].ToUpperInvariant();

                if (qualifier == "-S")
                {
                    schedule = true;
                    restore = false;
                    backup = false;
                }
                else if (qualifier == "-B")
                {
                    restore = false;
                }
                else if (qualifier == "-R")
                {
                    backup = false;
                }

                if (args.Length > 1)
                {
                    targetName = args[1];
                }
            }

            ConsoleDelegate consoleDelegate = new ConsoleDelegate();

            if (schedule)
            {
                ScheduleConfigurationElementCollection schedules = null;

                if (!String.IsNullOrEmpty(targetName))
                {
                    ScheduleConfigurationElement config = SThreeQLConfiguration.Section.Schedules[targetName];

                    if (config != null)
                    {
                        schedules = new ScheduleConfigurationElementCollection();
                        schedules.Add(config);
                    }
                    else
                    {
                        System.Console.WriteLine(String.Concat("There is no schedule defined for the name \"" + targetName + "\".\n"));
                    }
                }
                else
                {
                    schedules = SThreeQLConfiguration.Section.Schedules;
                }


                new Scheduler(schedules, System.Console.Out, System.Console.Error).Start();
            }
            else
            {
                if (backup)
                {
                    ExecuteBackup(consoleDelegate, targetName);
                }

                if (restore)
                {
                    ExecuteRestore(consoleDelegate, targetName);
                }
            }
        }

        static void ExecuteBackup(ConsoleDelegate consoleDelegate, string targetName)
        {
            if (!String.IsNullOrEmpty(targetName))
            {
                DatabaseTargetConfigurationElement target = SThreeQLConfiguration.Section.BackupTargets[targetName];

                if (target != null)
                {
                    ExecuteBackup(consoleDelegate, target);
                }
                else
                {
                    System.Console.Error.WriteLine(String.Concat("There is no backup target defined for the name \"", targetName, "\"."));
                }

                System.Console.WriteLine();
            }
            else
            {
                foreach (DatabaseTargetConfigurationElement target in SThreeQLConfiguration.Section.BackupTargets)
                {
                    ExecuteBackup(consoleDelegate, target);
                    System.Console.WriteLine();
                }
            }
        }

        static void ExecuteBackup(ConsoleDelegate consoleDelegate, DatabaseTargetConfigurationElement target)
        {
            try
            {
                BackupTask task = new BackupTask(target);
                task.BackupDelegate = consoleDelegate;
                task.TransferDelegate = consoleDelegate;

                TaskExecutionResult result = task.Execute();

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

        static void ExecuteRestore(ConsoleDelegate consoleDelegate, string targetName)
        {
            if (!String.IsNullOrEmpty(targetName))
            {
                DatabaseRestoreTargetConfigurationElement target = SThreeQLConfiguration.Section.RestoreTargets[targetName];

                if (target != null)
                {
                    ExecuteRestore(consoleDelegate, target);
                }
                else
                {
                    System.Console.Error.WriteLine(String.Concat("There is no restore target defined for the name \"", targetName, "\"."));
                }

                System.Console.WriteLine();
            }
            else
            {
                foreach (DatabaseRestoreTargetConfigurationElement target in SThreeQLConfiguration.Section.RestoreTargets)
                {
                    ExecuteRestore(consoleDelegate, target);
                    System.Console.WriteLine();
                }
            }
        }

        static void ExecuteRestore(ConsoleDelegate consoleDelegate, DatabaseRestoreTargetConfigurationElement target)
        {
            try
            {
                RestoreTask task = new RestoreTask(target);
                task.RestoreDelegate = consoleDelegate;
                task.TransferDelegate = consoleDelegate;

                TaskExecutionResult result = task.Execute();

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

        static void WriteError(Exception ex)
        {
            System.Console.Error.WriteLine("Whoops: {0}", ex.Message);
        }
    }
}
