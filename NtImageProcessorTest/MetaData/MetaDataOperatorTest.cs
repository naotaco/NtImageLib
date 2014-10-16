using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NtImageProcessorTest.MetaData
{
    [TestClass]
    public class MetaDataOperatorTest
    {
        [TestMethod]
        public void ParseInvalidImage()
        {
            foreach (string file in TestFiles.InvalidImages)
            {
                var stream = TestUtil.GetResourceStream(file);
                Assert.ThrowsException<UnsupportedFileFormatException>(() =>
                {
                    NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);
                });
                var array = TestUtil.GetResourceByteArray(file);
                Assert.ThrowsException<UnsupportedFileFormatException>(() =>
                {
                    NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);
                });
            }
        }

        [TestMethod]
        public void ParseValidImage()
        {
            foreach (string file in TestFiles.ValidImages)
            {
                var stream = TestUtil.GetResourceStream(file);
                NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                var array = TestUtil.GetResourceByteArray(file);
                NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);
            }
        }

        [TestMethod]
        public void ParseTestWithoutGeotag()
        {
            foreach (string file in TestFiles.ImagesWithoutGeotag)
            {
                var stream = TestUtil.GetResourceStream(file);
                var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                var array = TestUtil.GetResourceByteArray(file);
                var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                TestUtil.CompareJpegMetaData(meta1, meta2, file, false);
            }
        }

        [TestMethod]
        public void ParseTestWithLessTag()
        {
            foreach (string file in TestFiles.ImagesWithoutGeotagAndExiftag)
            {
                var stream = TestUtil.GetResourceStream(file);
                var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                var array = TestUtil.GetResourceByteArray(file);
                var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                TestUtil.CompareJpegMetaData(meta1, meta2, file, false, false);
            }
        }

        [TestMethod]
        public void ParseTestWithGeotag()
        {
            foreach (string file in TestFiles.GeotagImages)
            {
                var stream = TestUtil.GetResourceStream(file);
                var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                var array = TestUtil.GetResourceByteArray(file);
                var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                TestUtil.CompareJpegMetaData(meta1, meta2, file, true);
            }
        }
    }
}
