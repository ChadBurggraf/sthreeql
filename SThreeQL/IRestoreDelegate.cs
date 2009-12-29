using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Defines a restore delegate.
    /// </summary>
    public interface IRestoreDelegate
    {
        /// <summary>
        /// Called when a database restore is complete.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        void OnRestoreComplete(DatabaseRestoreTargetConfigurationElement target);

        /// <summary>
        /// Called when a database restore begins.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        void OnRestoreStart(DatabaseRestoreTargetConfigurationElement target);

        /// <summary>
        /// Called when a database backup file has been decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        void OnDecompressComplete(DatabaseRestoreTargetConfigurationElement target);

        /// <summary>
        /// Called when a database backup file is about to be decompressed.
        /// </summary>
        /// <param name="target">The restore's target.</param>
        void OnDeompressStart(DatabaseRestoreTargetConfigurationElement target);
    }
}
