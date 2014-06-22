using NtImageProcessor.MetaData.Structure;
using System;
using System.Collections.Generic;
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
            GPSVersionEntry.IntValues = new UInt32[] { 2, 2, 0, 0 };
            gpsIfdData.Entries.Add(GPSVersionEntry.Tag, GPSVersionEntry);

            var LatitudeRefEntry = new Entry()
            {
                Tag = 0x1,
                Type = Entry.EntryType.Ascii,
                Count = 2,
            };
            char latRef;
            if (position.Coordinate.Latitude > 0)
            {
                latRef = 'N';
            }
            else
            {
                latRef = 'S';
            }
            LatitudeRefEntry.IntValues = new UInt32[] { (byte)latRef, 0 };
            gpsIfdData.Entries.Add(LatitudeRefEntry.Tag, LatitudeRefEntry);

            var LatitudeEntry = new Entry()
            {
                Tag = 0x2,
                Type = Entry.EntryType.Rational,
                Count = 3,
            };
            var deg = Math.Floor(position.Coordinate.Latitude);
            var min = Math.Floor((position.Coordinate.Latitude - deg) * 60);
            var sec = Util.ToRoundUp(((position.Coordinate.Latitude - deg) * 60 - min), 3);
            LatitudeEntry.DoubleValues = new double[] { deg, min, sec };
            gpsIfdData.Entries.Add(LatitudeEntry.Tag, LatitudeEntry);

            var LongitudeRef = new Entry()
            {
                Tag = 0x3,
                Type = Entry.EntryType.Ascii,
                Count = 3,
            };
            char lonRef;
            if (position.Coordinate.Longitude > 0)
            {
                lonRef = 'E';
            }
            else
            {
                lonRef = 'W';
            }
            LongitudeRef.IntValues = new UInt32[] { (byte)lonRef, 0 };
            gpsIfdData.Entries.Add(LongitudeRef.Tag, LongitudeRef);

            var Longitude = new Entry()
            {
                Tag = 0x4,
                Type = Entry.EntryType.Rational,
                Count = 3,
            };
            var LonDeg = Math.Floor(position.Coordinate.Longitude);
            var LonMin = Math.Floor((position.Coordinate.Longitude - LonDeg) * 60);
            var LonSec = Util.ToRoundUp(((position.Coordinate.Longitude - LonDeg) * 60 - LonMin), 3);
            LongitudeRef.DoubleValues = new double[] { LonDeg, LonMin, LonSec };
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
            GpsMapDetum.value = Util.ConvertToByte("WGS-84");
            gpsIfdData.Entries.Add(GpsMapDetum.Tag, GpsMapDetum);

            var GpsDateStamp = new Entry()
            {
                Tag = 0x1D,
                Type = Entry.EntryType.Ascii,
                Count = 11,
            };
            String str = position.Coordinate.Timestamp.Year.ToString("D4") + ":" + position.Coordinate.Timestamp.Month.ToString("D2") + ":" + position.Coordinate.Timestamp.Day.ToString("D2");
            GpsDateStamp.value = Util.ConvertToByte(str);
            gpsIfdData.Entries.Add(GpsDateStamp.Tag, GpsDateStamp);

            return gpsIfdData;
        }
    }
}
