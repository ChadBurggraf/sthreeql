//-----------------------------------------------------------------------
// <copyright file="ScheduleConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a schedule configuration element.
    /// </summary>
    public class ScheduleConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the schedule's backup targets collection.
        /// </summary>
        [ConfigurationProperty("backupTargets")]
        public ScheduleTargetConfigurationElementCollection BackupTargets
        {
            get { return (ScheduleTargetConfigurationElementCollection)(this["backupTargets"] ?? (this["backupTargets"] = new ScheduleTargetConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the schedule's repeat type.
        /// </summary>
        [ConfigurationProperty("repeat", IsRequired = true)]
        public ScheduleRepeatType Repeat
        {
            get { return (ScheduleRepeatType)this["repeat"]; }
            set { this["repeat"] = value; }
        }

        /// <summary>
        /// Gets the schedule's restore targets collection.
        /// </summary>
        [ConfigurationProperty("restoreTargets")]
        public ScheduleTargetConfigurationElementCollection RestoreTargets
        {
            get { return (ScheduleTargetConfigurationElementCollection)(this["restoreTargets"] ?? (this["restoreTargets"] = new ScheduleTargetConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets or sets the schedule's start date and time.
        /// </summary>
        [ConfigurationProperty("startDate", IsRequired = true)]
        public DateTime StartDate
        {
            get { return (DateTime)this["startDate"]; }
            set { this["startDate"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this element is read-only.
        /// </summary>
        /// <returns>A value indicating whether this element is read-only.</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
