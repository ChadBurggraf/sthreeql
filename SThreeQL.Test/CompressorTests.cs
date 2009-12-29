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
        public void Compressor_CanCompressZlib()
        {
            ICompressor compressor = new ZlibCompressor();
            string archive = compressor.Compress(TestFilePath);

            Assert.IsTrue(File.Exists(archive));
        }

        [TestMethod]
        public void Compressor_CanDecompressZlib()
        {
            ICompressor compressor = new ZlibCompressor();
            string file = compressor.Decompress(compressor.Compress(TestFilePath));

            Assert.IsTrue(File.Exists(file));
            Assert.AreEqual(TestFilePath, file);
        }
    }
}
