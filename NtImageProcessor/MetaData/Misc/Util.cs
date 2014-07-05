using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Structure;

namespace NtImageProcessor.MetaData.Misc
{
    public static class Util
    {
        public static UInt32 GetUIntValue(byte[] data, int address, int length, Definitions.Endian endian)
        {
            // if bigger than 4 bytes, can't set to int type.
            if (length > 4 || length <= 0)
            {
                throw new InvalidCastException("Uint32 type can't store more than 4 bytes");
            }

            UInt32 value = 0;
            for (int i = 0; i < length; i++)
            {
                // Debug.WriteLine(data[address + i].ToString("X"));
                if (endian == Definitions.Endian.Little)
                {
                    value += (UInt32)data[address + i] << (i * 8);
                }
                else
                {
                    value += (UInt32)data[address + i] << ((length - 1 - i) * 8);
                }
            }
            // Debug.WriteLine("Return " + value.ToString("X"));
            return value;
        }

        public static byte[] ToByte(UInt32 value, int length, Definitions.Endian endian)
        {
            if (length > 4 || length <= 0)
            {
                throw new InvalidCastException();
            }
            var ret = new byte[length];
            if (endian == Definitions.Endian.Little)
            {
                for (int i = 0; i < length; i++)
                {
                    ret[i] = (byte)(value & 0xFF);
                    value = value >> 8;
                }
            }
            else
            {
                for (int i = length - 1; i >= 0; i--)
                {
                    ret[i] = (byte)(value & 0xFF);
                    value = value >> 8;
                    // Debug.WriteLine("byte: " + ret[i].ToString("X"));
                }
            }
            // after conversion, remaining value must be 0.
            if (value > 0)
            {
                throw new OverflowException();
            }

            return ret;
        }

        public static byte[] ToByte(Int32 value, int length, Definitions.Endian endian)
        {
            if (value > Int32.MaxValue)
            {
                throw new OverflowException();
            }
            else if (value < Int32.MinValue)
            {
                throw new InvalidCastException();
            }

            bool isNegative = false;
            if (value < 0)
            {
                isNegative = true;
            }

            var ByteValue = new byte[length];
            if (isNegative)
            {
                ByteValue = ToByte((UInt32)(value * -1), length, endian);
                // set the highest bit
                if (endian == Definitions.Endian.Little)
                {
                    if ((ByteValue[ByteValue.Length - 1] & 0x80) == 0x80)
                    {
                        // if the highest bit is already set, it means overflow.
                        throw new OverflowException();
                    }
                    ByteValue[ByteValue.Length - 1] |= 0x80;
                }
                else
                {
                    if ((ByteValue[0] & 0x80) == 0x80)
                    {
                        // if the highest bit is already set, it means overflow.
                        throw new OverflowException();
                    }
                    ByteValue[0] |= 0x80;
                }
            }
            else
            {
                ByteValue = ToByte((UInt32)value, length, endian);
                if (endian == Definitions.Endian.Little)
                {
                    if ((ByteValue[ByteValue.Length - 1] & 0x80) == 0x80)
                    {
                        // if the highest bit is already set, it means overflow.
                        throw new OverflowException();
                    }
                }
                else
                {
                    if ((ByteValue[0] & 0x80) == 0x80)
                    {
                        // if the highest bit is already set, it means overflow.
                        throw new OverflowException();
                    }
                }
            }

            return ByteValue;
        }

        public static UnsignedFraction ToUnsignedFraction(double value)
        {
            if (value < 0)
            {
                throw new InvalidCastException();
            }

            if (value > UInt32.MaxValue)
            {
                throw new OverflowException();
            }

            var fraction = new UnsignedFraction();
            UInt32 denominator = 1;
            while (value - System.Math.Floor(value) != 0)
            {
                value *= 10;
                denominator *= 10;
            }

            if (value > UInt32.MaxValue || denominator > UInt32.MaxValue)
            {
                throw new OverflowException();
            }

            fraction.Numerator = (UInt32)System.Math.Floor(value);
            fraction.Denominator = denominator;
            return fraction;
        }

