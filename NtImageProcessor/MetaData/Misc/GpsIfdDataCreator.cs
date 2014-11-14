using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace NtImageProcessor.MetaData.Misc
{
    public static class GpsIfdDataCreator
    {
        public static IfdData CreateGpsIfdData(Geoposition position)
        {
            var gpsIfdData = new IfdData();
            gpsIfdData.Entries = new Dictionary<UInt32, Entry>();

            var GPSVersionEntry = new Entry()
            {
                Tag = 0x0,
                Type = Entry.EntryType.Byte,
                Count = 4,
            };
            GPSVersionEntry.UIntValues = new UInt32[] { 2, 2, 0, 0 };
            gpsIfdData.Entries.Add(GPSVersionEntry.Tag, GPSVersionEntry);

            var LatitudeRefEntry = new Entry()
            {
                Tag = 0x1,
                Type = Entry.EntryType.Ascii,
                Count = 2,
            };
            char latRef;
#if WINDOWS_APP
            var latitude = position.Coordinate.Point.Position.Latitude;
#elif WINDOWS_PHONE
            var latitude = position.Coordinate.Latitude;
#endif
            if (latitude > 0)
            {
                latRef = 'N';
            }
            else
            {
                latRef = 'S';
                latitude *= -1;
            }
            LatitudeRefEntry.UIntValues = new UInt32[] { (byte)latRef, 0 };
            gpsIfdData.Entries.Add(LatitudeRefEntry.Tag, LatitudeRefEntry);

            var LatitudeEntry = new Entry()
            {
                Tag = 0x2,
                Type = Entry.EntryType.Rational,
                Count = 3,
            };
            var LatDeg = Math.Floor(latitude);
            var LatMin = Math.Floor((latitude - LatDeg) * 60);
            var LatSec = Util.ToRoundUp(((latitude - LatDeg) * 60 - LatMin) * 60, 2);
            Debug.WriteLine("Latitude: " + LatDeg + " " + LatMin + " " + LatSec);
            try
            {
                LatitudeEntry.DoubleValues = new double[] { LatDeg, LatMin, LatSec };
            }
            catch (OverflowException)
            {
                var sec = Util.ToRoundUp(((latitude - LatDeg) * 60 - LatMin) * 60, 0);
                Debug.WriteLine("Latitude: " + LatDeg + " " + LatMin + " " + sec);
                LatitudeEntry.DoubleValues = new double[] { LatDeg, LatMin, sec };
            }
            gpsIfdData.Entries.Add(LatitudeEntry.Tag, LatitudeEntry);

            var LongitudeRef = new Entry()
            {
                Tag = 0x3,
                Type = Entry.EntryType.Ascii,
                Count = 3,
            };
            char lonRef;
#if WINDOWS_APP
            var longitude = position.Coordinate.Point.Position.Longitude;
#elif WINDOWS_PHONE
            var longitude = position.Coordinate.Longitude;
#endif
            if (latitude > 0)
            {
                lonRef = 'E';
            }
            else
            {
                lonRef = 'W';
                longitude *= -1;
            }
            LongitudeRef.UIntValues = new UInt32[] { (byte)lonRef, 0 };
            gpsIfdData.Entries.Add(LongitudeRef.Tag, LongitudeRef);

            var Longitude = new Entry()
            {
                Tag = 0x4,
                Type = Entry.EntryType.Rational,
                Count = 3,
            };
            var LonDeg = Math.Floor(longitude);
            var LonMin = Math.Floor((longitude - LonDeg) * 60);
            var LonSec = Util.ToRoundUp(((longitude - LonDeg) * 60 - LonMin) * 60, 2);
            Debug.WriteLine("Longitude: " + LonDeg + " " + LonMin + " " + LonSec);
            try
            {
                Longitude.DoubleValues = new double[] { LonDeg, LonMin, LonSec };
            }
            catch (OverflowException)
            {
                var sec = Util.ToRoundUp(((longitude - LonDeg) * 60 - LonMin) * 60, 0);
                Debug.WriteLine("Longitude: " + LonDeg + " " + LonMin + " " + sec);
                Longitude.DoubleValues = new double[] { LonDeg, LonMin, sec };
            }
            gpsIfdData.Entries.Add(Longitude.Tag, Longitude);

            var TimeStampEntry = new Entry()
            {
                Tag = 0x7,
                Type = Entry.EntryType.Rational,
                Count = 3,
            };
            TimeStampEntry.DoubleValues = new double[] { position.Coordinate.Timestamp.Hour, position.Coordinate.Timestamp.Minute, position.Coordinate.Timestamp.Second };
            gpsIfdData.Entries.Add(TimeStampEntry.Tag, TimeStampEntry);

            var GpsMapDetum = new Entry()
            {
                Tag = 0x12,
                Type = Entry.EntryType.Ascii,
                Count = 7,
            };
            GpsMapDetum.value = Util.ToByte("WGS-84");
            gpsIfdData.Entries.Add(GpsMapDetum.Tag, GpsMapDetum);

            var GpsDateStamp = new Entry()
            {
                Tag = 0x1D,
                Type = Entry.EntryType.Ascii,
                Count = 11,
            };
            String str = position.Coordinate.Timestamp.Year.ToString("D4") + ":" + position.Coordinate.Timestamp.Month.ToString("D2") + ":" + position.Coordinate.Timestamp.Day.ToString("D2");
            GpsDateStamp.value = Util.ToByte(str);
            gpsIfdData.Entries.Add(GpsDateStamp.Tag, GpsDateStamp);

            return gpsIfdData;
        }
    }
}
