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
                using (var stream = TestUtil.GetResourceStream(file))
                {
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
                GC.Collect();
            }
        }

        [TestMethod]
        public void ParseValidImage()
        {
            foreach (string file in TestFiles.ValidImages)
            {
                using (var stream = TestUtil.GetResourceStream(file))
                {
                    NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                    var array = TestUtil.GetResourceByteArray(file);
                    NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);
                }
                GC.Collect();
            }
        }

        [TestMethod]
        public void ParseTestWithoutGeotag()
        {
            foreach (string file in TestFiles.ImagesWithoutGeotag)
            {
                using (var stream = TestUtil.GetResourceStream(file))
                {
                    var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                    var array = TestUtil.GetResourceByteArray(file);
                    var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                    TestUtil.CompareJpegMetaData(meta1, meta2, file, false);
                }
                GC.Collect();
            }
        }

        [TestMethod]
        public void ParseTestWithLessTag()
        {
            foreach (string file in TestFiles.ImagesWithoutGeotagAndExiftag)
            {
                using (var stream = TestUtil.GetResourceStream(file))
                {
                    var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                    var array = TestUtil.GetResourceByteArray(file);
                    var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                    TestUtil.CompareJpegMetaData(meta1, meta2, file, false, false);
                }
                GC.Collect();
            }
        }

        [TestMethod]
        public void ParseTestWithGeotag()
        {
            foreach (string file in TestFiles.GeotagImages)
            {
                using (var stream = TestUtil.GetResourceStream(file))
                {
                    var meta1 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(stream);

                    var array = TestUtil.GetResourceByteArray(file);
                    var meta2 = NtImageProcessor.MetaData.JpegMetaDataParser.ParseImage(array);

                    TestUtil.CompareJpegMetaData(meta1, meta2, file, true);
                }
                GC.Collect();
            }
        }
    }
}
