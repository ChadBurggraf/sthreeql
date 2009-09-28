using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Affirma.ThreeSharp;
using Affirma.ThreeSharp.Query;
using Affirma.ThreeSharp.Model;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Provides common helpers.
    /// </summary>
    public static class Common
    {
        private const string RAR_START_ERROR = "WinRAR does not appear to be installed at the location specified in the configuration.";
        private const string RAR_RUN_ERROR = "An error occurred while running WinRAR.";
        private static bool? compressionEnabled;
        private static string winRarPath;

        /// <summary>
        /// Gets a value indicating whether compression is enabled by 
        /// the definition of winRarLocation in the configuration.
        /// </summary>
        public static bool CompressionEnabled
        {
            get
            {
                if (compressionEnabled == null)
                {
                    compressionEnabled = !String.IsNullOrEmpty(WinRarPath) &&
                        File.Exists(WinRarPath);
                }

                return compressionEnabled.Value;
            }
        }

        /// <summary>
        /// Compresses the file at the given path. Retains the same
        /// filename after compression.
        /// </summary>
        /// <param name="path">The path of the file to compress.</param>
        public static void CompressFile(string path)
        {
            if (CompressionEnabled)
            {
                string tempPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".rar");

                Process rar = CreateRarProcess();
                rar.StartInfo.Arguments = String.Format("a -ep \"{0}\" \"{1}\"",
                    tempPath,
                    path
                );

                if (rar.Start())
                {
                    rar.WaitForExit();

                    if (rar.ExitCode == 0)
                    {
                        File.Delete(path);
                        File.Move(tempPath, path);
                    }
                    else
                    {
                        throw new Exception(RAR_RUN_ERROR);
                    }
                }
                else
                {
                    throw new InvalidOperationException(RAR_START_ERROR);
                }
            }
        }

        /// <summary>
        /// Creates a connection string from the given database target configuration.
        /// </summary>
        /// <param name="config">The database target to create a connection string for.</param>
        /// <returns>A connection string.</returns>
        public static string CreateConnectionString(DatabaseTargetConfigurationElement config)
        {
            return String.Concat(
                "data source=", config.DataSource, ";",
                "user id=", config.UserId, ";",
                "password=", config.Password, ";",
                "connection timeout=", SThreeQLConfiguration.Section.DatabaseTimeout, ";"
            );
        }

        /// <summary>
        /// Creates a process that can be used to invoke WinRAR.
        /// </summary>
        /// <returns>The created process.</returns>
        private static Process CreateRarProcess()
        {
            Process rar = new Process();
            rar.StartInfo.UseShellExecute = false;
            rar.StartInfo.FileName = WinRarPath;
            rar.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            rar.StartInfo.RedirectStandardOutput = true;
            rar.StartInfo.RedirectStandardError = true;

            return rar;
        }

        /// <summary>
        /// Decompresses the file at the given path.
        /// Retains the same filename after decompression.
        /// </summary>
        /// <param name="path">The path of the file to decompress.</param>
        public static void DecompressFile(string path)
        {
            if (CompressionEnabled)
            {
                string tempDirPath = Path.Combine(Path.GetDirectoryName(path), Path.GetRandomFileName());

                try
                {
                    Directory.CreateDirectory(tempDirPath);

                    Process rar = CreateRarProcess();
                    rar.StartInfo.Arguments = String.Format("x \"{0}\" *.* \"{1}\\\"",
                        path,
                        tempDirPath
                    );

                    if (rar.Start())
                    {
                        rar.WaitForExit();

                        if (rar.ExitCode == 0)
                        {
                            string[] files = Directory.GetFiles(tempDirPath);

                            if (files.Length > 0)
                            {
                                File.Delete(path);
                                File.Move(files[0], path);
                            }
                            else
                            {
                                throw new Exception("There were no files in the given archive.");
                            }
                        }
                        else
                        {
                            throw new Exception(RAR_RUN_ERROR);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(RAR_START_ERROR);
                    }
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempDirPath))
                        {
                            Directory.Delete(tempDirPath, true);
                        }
                    }
                    catch
                    {
                        // Eat it.
                    }
                }
            }
        }

        /// <summary>
        /// Gets an embedded resource's text contents.
        /// </summary>
        /// <param name="resourceName">The name of an embedded resource.</param>
        /// <returns>A resource's text contents.</returns>
        public static string GetEmbeddedResourceText(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Gets a redirect URL from the given service if it is currently redirecting.
        /// Returns null if the service is not redirecting.
        /// </summary>
        /// <param name="service">The service to get the redirect URL for.</param>
        /// <param name="bucketName">The name of the bucket to get the redirect URL for.</param>
        /// <returns>A redirect URL, or null if not applicable.</returns>
        public static string GetRedirectUrl(this IThreeSharp service, string bucketName, string prefix)
        {
            string redirectUrl = null;

            using (BucketListRequest testRequest = new BucketListRequest(bucketName, prefix))
            {
                testRequest.Method = "HEAD";

                using (BucketListResponse testResponse = service.BucketList(testRequest))
                {
                    if (testResponse.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        redirectUrl = testResponse.Headers["Location"].ToString();
                    }
                }
            }

            return redirectUrl;
        }

        /// <summary>
        /// Gets the AWS key name to use for the given catalog name and the current
        /// system time.
        /// </summary>
        /// <param name="catalogName">The catalog name to get the AWS key name for.</param>
        /// <returns>An AWS key name.</returns>
        public static string ToCatalogNameAWSKey(this string catalogName)
        {
            return String.Concat(
                catalogName.ToCatalogNamePrefix(),
                "_",
                DateTime.Now.ToISO8601UTCString(),
                (CompressionEnabled ? ".rar" : ".bak")
            );
        }

        /// <summary>
        /// Gets the AWS-safe prefix to use for the given catalog name.
        /// </summary>
        /// <param name="catalogName">The catalog name to get an AWS prefix for.</param>
        /// <returns>An AWS prefix.</returns>
        public static string ToCatalogNamePrefix(this string catalogName)
        {
            return Regex.Replace(
                Regex.Replace(catalogName, @"[^a-z0-9]", "_", RegexOptions.IgnoreCase),
                @"_+",
                "_"
            ).ToLowerInvariant();
        }

        /// <summary>
        /// Returns a string representation of the given DateTime object
        /// that conforms to ISO 8601 (in UTC).
        /// </summary>
        /// <param name="dateTime">The DateTime object to convert.</param>
        /// <returns>A string.</returns>
        public static string ToISO8601UTCString(this DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();
            return String.Format("{0:s}.{0:fff}Z", dateTime);
        }

        /// <summary>
        /// Gets the path to the WinRAR executable as defined in the configuration.
        /// </summary>
        public static string WinRarPath
        {
            get
            {
                if (winRarPath == null)
                {
                    winRarPath = SThreeQLConfiguration.Section.WinRarLocation.Trim();

                    if (!String.IsNullOrEmpty(winRarPath))
                    {
                        winRarPath = Path.Combine(winRarPath, "rar.exe");
                    }
                }

                return winRarPath;
            }
        }
    }
}
