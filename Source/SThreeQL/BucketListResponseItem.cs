//-----------------------------------------------------------------------
// <copyright file="BucketListResponseItem.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.Xml;

    /// <summary>
    /// Represents an object in a bucket list response.
    /// </summary>
    public class BucketListResponseItem
    {
        /// <summary>
        /// Gets or sets the item's AWS key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the item's AWS modification date.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets or sets the item's size in bytes.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Creates a new item from the given XML node.
        /// </summary>
        /// <param name="contentsNode">An XML node containing the item's definition.</param>
        /// <returns>The created item.</returns>
        public static BucketListResponseItem Create(XmlNode contentsNode)
        {
            return new BucketListResponseItem()
            {
                Key = contentsNode.SelectSingleNode("*[local-name()='Key']").InnerXml,
                LastModified = DateTime.Parse(contentsNode.SelectSingleNode("*[local-name()='LastModified']").InnerXml),
                Size = Int64.Parse(contentsNode.SelectSingleNode("*[local-name()='Size']").InnerXml)
            };
        }
    }
}
