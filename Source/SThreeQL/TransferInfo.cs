//-----------------------------------------------------------------------
// <copyright file="TransferInfo.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;

    /// <summary>
    /// Encapsulates meta data about a network transfer.
    /// </summary>
    [Serializable]
    public class TransferInfo
    {
        /// <summary>
        /// Initializes a new instance of the TransferInfo class.
        /// </summary>
        public TransferInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the TransferInfo class.
        /// </summary>
        /// <param name="info">The existing <see cref="TransferInfo"/> to construct this instance from.</param>
        public TransferInfo(TransferInfo info)
        {
            if (info != null)
            {
                this.BytesTransferred = info.BytesTransferred;
                this.FileName = info.FileName;
                this.FileSize = info.FileSize;
            }
        }

        /// <summary>
        /// Gets or sets the number of bytes transferred.
        /// </summary>
        public long BytesTransferred { get; set; }

        /// <summary>
        /// Gets or sets the name of the file being transferred.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file being transferred.
        /// </summary>
        public long FileSize { get; set; }
    }
}
