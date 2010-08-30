//-----------------------------------------------------------------------
// <copyright file="DatabaseTargetEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;

    /// <summary>
    /// Arguments for database target events.
    /// </summary>
    [Serializable]
    public class DatabaseTargetEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the target's catalog name.
        /// </summary>
        public string CatalogName { get; set; }

        /// <summary>
        /// Gets or sets the target's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the target's current transfer state, if applicable.
        /// </summary>
        public TransferInfo Transfer { get; set; }
    }
}
