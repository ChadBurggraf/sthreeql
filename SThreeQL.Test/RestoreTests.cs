using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SThreeQL.Configuration;

namespace SThreeQL.Test
{
    [TestClass]
    public class RestoreTests
    {
        private static readonly object locker = new object();
        private static DatabaseRestoreTargetConfigurationElement restoreConfig;

        [ClassCleanup]
        public static void Cleanup()
        {
            using (SqlConnection connection = new SqlConnection(restoreConfig.ConnectionString))
            {
                connection.Open();

                try
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandTimeout = SThreeQLConfiguration.Section.DatabaseTimeout;
                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(
                            new SqlScript("Drop.sql").Text,
                            restoreConfig.RestoreCatalogName);

                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            lock (locker)
            {
                var q = (from DatabaseTargetConfigurationElement be in SThreeQLConfiguration.Section.BackupTargets
                         join DatabaseRestoreTargetConfigurationElement re in SThreeQLConfiguration.Section.RestoreTargets on be.CatalogName equals re.CatalogName
                         select new
                         {
                             BackupConfig = be,
                             RestoreConfig = re
                         }).First();

                restoreConfig = q.RestoreConfig;
                new BackupTask(q.BackupConfig).Execute();
            }
        }

        [TestMethod]
        public void Restore_CanDownloadDatabaseBackup()
        {
            RestoreTask task = new RestoreTask(restoreConfig);
            string path = task.DownloadBackup();
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void Restore_CanExecuteRestoreTask()
        {
            new RestoreTask(restoreConfig).Execute();
        }

        [TestMethod]
        public void Restore_CanRestoreDatabase()
        {
            RestoreTask task = new RestoreTask(restoreConfig);
            string path = task.DownloadBackup();
            task.RestoreDatabase(path);
        }
    }
}
