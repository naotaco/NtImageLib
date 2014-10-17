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
            var task = WaitLocation();
            task.Wait();
            var position = task.Result;
            Debug.WriteLine("pos: " + position.Coordinate.Longitude + " " + position.Coordinate.Latitude);
            var mediaLibrary = new MediaLibrary();

            int count = 0;
            foreach (string filename in TestFiles.GeotagTargetImages)
            {
                var image = TestUtil.GetResourceByteArray(filename);
                var originalMetadata = JpegMetaDataParser.ParseImage(image);
                var NewImage = MetaDataOperator.AddGeoposition(image, position);
                var newMetadata = JpegMetaDataParser.ParseImage(NewImage);
                try
                {
                    var pic = mediaLibrary.SavePicture(string.Format("Exif addition test_" + count + "_{0:yyyyMMdd_HHmmss}.jpg", DateTime.Now), NewImage);
                }
                catch (NullReferenceException) { }

                TestUtil.IsGpsDataAdded(originalMetadata, newMetadata);

                using (var imageStream = TestUtil.GetResourceStream(filename))
                {
                    originalMetadata = JpegMetaDataParser.ParseImage(imageStream);
                    var newImageStream = MetaDataOperator.AddGeoposition(imageStream, position);
                    try
                    {
                        var pic2 = mediaLibrary.SavePicture(string.Format("Exif addition test_" + count + "_stream_{0:yyyyMMdd_HHmmss}.jpg", DateTime.Now), newImageStream);
                    }
                    catch (NullReferenceException) { }
                    finally { newImageStream.Close(); }

                    TestUtil.IsGpsDataAdded(originalMetadata, newMetadata);
                    imageStream.Close();
                    newImageStream.Close();
                    count++;
                }
                GC.Collect(); // Saving many big images in short time, memory mey be run out and it may throws NullReferenceException.
            }
        }

        [TestMethod]
        public void GeotagingFailure()
        {
            var task = WaitLocation();
            task.Wait();
            var position = task.Result;
            Debug.WriteLine("pos: " + position.Coordinate.Longitude + " " + position.Coordinate.Latitude);
            var mediaLibrary = new MediaLibrary();

            foreach (string filename in TestFiles.GeotagImages)
            {
                var image = TestUtil.GetResourceByteArray(filename);
                Assert.ThrowsException<GpsInformationAlreadyExistsException>(() =>
                {
                    var NewImage = MetaDataOperator.AddGeoposition(image, position);
                });

                GC.Collect();
                using (var imageStream = TestUtil.GetResourceStream(filename))
                {
                    Assert.ThrowsException<GpsInformationAlreadyExistsException>(() =>
                    {
                        var newImageStream = MetaDataOperator.AddGeoposition(imageStream, position);
                        newImageStream.Close();
                    });
                }
            }
        }

        [TestMethod]
        public void OverwriteGeotag()
        {
            var task = WaitLocation();
            task.Wait();
            var position = task.Result;
            Debug.WriteLine("pos: " + position.Coordinate.Longitude + " " + position.Coordinate.Latitude);
            var mediaLibrary = new MediaLibrary();

            int count = 0;
            foreach (string filename in TestFiles.ValidImages)
            {
                var image = TestUtil.GetResourceByteArray(filename);
                var originalMetadata = JpegMetaDataParser.ParseImage(image);
                var NewImage = MetaDataOperator.AddGeoposition(image, position, true);
                var newMetadata = JpegMetaDataParser.ParseImage(NewImage);

                try
                {
                    var pic = mediaLibrary.SavePicture(string.Format("Exif addition test_" + count + "_{0:yyyyMMdd_HHmmss}.jpg", DateTime.Now), NewImage);
                }
                catch (NullReferenceException) { }

                GC.Collect();
                TestUtil.IsGpsDataAdded(originalMetadata, newMetadata);

                using (var imageStream = TestUtil.GetResourceStream(filename))
                {
                    originalMetadata = JpegMetaDataParser.ParseImage(imageStream);
                    var newImageStream = MetaDataOperator.AddGeoposition(imageStream, position, true);
                    try
                    {
                        var pic2 = mediaLibrary.SavePicture(string.Format("Exif addition test_" + count + "_stream_{0:yyyyMMdd_HHmmss}.jpg", DateTime.Now), newImageStream);
                    }
                    catch (NullReferenceException) { }
                    finally { newImageStream.Close(); }
                    TestUtil.IsGpsDataAdded(originalMetadata, newMetadata);

                    count++;
                }
                GC.Collect(); // Saving many big images in short time, memory mey be run out and it may throws NullReferenceException.
            }
        }

    }
}
