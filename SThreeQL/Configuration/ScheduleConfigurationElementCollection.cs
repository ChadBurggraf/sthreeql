using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a collection of schedule configuration elements.
    /// </summary>
    public class ScheduleConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets or sets an element by its name.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>An element.</returns>
        public new ScheduleConfigurationElement this[string name]
        {
            get { return (ScheduleConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Adds an element to the end of the collection.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(ScheduleConfigurationElement element)
        {
            BaseAdd(element);
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The newly created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ScheduleConfigurationElement();
        }

        /// <summary>
        /// Gets an element's key.
        /// </summary>
        /// <param name="element">The element to get the key for.</param>
        /// <returns>The element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ScheduleConfigurationElement)element).Name;
        }
    }
}
