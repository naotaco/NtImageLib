using NtImageProcessor.MetaData.Composer;
using NtImageProcessor.MetaData.Misc;
using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            // parse given image first
            var exif = JpegMetaDataParser.ParseImage(image);

            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                Debug.WriteLine("This image contains GPS information. Return.");
                throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
            }

            // Create IFD structure from given GPS info
            var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

            // Add GPS info to exif structure
            exif.GpsIfd = gpsIfdData;

            return JpegMetaDataProcessor.SetMetaData(image, exif);
        }

        /// <summary>
        /// Add geometory information to given image as Exif data.
        /// </summary>
        /// <param name="image">Raw data of Jpeg file as a stream.</param>
        /// <param name="position">Geometory information</param>
        /// <returns>Jpeg data with geometory information.</returns>
        public static Stream AddGeoposition(Stream image, Geoposition position)
        {
            Debug.WriteLine("Longitude : " + position.Coordinate.Longitude + " Latitude: " + position.Coordinate.Latitude);

            // parse given image first
            var exif = JpegMetaDataParser.ParseImage(image);
            if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
            {
                Debug.WriteLine("This image contains GPS information. Return.");
                throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
            }

            // Create IFD structure from given GPS info
            var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

            // Add GPS info to exif structure
            exif.GpsIfd = gpsIfdData;

            // Todo: Add meta data to jpeg file as stream.
            return JpegMetaDataProcessor.SetMetaData(image, exif);
        }
    }
}
