using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SThreeQL.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="AWSTargetConfigurationElement"/>s.
    /// </summary>
    public class AWSTargetConfigurationElementCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// Gets an element by its bucket name.
        /// </summary>
        /// <param name="bucketName">The bucket name of the element to get.</param>
        /// <returns>An element.</returns>
        public new AWSTargetConfigurationElement this[string bucketName]
        {
            get { return (AWSTargetConfigurationElement)BaseGet(bucketName); }
        }

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The newly created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new AWSTargetConfigurationElement();
        }

        /// <summary>
        /// Gets an element's key.
        /// </summary>
        /// <param name="element">The element to get the key for.</param>
        /// <returns>The element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AWSTargetConfigurationElement)element).BucketName;
        }
    }
}
