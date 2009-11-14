using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    public class ScheduleQueue
    {
        /// <summary>
        /// Gets the next execution date for the given schedule.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static DateTime GetNextExecuteDate(ScheduleConfigurationElement schedule)
        {
            if (DateTime.Now < schedule.StartDate)
            {
                return schedule.StartDate;
            }

            //
            // TODO: The only repeat type is Daily right now.
            //

            int days = (int)Math.Ceiling(DateTime.Now.Subtract(schedule.StartDate).TotalDays);
            return schedule.StartDate.AddDays(days);
        }
    }
}
