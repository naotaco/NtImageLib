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
        public void UIntToByte()
        {
            foreach (UInt32 key in IntByteData.Keys)
            {
                for (int length = 0; length < 6; length++)
                {
                    if (length == 0 || length == 5)
                    {
                        // Invalid length.
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var e = IntByteData[key];
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                        }, "");
                        continue;
                    }

                    UInt64 maxNum = (UInt64)Math.Pow(2, (length * 8));
                    if (key >= maxNum)
                    {
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                            a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in big endian. length: " + length);
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in little endian. length: " + length);
                        continue;
                    }

                    var expected_big = TestUtil.GetLastElements(IntByteData[key], length);
                    var actual_big = Util.ToByte(key, length, Definitions.Endian.Big);
                    TestUtil.AreEqual(expected_big, actual_big, "Big endian Int->Byte test.");

                    var expected_Little = TestUtil.GetLastElements(IntByteData[key], length);
                    Array.Reverse(expected_Little);
                    var actual_little = Util.ToByte(key, length, Definitions.Endian.Little);
                    TestUtil.AreEqual(expected_Little, actual_little, "Little endian Int->Byte test.");

                }
            }
        }

        public static Dictionary<UInt32, byte[]> IntByteData2 = new Dictionary<UInt32, byte[]>()
        {
            // big endian.
            {0, new byte[]{0}},
            {1, new byte[]{1}},
            {0xFF, new byte[]{0, 0xFF}},
            {0xFF01, new byte[] {0xFF, 1}},
            {0xFFFF, new byte[] {0,0,0xFF, 0xFF}},
            {0xFFFF0000, new byte[] {0xFF, 0xFF, 0,0}},
        };

        [TestMethod]
        public void ByteToUInt_ShortArray()
        {
            // upto 4 bytes array
            foreach (UInt32 key in IntByteData2.Keys)
            {
                var Array_Big = new byte[IntByteData2[key].Length];
                IntByteData2[key].CopyTo(Array_Big, 0);
                var Array_Little = new byte[IntByteData2[key].Length];
                IntByteData2[key].CopyTo(Array_Little, 0);

                Array.Reverse(Array_Little);

                Assert.AreEqual(key, Util.GetUIntValue(Array_Little, 0, Array_Little.Length, Definitions.Endian.Little));
                Assert.AreEqual(key, Util.GetUIntValue(Array_Big, 0, Array_Big.Length, Definitions.Endian.Big));

                Assert.ThrowsException<InvalidCastException>(() =>
                {
                    var a = Util.GetUIntValue(Array_Little, 0, 0, Definitions.Endian.Little);
                }, "Invalid cast exception in Big endian. key: " + key);
                Assert.ThrowsException<InvalidCastException>(() =>
                {
                    var a = Util.GetUIntValue(Array_Big, 0, 0, Definitions.Endian.Big);
                }, "Invalid cast exception in Little endian. key: " + key);

                if (Array_Big.Length >= 4)
                {
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Big);
                    }, "InvalidCastException in Big endian. key: " + key);
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Little);
                    }, "InvalidCastException in Little endian. key: " + key);
                }
                else
                {
                    Assert.ThrowsException<IndexOutOfRangeException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Big.Length + 1, Definitions.Endian.Big);
                    }, "IIndexOutOfRangeException in Big endian. key: " + key);

                    Assert.ThrowsException<IndexOutOfRangeException>(() =>
                    {
                        var a = Util.GetUIntValue(Array_Little, 0, Array_Little.Length + 1, Definitions.Endian.Little);
                    }, "IIndexOutOfRangeException in Little endian. key: " + key);
                }
            }
        }

        public readonly Dictionary<Int32, byte[]> IntByteData3 = new Dictionary<Int32, byte[]>()
        {
            // expected array is in Big endian.
            { 0, new byte[]{0,0,0,0}},
            { 1, new byte[]{0,0,0,1}},
            { 0xFF, new byte[]{0,0,0,0xFF}},
            { 0x1FF, new byte[]{0,0,1,0xFF}},
            { 0xFFFF, new byte[]{0,0,0xFF, 0xff}},
            { 0x7FFFFFFF, new byte[]{0x7F,0xff, 0xff, 0xff}},
            { -1, new byte[]{0xFF, 0xFF,0xFF,0xFF}},
            { -0xFFFF, new byte[]{0xFF, 0xFF, 0x00, 0x01}},
            { -0x7FFFFFFF, new byte[]{0x80, 0x00, 0x00, 0x01}},
        };

        [TestMethod]
        public void IntToByte_Exception()
        {
            foreach (Int32 key in IntByteData3.Keys)
            {
                for (int length = 0; length < 6; length++)
                {
                    if (length == 0 || length == 5)
                    {
                        // Invalid length.
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var e = IntByteData3[key];
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                        }, "");
                        continue;
                    }

                    Int64 maxNum = (Int64)(Math.Pow(2, (length * 8)) / 2 - 1);
                    if (key > maxNum)
                    {
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                            a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in big endian. length: " + length);
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Overflow exception in little endian. length: " + length);
                        continue;
                    }
                    else if (key < -(maxNum + 1))
                    {
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Big);
                        }, "Invalid cast exception");

                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            var a = Util.ToByte(key, length, Definitions.Endian.Little);
                        }, "Invalid cast exception");
                    }
                }
            }
        }

        [TestMethod]
        public void IntToByte_Normal()
        {
            foreach (Int32 key in IntByteData3.Keys)
            {
                for (int length = 1; length < 5; length++)
                {
                    Int64 maxNum = (Int64)(Math.Pow(2, (length * 8)) / 2 - 1);
                    if (key > maxNum || key < -maxNum)
                    {
                        continue;
                    }

                    var expected_big = TestUtil.GetLastElements(IntByteData3[key], length);
                    var actual_big = Util.ToByte(key, length, Definitions.Endian.Big);
                    TestUtil.AreEqual(expected_big, actual_big, "Big endian Int->Byte test. key: " + key);

                    var expected_Little = TestUtil.GetLastElements(IntByteData3[key], length);
                    Array.Reverse(expected_Little);

                    var actual_little = Util.ToByte(key, length, Definitions.Endian.Little);
                    TestUtil.AreEqual(expected_Little, actual_little, "Little endian Int->Byte test. key: " + key);
                }
            }
        }

        [TestMethod]
        public void ByteToInt_LongArray()
        {
            var array1 = new byte[] { 0, 0, 0, 1, 2, 3, 0, 0, 0 };
            Assert.AreEqual((UInt32)0, Util.GetUIntValue(array1, 0, 1, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0, Util.GetUIntValue(array1, 0, 1, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)1, Util.GetUIntValue(array1, 3, 1, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)1, Util.GetUIntValue(array1, 3, 1, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0x102, Util.GetUIntValue(array1, 3, 2, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)0x201, Util.GetUIntValue(array1, 3, 2, Definitions.Endian.Little));
            Assert.AreEqual((UInt32)0x10203, Util.GetUIntValue(array1, 3, 3, Definitions.Endian.Big));
            Assert.AreEqual((UInt32)0x30201, Util.GetUIntValue(array1, 3, 3, Definitions.Endian.Little));
        }

        public static double[] TestDoubleData = new double[] { -0xFFFFFFFF, -0xFF, -1, 0, 1, 0xFF, 0xFFFFFFFF, -1.3, 3.3 };

        [TestMethod]
        public void DoubleToUnsignedFraction()
        {
            foreach (double value in TestDoubleData)
            {
                if (value < 0)
                {
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.ToUnsignedFraction(value);
                    }, "Invalid cast exception with value less than 0");
                    continue;
                }
                var fraction = Util.ToUnsignedFraction(value);
                Assert.AreEqual(value, ((double)fraction.Numerator / (double)fraction.Denominator));
            }
        }

        [TestMethod]
        public void DoubleToSignedFraction()
        {
            foreach (double value in TestDoubleData)
            {
                if (value > 0xEFFFFFFF)
                {
                    Assert.ThrowsException<OverflowException>(() =>
                    {
                        var a = Util.ToSignedFraction(value);
                    }, "Too big for signed int32 type");
                    continue;
                }
                else if (value < -0xEFFFFFFF)
                {
                    Assert.ThrowsException<InvalidCastException>(() =>
                    {
                        var a = Util.ToSignedFraction(value);
                    }, "too small for signed int type");
                    continue;
                }

                var fraction = Util.ToSignedFraction(value);
                Assert.AreEqual(value, ((double)fraction.Numerator / (double)fraction.Denominator));
            }
        }

        public static readonly Dictionary<Int32, byte[]> IntByteData4 = new Dictionary<Int32, byte[]>()
        {
            // big endian.
            {0, new byte[]{0}},
            {1, new byte[]{1}},
            {2, new byte[]{0, 2}},
            {0xFF, new byte[]{0, 0xFF}},
            {0x7F01, new byte[] {0x7F, 1}},
            {0xFFFF, new byte[] {0,0,0xFF, 0xFF}},
            {0x7FFF0000, new byte[] {0x7F, 0xFF, 0,0}},
            {-1, new byte[]{0xFF}},
            {-128, new byte[]{0x80}},
            {-2, new byte[] {0xFF, 0xFE}},
            {-0xFF, new byte[]{0xFF, 0x01}},
            {-0x7FFF, new byte[] {0x80, 0x01}},
            {-0x7FFFFFFF, new byte[]{0x80, 0x00, 0x00, 0x01}},
        };

        [TestMethod]
        public void ByteToSInt_Normal()
        {
            foreach (Int32 key in IntByteData4.Keys)
            {
                var value_big = new byte[IntByteData4[key].Length];
                IntByteData4[key].CopyTo(value_big, 0);
                var actual_big = Util.GetSIntValue(value_big, 0, value_big.Length, Definitions.Endian.Big);
                Assert.AreEqual(key, actual_big, "key: " + key);

                var value_little = new byte[IntByteData4[key].Length];
                IntByteData4[key].CopyTo(value_little, 0);
                Array.Reverse(value_little);

                var actual_little = Util.GetSIntValue(value_little, 0, value_little.Length, Definitions.Endian.Little);
                Assert.AreEqual(key, actual_little, "key: " + key);

            }
        }

        public static readonly byte[] TestByteArray1 = new byte[] { 0x00, 0x00, 0x1, 0x2, 0x3, 0x4 };
        public static readonly byte[] TestByteArray2 = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x4 };

        [TestMethod]
        public void ByteToSInt_Exception()
        {
            Assert.AreEqual(0, Util.GetSIntValue(TestByteArray1, 0, 1, Definitions.Endian.Big));
            Assert.AreEqual(0, Util.GetSIntValue(TestByteArray1, 0, 1, Definitions.Endian.Little));
            Assert.AreEqual(1, Util.GetSIntValue(TestByteArray1, 2, 1, Definitions.Endian.Big));
            Assert.AreEqual(1, Util.GetSIntValue(TestByteArray1, 2, 1, Definitions.Endian.Little));
            Assert.AreEqual(0x102, Util.GetSIntValue(TestByteArray1, 2, 2, Definitions.Endian.Big));
            Assert.AreEqual(0x201, Util.GetSIntValue(TestByteArray1, 2, 2, Definitions.Endian.Little));
            Assert.AreEqual(0x10203, Util.GetSIntValue(TestByteArray1, 1, 4, Definitions.Endian.Big));
            Assert.AreEqual(0x3020100, Util.GetSIntValue(TestByteArray1, 1, 4, Definitions.Endian.Little));


            Assert.ThrowsException<IndexOutOfRangeException>(() =>
            {
                var a = Util.GetSIntValue(TestByteArray1, 6, 1, Definitions.Endian.Little);
            });

            Assert.ThrowsException<InvalidCastException>(() =>
            {
                var a = Util.GetSIntValue(TestByteArray1, 2, 0, Definitions.Endian.Little);
            });

            Assert.ThrowsException<InvalidCastException>(() =>
            {
                var a = Util.GetSIntValue(TestByteArray1, 2, 7, Definitions.Endian.Little);
            });

        }
    }
}
