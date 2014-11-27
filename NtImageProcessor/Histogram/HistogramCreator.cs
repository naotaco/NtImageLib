using System;
using System.Diagnostics;
using System.Threading.Tasks;
#if WINDOWS_PHONE
using System.Windows.Media.Imaging;
#elif WINDOWS_PHONE_APP||WINDOWS_APP||NETFX_CORE
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

#endif

namespace NtImageProcessor
{
    public class HistogramCreator
    {
        private int[] red, green, blue;

        private int shiftBytes;

        public int PixelSkipRate = 4;

        /// <summary>
        /// Used to specify histogram resolution.
        /// </summary>
        public enum HistogramResolution
        {
            Resolution_256,
            Resolution_128,
            Resolution_64,
            Resolution_32,
        };

        public int Resolution { get; set; }
        private HistogramResolution histogramResolution;

        /// <summary>
        /// This action will be called after histogram data has created.
        /// Arguments contains counts of each colors, Red, Green and Blue.
        /// Length of the arrays will be the specified resolution, 32, 64, 128 or 256.
        /// </summary>
        public event Action<int[], int[], int[]> OnHistogramCreated;

        public bool IsRunning
        {
            get;
            set;
        }

        /// <summary>
        /// Initialize with histogram resolution.
        /// In case 256 is selected, histogram will be 256-level.
        /// </summary>
        /// <param name="resolution">Resolution of historgram.</param>
        public HistogramCreator(HistogramResolution resolution)
        {
            histogramResolution = resolution;
            switch (resolution)
            {
                case HistogramResolution.Resolution_256:
                    Resolution = 256;
                    shiftBytes = 0;
                    break;
                case HistogramResolution.Resolution_128:
                    Resolution = 128;
                    shiftBytes = 1;
                    break;
                case HistogramResolution.Resolution_64:
                    Resolution = 64;
                    shiftBytes = 2;
                    break;
                case HistogramResolution.Resolution_32:
                    Resolution = 32;
                    shiftBytes = 3;
                    break;
                default:
                    Resolution = 256;
                    shiftBytes = 0;
                    break;
            }

            _init();
            IsRunning = false;
        }

        private void _init()
        {
            red = new int[Resolution];
            green = new int[Resolution];
            blue = new int[Resolution];

            for (int i = 0; i < Resolution; i++)
            {
                red[i] = 0;
                green[i] = 0;
                blue[i] = 0;
            }
        }

        /// <summary>
        /// Start to create histogram. Once it's completed, OnHistogramCreated will be called.
        /// </summary>
        /// <param name="source">Source image</param>
        /// <returns></returns>
        public async Task CreateHistogramAsync(WriteableBitmap source)
        {
            if (IsRunning)
            {
                Debug.WriteLine("it's ruuning. skip.");
                return;
            }

            _init();

            IsRunning = true;

            await Task.Factory.StartNew(() => { CalculateHistogram(source); });

        }

#if WINDOWS_APP
        /// <summary>
        /// Start to create histogram. Once it's completed, OnHistogramCreated will be called.
        /// Recommend to run this method on background task with lower priority.
        /// </summary>
        /// <param name="source">Source image</param>
        /// <returns></returns>
        public void CreateHistogram(WriteableBitmap source)
        {
            if (source == null || source.PixelBuffer == null)
            {
                return;
            }
            _init();
            CalculateHistogramFromPixelBuffer(source);
        }
#endif

        /// <summary>
        /// Calculate histogram from WritableBitmap.Pixelbuffer.
        /// </summary>
        /// <param name="writableBitmap"></param>
        private void CalculateHistogramFromPixelBuffer(WriteableBitmap writableBitmap)
        {
            var pixels = writableBitmap.PixelBuffer.ToArray();

            for (int i = 0; i < pixels.Length; i += (PixelSkipRate << 2))
            {
                SortPixel(pixels[i], PixelColor.Blue);
                SortPixel(pixels[i + 1], PixelColor.Green);
                SortPixel(pixels[i + 2], PixelColor.Red);
            }

            for (int i = 0; i < Resolution; i++)
            {
                red[i] = red[i] >> 4;
                green[i] = green[i] >> 4;
                blue[i] = blue[i] >> 4;
            }

            if (OnHistogramCreated != null)
            {
                OnHistogramCreated(red, green, blue);
            }
            IsRunning = false;
        }

        private void CalculateHistogram(WriteableBitmap writableBitmap)
        {
#if WINDOWS_PHONE
            var pixels = writableBitmap.Pixels;
            for (int i = 0; i < pixels.Length; i += 3)
            {
                SortPixel(pixels[i]);
            }
            // normalize values.

            for (int i = 0; i < Resolution; i++)
            {
                red[i] = red[i] >> 4;
                green[i] = green[i] >> 4;
                blue[i] = blue[i] >> 4;
            }

            if (OnHistogramCreated != null)
            {
                OnHistogramCreated(red, green, blue);
            }

            Debug.WriteLine("finished.");

            IsRunning = false;
#endif
        }

        private void SortPixel(int value)
        {
            int b = (value & 0xFF);
            value = value >> 8;
            int g = (value & 0xFF);
            value = value >> 8;
            int r = value & 0xFF;

            if (shiftBytes != 0)
            {
                r = r >> shiftBytes;
                g = g >> shiftBytes;
                b = b >> shiftBytes;
            }

            red[r]++;
            green[g]++;
            blue[b]++;
        }

        private void SortPixel(byte v, PixelColor color)
        {
            int value = (int)v;

            if (shiftBytes != 0)
            {
                value = value >> shiftBytes;
            }

            switch (color)
            {
                case PixelColor.Red:
                    red[value]++;
                    break;
                case PixelColor.Blue:
                    blue[value]++;
                    break;
                case PixelColor.Green:
                    green[value]++;
                    break;
            }
        }

        enum PixelColor
        {
            Red,
            Blue,
            Green,
        }
    }
}
