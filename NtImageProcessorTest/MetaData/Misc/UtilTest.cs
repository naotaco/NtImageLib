using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;

namespace NtImageProcessorTest.MetaData.Misc
{
    [TestClass]
    public class UtilTest
    {
        public static Dictionary<UInt32, byte[]> IntByteData = new Dictionary<UInt32, byte[]>()
        {
            // expected array is in Big endian.
            { 0, new byte[]{0,0,0,0}},
            { 1, new byte[]{0,0,0,1}},
            { 0xFF, new byte[]{0,0,0,0xFF}},
            { 0x1FF, new byte[]{0,0,1,0xFF}},
            { 0xFFFF, new byte[]{0,0,0xFF, 0xff}},
            { 0x1FFFFFF, new byte[]{1,0xff, 0xff, 0xff}},
            { 0xFFFFFFFF, new byte[]{0xFF, 0xff, 0xff,0xff}},
        };

        [TestMethod]
        public void IntToByte()
        {
            foreach (UInt32 key in IntByteData.Keys)
            {
                for (int length = 0; length < 6; length++)
                {
                    if (length == 1 || length == 2 || length == 3)
                    {
                        // todo.
                        continue;
                    }

                    if (length == 0 || length == 5)
                    {
                        // Invalid length.
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var expected_big = IntByteData[key];
                            var actual_big = Util.ConvertToByte(key, length, Definitions.Endian.Big);
                        });
                    }
                    else
                    {
                        var expected_big = IntByteData[key];
                        var actual_big = Util.ConvertToByte(key, length, Definitions.Endian.Big);
                        TestUtil.AreEqual(expected_big, actual_big);
                    }

                    if (length == 0 || length == 5)
                    {                        
                        // Invalid length.
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var expected_Little = IntByteData[key];
                            var actual_little = Util.ConvertToByte(key, length, Definitions.Endian.Little);

                        });
                    }
                    else
                    {
                        var expected_Little = new byte[length];
                        IntByteData[key].CopyTo(expected_Little, 0);
                        Array.Reverse(expected_Little);
                        var actual_little = Util.ConvertToByte(key, length, Definitions.Endian.Little);
                        TestUtil.AreEqual(expected_Little, actual_little);
                    }
                }
            }
        }
    }
}
