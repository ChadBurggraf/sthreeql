using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents the SThreeQL configuration section.
    /// </summary>
    public class SThreeQLConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Gets the collection of AWS targets defined for backup/restore scenarios.
        /// </summary>
        [ConfigurationProperty("awsTargets")]
        public AWSTargetConfigurationElementCollection AWSTargets
        {
            get { return (AWSTargetConfigurationElementCollection)(this["awsTargets"] ?? new AWSTargetConfigurationElementCollection()); }
        }

        /// <summary>
        /// Gets the collection of backup database targets.
        /// </summary>
        [ConfigurationProperty("backupTargets")]
        public DatabaseTargetConfigurationElementCollection BackupTargets
        {
            get { return (DatabaseTargetConfigurationElementCollection)(this["backupTargets"] ?? new DatabaseTargetConfigurationElementCollection()); }
        }

        /// <summary>
        /// Gets the timeout, in seconds, for database connections and commands.
        /// </summary>
        [ConfigurationProperty("databaseTimeout", IsRequired = false, DefaultValue = 600)]
        public int DatabaseTimeout
        {
            get { return (int)this["databaseTimeout"]; }
        }

        /// <summary>
        /// Gets the collection of restore database targets.
        /// </summary>
        [ConfigurationProperty("restoreTargets")]
        public DatabaseRestoreTargetConfigurationElementCollection RestoreTargets
        {
            get { return (DatabaseRestoreTargetConfigurationElementCollection)(this["restoreTargets"] ?? new DatabaseRestoreTargetConfigurationElementCollection()); }
        }

        /// <summary>
        /// Gets a reference to the section from the configuration.
        /// </summary>
        public static SThreeQLConfiguration Section
        {
            get { return (SThreeQLConfiguration)ConfigurationManager.GetSection("sThreeQL"); }
        }

        /// <summary>
        /// Gets the interval, in hours, between processing runs when running as a service.
        /// </summary>
        [ConfigurationProperty("serviceInterval", IsRequired = false, DefaultValue = 24d)]
        public double ServiceInterval
        {
            get { return (double)this["serviceInterval"]; }
        }

        /// <summary>
        /// Gets a value indicating whether to use SSL when communicating with AWS.
        /// </summary>
        [ConfigurationProperty("useSsl", IsRequired = false)]
        public bool UseSSL
        {
            get { return (bool)this["useSsl"]; }
        }

        /// <summary>
        /// Gets the location of the WinRAR installation directory if compression is desired.
        /// </summary>
        [ConfigurationProperty("winRarLocation", IsRequired = false)]
        public string WinRarLocation
        {
            get { return (string)this["winRarLocation"]; }
        }
    }
}
