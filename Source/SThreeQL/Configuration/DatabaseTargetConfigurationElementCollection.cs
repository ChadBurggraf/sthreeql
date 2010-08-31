//-----------------------------------------------------------------------
// <copyright file="DatabaseTargetConfigurationElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="DatabaseTargetConfigurationElement"/>s.
    /// </summary>
    public class DatabaseTargetConfigurationElementCollection : ConfigurationElementCollection<DatabaseTargetConfigurationElement>
    {
        /// <summary>
        /// Gets or sets the temporary storage directory to use for targets in this collection.
        /// Defaults to the system temporary directy when empty.
        /// </summary>
        [ConfigurationProperty("tempDir", IsRequired = false)]
        public string TempDir
        {
            get { return base["tempDir"].ToStringWithDefault(Path.GetTempPath()); }
            set { base["tempDir"] = value; }
        }

        /// <summary>
        /// Gets the element with the given key from the collection.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>The element with the given key.</returns>
        public new DatabaseTargetConfigurationElement this[string name]
        {
            get { return (DatabaseTargetConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(DatabaseTargetConfigurationElement item)
        {
            return 0 < (from e in this
                        where item.Name.Equals(e.Name, StringComparison.OrdinalIgnoreCase)
                        select e).Count();
        }

        /// <summary>
        /// Gets the unique key of the given element.
        /// </summary>
        /// <param name="element">The element to get the key of.</param>
        /// <returns>The given element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatabaseTargetConfigurationElement)element).Name;
        }
    }
}
