//-----------------------------------------------------------------------
// <copyright file="DatabaseRestoreTargetConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a restore database target's configuration element.
    /// </summary>
    public class DatabaseRestoreTargetConfigurationElement : DatabaseTargetConfigurationElement
    {
        /// <summary>
        /// Gets or sets the target's restore catalog name.
        /// Defaults to the value of <see cref="DatabaseTargetConfigurationElement.CatalogName"/> when empty.
        /// </summary>
        [ConfigurationProperty("restoreCatalogName", IsRequired = false)]
        public string RestoreCatalogName
        {
            get { return this["restoreCatalogName"].ToStringWithDefault(CatalogName); }
            set { this["restoreCatalogName"] = value; }
        }

        /// <summary>
        /// Gets or sets the target's restore file location.
        /// </summary>
        [ConfigurationProperty("restorePath", IsRequired = true)]
        public string RestorePath
        {
            get { return (string)this["restorePath"]; }
            set { this["restorePath"] = value; }
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
