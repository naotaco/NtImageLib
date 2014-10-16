using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NtImageProcessorTest
{
    public static class TestUtil
    {
        public static void AreEqual(byte[] expected, byte[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static void AreEqual(Int32[] expected, Int32[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static void AreEqual(UInt32[] expected, UInt32[] actual, string message = "")
        {
            Assert.AreEqual(expected.Length, actual.Length, message + " at length comparison");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], message + " at element comparison. i: " + i);
            }
        }

        public static byte[] GetLastElements(byte[] array, int newLength)
        {
            var newArray = new byte[newLength];
            Array.Copy(array, array.Length - newLength, newArray, 0, newLength);
            return newArray;
        }

        public static byte[] GetFirstElements(byte[] array, int newLength)
        {
            var newArray = new byte[newLength];
            Array.Copy(array, newArray, newLength);
            return newArray;
        }


        public static Stream GetResourceStream(string filename)
        {
            return Application.GetResourceStream(new Uri("Assets/TestImages/" + filename, UriKind.Relative)).Stream;
        }

        public static byte[] GetResourceByteArray(string filename)
        {
            Stream myFileStream = GetResourceStream(filename);
            byte[] buf = new byte[100000000];
            if (myFileStream.CanRead)
            {
                int read;
                read = myFileStream.Read(buf, 0, (int)myFileStream.Length);
                if (read > 0)
                {
                    var image = new byte[read];
                    Array.Copy(buf, image, read);
                    return image;
                }
            }
            return null;
        }

        public static void CompareJpegMetaData(JpegMetaData meta1, JpegMetaData meta2, string filename, bool GpsIfdExists, bool ExifIfdExists = true)
        {
            Debug.WriteLine("file: " + filename);
            TestUtil.AreEqual(meta1.App1Data, meta2.App1Data, filename + " App1 data");
            TestUtil.AreEqual(meta1.PrimaryIfd, meta2.PrimaryIfd, filename + "Primary IFD");

            if (ExifIfdExists)
            {
                TestUtil.AreEqual(meta1.ExifIfd, meta2.ExifIfd, filename + "Exif IFD");
            }
            else
            {
                Assert.IsNull(meta1.ExifIfd);
                Assert.IsNull(meta2.ExifIfd);
            }

            if (GpsIfdExists)
            {
                TestUtil.AreEqual(meta1.GpsIfd, meta2.GpsIfd, filename + "Gps IFD");
            }
            else
            {
                Assert.IsNull(meta1.GpsIfd);
                Assert.IsNull(meta2.GpsIfd);
            }

        }

        public static void AreEqual(IfdData data1, IfdData data2, string message)
        {
            message = message + " ";
            Assert.AreEqual(data1.NextIfdPointer, data2.NextIfdPointer, message + "Next IFD pointer");
            Assert.AreEqual(data1.Length, data2.Length, message + "length");
            Assert.AreEqual(data1.Offset, data2.Offset, message + "offset");
            Assert.AreEqual(data1.Entries.Count, data2.Entries.Count, message + "entry num");

        }
    }
}
