using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vGamePad
{
    public partial class InformationForm : Form
    {
        public InformationForm()
        {
            InitializeComponent();
            this.timer1.Start();
        }

        private string CalcAstTime()
        {
            DateTime datetime = DateTime.Now;
            double sec = datetime.Hour * 60 * 60 + datetime.Minute * 60 + datetime.Second;
            sec = (sec * 20) % (24 * 60 * 60);
            double h = Math.Floor((double)(sec / 3600));
            double m = Math.Floor((double)(sec / 60)) % 60;
            string timestr = String.Format("AST {0:00}:", h) + String.Format("{0:00}", m);

            double rt = 0.0;
            string temp = "";
            if (h < 6)
            {
                rt = (6 * 60 * 60 - sec) / 20;
                temp = "朝まであと";
            }
            else if (h < 18)
            {
                // 夜まで
                rt = (18 * 60 * 60 - sec) / 20;
                temp = "夜まであと";
            }
            else
            {
                // 朝まで
                rt = (24 * 60 * 60 - sec + 6 * 60 * 60) / 20;
                temp = "朝まであと";
            }
            string rstr = temp + String.Format("{0:00}分", Math.Floor(rt / 60)) + String.Format("{0:00}秒", Math.Floor(rt % 60));
            return timestr + "  " + rstr;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label1.Text = CalcAstTime();
        }
    }
}
