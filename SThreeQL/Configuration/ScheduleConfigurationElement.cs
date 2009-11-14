using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a schedule configuration element.
    /// </summary>
    public class ScheduleConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the schedule's backup targets collection.
        /// </summary>
        [ConfigurationProperty("backupTargets")]
        public ScheduleTargetConfigurationElementCollection BackupTagets
        {
            get { return (ScheduleTargetConfigurationElementCollection)(this["backupTargets"] ?? new ScheduleTargetConfigurationElementCollection()); }
        }

        /// <summary>
        /// Gets the name of the schedule.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        /// <summary>
        /// Gets the schedule's repeat type.
        /// </summary>
        [ConfigurationProperty("repeat", IsRequired = true)]
        public ScheduleRepeatType Repeat
        {
            get { return (ScheduleRepeatType)this["repeat"]; }
        }

        /// <summary>
        /// Gets the schedule's restore targets collection.
        /// </summary>
        [ConfigurationProperty("restoreTargets")]
        public ScheduleTargetConfigurationElementCollection RestoreTargets
        {
            get { return (ScheduleTargetConfigurationElementCollection)(this["restoreTargets"] ?? new ScheduleTargetConfigurationElementCollection()); }
        }

        /// <summary>
        /// Gets the schedule's start date and time.
        /// </summary>
        [ConfigurationProperty("startDate", IsRequired = true)]
        public DateTime StartDate
        {
            get { return (DateTime)this["startDate"]; }
        }
    }

    /// <summary>
    /// Defines the possible schedule repeat types.
    /// </summary>
    public enum ScheduleRepeatType
    {
        /// <summary>
        /// Identifies a schedule that repeats daily.
        /// </summary>
        Daily
    }
}
