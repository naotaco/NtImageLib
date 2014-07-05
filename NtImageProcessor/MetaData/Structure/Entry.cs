using NtImageProcessor.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// This value determins internal alignment of raw data, "value".
        /// </summary>
        public const Definitions.Endian InternalEndian = Definitions.Endian.Big;

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
        /// Note that alignment of this raw value is determined by "InternalEndian".
        /// before storing byte array directly, align it to the endian.
        /// </summary>
        public byte[] value { get; set; }

        /// <summary>
        /// Get/set values as unsigned int.
        /// This method stores its value as byte array.
        /// To determin it's size (char, shoft, long), set Type of this class before setting values.
        /// </summary>
        public UInt32[] UIntValues
        {
            get
            {
                var v = new UInt32[this.Count];
                var len = Util.FindDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    v[i] = Util.GetUIntValue(value, i * len, len, InternalEndian);
                }
                return v;
            }
            set
            {
                this.Count = (UInt32)value.Length;
                var newValue = new byte[value.Length * Util.FindDataSize(this.Type)];
                for (int i = 0; i < value.Length; i++)
                {
                    Debug.WriteLine("value: " + value[i]);
                    var v = Util.ToByte(value[i], Util.FindDataSize(this.Type), InternalEndian);
                    Array.Copy(v, 0, newValue, i * Util.FindDataSize(this.Type), v.Length);
                }
                this.value = newValue;
            }
        }

        /// <summary>
        /// Get/set values as signed int.
        /// This method stores its value as byte array.
        /// To determin it's size (char, shoft, long), set Type of this class before setting values.
        /// </summary>
        public Int32[] IntValues
        {
            get
            {
                var v = new Int32[this.Count];
                var len = Util.FindDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    v[i] = Util.GetSIntValue(value, i * len, len, InternalEndian);
                }
                return v;
            }
            set 
            {
                this.Count = (UInt32)value.Length;
                var newValue = new byte[value.Length * Util.FindDataSize(this.Type)];
                for (int i = 0; i < value.Length; i++)
                {
                    var v = Util.ToByte(value[i], Util.FindDataSize(this.Type), InternalEndian);
                    Array.ConstrainedCopy(v, 0, newValue, i * Util.FindDataSize(this.Type), v.Length);
                }
                this.value = newValue;
            }
        }

        /// <summary>
        /// get/set values as unsigned rational data.
        /// </summary>
        public UnsignedFraction[] UFractionValues
        {
            get
            {
                var v = new UnsignedFraction[Count];
                var len = Util.FindDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    //v[i] = Util.GetUIntValue(value, i * len, len); 
                    var f = new UnsignedFraction();
                    f.Numerator = Util.GetUIntValue(value, i * len, 4, InternalEndian);
                    f.Denominator = Util.GetUIntValue(value, i * len + 4, 4, InternalEndian);
                    v[i] = f;
                }
                return v;
            }
            set
            {
                this.Count = (UInt32)value.Length;
                this.Type = EntryType.Rational;
                var newValue = new byte[value.Length * Util.FindDataSize(this.Type)];
                for (int i = 0; i < value.Length; i++)
                {
                    var v = Util.ToByte(value[i], InternalEndian);
                    Array.Copy(v, 0, newValue, i * Util.FindDataSize(this.Type), Util.FindDataSize(this.Type));
                }
                this.value = newValue;
            }
        }

        /// <summary>
        /// get/set values as Signed rational data.
        /// </summary>
        public SignedFraction[] SFractionValues
        {
            get
            {
                var v = new SignedFraction[Count];
                var len = Util.FindDataSize(this.Type);
                for (int i = 0; i < Count; i++)
                {
                    //v[i] = Util.GetUIntValue(value, i * len, len); 
                    var f = new SignedFraction();
                    f.Numerator = Util.GetSIntValue(value, i * len, 4, InternalEndian);
                    f.Denominator = Util.GetSIntValue(value, i * len + 4, 4, InternalEndian);
                    v[i] = f;
                }
                return v;
            }
            set
            {
                this.Count = (UInt32)value.Length;
                this.Type = EntryType.SRational;
                var newValue = new byte[value.Length * Util.FindDataSize(this.Type)];
                for (int i = 0; i < value.Length; i++)
                {
                    var v = Util.ToByte(value[i], InternalEndian);
                    Array.Copy(v, 0, newValue, i * Util.FindDataSize(this.Type), Util.FindDataSize(this.Type));
                }
                this.value = newValue;
            }
        }

        /// <summary>
        /// Get/set double values.
        /// This property supports only Rational and SRational types;
        /// specify its type before setting.
        /// </summary>
        public double[] DoubleValues
        {
            get
            {

                var v = new double[Count];
                var len = Util.FindDataSize(this.Type);
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
            set
            {
                this.Count = (UInt32)value.Length;
                var newValue = new byte[value.Length * Util.FindDataSize(this.Type)];
                for (int i = 0; i < value.Length; i++)
                {
                    var v = new byte[Util.FindDataSize(this.Type)];
                    switch (this.Type)
                    {
                        case EntryType.Rational:
                            var f = Util.ToUnsignedFraction(value[i]);
                            v = Util.ToByte(f, InternalEndian);
                            break;
                        case EntryType.SRational:
                            var f1 = Util.ToSignedFraction(value[i]);
                            v = Util.ToByte(f1, InternalEndian);
                            break;
                    }
                    Array.Copy(v, 0, newValue, i * Util.FindDataSize(this.Type), Util.FindDataSize(this.Type));
                }
                this.value = newValue;
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
