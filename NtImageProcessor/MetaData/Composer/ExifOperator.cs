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
    public static class ExifOperator
    {
        public static byte[] InsertGpsIfdSection(byte[] OriginalImage, IfdData gpsIfdData)
        {
            
            var exif = ExifParser.ParseImage(OriginalImage);

            if (exif.PrimaryIfd.Entries.ContainsKey(0x8825))
            {
                Debug.WriteLine("This image contains GPS information. Return.");
                throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
            }
            
            // TODO: implement insertion code here.



            return null;
        }
    }
}
