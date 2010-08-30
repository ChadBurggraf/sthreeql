//-----------------------------------------------------------------------
// <copyright file="AWSTargetConfigurationElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="AWSTargetConfigurationElement"/>s.
    /// </summary>
    public class AWSTargetConfigurationElementCollection : ConfigurationElementCollection<AWSTargetConfigurationElement>
    {
        /// <summary>
        /// Gets the element with the given key from the collection.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to get the element for.</param>
        /// <returns>The element with the given key.</returns>
        public new AWSTargetConfigurationElement this[string bucketName]
        {
            get { return (AWSTargetConfigurationElement)BaseGet(bucketName); }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(AWSTargetConfigurationElement item)
        {
            return 0 < (from e in this
                        where item.BucketName.Equals(e.BucketName, StringComparison.OrdinalIgnoreCase)
                        select e).Count();
        }

        /// <summary>
        /// Gets the unique key of the given element.
        /// </summary>
        /// <param name="element">The element to get the key of.</param>
        /// <returns>The given element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AWSTargetConfigurationElement)element).BucketName;
        }
    }
}
