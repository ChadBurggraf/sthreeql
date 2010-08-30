//-----------------------------------------------------------------------
// <copyright file="SThreeQLConfiguration.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the SThreeQL configuration section.
    /// </summary>
    public class SThreeQLConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets a reference to the section from the configuration.
        /// </summary>
        public static SThreeQLConfiguration Section
        {
            get { return (SThreeQLConfiguration)ConfigurationManager.GetSection("sThreeQL"); }
        }

        /// <summary>
        /// Gets the collection of AWS targets defined for backup/restore scenarios.
        /// </summary>
        [ConfigurationProperty("awsTargets")]
        public AWSTargetConfigurationElementCollection AWSTargets
        {
            get { return (AWSTargetConfigurationElementCollection)(this["awsTargets"] ?? (this["awsTargets"] = new AWSTargetConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets the collection of backup database targets.
        /// </summary>
        [ConfigurationProperty("backupTargets")]
        public DatabaseTargetConfigurationElementCollection BackupTargets
        {
            get { return (DatabaseTargetConfigurationElementCollection)(this["backupTargets"] ?? (this["backupTargets"] = new DatabaseTargetConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets or sets the timeout, in seconds, for database connections and commands.
        /// </summary>
        [ConfigurationProperty("databaseTimeout", IsRequired = false, DefaultValue = 600)]
        public int DatabaseTimeout
        {
            get { return (int)this["databaseTimeout"]; }
            set { this["databaseTimeout"] = value; }
        }

        /// <summary>
        /// Gets the collection of datasources.
        /// </summary>
        [ConfigurationProperty("dataSources")]
        public DataSourceConfigurationElementCollection DataSources
        {
            get { return (DataSourceConfigurationElementCollection)(this["dataSources"] ?? (this["dataSources"] = new DataSourceConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets the collection of restore database targets.
        /// </summary>
        [ConfigurationProperty("restoreTargets")]
        public DatabaseRestoreTargetConfigurationElementCollection RestoreTargets
        {
            get { return (DatabaseRestoreTargetConfigurationElementCollection)(this["restoreTargets"] ?? (this["restoreTargets"] = new DatabaseRestoreTargetConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets the collection of schedules.
        /// </summary>
        [ConfigurationProperty("schedules")]
        public ScheduleConfigurationElementCollection Schedules
        {
            get { return (ScheduleConfigurationElementCollection)(this["schedules"] ?? (this["schedules"] = new ScheduleConfigurationElementCollection())); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL when communicating with AWS.
        /// </summary>
        [ConfigurationProperty("useSsl", IsRequired = false)]
        public bool UseSSL
        {
            get { return (bool)this["useSsl"]; }
            set { this["useSsl"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the secion is read-only.
        /// </summary>
        /// <returns>A value indicating whether the section is read-only.</returns>
        public override bool IsReadOnly()
        {
            return false;
        }
    }
}
