using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Affirma.ThreeSharp.Statistics;
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
                        new BackupTask(config).Execute(System.Console.Out, System.Console.Error);
                    }
                    else
                    {
                        System.Console.WriteLine(String.Concat("There is no backup task defined for the name \"", taskName, "\".\n"));
                    }

                    System.Console.WriteLine();
                }
                else
                {
                    foreach (DatabaseTargetConfigurationElement config in SThreeQLConfiguration.Section.BackupTargets)
                    {
                        new BackupTask(config).Execute(System.Console.Out, System.Console.Error);
                        System.Console.WriteLine();
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
                        new RestoreTask(config).Execute(System.Console.Out, System.Console.Error);
                    }
                    else
                    {
                        System.Console.WriteLine(String.Concat("There is no restore task defined for the name \"", taskName, "\".\n"));
                    }

                    System.Console.WriteLine();
                }
                else
                {
                    foreach (DatabaseRestoreTargetConfigurationElement config in SThreeQLConfiguration.Section.RestoreTargets)
                    {
                        new RestoreTask(config).Execute(System.Console.Out, System.Console.Error);
                        System.Console.WriteLine();
                    }
                }
            }

            System.Console.WriteLine("Press any key to quit.");
            System.Console.ReadKey();
        }
    }
}
