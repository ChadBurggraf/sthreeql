//-----------------------------------------------------------------------
// <copyright file="ScheduleTargetConfigurationElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a backup or restore target reference in a schedule configuration element.
    /// </summary>
    public class ScheduleTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the name of the target this element is referring to.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
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
