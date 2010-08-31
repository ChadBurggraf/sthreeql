//-----------------------------------------------------------------------
// <copyright file="ICompressor.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace SThreeQL
{
    using System;

    /// <summary>
    /// Interface definition for file compressors.
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Compresses the file at the given path. Returns the path to the
        /// compressed file.
        /// </summary>
        /// <param name="path">The path to compress.</param>
        /// <returns>The path of the compressed file.</returns>
        string Compress(string path);

        /// <summary>
        /// Decompresses the file at the given path. Returns the path of the
        /// decompressed file.
        /// </summary>
        /// <param name="path">The path to decompress.</param>
        /// <returns>The path of the decompressed file.</returns>
        string Decompress(string path);
    }
}
