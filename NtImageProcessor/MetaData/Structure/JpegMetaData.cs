using NtImageProcessor.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Structure
{
    public class JpegMetaData
    {


        /// <summary>
        /// Address of TIFF header.
        /// This is origin of all offset values.
        /// </summary>
        int TiffHeaderPointer { get; set; }

        int PrimaryIfdOffset { get; set; }
        int ExifIfdOffset { get; set; }
        int GpsIfdOffset { get; set; }

        /// <summary>
        /// Stores  entries in Primary IFD (known as 0th IFD).
        /// </summary>
        public IfdData PrimaryIfd { get; set; }

        /// <summary>
        /// Stores entries in EXIF IFD.
        /// </summary>
        public IfdData ExifIfd { get; set; }

        /// <summary>
        /// Stores enties in GPS IFD.
        /// </summary>
        public IfdData GpsIfd { get; set; }

        /// <summary>
        /// Raw data of App1 section.
        /// </summary>
        public byte[] App1Data { get; set; }

        /// <summary>
        /// Length of body (Other than App1 data) of Jpeg file.
        /// </summary>
        public long BodyLength { get; set; }

        public JpegMetaData() { }

        /// <summary>
        /// Returns whether geotag is recorded or not.
        /// </summary>
        /// <returns>True if geotag is exists</returns>
        public bool IsGeotagExist
        {
            get
            {
                if (!PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG)) { return false; }
                if (GpsIfd == null || GpsIfd.Length < 1) { return false; }
                if (GpsIfd.Entries.ContainsKey(Definitions.GPS_STATUS_TAG) &&
                    GpsIfd.Entries[Definitions.GPS_STATUS_TAG].StringValue.Contains(Definitions.GPS_STATUS_MEASUREMENT_VOID)) { return false; }
                return true;
            }
        }
    }
}
