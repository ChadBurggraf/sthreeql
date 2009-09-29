using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="DatabaseTargetConfigurationElement"/>s.
    /// </summary>
    public class DatabaseTargetConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets an element by its name.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>An element.</returns>
        public new DatabaseTargetConfigurationElement this[string name]
        {
            get { return (DatabaseTargetConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Gets the temporary storage directory to use for targets in this collection.
        /// Defaults to the system temporary directy when empty.
        /// </summary>
        [ConfigurationProperty("tempDir", IsRequired = false)]
        public string TempDir
        {
            get { return base["tempDir"].ToStringWithDefault(Path.GetTempPath()); }
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The newly created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new DatabaseTargetConfigurationElement();
        }

        /// <summary>
        /// Gets an element's key.
        /// </summary>
        /// <param name="element">The element to get the key for.</param>
        /// <returns>The element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseTargetConfigurationElement)element).Name;
        }
    }
}
