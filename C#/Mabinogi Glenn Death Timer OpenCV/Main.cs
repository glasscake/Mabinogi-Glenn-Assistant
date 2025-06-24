using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech;
using System.Speech.Synthesis;
using static System.Windows.Forms.LinkLabel;
using System.Text.RegularExpressions;



namespace Mabi_CV
{
    public partial class Main : Form
    {
        Thread DoomMonitor;
        List<DoomTimer> timers = new List<DoomTimer>();
        CancellationTokenSource cts_doom = new CancellationTokenSource();
        ScreenCapture screencap = new ScreenCapture();


        public Main()
        {
            InitializeComponent();
            start_doom_monitor();
            Thread test = new Thread(() => Boss_HP_Monitor());
            test.Start();
        }

        private void btn_debugging_Click(object sender, EventArgs e)
        {
        }

        private void reset_doom_monitor()
        {
            stop_doom_monitor();
            start_doom_monitor();
        }
        private void stop_doom_monitor()
        {
            cts_doom.Cancel();
            Stopwatch timeoput = new Stopwatch();
            timeoput.Start();
            while (DoomMonitor.ThreadState == System.Threading.ThreadState.Running && timeoput.ElapsedMilliseconds < 10 * 1000)
            {

            }
        }
        private void start_doom_monitor()
        {
            cts_doom = new CancellationTokenSource();
            DoomMonitor = new Thread(() => DoomParser(cts_doom.Token));
            DoomMonitor.Start();
        }
        private void Boss_HP_Monitor()
        {
            OCR reader = new OCR();
            SubCapture HpBar_cap = new SubCapture(screencap.GetCrop, new OpenCvSharp.Rect(1022, 1250, 1535 - 1022, 1280 - 1250));
            while (true)
            {
                Thread.Sleep(33);
                pb_bosshp.Invoke(() => pb_bosshp.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(HpBar_cap.Crop));
            }
        }


        private void DoomParser(CancellationToken token)
        {
            Mat mask = new Mat();
            OCR reader = new OCR();

            SubCapture DoomWindow_Cap = new SubCapture(screencap.GetCrop ,new OpenCvSharp.Rect(2170, 160, 2560 - 2170, 318 - 160));

            string output;
            Taylors_Countdown_Timer FirstTry = new Taylors_Countdown_Timer(10);
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(15);

                mask = DoomWindow_Cap.Crop.Clone();
                Cv2.Resize(mask, mask, new OpenCvSharp.Size(mask.Width * 3, mask.Height * 3));

                DoomWindow_Cap.CorrectGamma(mask, mask, .3);

                Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(mask, new Scalar(0, 0, 210), new Scalar(180, 100, 255), mask);

                Cv2.GaussianBlur(mask, mask, new OpenCvSharp.Size(7, 7), 0);
                Cv2.Threshold(mask, mask, 10, 255, ThresholdTypes.Binary);

                //Cv2.ImShow("mask", mask);
                //Cv2.ImShow("unmasked", sw.debugging_mat);
                Cv2.WaitKey(1);
                pb_debugging.Invoke(() => pb_debugging.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask));
                output = reader.Read(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask));
                List<DoomTimer> reader_timers = reader.ParseDoom(output);
                Cull_DoomTimer_List(reader_timers, ref timers);
                output = "";
                foreach (DoomTimer timer in timers)
                {
                    output = output + timer.Name + " has " + timer.Timer.Time_Remaining.ToString() + " and has reoccured" + timer.Rerecognition_Count.ToString() + " \n";
                }

                richtx_debugging.Invoke(() => richtx_debugging.Text = output);
            }
            mask.Dispose();
            timers.ForEach(item => item.Dispose());
            timers.Clear();

        }



        private void Cull_DoomTimer_List(List<DoomTimer> fresh, ref List<DoomTimer> reoccuring)
        {
            if (fresh == null) { return; }
            if (fresh.Count == 0) { return; }

            //do we have 4 good names with lots of recognitions
            int maxcount = 0;
            if (reoccuring.Count > 0)
            {
                maxcount = reoccuring.Select(item => item.Rerecognition_Count).Max();
            }
            if (reoccuring.Count >= 4 && maxcount > 25)
            {
                //now get rid of any that are not close to the max count
                reoccuring.RemoveAll(item => item.Rerecognition_Count < maxcount * 0.5);
                reoccuring.ForEach(item => item.Change_Beep(ckbx_doom_Beep.Checked));
                reoccuring.ForEach(item => item.Change_voice(ckbx_doomVoice.Checked));
            }

            //if we are empty lets start to fill the list
            if (reoccuring.Count == 0)
            {
                //clone in the current list of timers we have
                reoccuring = new List<DoomTimer>(fresh);
                return;
            }


            //ok lets check how close the names are in the reoccuring list and the new list if theyre within 2 characters of each other then we know we have a good read
            //there is probably a cleaner way to do this with linq
            foreach (DoomTimer org_timer in reoccuring)
            {
                foreach (DoomTimer fresh_timer in fresh.ToList())
                {
                    if (LevenshteinDistance.Compute(fresh_timer.Name, org_timer.Name) <= 2)
                    {
                        org_timer.Rerecognition_Count++;
                        fresh.Remove(fresh_timer);
                    }
                }
            }
            //now we add in all of the unused items
            reoccuring.AddRange(fresh);




        }


        private void Main_Load(object sender, EventArgs e)
        {
            Application.OpenForms[0].Location = Screen.AllScreens[1].WorkingArea.Location;
        }

        private void btn_resetDoom_Click(object sender, EventArgs e)
        {
            reset_doom_monitor();
        }
    }

}
