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
        static public InformationForm _Form = null;


        public InformationForm()
        {
            _Form = this; 
            InitializeComponent();

            // バッテリーの有無
            if ((SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) == BatteryChargeStatus.NoSystemBattery)
            {
                Properties.Settings.Default.Battery = false;
            }
            else
            {
                Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                this._BatteryClock.Start();
            }
            this._ClockTimer.Start();

            this.label1.Width = 260;
            this.label1.Height = 22;
            this.label2.Width = 260;
            this.label2.Height = 22;
            bool _AstClock = Properties.Settings.Default.AstClock;
            bool _Battery = Properties.Settings.Default.Battery;
            // 幅と表示位置
            if (_AstClock && _Battery)
            {
                this.Width = 560;
                label1.Location = new Point(12, 9);
                label2.Location = new Point(this.Width / 2, 9);
            }
            else
            {
                this.Width = 280;
                if (_AstClock)
                {
                    label1.Location = new Point(12, 9);
                    label2.Location = new Point(-200, -200);
                }
                else
                {
                    label1.Location = new Point(-200, -200);
                    label2.Location = new Point(this.Width / 2, 9);
                }
            }
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, Screen.PrimaryScreen.WorkingArea.Top);
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

        //static int n = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            float f = SystemInformation.PowerStatus.BatteryLifePercent;
            if (f > 1)
            {
                this.label2.Text = "???%:";
            }
            else
            {
                this.label2.Text = String.Format("{0,3}%:", f * 100);
            }
            if ((SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.Charging) == BatteryChargeStatus.Charging)
            {
                this.label2.Text += "充電中";
                //for (int i = 0; i < n; i++)
                //{
                //    this.label2.Text += ".";
                //}
                //n++;
                //if (n == 4)
                //{
                //    n = 0;
                //}
                _PlayTime = false;
                
            }
            else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && f == 1.00)
            {
                this.label2.Text += "AC電源";
                _PlayTime = false;
            }
            else
            {
                this.label2.Text += CalcPlayTime();
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
            //switch (SystemInformation.PowerStatus.BatteryChargeStatus)
            //{
            //    case BatteryChargeStatus.High:
            //        Console.WriteLine("バッテリー残量は十分あります");
            //        break;
            //    case BatteryChargeStatus.Low:
            //        Console.WriteLine("バッテリー残量が少ないです");
            //        break;
            //    case BatteryChargeStatus.Critical:
            //        Console.WriteLine("すぐに充電してください");
            //        break;
            //    case BatteryChargeStatus.Charging:
            //        Console.WriteLine("充電中です");
            //        break;
            //    case BatteryChargeStatus.NoSystemBattery:
            //        Console.WriteLine("バッテリーを使用していません");
            //        break;
            //    case BatteryChargeStatus.Unknown:
            //        Console.WriteLine("バッテリー状態は不明です");
            //        break;
            //}
        }

        private bool _PlayTime;
        private float _StartBatteryLife = float.MinValue;
        private DateTime _StartTime = DateTime.MinValue;
        private bool _SetStartTime = false;
        private string CalcPlayTime()
        {
            string str = "計測中";
            if (_PlayTime == false && SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline )
            {
                _PlayTime = true;   // 計測開始
                // 初期値のバッテリー残量ほ保存し計測開始
                _StartBatteryLife = SystemInformation.PowerStatus.BatteryLifePercent - 0.01f;
                _SetStartTime = false;

            }
            if (_StartBatteryLife == SystemInformation.PowerStatus.BatteryLifePercent && !_SetStartTime)
            {
                // 計測開始時間を保存する
                _StartTime = DateTime.Now;
                _SetStartTime = true;
            }
            if (_SetStartTime)
            {
            }
            return str;
        }

        private void InformationForm_Load(object sender, EventArgs e)
        {
            //this.label1.BackColor = Color.Aqua;　// テスト用
            //this.label2.BackColor = Color.Aqua; //　テスト用
            this.Height = 40;
            SetRegion();
        }

        public void SetRegion()
        {
            this.Region = null;
            GraphicsPath _path = new GraphicsPath();
            _path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
            _path.AddRectangle(new Rectangle(10, 0, this.Width - 20, this.Height));
            _path.AddRectangle(new Rectangle(0, 10, this.Width, this.Height - 20));
            _path.AddPie(0, 0, 20, 20, 180, 90);
            _path.AddPie(0, this.Height / 2, 20, 20, 90, 90);
            _path.AddPie(this.Width - 10 * 2, 0, 20, 20, 270, 90);
            _path.AddPie(this.Width - 10 * 2, this.Height / 2, 20, 20, 0, 90);
            this.Region = new Region(_path);

            this.Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen pen = new Pen(Color.White, 4);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, 10, 2, this.Width - 10, 2);
            e.Graphics.DrawLine(pen, 10, this.Height - 2, this.Width - 10, this.Height - 2);
            e.Graphics.DrawLine(pen, 2, 10, 2, this.Height - 10);
            e.Graphics.DrawLine(pen, this.Width - 2, 10, this.Width - 2, this.Height - 10);
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
            this.Location = new Point(x / 2 - this.Width / 2, Screen.PrimaryScreen.WorkingArea.Top);
        }

        private void InformationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _Form = null;
        }

        public void SetLabelState(bool bAstClock, bool bBattery)
        {
            label1.Visible = bAstClock;
            label2.Visible = bBattery;
            if (!bAstClock && bBattery)
            {
                label2.Location = new Point(12, 9);
            }
            else
            {
                label1.Location = new Point(12, 9);
                label2.Location = new Point(this.Width / 2, 9);
            }
        }

        public static void ChangeSetting(string SettingName, bool value)
        {
            if (_Form != null)
            {
                bool _AstClock = Properties.Settings.Default.AstClock;
                bool _Battery = Properties.Settings.Default.Battery;

                _Form.Height = 40;

                // 条件に応じて値を変更する
                if (SettingName == "AstClock")
                {
                    _AstClock = value;
                }
                if (SettingName == "Battery")
                {
                    _Battery = value;
                }

                // 幅の変更と表示位置の修正
                if (_AstClock && _Battery)
                {
                    _Form.Width = 560;
                }
                else
                {
                    _Form.Width = 280;
                }
                _Form.Enabled = false;
                _Form.SetPostion(Screen.PrimaryScreen.Bounds.Width, -1);
                _Form.SetRegion();
                _Form.SetLabelState(_AstClock, _Battery);

                // 表示、非表示の設定
                if (_AstClock || _Battery)
                {
                    _Form.Visible = true;
                }
                else
                {
                    _Form.Visible = false;
                }
                _Form.Update();
                _Form.Enabled = true;
            }
        }
    }
}
