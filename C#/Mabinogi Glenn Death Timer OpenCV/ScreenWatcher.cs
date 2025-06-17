using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HPPH;
using OpenCvSharp;
using ScreenCapture;
using ScreenCapture.NET;


namespace Mabi_CV
{
    public class ScreenWatcher
    {
        DX11ScreenCaptureService screenCaptureService = new DX11ScreenCaptureService();
        public Bitmap debugging;
        DX11ScreenCapture screenCapture;
        CaptureZone<ColorBGRA> fullscreen;
        private int garbagecollector_counter;
        public ScreenWatcher()
        {
            IEnumerable<GraphicsCard> graphicsCards = screenCaptureService.GetGraphicsCards();
            IEnumerable<Display> displays = screenCaptureService.GetDisplays(graphicsCards.First());
            screenCapture = screenCaptureService.GetScreenCapture(displays.First());
            fullscreen = screenCapture.RegisterCaptureZone(0, 0, screenCapture.Display.Width, screenCapture.Display.Height);
            screenCapture.CaptureScreen();
            newimage();
        }

        public unsafe void newimage()
        {
            garbagecollector_counter++;
            screenCapture.CaptureScreen();
            using (fullscreen.Lock())
            {
                IImage<ColorBGRA> image = fullscreen.Image;
                Mat material = new Mat(image.Height, image.Width, MatType.CV_8UC4);
                image.CopyTo(new Span<ColorBGRA>((void*)material.DataPointer, image.Width * image.Height));
                debugging = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(material);
                material.Dispose();
            }
            if(garbagecollector_counter > 40)
            {
                GC.Collect();
                garbagecollector_counter = 0;
            }
        }


    }
}