        public static SignedFraction ToSignedFraction(double value)
        {
            if (value > Int32.MaxValue)
            {
                throw new OverflowException();
            }

            if (value < Int32.MinValue)
            {
                throw new InvalidCastException();
            }

            var fraction = new SignedFraction();
            Int32 denominator = 1;
            while (value - System.Math.Floor(value) != 0)
            {
                value *= 10;
                denominator *= 10;
            }

            if (value > Int32.MaxValue || denominator > Int32.MaxValue)
            {
                throw new OverflowException();
            }

            fraction.Numerator = (Int32)System.Math.Floor(value);
            fraction.Denominator = denominator;
            return fraction;

        }

        public static byte[] ToByte(UnsignedFraction value, Definitions.Endian endian)
        {
            var ret = new byte[8];
            Array.Copy(Util.ToByte(value.Numerator, 4, endian), 0, ret, 0, 4);
            Array.Copy(Util.ToByte(value.Denominator, 4, endian), 0, ret, 4, 4);
            return ret;
        }

        public static byte[] ToByte(SignedFraction value, Definitions.Endian endian)
        {
            var ret = new byte[8];
            Array.Copy(Util.ToByte(value.Numerator, 4, endian), 0, ret, 0, 4);
            Array.Copy(Util.ToByte(value.Denominator, 4, endian), 0, ret, 4, 4);
            return ret;
        }

        public static byte[] ToByte(string str)
        {
            var ret = new byte[str.Length + 1];
            int i = 0;
            foreach (char c in str)
            {
                ret[i] = (byte)c;
                i++;
            }
            ret[str.Length] = 0;
            Debug.WriteLine(str + " " + ret.Length);
            return ret;
        }

        public static Int32 GetSIntValue(byte[] data, int address, int length, Definitions.Endian endian)
        {
            // if bigger than 4 bytes, can't set to int type.
            if (length > 4)
            {
                throw new InvalidCastException("Uint32 type can't store more than 4 bytes");
            }
            if (length < 1)
            {
                throw new InvalidCastException();
            }

            Int32 value = 0;
            bool IsNegative = false;
            // if highest bit is set, it's negative value.
            if (endian == Definitions.Endian.Big)
            {
                if ((data[address] & 0x80) == 0x80)
                {
                    IsNegative = true;
                    data[address] &= 0x7F; 
                }
                Debug.WriteLine("negative number: " + data[address + length - 1]);
            }
            else
            {
                if ((data[address + length - 1] & 0x80) == 0x80)
                {
                    IsNegative = true;
                    data[address + length - 1] &= 0x7F;
                    Debug.WriteLine("negative number: " + data[address + length - 1]);
                }
            }


            for (int i = 0; i < length; i++)
            {
                // Debug.WriteLine(data[address + i].ToString("X"));
                if (endian == Definitions.Endian.Little)
                {
                    value += (Int32)data[address + i] << (i * 8);
                    Debug.WriteLine("value: " + value);
                }
                else
                {
                    value += (Int32)data[address + i] << ((length - 1 - i) * 8);
                }
            }
            Debug.WriteLine("Return " + value.ToString("X"));
            if (IsNegative)
            {
                value *= -1;
            }
            return value;
        }

        public static double ToRoundUp(double dValue, int iDigits)
        {
            double dCoef = System.Math.Pow(10, iDigits);

            return dValue > 0 ? System.Math.Ceiling(dValue * dCoef) / dCoef :
                                System.Math.Floor(dValue * dCoef) / dCoef;
        }

        public static int FindDataSize(NtImageProcessor.MetaData.Structure.Entry.EntryType t)
        {
            int valueSize = 0;
            switch (t)
            {
                case Entry.EntryType.Ascii:
                case Entry.EntryType.Byte:
                case Entry.EntryType.Undefined: // ?
                    valueSize = 1;
                    break;
                case Entry.EntryType.Short:
                case Entry.EntryType.SShort:
                    valueSize = 2;
                    break;
                case Entry.EntryType.Long:
                case Entry.EntryType.SLong:
                    valueSize = 4;
                    break;
                case Entry.EntryType.Rational:
                case Entry.EntryType.SRational:
                    valueSize = 8;
                    break;
                default:
                    break;
            }
            return valueSize;
        }

        public static void DumpFirst16byte(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                if (i > (data.Length - 1))
                {
                    break;
                }
                sb.Append(data[i].ToString("X2"));
                sb.Append(" ");
            }
            Debug.WriteLine(sb.ToString());
        }

