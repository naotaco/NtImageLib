using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessor.MetaData.Parser
{
    public static class IfdParser
    {
        private const int ENTRY_SIZE = 12;
        
        /// <summary>
        /// Parse IFD section of Jpeg's header.
        /// </summary>
        /// <param name="App1Data">Raw data of App1 section</param>
        /// <param name="IfdOffset">Offset to the target IFD section from start of App1 data.</param>
        /// <returns>All entries in given IFD section.</returns>
        public static IfdData ParseIfd(byte[] App1Data, UInt32 IfdOffset)
        {
            var ifd = new IfdData();
            ifd.Offset = IfdOffset;
            var entries = new Dictionary<UInt32, Entry>();
            var EntryNum = Util.GetUIntValue(App1Data, (int)IfdOffset, 2);
            Debug.WriteLine("Entry num: " + EntryNum);

            ifd.NextIfdPointer = Util.GetUIntValue(App1Data, (int)IfdOffset + 2 + (int)EntryNum * ENTRY_SIZE, 4);

            // if there's no extra data area, (if all data is 4 bytes or less), this is length of this IFD section.
            ifd.Length = 2 + EntryNum * ENTRY_SIZE + 4; // entry num (2 bytes), each entries (12 bytes each), Next IFD pointer (4 byte)
            
            for (int i = 0; i < EntryNum; i++)
            {
                // Debug.WriteLine("--- Entry[" + i + "] ---");
                var EntryOrigin = (int)IfdOffset + 2 + i * ENTRY_SIZE;

                var entry = new Entry();

                // tag
                entry.Tag = Util.GetUIntValue(App1Data, EntryOrigin, 2);
                var tagTypeName = Util.TagNames[entry.Tag];
                // Debug.WriteLine("Tag: " + entry.Tag.ToString("X") + " " + tagTypeName);

                // type
                var typeValue = Util.GetUIntValue(App1Data, EntryOrigin + 2, 2);
                entry.Type = Util.ConvertToEntryType(typeValue);
                // Debug.WriteLine("Type: " + entry.Type.ToString());

                // count
                entry.Count = Util.GetUIntValue(App1Data, EntryOrigin + 4, 4);
                // Debug.WriteLine("Count: " + entry.Count);

                var valueSize = 0;
                valueSize = Util.ConvertToDataSize(entry.Type);
                var TotalValueSize = valueSize * (int)entry.Count;
                // Debug.WriteLine("Total value size: " + TotalValueSize);

                var valueBuff = new byte[TotalValueSize];

                if (TotalValueSize <= 4)
                {
                    // in this case, the value is stored directly here.
                    Array.Copy(App1Data, EntryOrigin + 8, valueBuff, 0, TotalValueSize);
                }
                else
                {
                    // other cases, actual value is stored in separated area
                    var EntryValuePointer = (int)Util.GetUIntValue(App1Data, EntryOrigin + 8, 4);
                    Array.Copy(App1Data, EntryValuePointer, valueBuff, 0, TotalValueSize);

                    // If there's extra data, its length should be added to total length.
                    ifd.Length += (UInt32)TotalValueSize;
                }

                entry.value = valueBuff;

                /*
                switch (entry.Type)
                {
                    case Entry.EntryType.Ascii:
                        Debug.WriteLine("value: " + entry.StringValue + Environment.NewLine + Environment.NewLine);
                        Debug.WriteLine(" ");
                        break;
                    case Entry.EntryType.Byte:
                    case Entry.EntryType.Undefined:
                        if (entry.Tag == 0x927C)
                        {
                            Debug.WriteLine("Maker note is too long to print.");
                        }
                        else
                        {
                            foreach (int val in entry.IntValues)
                            {
                                Debug.WriteLine("value: " + val.ToString("X"));
                            }
                        }
                        break;
                    case Entry.EntryType.Short:
                    case Entry.EntryType.SShort:
                    case Entry.EntryType.Long:
                    case Entry.EntryType.SLong:
                        foreach (int val in entry.IntValues)
                        {
                            Debug.WriteLine("value: " + val);
                        }
                        break;
                    case Entry.EntryType.Rational:
                    case Entry.EntryType.SRational:
                        foreach (double val in entry.DoubleValues)
                        {
                            Debug.WriteLine("value: " + val);
                        }
                        break;
                    default:
                        break;
                }
                 * */

                entries[entry.Tag] = entry;
            }

            
            ifd.Entries = entries;
            return ifd;
        }


    }
}
