using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NtImageProcessor
{
    public class HistogramCreator
    {
        private BitmapImage image;
        private WriteableBitmap writableBitmap;
        
        private int[] red, green, blue;

        public enum HistogramResolution {
            Resolution_256,
            Resolution_128,
            Resolution_64,
            Resolution_32,
        };
        
        public int MaxFrequency { get; set; }
        public int Resolution { get; set; }
        private HistogramResolution histogramResolution;

        public event Action<int[], int[], int[]> OnHistogramCreated;

        public bool IsRunning
        {
            get;
            set;
        }

        public HistogramCreator(HistogramResolution resolution, int maxFrequency)
        {
            MaxFrequency = maxFrequency;
            histogramResolution = resolution;
            switch (resolution)
            {
                case HistogramResolution.Resolution_256:
                    Resolution = 256;
                    break;
                case HistogramResolution.Resolution_128:
                    Resolution = 128;
                    break;
                case HistogramResolution.Resolution_64:
                    Resolution = 64;
                    break;
                case HistogramResolution.Resolution_32:
                    Resolution = 32;
                    break;
                default:
                    Resolution = 256;
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

        public void CreateHistogram(BitmapImage source)
        {
            if (IsRunning)
            {
                Debug.WriteLine("it's ruuning. skip.");
                return;
            }

            _init();

            IsRunning = true;
           
            writableBitmap = new WriteableBitmap(source);
            var thread = new System.Threading.Thread(CalculateHistogram);
            Debug.WriteLine("starting thread: " + thread.ManagedThreadId);
            thread.Start();
            // Debug.WriteLine("started thread: " + thread.ManagedThreadId);
        }

        private void CalculateHistogram()
        {
            // Debug.WriteLine("start calculate");

            int shift = 0;
            switch (histogramResolution)
            {
                case HistogramResolution.Resolution_256:
                    break;
                case HistogramResolution.Resolution_128:
                    shift = 1;
                    break;
                case HistogramResolution.Resolution_64:
                    shift = 2;
                    break;
                case HistogramResolution.Resolution_32:
                    shift = 3;
                    break;
            }

            foreach (int v in writableBitmap.Pixels)
            {
                int value = v;
                
                int b = (value & 0xFF);
                value = value >> 8;
                int g = (value & 0xFF);
                value = value >> 8;
                int r = value & 0xFF;

                if (shift != 0)
                {
                    r = r >> shift;
                    g = g >> shift;
                    b = b >> shift;
                }

                red[r]++;
                green[g]++;
                blue[b]++;
            }
            
            // normalize values.
            double scaleFactor = (double)MaxFrequency / (double)writableBitmap.Pixels.Length;
            // Debug.WriteLine("scale: " + scaleFactor);

            for (int i = 0; i < Resolution; i++)
            {
                red[i] = (int)((double)red[i] * scaleFactor);
                green[i] = (int)((double)green[i] * scaleFactor);
                blue[i] = (int)((double)blue[i] * scaleFactor);
            }

            if (OnHistogramCreated != null)
            {
                OnHistogramCreated(red, green, blue);
            }

            IsRunning = false;
        }

    }
}
