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
using Vortice.Mathematics;
using Tesseract;



namespace Mabi_CV
{
    public class ScreenWatcher
    {
        DX11ScreenCaptureService screenCaptureService = new DX11ScreenCaptureService();
        IEnumerable<GraphicsCard> graphicsCards;
        IEnumerable<Display> displays;
        DX11ScreenCapture screenCapture;

        public Bitmap debugging;
        public Mat debugging_mat;

        CaptureZone<ColorBGRA> fullscreen;
        private int garbagecollector_counter;
        public ScreenWatcher()
        {
            setupScrenCap();
            OpenCvSharp.Rect box = new OpenCvSharp.Rect(0,0, screenCapture.Display.Width, screenCapture.Display.Height);
            _ScreenWatcher(box);
        }
        public ScreenWatcher(OpenCvSharp.Rect cropbox)
        {
            _ScreenWatcher(cropbox);
        }
        
        private void _ScreenWatcher(OpenCvSharp.Rect cropbox)
        {
            setupScrenCap();
            fullscreen = screenCapture.RegisterCaptureZone(cropbox.TopLeft.X, cropbox.TopLeft.Y, cropbox.Width, cropbox.Height);
            screenCapture.CaptureScreen();
            newimage();
        }

        private void setupScrenCap()
        {
            if (graphicsCards != null) { return; }
            graphicsCards = screenCaptureService.GetGraphicsCards();
            displays = screenCaptureService.GetDisplays(graphicsCards.First());
            screenCapture = screenCaptureService.GetScreenCapture(displays.First());
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
                debugging_mat = material.Clone();
                material.Dispose();
            }
            if(garbagecollector_counter > 300)
            {
                GC.Collect();
                garbagecollector_counter = 0;
            }
        }


    }
    public class OCR
    {
        TesseractEngine engine = new TesseractEngine(@"./Refrences", "eng", EngineMode.Default);

        public  OCR() 
        {
           
        }

        public string Read(Bitmap img)
        {
            Page page = engine.Process(img);
            string results = page.GetText();
            page.Dispose();
            return results;
        }

    }
}
