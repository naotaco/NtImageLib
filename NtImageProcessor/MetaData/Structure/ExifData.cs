using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Structure
{
    public class ExifData
    {
        public enum Endian
        {
            Little,
            Big,
        };

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
        public Dictionary<UInt32, Entry> PrimaryIFDEntries { get; set; }

        /// <summary>
        /// Stores entries in EXIF IFD.
        /// </summary>
        public Dictionary<UInt32, Entry> ExifIfdEntries { get; set; }

        /// <summary>
        /// Stores enties in GPS IFD.
        /// </summary>
        public Dictionary<UInt32, Entry> GpsIfdEntries { get; set; }

        /// <summary>
        /// Stores all data other than supported IFD sections as Raw byte array.
        /// </summary>
        public byte[] UnsupportedData { get; set; }

        public ExifData() { }


    }
}
