using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData
{
    public static class ExifParser
    {
        private const UInt32 JPEG_SOI_MARKER = 0xD8FF;
        private const UInt32 APP1_MARKER = 0xE1FF;
        private const UInt32 APP1_OFFSET = 0x0C;

        private const UInt32 TIFF_BIG_ENDIAN = 0x4D4D;
        private const UInt32 TIFF_LITTLE_ENDIAN = 0x4949;

        private const UInt32 TIFF_IDENTIFY_CODE = 0x002A;

        private const UInt32 EXIF_IFD_POINTER_TAG = 0x8769;
        private const UInt32 GPS_IFD_POINTER_TAG = 0x8825;
                
        public static ExifData ParseImage(byte[] image)
        {
            Debug.WriteLine("ParseImage start. image length: " + image.Length);

            var exif = new ExifData();

            // check SOI, Start of image marker.
            if (Util.GetUIntValue(image, 0, 2) != JPEG_SOI_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid SOI marker. value: " + Util.GetUIntValue(image, 0, 2));
            }

            // check APP1 maerker
            if (Util.GetUIntValue(image, 2, 2) != APP1_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid APP1 marker. value: " + Util.GetUIntValue(image, 2, 2));
            }

            UInt32 App1Size = Util.GetUIntValue(image, 4, 2);
            Debug.WriteLine("App1 size: " + App1Size);

            var exifHeader = Encoding.UTF8.GetString(image, 6, 4);
            if (exifHeader != "Exif")
            {
                throw new UnsupportedFileFormatException("Can't fine \"Exif\" mark. value: " + exifHeader);
            }

            byte[] App1Data = new Byte[App1Size];
            Array.Copy(image, (int)APP1_OFFSET, App1Data, 0, (int)App1Size);

            // Check TIFF header.
            if (Util.GetUIntValue(App1Data, 0, 2) != TIFF_LITTLE_ENDIAN)
            {
                throw new UnsupportedFileFormatException("Currently, only little endian is supported.");
            }
            if (Util.GetUIntValue(App1Data, 2, 2) != TIFF_IDENTIFY_CODE)
            {
                throw new UnsupportedFileFormatException("TIFF identify code (0x2A00) couldn't find.");
            }

            // find out a pointer to 1st IFD
            var PrimaryIfdPointer = Util.GetUIntValue(App1Data, 4, 4);
            Debug.WriteLine("Primary IFD pointer: " + PrimaryIfdPointer);

            // parse primary (0th) IFD section
            exif.PrimaryIFDEntries = Parser.IfdParser.ParseIfd(App1Data, PrimaryIfdPointer);

            // parse Exif IFD section
            if (exif.PrimaryIFDEntries.ContainsKey(EXIF_IFD_POINTER_TAG))
            {
                exif.ExifIfdEntries = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIFDEntries[EXIF_IFD_POINTER_TAG].IntValues[0]);
            }

            // parse GPS data.
            if (exif.PrimaryIFDEntries.ContainsKey(GPS_IFD_POINTER_TAG))
            {
                exif.GpsIfdEntries = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIFDEntries[GPS_IFD_POINTER_TAG].IntValues[0]);
            }

            return exif;
        }

        public static byte[] SetExifData(ExifData e)
        {
            return null;

        }
        
    }
}
