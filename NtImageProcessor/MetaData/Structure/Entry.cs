using NtImageProcessor.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Structure
{
    public class Entry
    {
        public const int TAG_LEN = 2;
        public const int TYPE_LEN = 2;
        public const int COUNT_LEN = 4;
        public const int OFFSET_LEN = 4;

        /// <summary>
        /// 
        /// </summary>
        public enum EntryType
        {
            /// <summary>
            /// 8bit unsigned integer.
            /// </summary>
            Byte,
            /// <summary>
            /// ASCII characters.
            /// </summary>
            Ascii,
            /// <summary>
            /// 16bit unsigned integer
            /// </summary>
            Short,
            /// <summary>
            /// 32bit unsigned integer
            /// </summary>
            Long,
            /// <summary>
            ///  numerator as Long, denominator as Long
            /// </summary>
            Rational,
            /// <summary>
            /// Byte
            /// </summary>
            Undefined,
            /// <summary>
            /// Signed short
            /// </summary>
            SShort,
            /// <summary>
            /// Signed long
            /// </summary>
            SLong, // signed long
            /// <summary>
            /// Signed Rational. numerator and denominagor are in Signed long.
            /// </summary>
            SRational,
        };

        public UInt32 Tag
        {
            get;
            set;
        }

        public EntryType Type { get; set; }

        /// <summary>
        /// Note that this value means a number of data. If Count is 1 and its type is Long, actual size will be 32 bit.
        /// </summary>
        public UInt32 Count { get; set; }

        /// <summary>
        /// In case of entry's size is 5 bytes or more, the value is not recorded in same section.
        /// It's start address can be calcurated by Tiff header addr + this offset value.
        /// </summary>
        public UInt32 Offset { get; set; }

        /// <summary>
        /// Stores raw value data.
        /// </summary>
        public byte[] value { get; set; }

        /// <summary>
        /// Returns value as integer
        /// </summary>
        public UInt32[] IntValues
        {
            get
            {
                var v = new UInt32[this.Count];
                var len = Util.ConvertToDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    v[i] = Util.GetUIntValue(value, i * len, len);
                }
                return v;
            }
        }

        /// <summary>
        /// Returns value as unsigned rational data
        /// </summary>
        public UnsignedFraction[] UFractionValues
        {
            get
            {
                var v = new UnsignedFraction[Count];
                var len = Util.ConvertToDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    //v[i] = Util.GetUIntValue(value, i * len, len); 
                    var f = new UnsignedFraction();
                    f.Numerator = Util.GetUIntValue(value, i * len, 4);
                    f.Denominator = Util.GetUIntValue(value, i * len + 4, 4);
                    v[i] = f;
                }
                return v;
            }
        }

        public SignedFraction[] SFractionValues
        {
            get
            {
                var v = new SignedFraction[Count];
                var len = Util.ConvertToDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    //v[i] = Util.GetUIntValue(value, i * len, len); 
                    var f = new SignedFraction();
                    f.Numerator = Util.GetSIntValue(value, i * len, 4);
                    f.Denominator = Util.GetSIntValue(value, i * len + 4, 4);
                    v[i] = f;
                }
                return v;
            }
        }

        public double[] DoubleValues
        {
            get
            {

                var v = new double[Count];
                var len = Util.ConvertToDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    double val = 0;
                    switch (this.Type)
                    {
                        case EntryType.Rational:
                            val = (double)this.UFractionValues[i].Numerator / (double)this.UFractionValues[i].Denominator;
                            break;
                        case EntryType.SRational:
                            val = (double)this.SFractionValues[i].Numerator / (double)this.SFractionValues[i].Denominator;
                            break;
                    }
                    v[i] = val;
                }
                return v;
            }
        }

        public string StringValue
        {
            get
            {
                return Encoding.UTF8.GetString(value, 0, value.Length);
            }
        }

        public Entry()
        {
        }


    }
}
