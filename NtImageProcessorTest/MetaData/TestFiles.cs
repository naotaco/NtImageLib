using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtImageProcessorTest.MetaData
{
    internal static class TestFiles
    {
        internal static string[] ValidImages = new string[]{
            "image_bigendian_large.jpg",
            "image_negative_value.jpg",
            "image_with_app0.jpg",
            "image_less_tag.jpg",
            "image_with_geotag.JPG",
            "image_littleendian_large.jpg",
            "image_positive_value.jpg",
            "image_with_geotag_large.JPG",
        };

        internal static string[] ImagesWithoutGeotag = new string[]{            
            // "image_bigendian_large.jpg",// unfortunately, photo from Lumia 920 includes GPS IFD offset info, but does not contain actual location info..
            "image_negative_value.jpg",
            "image_with_app0.jpg",
            "image_littleendian_large.jpg",
            "image_positive_value.jpg",
        };

        internal static string[] ImagesWithNegativeValues = new string[]{
            "image_negative_value.jpg", 
        };

        internal static string[] GeotagTargetImages = new string[]{
            "image_negative_value.jpg",
            "image_with_app0.jpg",
            "image_littleendian_large.jpg",
            "image_positive_value.jpg",
            "image_less_tag.jpg",
        };

        internal static string[] ImagesWithoutGeotagAndExiftag = new string[]{
            "image_less_tag.jpg",
        };

        internal static string[] GeotagImages = new string[]{
            "image_with_geotag_large.JPG",
            "image_with_geotag.JPG",
        };

        internal static string[] InvalidImages = new string[]{   
            "image_png.png",
        };
    }
}
