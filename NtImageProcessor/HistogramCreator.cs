using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NtImageProcessor
{
    public class HistogramCreator
    {
        private int[] red, green, blue;

        private int shiftBytes;

        public enum HistogramResolution
        {
            Resolution_256,
            Resolution_128,
            Resolution_64,
            Resolution_32,
        };

        public int Resolution { get; set; }
        private HistogramResolution histogramResolution;

        public event Action<int[], int[], int[]> OnHistogramCreated;

        public bool IsRunning
        {
            get;
            set;
        }

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

            IsRunning = false;
        }

        public async Task CreateHistogram(BitmapImage source)
        {
            if (IsRunning)
            {
                Debug.WriteLine("it's ruuning. skip.");
                return;
            }

            _init();

            IsRunning = true;

            var writeableBitmap = new WriteableBitmap(source);

            await Task.Run(() => { CalculateHistogram(writeableBitmap); });
        }

        private void CalculateHistogram(WriteableBitmap writableBitmap)
        {

            //foreach (int v in writableBitmap.Pixels)
            for (int i = 0; i < writableBitmap.Pixels.Length; i += 13)
            {
                int value = writableBitmap.Pixels[i];

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
            
            // normalize values.
            
            for (int i = 0; i < Resolution; i++)
            {
                red[i] = red[i] >> 2;
                green[i] = green[i] >> 2;
                blue[i] = blue[i] >> 2;
            }
            
            if (OnHistogramCreated != null)
            {
                OnHistogramCreated(red, green, blue);
            }

            IsRunning = false;
        }

    }
}
