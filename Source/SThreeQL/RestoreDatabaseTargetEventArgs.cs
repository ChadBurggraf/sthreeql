//-----------------------------------------------------------------------
// <copyright file="RestoreDatabaseTargetEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;

    /// <summary>
    /// Arguments for restore database events.
    /// </summary>
    [Serializable]
    public class RestoreDatabaseTargetEventArgs : DatabaseTargetEventArgs
    {
        /// <summary>
        /// Gets or sets the target's restore catalog name.
        /// </summary>
        public string RestoreCatalogName { get; set; }

        /// <summary>
        /// Gets or sets the target's restore path.
        /// </summary>
        public string RestorePath { get; set; }
    }
}
