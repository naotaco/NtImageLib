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




            // todo: don't forget to re-calcurate offset of each sections.

            return image;
        }
    }
}