        public static void DumpByteArray(byte[] array, int from, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = from; i < from + length; i++)
            {
                if (i > (array.Length - 1))
                {
                    break;
                }
                sb.Append(array[i].ToString("X2"));
                sb.Append(" ");
                if (i % 16 == 15)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            Debug.WriteLine(sb.ToString());
        }

        public static void DumpByteArrayAll(byte[] array)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString("X2"));
                sb.Append(" ");
                if (i % 16 == 15)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            Debug.WriteLine(sb.ToString());
        }

        public static Entry.EntryType ToEntryType(UInt32 value)
        {
            switch (value)
            {
                case 0x1:
                    return Entry.EntryType.Byte;
                case 0x2:
                    return Entry.EntryType.Ascii;
                case 0x3:
                    return Entry.EntryType.Short;
                case 0x4:
                    return Entry.EntryType.Long;
                case 0x5:
                    return Entry.EntryType.Rational;
                case 0x7:
                    return Entry.EntryType.Undefined;
                case 0x8:
                    return Entry.EntryType.SShort;
                case 0x9:
                    return Entry.EntryType.SLong;
                case 0xA:
                    return Entry.EntryType.SRational;
                default:
                    return Entry.EntryType.Undefined;
            }
        }

        public static UInt32 ToUInt32(Entry.EntryType value)
        {
            switch (value)
            {
                case Entry.EntryType.Byte:
                    return 0x1;
                case Entry.EntryType.Ascii:
                    return 0x2;
                case Entry.EntryType.Short:
                    return 0x3;
                case Entry.EntryType.Long:
                    return 0x4;
                case Entry.EntryType.Rational:
                    return 0x5;
                case Entry.EntryType.Undefined:
                    return 0x7;
                case Entry.EntryType.SShort:
                    return 0x8;
                case Entry.EntryType.SLong:
                    return 0x9;
                case Entry.EntryType.SRational:
                    return 0xA;
                default:
                    return 0x0;
            }
        }

        public static Dictionary<UInt32, String> TagNames = new Dictionary<uint, string>()
        {
            // TIFF header
            { 0x0100, "ImageWidth"},
            { 0x0101, "ImageLength"},
            { 0x0102, "BitsPerSample"},
            { 0x0103, "Compression"},
            { 0x0106, "PhotometricInterpretation"},
            { 0x010E, "ImageDescription"},
            { 0x010F, "Make"},
            { 0x0110, "Model"},
            { 0x0111, "StripOffsets"},
            { 0x0112, "Orientation"},
            { 0x0115, "SamplesPerPixel"},
            { 0x0116, "RowsPerStrip"},
            { 0x0117, "StripByteCounts"},
            { 0x011A, "XResolution"},
            { 0x011B, "YResolution"},
            { 0x011C, "PlanarConfiguration"},
            { 0x0128, "ResolutionUnit"},
            { 0x012D, "TransferFunction"},
            { 0x0131, "Software"},
            { 0x0132, "DateTime"},
            { 0x013B, "Artist"},
            { 0x013E, "WhitePoint"},
            { 0x013F, "PrimaryChromaticities"},
            { 0x0201, "JPEGInterchangeFormat"},
            { 0x0202, "JPEGInterchangeFormatLength"},
            { 0x0211, "YCbCrCoefficients"},
            { 0x0212, "YCbCrSubSampling"},
            { 0x0213, "YCbCrPositioning"},
            { 0x0214, "ReferenceBlackWhite"},
            { 0x8298, "Copyright"},
            { 0x8769, "Exif IFD Pointer"},
            { 0x8825, "GPS IFD Pointer"},
            // Exif header
            { 0x829A, "ExposureTime"},
            { 0x829D, "FNumber"},
            { 0x8822, "ExposureProgram"},
            { 0x8824, "SpectralSensitivity"},
            { 0x8827, "PhotographicSensitivity"},
            { 0x8828, "OECF"},
            { 0x8830, "SensitivityType"},
            { 0x8831, "StandardOutputSensitivity"},
            { 0x8832, "RecommendedExposureIndex"},
            { 0x8833, "ISOSpeed"},
            { 0x8834, "ISOSpeedLatitudeyyy"},
            { 0x8835, "ISOSpeedLatitudezzz"},
            { 0x9000, "ExifVersion"},
            { 0x9003, "DateTimeOriginal"},
            { 0x9004, "DateTimeDigitized"},
            { 0x9101, "ComponentsConfiguration"},
            { 0x9102, "CompressedBitsPerPixel"},
            { 0x9201, "ShutterSpeedValue"},
            { 0x9202, "ApertureValue"},
            { 0x9203, "BrightnessValue"},
            { 0x9204, "ExposureBiasValue"},
            { 0x9205, "MaxApertureValue"},
            { 0x9206, "SubjectDistance"},
            { 0x9207, "MeteringMode"},
            { 0x9208, "LightSource"},
            { 0x9209, "Flash"},
            { 0x920A, "FocalLength"},
            { 0x9214, "SubjectArea"},
            { 0x927C, "MakerNote"},
            { 0x9286, "UserComment"},
            { 0x9290, "SubSecTime"},
            { 0x9291, "SubSecTimeOriginal"},
            { 0x9292, "SubSecTimeDigitized"},
            { 0xA000, "FlashpixVersion"},
            { 0xA001, "ColorSpace"},
            { 0xA002, "PixelXDimension"},
            { 0xA003, "PixelYDimension"},
            { 0xA004, "RelatedSoundFile"},
            { 0xA005, "Interoperability IFD Pointer"},
            { 0xA20B, "FlashEnergy"},
            { 0xA20C, "SpatialFrequencyResponse"},
            { 0xA20E, "FocalPlaneXResolution"},
            { 0xA20F, "FocalPlaneYResolution"},
            { 0xA210, "FocalPlaneResolutionUnit"},
            { 0xA214, "SubjectLocation"},
            { 0xA215, "ExposureIndex"},
            { 0xA217, "SensingMethod"},
            { 0xA300, "FileSource"},
            { 0xA301, "SceneType"},
            { 0xA302, "CFAPattern"},
            { 0xA401, "CustomRendered"},
            { 0xA402, "ExposureMode"},
            { 0xA403, "WhiteBalance"},
            { 0xA404, "DigitalZoomRatio"},
            { 0xA405, "FocalLengthIn35mmFilm"},
            { 0xA406, "SceneCaptureType"},
            { 0xA407, "GainControl"},
            { 0xA408, "Contrast"},
            { 0xA409, "Saturation"},
            { 0xA40A, "Sharpness"},
            { 0xA40B, "DeviceSettingDescription"},
            { 0xA40C, "SubjectDistanceRange"},
            { 0xA420, "ImageUniqueID"},
            { 0xA430, "CameraOwnerName"},
            { 0xA431, "BodySerialNumber"},
            { 0xA432, "LensSpecification"},
            { 0xA433, "LensMake"},
            { 0xA434, "LensModel"},
            { 0xA435, "LensSerialNumber"},
            { 0xA500, "Gamma"},
            // GPS header.
            { 0x0000, "GPSVersionID"},
            { 0x0001, "GPSLatitudeRef"},
            { 0x0002, "GPSLatitude"},
            { 0x0003, "GPSLongitudeRef"},
            { 0x0004, "GPSLongitude"},
            { 0x0005, "GPSAltitudeRef"},
            { 0x0006, "GPSAltitude"},
            { 0x0007, "GPSTimeStamp"},
            { 0x0008, "GPSSatellites"},
            { 0x0009, "GPSStatus"},
            { 0x000A, "GPSMeasureMode"},
            { 0x000B, "GPSDOP"},
            { 0x000C, "GPSSpeedRef"},
            { 0x000D, "GPSSpeed"},
            { 0x000E, "GPSTrackRef"},
            { 0x000F, "GPSTrack"},
            { 0x0010, "GPSImgDirectionRef"},
            { 0x0011, "GPSImgDirection"},
            { 0x0012, "GPSMapDatum"},
            { 0x0013, "GPSDestLatitudeRef"},
            { 0x0014, "GPSDestLatitude"},
            { 0x0015, "GPSDestLongitudeRef"},
            { 0x0016, "GPSDestLongitude"},
            { 0x0017, "GPSDestBearingRef"},
            { 0x0018, "GPSDestBearing"},
            { 0x0019, "GPSDestDistanceRef"},
            { 0x001A, "GPSDestDistance"},
            { 0x001B, "GPSProcessingMethod"},
            { 0x001C, "GPSAreaInformation"},
            { 0x001D, "GPSDateStamp"},
            { 0x001E, "GPSDifferential"},
            { 0x001F, "GPSHPositioningError"},

        };
    }
}
