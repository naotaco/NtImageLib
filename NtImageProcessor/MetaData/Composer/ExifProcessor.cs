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
    public static class ExifProcessor
    {
        /// <summary>
        /// Set exif data to Jpeg file.
        /// Note that this function overwrites ALL Exif data in the given image.
        /// </summary>
        /// <param name="image">Target image</param>
        /// <param name="exif">An Exif data which will be added to the image.</param>
        /// <returns></returns>
        public static byte[] SetExifData(byte[] image, ExifData exif)
        {
            // All data after this pointer will be kept.
            var FirstIfdPointer = exif.PrimaryIfd.NextIfdPointer;

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

            // Note: App1 size and ID are fixed to Big endian.
            UInt32 App1Size = Util.GetUIntValue(image, 4, 2, false);
            Debug.WriteLine("App1 size: " + App1Size.ToString("X"));

            // each pointer sections should be updated later (after each sections' size are fixed).

            if (exif.ExifIfd != null && !exif.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                // Add Exif IFD section's pointer entry as dummy.
                exif.PrimaryIfd.Entries.Add(
                    Definitions.EXIF_IFD_POINTER_TAG, 
                    new Entry() { 
                        Tag = Definitions.EXIF_IFD_POINTER_TAG,
                        Type = Entry.EntryType.Long,
                        Count = 1, 
                        value = new byte[] { 0, 0, 0, 0 } 
                    });
            }

            if (exif.GpsIfd != null && !exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                // Add GPS IFD section's pointer entry as dummy
                exif.PrimaryIfd.Entries.Add(
                    Definitions.GPS_IFD_POINTER_TAG,
                    new Entry()
                    {
                        Tag = Definitions.GPS_IFD_POINTER_TAG,
                        Type = Entry.EntryType.Long,
                        Count = 1,
                        value = new byte[] { 0, 0, 0, 0 }
                    });
            }


            byte[] primaryIfd = IfdComposer.ComposeIfdsection(exif.PrimaryIfd);
            byte[] exifIfd = new byte[] { };
            byte[] gpsIfd = new byte[] { };
            if (exif.ExifIfd != null)
            {
                exifIfd = IfdComposer.ComposeIfdsection(exif.ExifIfd);
            }
            if (exif.GpsIfd != null)
            {
                gpsIfd = IfdComposer.ComposeIfdsection(exif.GpsIfd);
            }

            Debug.WriteLine("Size fixed. Primary: " + primaryIfd.Length.ToString("X") + " exif: " + exifIfd.Length.ToString("X") + " gps: " + gpsIfd.Length.ToString("X"));

            // now it's possible to calcurate pointer to Exif/GPS IFD
            var exifIfdPointer = 8 + primaryIfd.Length;
            var gpsIfdPointer = 8 + primaryIfd.Length + exifIfd.Length;
            
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.EXIF_IFD_POINTER_TAG))
            {
                exif.PrimaryIfd.Entries.Remove(Definitions.EXIF_IFD_POINTER_TAG);
            }

            var exifIfdPointerEntry = new Entry()
                {
                    Tag = Definitions.EXIF_IFD_POINTER_TAG,
                    Type = Entry.EntryType.Long,
                    Count = 1,
                };
            exifIfdPointerEntry.IntValues = new UInt32[] { (UInt32)exifIfdPointer };
            exif.PrimaryIfd.Entries.Add(Definitions.EXIF_IFD_POINTER_TAG, exifIfdPointerEntry);

            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                exif.PrimaryIfd.Entries.Remove(Definitions.GPS_IFD_POINTER_TAG);
            }

            var gpsIfdPointerEntry = new Entry()
            {
                Tag = Definitions.GPS_IFD_POINTER_TAG,
                Type = Entry.EntryType.Long,
                Count = 1,
            };
            gpsIfdPointerEntry.IntValues = new UInt32[] { (UInt32)gpsIfdPointer };
            exif.PrimaryIfd.Entries.Add(Definitions.GPS_IFD_POINTER_TAG, gpsIfdPointerEntry);

            var nextIfdPointer = 8 + primaryIfd.Length + exifIfd.Length + gpsIfd.Length;
            exif.PrimaryIfd.NextIfdPointer = (UInt32)nextIfdPointer;

            // finally, create byte array again for primary IFD.
            primaryIfd = IfdComposer.ComposeIfdsection(exif.PrimaryIfd);

            /// TODO: connect following byte array.
            /// 8 bytes: don't change
            /// primafy ifd
            /// exif ifd
            /// gps ifd
            /// data after FirstIfdPointer

            return image;
        }
    }
}
