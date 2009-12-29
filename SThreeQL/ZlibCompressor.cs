using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DotZLib;
using SThreeQL.Configuration;

namespace SThreeQL
{
    /// <summary>
    /// Implements simple path compression/decompression using GZip
    /// and ZLib1.dll.
    /// </summary>
    public class ZlibCompressor : ICompressor
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

            using (GZipStream gzip = new GZipStream(outputPath, CompressLevel.Best))
            {
                using (FileStream file = File.OpenRead(path))
                {
                    byte[] buffer = new byte[4096];
                    int count = 0;

                    while (0 < (count = file.Read(buffer, 0, buffer.Length)))
                    {
                        gzip.Write(buffer, 0, count);
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

            using (FileStream file = File.Create(outputPath))
            {
                using (GZipStream gzip = new GZipStream(path))
                {
                    byte[] buffer = new byte[4096];
                    int count = 0;

                    while (0 < (count = gzip.Read(buffer, 0, buffer.Length)))
                    {
                        file.Write(buffer, 0, count);
                    }
                }
            }

            return outputPath;
        }
    }
}
