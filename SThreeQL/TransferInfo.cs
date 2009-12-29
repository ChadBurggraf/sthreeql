using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Encapsulates meta data about a configured target transfer.
    /// </summary>
    public class TransferInfo
    {
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

        /// <summary>
        /// Gets or sets the target being transferred.
        /// </summary>
        public DatabaseTargetConfigurationElement Target { get; set; }
    }
}
