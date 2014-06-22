using NtImageProcessor.MetaData.Composer;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


            Debug.WriteLine("Longitude : " + position.Coordinate.Longitude + " Latitude: " + position.Coordinate.Latitude);

            var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

            return ExifOperator.InsertGpsIfdSection(image, gpsIfdData);

        }
    }
}
