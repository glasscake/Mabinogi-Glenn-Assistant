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
        Mabi_CV sw;
        Thread livestream;
        public Main()
        {
            InitializeComponent();

            sw = new Mabi_CV(new OpenCvSharp.Rect(2220, 160, 2560 - 2220, 318 - 160));
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
            Mat mask = new Mat();
            global::Mabi_CV.OCR reader = new global::Mabi_CV.OCR();
            string output;
            while (true)
            {
                Thread.Sleep(15);

                sw.newimage();
                mask = sw.debugging_mat.Clone();
                Cv2.Resize(mask, mask, new OpenCvSharp.Size(mask.Width * 3, mask.Height * 3));
               
                CorrectGamma(mask, mask, .3);
                
                Cv2.CvtColor(mask, mask, ColorConversionCodes.BGR2HSV);
                Cv2.InRange(mask, new Scalar(0, 0, 210), new Scalar(180, 100, 255), mask);

                
               
                Cv2.GaussianBlur(mask, mask, new OpenCvSharp.Size(7, 7), 0);
                Cv2.Threshold(mask, mask, 10, 255, ThresholdTypes.Binary);


                Cv2.ImShow("mask", mask);
                Cv2.ImShow("unmasked", sw.debugging_mat);
                Cv2.WaitKey(1);
                pb_debugging.Invoke(() => pb_debugging.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask));
                output = reader.Read(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mask));
                output = ParseDoom(output);
                richtx_debugging.Invoke(() => richtx_debugging.Text = output);
            }
        }

        public void CorrectGamma(Mat src, Mat dst, double gamma)
        {
            byte[] lut = new byte[256];
            for (int i = 0; i < lut.Length; i++)
            {
                lut[i] = (byte)(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0);
            }

            Cv2.LUT(src, lut, dst);
        }
        public string ParseDoom(string input)
        {
            string modified;
            modified = Regex.Replace(input, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            //count line returns
            string[] lines = modified.Split('\n');
            
            //start filtering out bad inputs
            if (lines.Length > 5 || lines.Length < 2)
            { return string.Format("cant read: too many or too few lines: {0} must be > 5 & < 2",lines.Length); }
            if (lines[0] != "Time Until the End")
            { return "cant read: \"Time Until the End\""; }

            //now we must find the time and the player name. Once we get a good lock on 'time until the end' we can be somewhat confident 
            //player names can not contain special characters, spaces, or capital letters after a lowercase
            //player names can contain the special character '+'

            //lets test the colon and see if we got a good read. we should see any valid character a-z 0-9 a space a : a space and a number
            Regex reg_colon = new Regex(@"\w : \d");
            Regex reg_min_sec = new Regex(@": \dmin \w\w?sec");
            Regex reg_sec_only = new Regex(@": \d\d?sec");
            Regex reg_parse_name = new Regex(@"[a-z A-Z 0-9 +]+ :");
            Regex reg_parse_min = new Regex(@"\d\d?min");
            Regex reg_parse_sec = new Regex(@"\d\d?sec");

            List<(string name, int min, int sec)> DoomTimers = new List<(string name, int min, int sec)>();
            

            foreach (string line in lines)
            {
                //lets do some cleanup to the input string. sometimes : gets read as > or <
                string clean = line.Replace('>', ':');
                clean = clean.Replace('<', ':');

                //test if we have a name and a colon at some time
                if (reg_colon.Count(clean) != 1) { continue; }
                //test if we have a valid time reading
                if(reg_min_sec.Count(clean) != 1 && reg_sec_only.Count(clean) != 1) { continue;}
                //we have a name and some time lets parse the name
                string name = reg_parse_name.Match(clean).Value;
                //remove the ' :'
                name = name.Substring(0, name.Length - 2);
                //find if we are minutes and seconds or just seconds only one can be true so no need for else if
                int minutes = 0;
                int seconds = 0;    
                if(reg_min_sec.Count(clean) == 1)
                {
                    //minutes seconds found parse out the two numbers
                    //minutes can only ever be single digit. well unless someone is really spending alot of time in the portal
                    string min = reg_parse_min.Match(clean).Value.Substring(0, 1);
                    seconds = parse_sec(clean, reg_parse_sec);
                    int.TryParse(min, out minutes);
                    //check if we got a bad read
                    if(minutes == 0) { continue;}
                }
                if(reg_sec_only.Count(clean) == 1)
                {
                    seconds = parse_sec(clean, reg_parse_sec);
                    //check if we got a bad read
                    if(seconds == 0) { continue;}
                }
                //we read the time back right lets put it all together
                DoomTimers.Add((name, minutes, seconds));
            }


            return modified;
        }

        private int parse_sec(string input, Regex reg)
        {
            int seconds;
            string sec = reg.Match(input).Value;
            //seconds can be double or single digit so we need to parse out just the digits
            sec = sec.Substring(0, sec.Length - 3);
            int.TryParse(sec, out seconds);
            return seconds;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            Application.OpenForms[0].Location = Screen.AllScreens[1].WorkingArea.Location;
        }
    }
}
