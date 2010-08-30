using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SThreeQL.Test
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Utility_CanGetEmbeddedSqlScripts()
        {
            Assert.IsTrue(!String.IsNullOrEmpty(new SqlScript("Backup.sql").Text));
            Assert.IsTrue(!String.IsNullOrEmpty(new SqlScript("Drop.sql").Text));
            Assert.IsTrue(!String.IsNullOrEmpty(new SqlScript("GetFiles.sql").Text));
            Assert.IsTrue(!String.IsNullOrEmpty(new SqlScript("Restore.sql").Text));
        }
    }
}
