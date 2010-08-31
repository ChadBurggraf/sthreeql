//-----------------------------------------------------------------------
// <copyright file="AwsTargetConfigurationElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a collection of <see cref="AwsTargetConfigurationElement"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
    public class AwsTargetConfigurationElementCollection : ConfigurationElementCollection<AwsTargetConfigurationElement>
    {
        /// <summary>
        /// Gets the element with the given key from the collection.
        /// </summary>
        /// <param name="bucketName">The name of the bucket to get the element for.</param>
        /// <returns>The element with the given key.</returns>
        public new AwsTargetConfigurationElement this[string bucketName]
        {
            get { return (AwsTargetConfigurationElement)BaseGet(bucketName); }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(AwsTargetConfigurationElement item)
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
            return ((AwsTargetConfigurationElement)element).BucketName;
        }
    }
}
