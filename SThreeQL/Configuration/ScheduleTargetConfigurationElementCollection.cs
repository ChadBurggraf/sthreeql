using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a collection of targets in a schedule configuration element.
    /// </summary>
    public class ScheduleTargetConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets an element by its name.
        /// </summary>
        /// <param name="bucketName">The name of the element to get.</param>
        /// <returns>An element.</returns>
        public new ScheduleTargetConfigurationElement this[string name]
        {
            get { return (ScheduleTargetConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The newly created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduleTargetConfigurationElement();
        }

        /// <summary>
        /// Gets an element's key.
        /// </summary>
        /// <param name="element">The element to get the key for.</param>
        /// <returns>The element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ScheduleTargetConfigurationElement)element).Name;
        }
    }
}
