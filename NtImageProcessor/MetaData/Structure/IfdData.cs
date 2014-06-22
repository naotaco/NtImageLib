using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Structure
{
    public class IfdData
    {

        public IfdData() { }

        /// <summary>
        /// Contains all entries
        /// </summary>
        public Dictionary<UInt32, Entry> Entries { get; set; }

        /// <summary>
        /// Offset of start point from TIFF header.
        /// </summary>
        public UInt32 Offset { get; set; }

        /// <summary>
        /// Total size of this IFD section.
        /// </summary>
        public UInt32 Length { get; set; }

        /// <summary>
        /// Contains address of 1st IFD seciton. only exists in Primary, 0th IFD section.
        /// </summary>
        public UInt32 NextIfdPointer { get; set; }
    }
}
