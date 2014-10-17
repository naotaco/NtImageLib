using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.Xna.Framework.Media;
using NtImageProcessor.MetaData;
using NtImageProcessor.MetaData.Misc;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using Windows.Devices.Geolocation;

namespace NtImageProcessorTest.MetaData
{
    [TestClass]
    public class Geotag
    {
        async Task<Geoposition> GetGeoposition()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;
            Geoposition position = null;
            try
            {
                position = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004) { }
            }
            return position;
        }

        Task<Geoposition> WaitLocation()
        {
            return GetGeoposition();
        }

        [TestMethod]
        public void GeoTagAddition()
        {
            // this is a dummy async task - replace it with any awaitable Task<T>
            var task = WaitLocation();
            task.Wait();
            var position = task.Result;
            Debug.WriteLine("pos: " + position.Coordinate.Longitude + " " + position.Coordinate.Latitude);

            foreach (string filename in TestFiles.ImagesWithoutGeotag)
            {
                var image = TestUtil.GetResourceByteArray(filename);
                var NewImage = MetaDataOperator.AddGeoposition(image, position);
                var pic = new MediaLibrary().SavePicture(string.Format("Exif addition test_{0:yyyyMMdd_HHmmss}.jpg", DateTime.Now), NewImage);
            }
        }


    }
}
