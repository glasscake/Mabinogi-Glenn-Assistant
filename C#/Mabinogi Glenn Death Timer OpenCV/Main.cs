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
             sw = new ScreenWatcher();
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
            while (true)
            {
                sw.newimage();
                Thread.Sleep(15);
                pb_debugging.Invoke(() => pb_debugging.Image = sw.debugging);
            }
        }
    }
}
