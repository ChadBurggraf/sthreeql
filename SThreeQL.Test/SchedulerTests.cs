using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SThreeQL.Configuration;

namespace SThreeQL.Test
{
    [TestClass]
    public class SchedulerTests
    {
        [TestMethod]
        public void Scheduler_GetsAccurateNextExecuteDates()
        {
            DateTime now = DateTime.Now;
            DateTime nowPlusOneHour = now.AddHours(1);
            DateTime nowPlusOneDay = now.AddDays(1);
            DateTime nowMinusOneDay = now.AddDays(-1);

            Assert.AreEqual(nowPlusOneHour, Scheduler.GetNextExecuteDate(new ScheduleConfigurationElement()
            {
                StartDate = nowPlusOneHour,
                Repeat = ScheduleRepeatType.Daily
            }));

            Assert.AreEqual(now, Scheduler.GetNextExecuteDate(new ScheduleConfigurationElement()
            {
                StartDate = now,
                Repeat = ScheduleRepeatType.Daily
            }));

            Assert.AreEqual(now, Scheduler.GetNextExecuteDate(new ScheduleConfigurationElement()
            {
                StartDate = nowMinusOneDay,
                Repeat = ScheduleRepeatType.Daily
            }));
        }
    }
}
