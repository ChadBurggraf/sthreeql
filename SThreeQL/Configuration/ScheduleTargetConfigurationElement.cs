using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a backup or restore target reference in a schedule configuration element.
    /// </summary>
    public class ScheduleTargetConfigurationElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the name of the target this element is referring to.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
        }
    }
}
