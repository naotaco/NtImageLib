using NtImageProcessor.MetaData.Composer;
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
            var exif = ExifParser.ParseImage(image);

            var PrimafyIfd = IfdComposer.ComposeIfdsection(exif.PrimaryIFDEntries, 8);

            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (byte b in PrimafyIfd)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(" ");

                i++;
                if (i == 16)
                {
                    i = 0;
                    sb.Append(Environment.NewLine);
                }
            }

            Debug.WriteLine("===Primary IFD section===");
            Debug.WriteLine(sb.ToString());

            var App1Data = new byte[PrimafyIfd.Length + 8];
            Array.Copy(PrimafyIfd, 0, App1Data, 8, PrimafyIfd.Length);
            var IfdData = Parser.IfdParser.ParseIfd(App1Data, 8);


            
            return image;
        }
    }
}
