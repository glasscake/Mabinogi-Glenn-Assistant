using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Mabi_CV
{
    public partial class Main : Form
    {
        ScreenWatcher sw;
        Thread livestream; 
        public Main()
        {
            InitializeComponent();
             sw = new ScreenWatcher(new OpenCvSharp.Rect(2220,160,2560-2220,318-160));
            livestream = new Thread(stress_test_spam);
            livestream.Start();

        }

        private void btn_debugging_Click(object sender, EventArgs e)
        {
            sw.newimage();
            pb_debugging.Image = sw.debugging;
        }

        private void stress_test_spam()
        {
            //BackgroundSubtractorMOG2 background = BackgroundSubtractorMOG2.Create(5,5);
            Mat mask = new Mat();

            Mabi_CV.OCR reader = new Mabi_CV.OCR();
            while (true)
            {
                Thread.Sleep(30);

                sw.newimage();
                mask = sw.debugging_mat.Clone();
                //Cv2.GaussianBlur(mask, mask, new OpenCvSharp.Size(1, 1), 0);
                
                Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(mask, new Scalar(0, 0, 220), new Scalar(180, 100, 255), mask);
                Cv2.GaussianBlur(mask, mask, new OpenCvSharp.Size(3, 3),0);

                //background.Apply(sw.debugging_mat, mask);

                Cv2.ImShow("mask",mask);
                Cv2.ImShow("unmasked", sw.debugging_mat);
                Cv2.WaitKey(1);
                pb_debugging.Invoke(() => pb_debugging.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask));
                richtx_debugging.Invoke(() => richtx_debugging.Text = reader.Read(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask)));
            }
        }
    }
}
