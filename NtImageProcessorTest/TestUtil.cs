using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
