using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Composer
{
    public static class JpegMetaDataProcessor
    {
        /// <summary>
        /// Set exif data to Jpeg file.
        /// Note that this function overwrites ALL Exif data in the given image.
        /// </summary>
        /// <param name="OriginalImage">Target image</param>
        /// <param name="MetaData">An Exif data which will be added to the image.</param>
        /// <returns>New image</returns>
        public static byte[] SetMetaData(byte[] OriginalImage, JpegMetaData MetaData)
        {
            var OriginalMetaData = JpegMetaDataParser.ParseImage(OriginalImage);
            // Debug.WriteLine("Original image size: " + OriginalImage.Length.ToString("X"));

            // It seems there's a bug handling little endian jpeg image on WP OS.
            var OutputImageMetadataEndian = Definitions.Endian.Big;
            
            // All data after this pointer will be kept.
            var FirstIfdPointer = MetaData.PrimaryIfd.NextIfdPointer;

            // check SOI, Start of image marker.
            if (Util.GetUIntValue(OriginalImage, 0, 2, Definitions.Endian.Big) != Definitions.JPEG_SOI_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid SOI marker. value: " + Util.GetUIntValue(OriginalImage, 0, 2, Definitions.Endian.Big));
            }

            // check APP1 maerker
            if (Util.GetUIntValue(OriginalImage, 2, 2, Definitions.Endian.Big) != Definitions.APP1_MARKER)
            {
                throw new UnsupportedFileFormatException("Invalid APP1 marker. value: " + Util.GetUIntValue(OriginalImage, 2, 2, Definitions.Endian.Big));
            }

            // Note: App1 size and ID are fixed to Big endian.
            UInt32 OriginalApp1DataSize = Util.GetUIntValue(OriginalImage, 4, 2, Definitions.Endian.Big);
            // Debug.WriteLine("Original App1 size: " + OriginalApp1DataSize.ToString("X"));

            // 0-5 byte: Exif identify code. E, x, i, f, \0, \0
            // 6-13 byte: TIFF header. endian, 0x002A, Offset to Primary IFD in 4 bytes. generally, 8 is set as the offset.
            var OriginalApp1Data = new byte[OriginalApp1DataSize];
            Array.Copy(OriginalImage, 6, OriginalApp1Data, 0, (int)OriginalApp1DataSize);

            // each pointer sections should be updated later (after each sections' size are fixed).

            if (MetaData.ExifIfd != null && !MetaData.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                // Add Exif IFD section's pointer entry as dummy.
                MetaData.PrimaryIfd.Entries.Add(
                    Definitions.EXIF_IFD_POINTER_TAG, 
                    new Entry() { 
                        Tag = Definitions.EXIF_IFD_POINTER_TAG,
                        Type = Entry.EntryType.Long,
                        Count = 1, 
                        value = new byte[] { 0, 0, 0, 0 } 
                    });
            }

            if (MetaData.GpsIfd != null && !MetaData.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                // Add GPS IFD section's pointer entry as dummy
                MetaData.PrimaryIfd.Entries.Add(
                    Definitions.GPS_IFD_POINTER_TAG,
                    new Entry()
                    {
                        Tag = Definitions.GPS_IFD_POINTER_TAG,
                        Type = Entry.EntryType.Long,
                        Count = 1,
                        value = new byte[] { 0, 0, 0, 0 }
                    });
            }


            byte[] primaryIfd = IfdComposer.ComposeIfdsection(MetaData.PrimaryIfd, OutputImageMetadataEndian);
            byte[] exifIfd = new byte[] { };
            byte[] gpsIfd = new byte[] { };
            if (MetaData.ExifIfd != null)
            {
                exifIfd = IfdComposer.ComposeIfdsection(MetaData.ExifIfd, OutputImageMetadataEndian);
            }
            if (MetaData.GpsIfd != null)
            {
                gpsIfd = IfdComposer.ComposeIfdsection(MetaData.GpsIfd, OutputImageMetadataEndian);
            }

            // Debug.WriteLine("Size fixed. Primary: " + primaryIfd.Length.ToString("X") + " exif: " + exifIfd.Length.ToString("X") + " gps: " + gpsIfd.Length.ToString("X"));

            // after size fixed, set each IFD sections' offset.
            MetaData.PrimaryIfd.Offset = 8; // fixed value             

            // now it's possible to calcurate pointer to Exif/GPS IFD
            var exifIfdPointer = 8 + primaryIfd.Length;
            var gpsIfdPointer = 8 + primaryIfd.Length + exifIfd.Length;
            
            if (MetaData.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                MetaData.PrimaryIfd.Entries.Remove(Definitions.EXIF_IFD_POINTER_TAG);
            }

            var exifIfdPointerEntry = new Entry()
                {
                    Tag = Definitions.EXIF_IFD_POINTER_TAG,
                    Type = Entry.EntryType.Long,
                    Count = 1,
                };
            exifIfdPointerEntry.UIntValues = new UInt32[] { (UInt32)exifIfdPointer };
            MetaData.PrimaryIfd.Entries.Add(Definitions.EXIF_IFD_POINTER_TAG, exifIfdPointerEntry);

            if (MetaData.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                MetaData.PrimaryIfd.Entries.Remove(Definitions.GPS_IFD_POINTER_TAG);
            }

            var gpsIfdPointerEntry = new Entry()
            {
                Tag = Definitions.GPS_IFD_POINTER_TAG,
                Type = Entry.EntryType.Long,
                Count = 1,
            };
            gpsIfdPointerEntry.UIntValues = new UInt32[] { (UInt32)gpsIfdPointer };
            MetaData.PrimaryIfd.Entries.Add(Definitions.GPS_IFD_POINTER_TAG, gpsIfdPointerEntry);

            var nextIfdPointer = 8 + primaryIfd.Length + exifIfd.Length + gpsIfd.Length;
            MetaData.PrimaryIfd.NextIfdPointer = (UInt32)nextIfdPointer;

            // finally, create byte array again
            primaryIfd = IfdComposer.ComposeIfdsection(MetaData.PrimaryIfd, OutputImageMetadataEndian);

            MetaData.ExifIfd.Offset = 8 + (UInt32)primaryIfd.Length;
            if (MetaData.ExifIfd != null)
            {
                exifIfd = IfdComposer.ComposeIfdsection(MetaData.ExifIfd, OutputImageMetadataEndian);
            }

            MetaData.GpsIfd.Offset = 8 + (UInt32)primaryIfd.Length + (UInt32)exifIfd.Length;
            if (MetaData.GpsIfd != null)
            {
                gpsIfd = IfdComposer.ComposeIfdsection(MetaData.GpsIfd, OutputImageMetadataEndian);
            }
            
            // 1st IFD data (after primary IFD data) should be kept
            var Original1stIfdData = new byte[OriginalApp1DataSize - OriginalMetaData.PrimaryIfd.NextIfdPointer];
            Array.Copy(OriginalApp1Data, (int)OriginalMetaData.PrimaryIfd.NextIfdPointer, Original1stIfdData, 0, Original1stIfdData.Length);

            // Build App1 section. From "Exif\0\0" to end of thumbnail data (1st IFD)
            // Exif00 + TIFF header + 3 IFD sections (Primary, Exif, GPS) + 1st IFD data from original data
            // var NewApp1Data = new byte[6 + 8 + primaryIfd.Length + exifIfd.Length + gpsIfd.Length + Original1stIfdData.Length];
            var NewApp1Data = new byte[6 + 8 + primaryIfd.Length + exifIfd.Length + gpsIfd.Length];
            // Debug.WriteLine("New App1 size: " + NewApp1Data.Length.ToString("X"));

            // Array.Copy(OriginalApp1Data, 0, NewApp1Data, 0, 6 + 8);
            Array.Copy(OriginalApp1Data, 0, NewApp1Data, 0, 6); // only EXIF00 should be copiec.

            var endianSection = Util.ToByte(0x4d4d, 2, OutputImageMetadataEndian);
            Array.Copy(endianSection, 0, NewApp1Data, 6, 2);

            var magicNumber = Util.ToByte(0x002A, 2, OutputImageMetadataEndian);
            Array.Copy(magicNumber, 0, NewApp1Data, 8, 2);

            var primaryIfdOffset = Util.ToByte(8, 4, OutputImageMetadataEndian);
            Array.Copy(primaryIfdOffset, 0, NewApp1Data, 10, 4);
            
            Array.Copy(primaryIfd, 0, NewApp1Data, 6 + 8, primaryIfd.Length);
            Array.Copy(exifIfd, 0, NewApp1Data, 6 + 8 + primaryIfd.Length, exifIfd.Length);
            Array.Copy(gpsIfd, 0, NewApp1Data, 6 + 8 + primaryIfd.Length + exifIfd.Length, gpsIfd.Length);
            // Array.Copy(Original1stIfdData, 0, NewApp1Data, 6 + 8 + primaryIfd.Length + exifIfd.Length + gpsIfd.Length, Original1stIfdData.Length);

            // Only size of App1 data is different.
            var NewImage = new byte[OriginalImage.Length - OriginalApp1DataSize + NewApp1Data.Length];
            // Debug.WriteLine("New image size: " + NewImage.Length.ToString("X"));

            // Copy SOI, App1 marker from original.
            Array.Copy(OriginalImage, 0, NewImage, 0, 2 + 2);

            // Important note again: App 1 data size is stored in Big endian.
            var App1SizeData = Util.ToByte((UInt32)NewApp1Data.Length, 2, Definitions.Endian.Big);
            Array.Copy(App1SizeData, 0, NewImage, 4, 2);

            // After that, copy App1 data to new image.
            Array.Copy(NewApp1Data, 0, NewImage, 6, NewApp1Data.Length);

            // At last, copy body from original image.
            Array.Copy(OriginalImage, 2 + 2 + 2 + (int)OriginalApp1DataSize, NewImage, 2 + 2 + 2 + NewApp1Data.Length, OriginalImage.Length - 2 - 2 - 2 - (int)OriginalApp1DataSize);

            // Util.DumpByteArray(NewImage,0,  256);

            return NewImage;
        }
    }
}
