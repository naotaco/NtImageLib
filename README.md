NtImageLib
==========

Provides functions to analyze / operate JPEG file in C#.

- No dependencies
- Works on WindowsPhone 8/8.1
+ For UWP (Windows 10), See [new one](https://github.com/naotaco/NtImageProcessorUwp)
- The latest Visual Studio Express 2012 is required to run included tests.

## JPEG metadata (Exif and other) operator

### Parser/Builder

JpegMetaDataParser parses metadata in JPEG image file simply.
Both of byte array and byte stream are supported as input format.
JpegMetaData structure may contains 3 sections(called IFD), Primary, Exif, and GPS.
Each IFD has a dictionary of Entry with keys.

```cs
var Metadata = JpegMetaDataParser.ParseImage(image);
```

JpegMetaDataProcessor#SetMetaData function overwrites metadata to given image.

```cs
var metadata = JpegMetaDataParser.ParseImage(image);
// do something to the metadata
var newImage = JpegMetaDataProcessor.SetMetaData(image, metadata);
```

### Add Geoposition to metadata

It has a method to add GPS IFD from Geoposition(Windows.Devices.Geolocation.Geoposition).

```cs
// parse given image first
var exif = JpegMetaDataParser.ParseImage(image);

// check whether the image already contains GPS section or not
if (exif.PrimaryIfd.Entries.ContainsKey(Definitions.GPS_IFD_POINTER_TAG))
{
	// You can throw excpetion
	throw new GpsInformationAlreadyExistsException("This image contains GPS information.");
}
// or overwrite section.

// Create IFD structure from given GPS info
var gpsIfdData = GpsIfdDataCreator.CreateGpsIfdData(position);

// Add GPS info to exif structure
exif.GpsIfd = gpsIfdData;

// create a new image with given location info
var newImage = JpegMetaDataProcessor.SetMetaData(image, exif);
```

## JPEG image analyzer

A class, HistogramCreator supports to show level of each colors.

```cs

private void SomethingInitialize()
{
	// Initialize instance with resolution setting
	histogramCreator = new HistogramCreator(HistogramCreator.HistogramResolution.Resolution_128);
	histogramCreator.OnHistogramCreated += histogramCreator_OnHistogramCreated;
}

public void ImageUpdated()
{
	// input image
	await histogramCreator.CreateHistogram(ImageSource);
}

void histogramCreator_OnHistogramCreated(int[] Count_R, int[] Count_G, int[] Count_B)
{
	// Do something with values.
	// Length of these arrays will be same, and it will be the resolution, 64, 128 or 256.
	// Pixel count of each levels are stored.
}

```

