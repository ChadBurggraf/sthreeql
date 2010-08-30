//-----------------------------------------------------------------------
// <copyright file="DataSourceConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a datasource definition.
    /// </summary>
    public class DataSourceConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the datasource's name.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// Gets or sets the password to use when connecting to the datasource.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get { return (string)(this["password"] ?? String.Empty); }
            set { this["password"] = value; }
        }

        /// <summary>
        /// Gets or sets the user ID to use when connecting to the datasource.
        /// </summary>
        [ConfigurationProperty("userId", IsRequired = false)]
        public string UserId
        {
            get { return (string)(this["userId"] ?? String.Empty); }
            set { this["userId"] = value; }
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
