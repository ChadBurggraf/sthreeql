using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a restore database target's configuration element.
    /// </summary>
    public class DatabaseRestoreTargetConfigurationElement : DatabaseTargetConfigurationElement
    {
        /// <summary>
        /// Gets the target's log name.
        /// </summary>
        [ConfigurationProperty("logName", IsRequired = true)]
        public string LogName
        {
            get { return (string)(this["logName"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the target's restore catalog name.
        /// </summary>
        [ConfigurationProperty("restoreCatalogName", IsRequired = true)]
        public string RestoreCatalogName
        {
            get { return (string)(this["restoreCatalogName"] ?? String.Empty); }
        }

        /// <summary>
        /// Gets the target's restore file location.
        /// </summary>
        [ConfigurationProperty("restorePath", IsRequired = true)]
        public string RestorePath
        {
            get { return (string)(this["restorePath"] ?? String.Empty); }
        }
    }
}
