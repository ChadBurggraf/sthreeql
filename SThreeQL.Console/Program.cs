using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            bool backup = true;
            bool restore = true;
            string taskName = null;

            if (args.Length > 0)
            {
                string qualifier = args[0].ToUpperInvariant();

                if (qualifier == "-B")
                {
                    restore = false;
                }
                else if (qualifier == "-R")
                {
                    backup = false;
                }

                if (args.Length > 1)
                {
                    taskName = args[1];
                }
            }

            if (backup)
            {
                if (!String.IsNullOrEmpty(taskName))
                {
                    DatabaseTargetConfigurationElement config = SThreeQLConfiguration.Section.BackupTargets[taskName];

                    if (config != null)
                    {
                        Backup(config);
                    }
                    else
                    {
                        System.Console.WriteLine(String.Concat("There is no backup task defined for the name \"", taskName, "\".\n"));
                    }
                }
                else
                {
                    foreach (DatabaseTargetConfigurationElement config in SThreeQLConfiguration.Section.BackupTargets)
                    {
                        Backup(config);
                    }
                }
            }

            if (restore)
            {
                if (!String.IsNullOrEmpty(taskName))
                {
                    DatabaseRestoreTargetConfigurationElement config = SThreeQLConfiguration.Section.RestoreTargets[taskName];

                    if (config != null)
                    {
                        Restore(config);
                    }
                    else
                    {
                        System.Console.WriteLine(String.Concat("There is no restore task defined for the name \"", taskName, "\".\n"));
                    }
                }
                else
                {
                    foreach (DatabaseRestoreTargetConfigurationElement config in SThreeQLConfiguration.Section.RestoreTargets)
                    {
                        Restore(config);
                    }
                }
            }

            System.Console.WriteLine("Press any key to quit.");
            System.Console.ReadKey();
        }

        static void Backup(DatabaseTargetConfigurationElement config)
        {
            System.Console.WriteLine(String.Concat("Executing backup target ", config.Name, "..."));
            System.Console.WriteLine("Backing up database...");

            TaskExecutionResult result = new BackupTask(config, UploadProgress, UploadComplete).Execute();

            if (result.Success)
            {
                System.Console.WriteLine("Done.");
            }
            else
            {
                System.Console.WriteLine(String.Concat("Failed: ", result.Exception.Message));
            }

            System.Console.WriteLine();
        }

        static void Restore(DatabaseRestoreTargetConfigurationElement config)
        {
            System.Console.WriteLine(String.Concat("Executing restore target ", config.Name, "..."));

            TaskExecutionResult result = new RestoreTask(config, DownloadProgress, DownloadComplete).Execute();

            if (result.Success)
            {
                System.Console.WriteLine("Done.");
            }
            else
            {
                System.Console.WriteLine(String.Concat("Failed: ", result.Exception.Message));
            }

            System.Console.WriteLine();
        }

        static void DownloadComplete()
        {
            System.Console.WriteLine("\nRestoring database...");
        }

        static void DownloadProgress(int percentComplete)
        {
            if (System.Console.CursorLeft > 0)
            {
                System.Console.CursorLeft = 0;
            }

            System.Console.Write(String.Format("Downloading: {0:###}%", percentComplete));
        }

        static void UploadComplete()
        {
            System.Console.WriteLine();
        }

        static void UploadProgress(int percentComplete)
        {
            if (System.Console.CursorLeft > 0)
            {
                System.Console.CursorLeft = 0;
            }

            System.Console.Write(String.Format("Uploading: {0:###}%", percentComplete));
        }
    }
}
