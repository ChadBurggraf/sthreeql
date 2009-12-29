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
        /// Gets the target's restore catalog name.
        /// Defaults to the value of <see cref="DatabaseTargetConfigurationElement.CatalogName"/> when empty.
        /// </summary>
        [ConfigurationProperty("restoreCatalogName", IsRequired = false)]
        public string RestoreCatalogName
        {
            get { return this["restoreCatalogName"].ToStringWithDefault(CatalogName); }
        }

        /// <summary>
        /// Gets the target's restore file location.
        /// </summary>
        [ConfigurationProperty("restorePath", IsRequired = true)]
        public string RestorePath
        {
            get { return (string)this["restorePath"]; }
        }
    }
}
