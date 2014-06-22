using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NtImageProcessor.MetaData.Misc;

namespace NtImageProcessor.MetaData.Composer
{
    public static class IfdComposer
    {
        /// <summary>
        /// Build byte data from IFD section data.
        /// </summary>
        /// <param name="data">Dictionary of Entry data in target IFD data</param>
        /// <param name="SectionOffset">Offset of this IFD section from TIFF header.</param>
        /// <returns></returns>
        public static byte[] ComposeIfdsection(Dictionary<UInt32, Entry> data, UInt32 SectionOffset)
        {
            // calcurate total size of IFD
            var TotalSize = 2; // number of entry
            UInt32 count = 0;
            foreach (Entry entry in data.Values)
            {
                count++;
                TotalSize += 12;

                // if value is more than 4 bytes, need separated section to store all data.
                if (entry.value.Length > 4)
                {
                    TotalSize += entry.value.Length;
                }
            }

            // area for pointer to next IFD section.
            TotalSize += 4;

            var ComposedData = new byte[TotalSize];

            // set data of entry num.
            var EntryNum = Util.ConvertToByte(count, 2);
            Array.Copy(EntryNum, ComposedData, EntryNum.Length);

            // set Next IFD pointer with 0
            var ifdPointerValue = Util.ConvertToByte(0, 4);

            Array.Copy(ifdPointerValue, 0, ComposedData, 2 + 12 * (int)count, 4);
            var ExtraDataSectionOffset = (UInt32)(2 + 12 * (int)count + 4);

            var keys = data.Keys.ToArray<UInt32>();
            Array.Sort(keys);

            int pointer = 2;
            foreach (UInt32 key in keys)
            {
                // tag in 2 bytes.
                var tag = Util.ConvertToByte(data[key].Tag, 2);
                Array.Copy(tag, 0, ComposedData, pointer, 2);
                pointer += 2;

                // type
                var type = Util.ConvertToByte(Util.ConvertFromEntryType(data[key].Type), 2);
                Array.Copy(type, 0, ComposedData, pointer, 2);
                pointer += 2;

                // count
                var c = Util.ConvertToByte(data[key].Count, 4);
                Array.Copy(c, 0, ComposedData, pointer, 4);
                pointer += 4;

                if (data[key].value.Length <= 4)
                {
                    // upto 4 bytes, copy value directly.
                    Array.Copy(data[key].value, 0, ComposedData, pointer, data[key].value.Length);
                }
                else
                {
                    // save actual data to extra area
                    Array.Copy(data[key].value, 0, ComposedData, (int)ExtraDataSectionOffset, data[key].value.Length);

                    // store pointer for extra area. Origin of pointer should be position of TIFF header.
                    var offset = Util.ConvertToByte(ExtraDataSectionOffset + SectionOffset, 4);
                    Array.Copy(offset, 0, ComposedData, pointer, 4);


                    ExtraDataSectionOffset += (UInt32)data[key].value.Length;

                }
                pointer += 4;

            }

            return ComposedData;
        }
    }
}
