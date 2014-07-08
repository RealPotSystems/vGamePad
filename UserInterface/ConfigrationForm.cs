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

        private bool m_region = false;
        private bool m_battery = false;
        private bool m_clock = false;

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
            // パスを設定して、右上のボタンだけにする
            m_path = new GraphicsPath();
            m_path.FillMode = FillMode.Winding;
            m_path.AddRectangle(new Rectangle(this.Width - 26, 0, 26, 26));
            this.Region = new Region(m_path);
            this.Location = SetPostion();
            SetButtonText();
        }

        /// <summary>
        /// 表示位置決定
        /// </summary>
        /// <returns>表示位置</returns>
        public Point SetPostion()
        {
            // 全体 - 幅
            int x = Screen.PrimaryScreen.Bounds.Width - this.Width;
            int y = Screen.PrimaryScreen.WorkingArea.Top;
            return new Point(x, y);
        }

        public void SetButtonText()
        {
            if (m_region)
            {
                this.label5.Text = "\uE0A2";
            }
            else
            {
                this.label5.Text = "\uE003";
            }
            if (m_battery)
            {
                this.label6.Text = "\uE0A2";
            }
            else
            {
                this.label6.Text = "\uE003";
            }
            if (m_clock)
            {
                this.label7.Text = "\uE0A2";
            }
            else
            {
                this.label7.Text = "\uE003";
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
            if ( m_region )
            {
                m_region = false;
                this.label5.Text = "\uE003";
            }
            else
            {
                m_region = true;
                this.label5.Text = "\uE0A2";
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (m_battery)
            {
                m_battery = false;
                this.label6.Text = "\uE003";
            }
            else
            {
                m_battery = true;
                this.label6.Text = "\uE0A2";
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {
            if (m_clock)
            {
                m_clock = false;
                this.label7.Text = "\uE003";
            }
            else
            {
                m_clock = true;
                this.label7.Text = "\uE0A2";
            }
        }
    }
}
