using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData
{
    public static class JpegMetaDataParser
    {              
        /// <summary>
        /// Parse jpeg image and returns it's metadata as structure.
        /// </summary>
        /// <param name="image">Jpeg file as byte array.</param>
        /// <returns>All metadata.</returns>
        public static JpegMetaData ParseImage(byte[] image)
        {
            Debug.WriteLine("ParseImage start. image length: " + image.Length);

            // Other than meta data sections are in Big endian.
            var endian = Definitions.Endian.Big;

            // check SOI, Start of image marker.
            if (Util.GetUIntValue(image, 0, 2, endian) != Definitions.JPEG_SOI_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid SOI marker. value: " + Util.GetUIntValue(image, 0, 2, endian));
            }

            // check APP1 maerker
            if (Util.GetUIntValue(image, 2, 2, endian) != Definitions.APP1_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid APP1 marker. value: " + Util.GetUIntValue(image, 2, 2, endian));
            }

            UInt32 App1Size = Util.GetUIntValue(image, 4, 2, endian);
            // Debug.WriteLine("App1 size: " + App1Size.ToString("X"));

            var exifHeader = Encoding.UTF8.GetString(image, 6, 4);
            if (exifHeader != "Exif")
            {
                throw new UnsupportedFileFormatException("Can't fine \"Exif\" mark. value: " + exifHeader);
            }

            var App1Data = new byte[App1Size];
            Array.Copy(image, (int)Definitions.APP1_OFFSET, App1Data, 0, (int)App1Size);
            return ParseApp1Data(App1Data);
        }

        /// <summary>
        /// Parse meta data in given image
        /// </summary>
        /// <param name="image">Jpeg file as stream</param>
        /// <returns>All meta data.</returns>
        public static JpegMetaData ParseImage(Stream image)
        {
            Debug.WriteLine("ParseImage start.");
            // Other than meta data sections are in Big endian.
            var endian = Definitions.Endian.Big;

            var soiMarker = new byte[2];
            image.Read(soiMarker, 0, 2);
            var app1Marker = new byte[2];
            image.Read(app1Marker, 0, 2);
            var app1sizeData = new byte[2];
            image.Read(app1sizeData, 0, 2);

            if (Util.GetUIntValue(soiMarker, 0, 2, endian) != Definitions.JPEG_SOI_MARKER ||
                Util.GetUIntValue(app1Marker, 0, 2, endian) != Definitions.APP1_MARKER)
            {
                throw new UnsupportedFileFormatException("SOI marker or app1 marker is wrong..");
            }

            var App1Size = Util.GetUIntValue(app1sizeData, 0, 2, endian);

            var exifHeader = new byte[6];
            image.Read(exifHeader, 0, 6);
            Util.DumpByteArrayAll(exifHeader);
            if (Encoding.UTF8.GetString(exifHeader, 0, 4) != "Exif")
            {
                throw new UnsupportedFileFormatException("Couldn't find \"Exif\" mark.");
            }

            var App1Data = new byte[App1Size];
            image.Read(App1Data, 0, (int)App1Size);
            return ParseApp1Data(App1Data);
        }

        /// <summary>
        /// Parse given data, App1 section data.
        /// </summary>
        /// <param name="App1Data">byte array of app1 data which starts from TIFF header.</param>
        /// <returns>all meta data.</returns>
        private static JpegMetaData ParseApp1Data(byte[] App1Data)
        {

            var exif = new JpegMetaData();
            exif.App1Data = App1Data;

            // Check TIFF header.
            var MetaDataEndian = Definitions.Endian.Little;
            if (Util.GetUIntValue(App1Data, 0, 2, Definitions.Endian.Little) == Definitions.TIFF_LITTLE_ENDIAN)
            {
                MetaDataEndian = Definitions.Endian.Little;
                // Debug.WriteLine("This metadata in Little endian");
            }
            else if (Util.GetUIntValue(App1Data, 0, 2, Definitions.Endian.Little) == Definitions.TIFF_BIG_ENDIAN)
            {
                MetaDataEndian = Definitions.Endian.Big;
                // Debug.WriteLine("This metadata in Big endian");
            }
            else
            {
                throw new UnsupportedFileFormatException("Currently, only little endian is supported.");
            }

            if (Util.GetUIntValue(App1Data, 2, 2, MetaDataEndian) != Definitions.TIFF_IDENTIFY_CODE)
            {
                throw new UnsupportedFileFormatException("TIFF identify code (0x2A00) couldn't find.");
            }

            // find out a pointer to 1st IFD
            var PrimaryIfdPointer = Util.GetUIntValue(App1Data, 4, 4, MetaDataEndian);
            // Debug.WriteLine("Primary IFD pointer: " + PrimaryIfdPointer);

            // parse primary (0th) IFD section
            exif.PrimaryIfd = Parser.IfdParser.ParseIfd(App1Data, PrimaryIfdPointer, MetaDataEndian);
            // Debug.WriteLine("Primary offset: " + exif.PrimaryIfd.Offset + " length: " + exif.PrimaryIfd.Length + " next ifd: " + exif.PrimaryIfd.NextIfdPointer);

            // parse Exif IFD section
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                exif.ExifIfd = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIfd.Entries[Definitions.EXIF_IFD_POINTER_TAG].UIntValues[0], MetaDataEndian);
                // Debug.WriteLine("Exif offset: " + exif.ExifIfd.Offset + " length: " + exif.ExifIfd.Length);
            }

            // parse GPS data.
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                exif.GpsIfd = Parser.IfdParser.ParseIfd(App1Data, exif.PrimaryIfd.Entries[Definitions.GPS_IFD_POINTER_TAG].UIntValues[0], MetaDataEndian);
                // Debug.WriteLine("GPS offset: " + exif.GpsIfd.Offset + " length: " + exif.GpsIfd.Length);
            }

            return exif;
        }
    }
}
