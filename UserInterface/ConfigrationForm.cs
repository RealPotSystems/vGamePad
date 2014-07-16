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
    public partial class ConfigrationForm : Form
    {
        /// <summary>
        /// 表示モード
        /// </summary>
        private bool m_fullMode = false;

        /// <summary>
        /// 切り取り線
        /// </summary>
        private GraphicsPath m_path = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ConfigrationForm()
        {
            InitializeComponent();
            // label8
            //自分自身のバージョン情報を取得する
            System.Diagnostics.FileVersionInfo ver =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(
                System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.label8.Text = ver.ProductName + " version " + ver.ProductVersion + "\nCopyright © 2014 Real Pot Systems (TAKUBON). All right reserved.";

            // バッテリーの有無でラベルのdisableにする
            // バッテリーの有無
            if ((SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) == BatteryChargeStatus.NoSystemBattery)
            {
                this._Battery.Enabled = false;
                this.label2.Enabled = false;
            }
            // パスを設定して、右上のボタンだけにする
            m_path = new GraphicsPath();
            m_path.FillMode = FillMode.Winding;
            m_path.AddRectangle(new Rectangle(this.Width - 26, 0, 26, 26));
            this.Region = new Region(m_path);
            this.SetPostion(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.WorkingArea.Top);
            SetButtonText();
        }

        /// <summary>
        /// 表示位置決定
        /// </summary>
        /// <returns>表示位置</returns>
        public void SetPostion(int x, int y)
        {
            // 全体 - 幅
            this.Location = new Point(x - this.Width, Screen.PrimaryScreen.WorkingArea.Top);
        }

        public void SetButtonText()
        {
            if (Properties.Settings.Default.Skeleton)
            {
                this._Region.Text = "\uE0A2";
            }
            else
            {
                this._Region.Text = "\uE003";
            }
            if (Properties.Settings.Default.Battery)
            {
                this._Battery.Text = "\uE0A2";
            }
            else
            {
                this._Battery.Text = "\uE003";
            }
            if (Properties.Settings.Default.AstClock)
            {
                this.S.Text = "\uE0A2";
            }
            else
            {
                this.S.Text = "\uE003";
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_fullMode)
            {
                this.Region = new Region(m_path);
                m_fullMode = false;
            }
            else
            {
                this.Region = null;
                m_fullMode = true;
            }
        }

        private void label5_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Skeleton)
            {
                this._Region.Text = "\uE003";
                Properties.Settings.Default.Skeleton = false;
            }
            else
            {
                this._Region.Text = "\uE0A2";
                Properties.Settings.Default.Skeleton = true;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Battery)
            {
                this._Battery.Text = "\uE003";
                Properties.Settings.Default.Battery = false;
            }
            else
            {
                this._Battery.Text = "\uE0A2";
                Properties.Settings.Default.Battery = true;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AstClock)
            {
                this.S.Text = "\uE003";
                Properties.Settings.Default.AstClock = false;
            }
            else
            {
                this.S.Text = "\uE0A2";
                Properties.Settings.Default.AstClock = true;
            }
        }
    }
}
