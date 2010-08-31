//-----------------------------------------------------------------------
// <copyright file="ScheduleTargetConfigurationElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="ScheduleTargetConfigurationElement"/>s.
    /// </summary>
    public class ScheduleTargetConfigurationElementCollection : ConfigurationElementCollection<ScheduleTargetConfigurationElement>
    {
        /// <summary>
        /// Gets the element with the given key from the collection.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>The element with the given key.</returns>
        public new ScheduleTargetConfigurationElement this[string name]
        {
            get { return (ScheduleTargetConfigurationElement)BaseGet(name); }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(ScheduleTargetConfigurationElement item)
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
            return ((ScheduleTargetConfigurationElement)element).Name;
        }
    }
}
