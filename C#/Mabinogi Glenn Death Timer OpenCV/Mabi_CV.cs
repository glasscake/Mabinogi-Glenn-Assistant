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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Speech.Synthesis;



namespace Mabi_CV
{
    public class Mabi_CV
    {
        DX11ScreenCaptureService screenCaptureService = new DX11ScreenCaptureService();
        IEnumerable<GraphicsCard> graphicsCards;
        IEnumerable<Display> displays;
        DX11ScreenCapture screenCapture;

        public Bitmap debugging;
        public Mat debugging_mat;

        CaptureZone<ColorBGRA> fullscreen;
        private int garbagecollector_counter;
        public Mabi_CV()
        {
            setupScrenCap();
            OpenCvSharp.Rect box = new OpenCvSharp.Rect(0, 0, screenCapture.Display.Width, screenCapture.Display.Height);
            _ScreenWatcher(box);
        }
        public Mabi_CV(OpenCvSharp.Rect cropbox)
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
            if (garbagecollector_counter > 300)
            {
                GC.Collect();
                garbagecollector_counter = 0;
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
    }
    public class OCR
    {
        TesseractEngine engine = new TesseractEngine(@"./Refrences", "eng", EngineMode.Default);

        public OCR()
        {

        }

        public string Read(Bitmap img)
        {
            Page page = engine.Process(img);
            string results = page.GetText();
            page.Dispose();
            return results;
        }
        public List<DoomTimer> ParseDoom(string input)
        {
            string modified;
            List<DoomTimer> timers = new List<DoomTimer>();
            modified = Regex.Replace(input, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
            //count line returns
            string[] lines = modified.Split('\n');

            //start filtering out bad inputs
            if (lines.Length > 5 || lines.Length < 2)
            {
                Console.WriteLine(string.Format("cant read: too many or too few lines: {0} must be > 5 & < 2", lines.Length));
                return null; 
            }
            if (lines[0] != "Time Until the End")
            {
                Console.WriteLine(("cant read: \"Time Until the End\""));
                return null; 
            }

            //now we must find the time and the player name. Once we get a good lock on 'time until the end' we can be somewhat confident 
            //player names can not contain special characters, spaces, or capital letters after a lowercase
            //player names can contain the special character '+'

            //lets test the colon and see if we got a good read. we should see any valid character a-z 0-9 a space a : a space and a number
            Regex reg_colon = new Regex(@"\w : \w");
            Regex reg_min_sec = new Regex(@": \wmin \w\w?sec");
            Regex reg_sec_only = new Regex(@": \w\w?sec");
            Regex reg_parse_name = new Regex(@"[a-z A-Z 0-9 +]+ :");
            Regex reg_parse_min = new Regex(@"\w\w?min");
            Regex reg_parse_sec = new Regex(@"\w\w?sec");


            foreach (string line in lines)
            {
                //lets do some cleanup to the input string. sometimes : gets read as > or <
                string clean = line.Replace('>', ':');
                clean = clean.Replace('<', ':');

                //test if we have a name and a colon at some time
                if (reg_colon.Count(clean) != 1) { continue; }
                //test if we have a valid time reading
                if (reg_min_sec.Count(clean) != 1 && reg_sec_only.Count(clean) != 1) { continue; }
                //we have a name and some time lets parse the name
                string name = reg_parse_name.Match(clean).Value;
                //remove the ' :'
                name = name.Substring(0, name.Length - 2);
                //find if we are minutes and seconds or just seconds only one can be true so no need for else if
                int minutes = 0;
                int seconds = 0;
                if (reg_min_sec.Count(clean) == 1)
                {
                    //minutes seconds found parse out the two numbers
                    //minutes can only ever be single digit. well unless someone is really spending alot of time in the portal
                    string min = reg_parse_min.Match(clean).Value.Substring(0, 1);
                    //filter out any 'i's that are acutally 1s
                    min = min.Replace("i", "1");
                    min = min.Replace("I", "1");
                    seconds = parse_sec(clean, reg_parse_sec);
                    int.TryParse(min, out minutes);
                    //check if we got a bad read
                    if (minutes == 0) { continue; }
                }
                if (reg_sec_only.Count(clean) == 1)
                {
                    seconds = parse_sec(clean, reg_parse_sec);
                    //check if we got a bad read
                    if (seconds == 0) { continue; }
                }
                //we read the time back right lets put it all together

                timers.Add(new DoomTimer(name, new Taylors_Countdown_Timer(minutes * 60 + seconds),false,false));
            }


            return timers;
        }
        private int parse_sec(string input, Regex reg)
        {
            int seconds;
            string sec = reg.Match(input).Value;
            //seconds can be double or single digit so we need to parse out just the digits
            sec = sec.Substring(0, sec.Length - 3);
            //1s sometimes get read as 'i' so we need to filter that out
            sec = sec.Replace("i", "1");
            sec = sec.Replace("I", "1");
            int.TryParse(sec, out seconds);
            return seconds;
        }



    }
    /// <summary>
    /// taken form https://stackoverflow.com/questions/2344320/comparing-strings-with-tolerance
    /// </summary>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }

    public class Taylors_Countdown_Timer
    {
        //A coutdown timer my wife made from first principles
        public int Time_Remaining
        {
            get {return calctime(); }
            private set {}
        }

         
        private Stopwatch timer = new Stopwatch();
        public int startingtime;
        public Taylors_Countdown_Timer (int input)
        {
            startingtime = input; //:)
            timer.Start();
        }
        private int calctime()
        {
            if (startingtime < (int)timer.ElapsedMilliseconds / 1000) 
            {
                timer.Stop(); 
                Time_Remaining = 0; 
                return 0; 
            }
            return startingtime - (int)timer.ElapsedMilliseconds / 1000;
        }
    }


    public class DoomTimer
    {
        public String Name;
        public Taylors_Countdown_Timer Timer;
        public bool Enable_beep;
        public bool Enable_speach;
        public string Error;
        public int Rerecognition_Count;
        Thread monitor;
        public DoomTimer(string error)
        {
            Error = error;
        }
        public DoomTimer(string name, Taylors_Countdown_Timer timer, bool enable_beep, bool enable_speach)
        {
            Name = name;
            Timer = timer;
            Enable_beep = enable_beep;
            Enable_speach = enable_speach;
            if (Enable_beep == true || enable_speach == true)
            {
                monitor = new Thread(monitor_timer);
            }
        }
        public void Change_Beep(bool state)
        {
            Enable_beep = state;
            if(Enable_beep == true && monitor.IsAlive == false)
            {
                monitor = new Thread(monitor_timer);
            }
        }

        public void Change_voice(bool state)
        {
            Enable_speach = state;
            if (Enable_speach == true && monitor.IsAlive == false)
            {
                monitor = new Thread(monitor_timer);
            }
        }

        private void monitor_timer()
        {
            while (Timer.Time_Remaining != 0)
            {
                if (Timer.Time_Remaining > 20 && Timer.Time_Remaining < 30 && Enable_beep == true)
                {
                    Console.Beep(400, 500);
                    Enable_beep = false;
                }
                if (Timer.Time_Remaining > 20 && Timer.Time_Remaining < 30 && Enable_speach == true)
                {
                    SpeechSynthesizer synth = new SpeechSynthesizer();
                    synth.SpeakAsync(Name + Timer.Time_Remaining.ToString() + " seconds");
                    Enable_speach = false;
                }
            }
        }

    }
}
