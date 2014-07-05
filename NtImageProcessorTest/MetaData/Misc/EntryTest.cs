using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Diagnostics;

namespace NtImageProcessorTest.MetaData.Misc
{
    [TestClass]
    public class EntryTest
    {
        private readonly Entry.EntryType[] UnsignedEntryTypes = new Entry.EntryType[] { Entry.EntryType.Short, Entry.EntryType.Long, Entry.EntryType.Byte };
        private readonly Entry.EntryType[] SignedEntryTypes = new Entry.EntryType[] { Entry.EntryType.SShort, Entry.EntryType.SLong };
        private readonly UInt32[] TestValues = new UInt32[] { 0, 1, 0xFF, 0xFFFF, 0xFFFFFFFF };
        private readonly Int32[] TestNegativeValues = new Int32[] { -1, -0xFF, -0xFFFF, -0x7FFFFFFF };


        [TestMethod]
        public void EntryBasicTest()
        {
            var entry1 = new Entry()
            {
                Tag = 0x02,
                Type = Entry.EntryType.Long,
                Count = 1,
            };
            entry1.UIntValues = new UInt32[] { 0 };

            TestUtil.AreEqual(new byte[] { 0, 0, 0, 0 }, entry1.value);
        }

        [TestMethod]
        public void EntryUnsignedValuesIOTest()
        {
            foreach (Entry.EntryType type in UnsignedEntryTypes)
            {
                foreach (UInt32 value in TestValues)
                {
                    var entry = new Entry()
                    {
                        Type = type,
                        Count = 1,
                        Tag = 0x0,
                    };

                    var maxValue = Math.Pow(2, 8 * Util.FindDataSize(type));
                    if (value >= maxValue)
                    {
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            entry.UIntValues = new UInt32[] { value };
                        });
                        continue;
                    }
                    entry.UIntValues = new UInt32[] { value };

                    Assert.AreEqual(value, entry.UIntValues[0]);
                }
            }
        }

        [TestMethod]
        public void EntrySignedValuesIOTest()
        {
            foreach (Int32 value in TestValues)
            {
                foreach (Entry.EntryType type in UnsignedEntryTypes)
                {
                    var entry = new Entry()
                    {
                        Type = type,
                        Count = 1,
                        Tag = 0x0,
                    };

                    var maxValue = Math.Pow(2, 8 * Util.FindDataSize(type)) / 2 - 1;
                    if (value >= maxValue)
                    {
                        Assert.ThrowsException<OverflowException>(() =>
                        {
                            entry.IntValues = new Int32[] { value };
                        });
                        continue;
                    }
                    else if (value < -maxValue)
                    {
                        Assert.ThrowsException<InvalidCastException>(() =>
                        {
                            entry.IntValues = new Int32[] { value };
                        });
                        continue;
                    }
                    entry.IntValues = new Int32[] { value };

                    Assert.AreEqual(value, entry.IntValues[0]);
                }
            }
        }

        private readonly double[] TestDoubleValues = new double[] { 0, 0.1, 2.1, 2435.324 };
        private readonly double[] TestNegativeDoubleValues = new double[] { -100, -10.322222, -1, -0.2, };

        [TestMethod]
        public void EntryDoubleIOTest()
        {
            foreach (double value in TestDoubleValues)
            {
                var entry = new Entry()
                {
                    Type = Entry.EntryType.Rational,
                    Count = 1,
                    Tag = 0x0,
                };
                entry.DoubleValues = new double[] { value };
                Assert.AreEqual(value, entry.DoubleValues[0], "value: " + value);

                var entry1 = new Entry()
                {
                    Type = Entry.EntryType.SRational,
                    Count = 1,
                    Tag = 0x0,
                };
                entry1.DoubleValues = new double[] { value };
                Assert.AreEqual(value, entry1.DoubleValues[0], "value: " + value);

            }

            foreach (double value in TestNegativeDoubleValues)
            {

                var entry = new Entry()
                {
                    Type = Entry.EntryType.Rational,
                    Count = 1,
                    Tag = 0x0,
                };
                Assert.ThrowsException<InvalidCastException>(() =>
                {
                    entry.DoubleValues = new double[] { value };
                });

                var entry1 = new Entry()
                {
                    Type = Entry.EntryType.SRational,
                    Count = 1,
                    Tag = 0x0,
                };
                entry1.DoubleValues = new double[] { value };
                Assert.AreEqual(value, entry1.DoubleValues[0], "value: " + value);
            }
        }

        private static readonly Dictionary<Int32[], Entry> EntryIntByteData = new Dictionary<Int32[], Entry>()
        {
            { new Int32[]{0},
                new Entry(){
                Type = Entry.EntryType.SShort,
                Count = 1,
                value = new byte[]{0,0},
            }},
            { new Int32[]{1,1},
                new Entry(){
                Type = Entry.EntryType.SShort,
                Count = 2,
                value = new byte[]{0,1,0,1},
            }},
            { new Int32[]{0x7FFF},
                new Entry(){
                Type = Entry.EntryType.SShort,
                Count = 1,
                value = new byte[]{0x7F,0xFF},
            }},
            { new Int32[]{-0x7FFF,-3},
                new Entry(){
                Type = Entry.EntryType.SShort,
                Count = 2,
                value = new byte[]{0xFF,0xFF,0x80,0x03},
            }},
            { new Int32[]{0},
                new Entry(){
                Type = Entry.EntryType.SLong,
                Count = 1,
                value = new byte[]{0,0,0,0},
            }},
            { new Int32[]{0x7FFFFFFF},
                new Entry(){
                Type = Entry.EntryType.SLong,
                Count = 1,
                value = new byte[]{0x7F, 0xFF, 0xFF, 0xFF},
            }},
            { new Int32[]{-0xFF},
                new Entry(){
                Type = Entry.EntryType.SLong,
                Count = 1,
                value = new byte[]{0x80,0,0,0xFF},
            }},
        };

        [TestMethod]
        public void EntryIntRawValueTest()
        {
            foreach (Int32[] values in EntryIntByteData.Keys)
            {
                TestUtil.AreEqual(values, EntryIntByteData[values].IntValues, "0 values 0 : " + values[0]);
                Debug.WriteLine("val: " + values[0]);

                var entry = new Entry()
                {
                    Type = EntryIntByteData[values].Type,
                    Count = EntryIntByteData[values].Count,
                };
                entry.IntValues = values;

                TestUtil.AreEqual(EntryIntByteData[values].value, entry.value, "1 values 0 : " + values[0]);
            }
        }

    }
}
