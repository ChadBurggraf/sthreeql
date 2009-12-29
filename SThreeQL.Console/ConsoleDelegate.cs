using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SThreeQL.Configuration;

namespace SThreeQL.Console
{
    public class ConsoleDelegate : IBackupDelegate, IRestoreDelegate, ITransferDelegate
    {
        #region IBackupDelegate Members

        public void OnBackupComplete(DatabaseTargetConfigurationElement target)
        {
            System.Console.WriteLine("Done.");
        }

        public void OnBackupStart(DatabaseTargetConfigurationElement target)
        {
            System.Console.Write("Backing up database {0}... ", target.CatalogName);
        }

        public void OnCompressComplete(DatabaseTargetConfigurationElement target)
        {
            System.Console.WriteLine("Done.");
        }

        public void OnCompressStart(DatabaseTargetConfigurationElement target)
        {
            System.Console.Write("Compressing the backup file... ");
        }

        #endregion

        #region IRestoreDelegate Members

        public void OnRestoreComplete(DatabaseRestoreTargetConfigurationElement target)
        {
            System.Console.WriteLine("Done.");
        }

        public void OnRestoreStart(DatabaseRestoreTargetConfigurationElement target)
        {
            System.Console.Write("Restoring catalog {0}... ", target.RestoreCatalogName);
        }

        public void OnDecompressComplete(DatabaseRestoreTargetConfigurationElement target)
        {
            System.Console.WriteLine("Done.");
        }

        public void OnDeompressStart(DatabaseRestoreTargetConfigurationElement target)
        {
            System.Console.Write("Decompressing the backup file... ");
        }

        #endregion

        #region ITransferDelegate Members

        public void OnTransferComplete(TransferInfo info)
        {
            System.Console.WriteLine();

            if (info.Target is DatabaseRestoreTargetConfigurationElement)
            {
                System.Console.WriteLine("Download complete.");   
            }
            else
            {
                System.Console.WriteLine("Upload complete.");
            }
        }

        public void OnTransferProgress(TransferInfo info)
        {
            if (info.FileSize > 0 && info.BytesTransferred > 0)
            {
                System.Console.CursorLeft = 0;
                System.Console.Write("{0} of {1} ({2}%)          ",
                    info.BytesTransferred.ToFileSize(),
                    info.FileSize.ToFileSize(),
                    (int)((double)info.BytesTransferred / info.FileSize * 100));
            }
        }

        public void OnTransferStart(TransferInfo info)
        {
            if (info.Target is DatabaseRestoreTargetConfigurationElement)
            {
                System.Console.WriteLine("Downloading file {0}...", info.FileName);
            }
            else
            {
                System.Console.WriteLine("Uploading file {0} ({1})...", info.FileName, info.FileSize.ToFileSize());
            }
        }

        #endregion
    }
}
