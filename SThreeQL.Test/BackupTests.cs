using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SThreeQL.Configuration;

namespace SThreeQL.Test
{
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void Backup_CanBackupDatabase()
        {
            DatabaseTargetConfigurationElement config = GetBackupTarget();
            BackupTask task = new BackupTask(config);
            string path = task.BackupDatabase();
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void Backup_CanExecuteBackupTask()
        {
            DatabaseTargetConfigurationElement config = GetBackupTarget();
            BackupTask task = new BackupTask(config);
            task.Execute();
        }

        [TestMethod]
        public void Backup_CanUploadDatabaseBackup()
        {
            DatabaseTargetConfigurationElement config = GetBackupTarget();
            BackupTask task = new BackupTask(config);
            string path = task.BackupDatabase();
            task.UploadBackup(path);
        }

        protected static DatabaseTargetConfigurationElement GetBackupTarget()
        {
            return (from DatabaseTargetConfigurationElement t in SThreeQLConfiguration.Section.BackupTargets
                    select t).First();
        }
    }
}
