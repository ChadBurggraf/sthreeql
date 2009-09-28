using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="DatabaseRestoreTargetConfigurationElement"/>s.
    /// </summary>
    public class DatabaseRestoreTargetConfigurationElementCollection : DatabaseTargetConfigurationElementCollection
    {
        /// <summary>
        /// Gets an element by its name.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>An element.</returns>
        public new DatabaseRestoreTargetConfigurationElement this[string name]
        {
            get { return (DatabaseRestoreTargetConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The newly created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseRestoreTargetConfigurationElement();
        }

        /// <summary>
        /// Gets an element's key.
        /// </summary>
        /// <param name="element">The element to get the key for.</param>
        /// <returns>The element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseRestoreTargetConfigurationElement)element).Name;
        }
    }
}
