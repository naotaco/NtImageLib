using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace NtImageProcessor.MetaData
{
    public static class MetaDataOperator
    {
        /// <summary>
        /// Add geometory information to given image as Exif data.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file</param>
        /// <param name="position">Geometory information</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static byte[] AddGeoposition(byte[] image, Geoposition position)
        {
            var exif = ExifParser.ParseImage(image);
            
            return image;
        }
    }
}
