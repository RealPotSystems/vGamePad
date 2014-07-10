using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace vGamePad
{
    public partial class InformationForm : Form
    {
        private bool _NoSystemBattery = false;
        public vGamePadForm m_vGamePadForm = null;

        public InformationForm()
        {
            InitializeComponent();

            // バッテリーの有無
            if (( SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) == BatteryChargeStatus.NoSystemBattery )
            {
                _NoSystemBattery = true;
            }
            else
            {
                Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                this._BatteryClock.Start();
            }
            
            this._ClockTimer.Start();
        }

        void SystemEvents_PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.StatusChange:
                    break;
            }
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

        static int n = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            float f = SystemInformation.PowerStatus.BatteryLifePercent;
            if ( f > 1 ) {
                this.label2.Text = "???%:";
            } else {
                this.label2.Text = String.Format("{0,3}%:", f * 100);
            }
            if ((SystemInformation.PowerStatus.BatteryChargeStatus&BatteryChargeStatus.Charging)==BatteryChargeStatus.Charging )
            {
                this.label2.Text += "充電中";

                for ( int i = 0 ; i < n ; i++ )
                {
                    this.label2.Text += ".";
                }
                n++;
                if ( n == 4 )
                {
                    n = 0;
                }
            }
            else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && f == 1.00)
            {
                this.label2.Text += "AC電源";
            }
            //switch (SystemInformation.PowerStatus.PowerLineStatus)
            //{
            //    case PowerLineStatus.Online:
            //        this.label2.Text = "AC電源";
            //        break;
            //    case PowerLineStatus.Offline:
            //        this.label2.Text = "電池";
            //        break;
            //    case PowerLineStatus.Unknown:
            //        this.label2.Text = "電源不明";
            //        break;
            //}
            switch (SystemInformation.PowerStatus.BatteryChargeStatus)
            {
                case BatteryChargeStatus.High:
                    Console.WriteLine("バッテリー残量は十分あります");
                    break;
                case BatteryChargeStatus.Low:
                    Console.WriteLine("バッテリー残量が少ないです");
                    break;
                case BatteryChargeStatus.Critical:
                    Console.WriteLine("すぐに充電してください");
                    break;
                case BatteryChargeStatus.Charging:
                    Console.WriteLine("充電中です");
                    break;
                case BatteryChargeStatus.NoSystemBattery:
                    Console.WriteLine("バッテリーを使用していません");
                    break;
                case BatteryChargeStatus.Unknown:
                    Console.WriteLine("バッテリー状態は不明です");
                    break;
            }
        }

        private void InformationForm_Load(object sender, EventArgs e)
        {
            if (_NoSystemBattery )
            {
                this.Width = 280;
            }
            else
            {
                this.Width = 560;
            }
            this.Height = 40;
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, Screen.PrimaryScreen.WorkingArea.Top);

            this.label1.Location = new Point(12, 9);
            this.label1.Width = 260;
            this.label1.Height = 22;
            //this.label1.BackColor = Color.Aqua;　// テスト用

            if (_NoSystemBattery)
            {
                this.label2.Visible = false;
            }
            this.label2.Location = new Point(this.Width / 2, 9);
            this.label2.Width = 260;
            this.label2.Height = 22;
            //this.label2.BackColor = Color.Aqua; //　テスト用

            GraphicsPath _path = new GraphicsPath();
            _path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
            _path.AddRectangle(new Rectangle(10, 0, this.Width - 20, this.Height));
            _path.AddRectangle(new Rectangle(0, 10, this.Width, this.Height - 20));
            _path.AddPie(0, 0, 20, 20, 180, 90);
            _path.AddPie(0, this.Height / 2, 20, 20, 90, 90);
            _path.AddPie(this.Width - 10 * 2, 0, 20, 20, 270, 90);
            _path.AddPie(this.Width - 10 * 2, this.Height / 2, 20, 20, 0, 90);
            this.Region = new Region(_path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen pen = new Pen(Color.White, 4);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, 10, 2, this.Width - 10, 2);
            e.Graphics.DrawLine(pen, 10, this.Height - 2 , this.Width - 10, this.Height - 2);
            e.Graphics.DrawLine(pen, 2, 10, 2, this.Height - 10 );
            e.Graphics.DrawLine(pen, this.Width - 2 , 10, this.Width - 2 , this.Height - 10 );
            e.Graphics.DrawArc(pen, 2, 2, 20, 20, 180, 90);
            e.Graphics.DrawArc(pen, 2, this.Height / 2 - 2, 20, 20, 90, 90);
            e.Graphics.DrawArc(pen, this.Width - 2 - 10 * 2, 2, 20, 20, 270, 90);
            e.Graphics.DrawArc(pen, this.Width - 2 - 10 * 2, this.Height / 2 - 2, 20, 20, 0, 90);
        }

        /// <summary>
        /// 表示位置決定
        /// </summary>
        /// <returns>表示位置</returns>
        public void SetPostion(int x, int y)
        {
            // 全体 - 幅
            this.Location = new Point(x/2 - this.Width/2, Screen.PrimaryScreen.WorkingArea.Top);
        }
    }
}
