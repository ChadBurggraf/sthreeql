//-----------------------------------------------------------------------
// <copyright file="GZipCompressor.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using Ionic.Zlib;

    /// <summary>
    /// Implements file compression and decompression using the Ionic Zlib library.
    /// </summary>
    public class GZipCompressor : ICompressor
    {
        /// <summary>
        /// Compresses the file at the given path. Returns the path to the
        /// compressed file.
        /// </summary>
        /// <param name="path">The path to compress.</param>
        /// <returns>The path of the compressed file.</returns>
        public string Compress(string path)
        {
            string outputPath = String.Concat(path, ".gz");

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (FileStream fs = File.OpenRead(path))
            {
                using (FileStream output = File.Create(outputPath))
                {
                    using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, CompressionLevel.BestCompression))
                    {
                        byte[] buffer = new byte[4096];
                        int count = 0;

                        while (0 < (count = fs.Read(buffer, 0, buffer.Length)))
                        {
                            gzip.Write(buffer, 0, count);
                        }
                    }
                }
            }

            return outputPath;
        }

        /// <summary>
        /// Decompresses the file at the given path. Returns the path of the
        /// decompressed file.
        /// </summary>
        /// <param name="path">The path to decompress.</param>
        /// <returns>The path of the decompressed file.</returns>
        public string Decompress(string path)
        {
            string outputPath = Regex.Replace(path, @"\.gz$", String.Empty, RegexOptions.IgnoreCase);

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (FileStream fs = File.OpenRead(path))
            {
                using (FileStream output = File.Create(outputPath))
                {
                    using (GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[4096];
                        int count = 0;

                        while (0 < (count = gzip.Read(buffer, 0, buffer.Length)))
                        {
                            output.Write(buffer, 0, count);
                        }
                    }
                }
            }

            return outputPath;
        }
    }
}
