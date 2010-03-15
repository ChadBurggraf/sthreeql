using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SThreeQL.Test
{
    [TestClass]
    public class CompressorTests
    {
        protected static string TestFilePath
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, "CompressMe.txt");
            }
        }

        [TestMethod]
        public void Compressor_CanCompressIonic()
        {
            ICompressor compressor = new GZipCompressor();
            string archive = compressor.Compress(TestFilePath);

            Assert.IsTrue(File.Exists(archive));
        }

        [TestMethod]
        public void Compressor_CanDecompressIonic()
        {
            ICompressor compressor = new GZipCompressor();
            string file = compressor.Decompress(compressor.Compress(TestFilePath));

            Assert.IsTrue(File.Exists(file));
            Assert.AreEqual(TestFilePath, file);
        }
    }
}
