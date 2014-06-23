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


               
        public static ExifData ParseImage(byte[] image)
        {
            Debug.WriteLine("ParseImage start. image length: " + image.Length);

            var exif = new ExifData();

            // check SOI, Start of image marker.
            if (Util.GetUIntValue(image, 0, 2, false) != Definitions.JPEG_SOI_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid SOI marker. value: " + Util.GetUIntValue(image, 0, 2, false));
            }

            // check APP1 maerker
            if (Util.GetUIntValue(image, 2, 2, false) != Definitions.APP1_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid APP1 marker. value: " + Util.GetUIntValue(image, 2, 2, false));
            }

            UInt32 App1Size = Util.GetUIntValue(image, 4, 2, false);
            Debug.WriteLine("App1 size: " + App1Size.ToString("X"));

            var exifHeader = Encoding.UTF8.GetString(image, 6, 4);
            if (exifHeader != "Exif")
            {
                throw new UnsupportedFileFormatException("Can't fine \"Exif\" mark. value: " + exifHeader);
            }

            byte[] App1Data = new Byte[App1Size];
            exif.App1Data = App1Data;
            Array.Copy(image, (int)Definitions.APP1_OFFSET, App1Data, 0, (int)App1Size);

            // Check TIFF header.
            if (Util.GetUIntValue(App1Data, 0, 2) != Definitions.TIFF_LITTLE_ENDIAN)
            {
                throw new UnsupportedFileFormatException("Currently, only little endian is supported.");
            }
            if (Util.GetUIntValue(App1Data, 2, 2) != Definitions.TIFF_IDENTIFY_CODE)
            {
                throw new UnsupportedFileFormatException("TIFF identify code (0x2A00) couldn't find.");
            }

            // find out a pointer to 1st IFD
            var PrimaryIfdPointer = Util.GetUIntValue(App1Data, 4, 4);
            Debug.WriteLine("Primary IFD pointer: " + PrimaryIfdPointer);

            // parse primary (0th) IFD section
            exif.PrimaryIfd = Parser.IfdParser.ParseIfd(App1Data, PrimaryIfdPointer);
            Debug.WriteLine("Primary offset: " + exif.PrimaryIfd.Offset + " length: " + exif.PrimaryIfd.Length + " next ifd: " + exif.PrimaryIfd.NextIfdPointer);

            // parse Exif IFD section
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                exif.ExifIfd = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIfd.Entries[Definitions.EXIF_IFD_POINTER_TAG].IntValues[0]);
                Debug.WriteLine("Exif offset: " + exif.ExifIfd.Offset + " length: " + exif.ExifIfd.Length);
            }

            // parse GPS data.
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                exif.GpsIfd = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIfd.Entries[Definitions.GPS_IFD_POINTER_TAG].IntValues[0]);
                Debug.WriteLine("GPS offset: " + exif.GpsIfd.Offset + " length: " + exif.GpsIfd.Length);
            }

            return exif;
        }

        public static byte[] SetExifData(ExifData e)
        {
            return null;

        }
        
    }
}
