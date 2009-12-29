using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Defines a backup delegate.
    /// </summary>
    public interface IBackupDelegate
    {
        /// <summary>
        /// Called when a database backup is complete.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        void OnBackupComplete(DatabaseTargetConfigurationElement target);

        /// <summary>
        /// Called when a database backup begins.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        void OnBackupStart(DatabaseTargetConfigurationElement target);

        /// <summary>
        /// Called when a database backup file has been compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        void OnCompressComplete(DatabaseTargetConfigurationElement target);

        /// <summary>
        /// Called when a database backup file is about to be compressed.
        /// </summary>
        /// <param name="target">The backup's target.</param>
        void OnCompressStart(DatabaseTargetConfigurationElement target);
    }
}
