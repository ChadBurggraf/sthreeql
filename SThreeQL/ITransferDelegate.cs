using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SThreeQL
{
    /// <summary>
    /// Defines a transfer delegate.
    /// </summary>
    public interface ITransferDelegate
    {
        /// <summary>
        /// Called when a transfer is complete.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        void OnTransferComplete(TransferInfo info);

        /// <summary>
        /// Called when a transfer's progress has been updated..
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        void OnTransferProgress(TransferInfo info);

        /// <summary>
        /// Called when a transfer begins.
        /// </summary>
        /// <param name="info">The transfer's meta data.</param>
        void OnTransferStart(TransferInfo info);
    }
}
