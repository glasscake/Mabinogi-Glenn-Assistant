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
        Thread HPMonitor;

        List<DoomTimer> timers = new List<DoomTimer>();
        CancellationTokenSource cts_doom = new CancellationTokenSource();
        CancellationTokenSource cts_HP = new CancellationTokenSource();
        ScreenCapture screencap = new ScreenCapture();

        bool UserInput_Boss_started;


        public Main()
        {
            InitializeComponent();
            //start_doom_monitor();
            start_HP_monitor();
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
            if (DoomMonitor == null) { return; }
            while (DoomMonitor.ThreadState == System.Threading.ThreadState.Running && timeoput.ElapsedMilliseconds < 10 * 1000)
            {

            }
        }

        private void start_HP_monitor()
        {
            cts_HP = new CancellationTokenSource();
            Thread HPMonitor = new Thread(() => Boss_HP_Monitor(cts_HP.Token));
            HPMonitor.Start();
        }
        private void reset_HP_monitor()
        {
            stop_HP_monitor();
            start_HP_monitor();
        }
        private void stop_HP_monitor()
        {
            cts_HP.Cancel();
            Stopwatch timeoput = new Stopwatch();
            timeoput.Start();
            if (HPMonitor == null) { return; }
            while (HPMonitor.ThreadState == System.Threading.ThreadState.Running && timeoput.ElapsedMilliseconds < 10 * 1000)
            {

            }
        }
        private void start_doom_monitor()
        {
            cts_doom = new CancellationTokenSource();
            DoomMonitor = new Thread(() => DoomParser(cts_doom.Token));
            DoomMonitor.Start();
        }

        private void Boss_HP_Monitor(CancellationToken token)
        {
            OCR reader = new OCR();
            Utils utils = new Utils();
            SubCapture HpBar_subcap = new SubCapture(screencap.GetCrop, utils.Textboxes_to_Rect(hp_tl_x, hp_tl_y, hp_br_x, hp_br_y));
            Mat mat_hp = new Mat();

            double Boss_HP;
            double Boss_HP_doubleCheckRead = 0;
            List<double> Boss_hp_history = new List<double>(); 


            //bools for if the speach synth already spoke
            bool p95, p75, p65, p55, p35, p25, p15;
            //a timeout where if we loose the boss hp bar for too long we reset the app
            Stopwatch Lost_BOSS_HP = new Stopwatch();
            Lost_BOSS_HP.Start();
            int reset_timeout = 240 * 1000;
            string read_text;
            List<(char,char)> replace_list = new List<(char,char)> { ('°', '.'), (',', '.') };

            Regex reg_hp_percent = new Regex(@"\d?\d.\d\d %");
            Regex reg_hp = new Regex(@"\d?\d.\d\d");

            while (true)
            {
                Thread.Sleep(200);
                if (Lost_BOSS_HP.ElapsedMilliseconds > reset_timeout) { break; }
                if (token.IsCancellationRequested == true) { break; }

                mat_hp = HpBar_subcap.Crop.Clone();
                //mat_hp = Cv2.ImRead("Refrences/bosshp/bosshp.jpg");

                mat_hp = BossHp_Filtering(mat_hp);

                read_text = reader.Read(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat_hp));
                
                read_text = StringReplace_List(read_text, replace_list);
                read_text = Regex.Replace(read_text, @"\n","");

                if (reg_hp_percent.IsMatch(read_text) == false) { continue; }

                //remove out the %
                Match match = reg_hp.Match(read_text); 
                

                if(double.TryParse(match.Value, out Boss_HP) == false) { continue; }

                #region check for failed hitcheck
                //are we seeing a large jump between the newest value and the last value without a big time difference
                //are we jumping up in hp? this should only happen if hitcheck was failed so the HP is going form 65 to 75 or 35 to 45
                try
                    {
                    if (
                        Boss_HP > Boss_hp_history.Average() &&                                                   //is the new hp larger than the average
                        ((Boss_HP > 80.0 && Boss_HP < 85.1) || (Boss_HP > 40.0 && Boss_HP < 45.1)) &&           //is the new hp close to 75% or 45%
                        ((Boss_hp_history.Average() > 75 && Boss_hp_history.Average() < 77) || (Boss_hp_history.Average() > 35 && Boss_hp_history.Average() < 37))//was the average close to  65 or 35
                        && !(Boss_HP >= Boss_HP_doubleCheckRead - 0.5 && Boss_HP <= Boss_HP_doubleCheckRead + 0.5) //have we already checked this?
                        )
                    {
                        //everything lines up with a failed hit check, lets wait a moment read again and see if the hp stays consistent
                        Thread.Sleep(1500);
                        Boss_HP_doubleCheckRead = Boss_HP;
                        continue;
                    }

                    //did the hitcheck fail? reset the average and anouncers
                    if (Boss_HP >= Boss_HP_doubleCheckRead - 1 && Boss_HP <= Boss_HP_doubleCheckRead + 1)
                    {
                        Boss_HP_doubleCheckRead = 0;
                        //reset the hitcheck anouncement
                        if (Math.Abs(Boss_HP - 85) < 8) { p75 = false; }
                        if (Math.Abs(Boss_HP - 45) < 8) { p35 = false; }
                        Boss_hp_history.Clear();
                    }
                }
                catch (Exception) { Console.WriteLine("exception when testing for failed hitcheck"); }
                #endregion

                Boss_hp_history.Add(Boss_HP);

                if (Boss_hp_history.Count > 10)
                {
                    Boss_hp_history.RemoveAt(0);
                }

                Boss_HP = Boss_hp_history.Average();





                //first we need to detect Cailleach and Cnoc Oighir. this will be how we know we started the boss room
                //or the user can press start to signify they are at the HM section
                //if (UserInput_Boss_started != true) { continue; }


                Lost_BOSS_HP.Restart();
                pb_bosshp.Invoke(() => pb_bosshp.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat_hp));
                richtx_debugging_HP.Invoke(() => richtx_debugging_HP.Text = Boss_HP.ToString());
            }
            mat_hp.Dispose();
        }

        private Mat BossHp_Filtering(Mat input)
        {
            Mat m0 = new Mat();
            Utils utils = new Utils();
            utils.CorrectGamma(input, input, .5);
            //Cv2.ImShow("gamma", input);
            Cv2.Resize(input, input, new OpenCvSharp.Size(input.Width * 2, input.Height * 2));
            Cv2.CvtColor(input, m0, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(m0, new Scalar(18, 150, 100), new Scalar(26, 255, 255), m0);
            Cv2.GaussianBlur(m0, m0, new OpenCvSharp.Size(5, 5), 0);
            //Cv2.ImShow("colors only", m0);
            Cv2.CvtColor(input, input, ColorConversionCodes.BGR2GRAY);
            Cv2.AddWeighted(input, 1, m0, -.4, 0, input);
            Cv2.Threshold(input, input, 150, 255, ThresholdTypes.Binary);
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(2, 2), new OpenCvSharp.Point(-1, -1));
            Cv2.Dilate(input, input, kernel, iterations: 3);
            m0.Dispose();
            return input;
        }

        private string StringReplace_List(string input, List<(char To_replace, char Replace_with)> list)
        {
            foreach (var item in list)
            {
                input = input.Replace(item.To_replace, item.Replace_with);
            }
            return input;
        }

        private void DoomParser(CancellationToken token)
        {
            Mat mask = new Mat();
            OCR reader = new OCR();
            Utils utils = new Utils();


            SubCapture DoomWindow_Cap = new SubCapture(screencap.GetCrop, utils.Textboxes_to_Rect(doom_tl_x, doom_tl_y, doom_br_x, doom_br_y));

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

                utils.CorrectGamma(mask, mask, .3);

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
                    output = output + timer.Name + " has " + timer.Timer.Time_Remaining.ToString() + " and has reoccured: " + timer.Rerecognition_Count_Name.ToString() + " Timer has reoccured: " + timer.Rerecognition_Count_Time + " \n";
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
                maxcount = reoccuring.Select(item => item.Rerecognition_Count_Name).Max();
            }
            if (reoccuring.Count >= 4 && maxcount > 25)
            {
                //now get rid of any that are not close to the max count
                reoccuring.RemoveAll(item => item.Rerecognition_Count_Name < maxcount * 0.5);
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
                    if (LevenshteinDistance.Compute(fresh_timer.Name, org_timer.Name) >= 3)
                    {
                        continue;
                    }
                    //ok we had a name match with less than 3 characters different increment the reoccurance counter
                    org_timer.Rerecognition_Count_Name++;
                    //lets check if the timer is being read correctly
                    int org_time, fresh_time;
                    org_time = org_timer.Timer.Time_Remaining;
                    fresh_time = fresh_timer.Timer.Time_Remaining;
                    //so we are going to check if we are within a 4 second window of the original timer. by adding 2 to the time and seeing if it is greater than the new time and vise versa
                    if (org_time + 2 > fresh_time && org_time - 2 < fresh_time)
                    {
                        org_timer.Rerecognition_Count_Time++;
                    }

                    //lets check if the time rerecognitions is close to the name rerecognitions. if its less we probably did not read the time in right the first time
                    //we dont care the other way around really because we are filtering out poorly read names before this seciton of code. so inherently all the names in this list should be pretty accurate
                    if (org_timer.Rerecognition_Count_Name * 0.9 < org_timer.Rerecognition_Count_Time)
                    {
                        fresh.Remove(fresh_timer);
                        continue;
                    }

                    //check if the timer is siginificantly greater than what it was. this means someone was in the portal
                    if (org_time + 10 < fresh_time || org_time - 5 > fresh_time)
                    {
                        //we are reading the new capture as longer than the original capture. lets recalculate out the time
                        org_timer.Timer.startingtime += fresh_time - org_time;
                        //reset the difference in counting so we are not constatnly recalculating the time
                        org_timer.Rerecognition_Count_Time = org_timer.Rerecognition_Count_Name;
                    }
                    fresh.Remove(fresh_timer);
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

        private void btn_startBoss_Click(object sender, EventArgs e)
        {
            UserInput_Boss_started = true;
        }

        private void btn_ResetHP_Click(object sender, EventArgs e)
        {
            reset_HP_monitor();
        }

        private void btn_debug_Click(object sender, EventArgs e)
        {
            Utils utils = new Utils();
            SubCapture HpBar_subcap = new SubCapture(screencap.GetCrop, utils.Textboxes_to_Rect(hp_tl_x, hp_tl_y, hp_br_x, hp_br_y));
            HpBar_subcap.Crop_Image.Save("t.jpg");
        }
    }

}
