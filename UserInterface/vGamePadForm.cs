using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Security.Permissions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace vGamePad
{
    /// <summary>
    /// ゲームパッドボタンクラス
    /// </summary>
    public class vButton
    {
        /// <summary>
        /// ボタンの半径
        /// </summary>
        protected int radius = 24;

        /// <summary>
        /// ボタンイメージ(オフ)
        /// </summary>
        public Image m_image_off { get; set; }

        /// <summary>
        /// ボタンイメージ(オン)
        /// </summary>
        public Image m_image_on { get; set; }

        /// <summary>
        /// 中心座標
        /// </summary>
        public Point m_point { get; set; }

        /// <summary>
        /// ポインターID
        /// </summary>
        public uint m_id { get; set; }

        /// <summary>
        /// サウンドストリーム
        /// </summary>
        private System.IO.Stream m_pushSound { get; set; }

        /// <summary>
        /// プレイヤーオブジェクト
        /// </summary>
        protected System.Media.SoundPlayer m_player = null;

        /// <summary>
        /// 音を鳴らすかのフラグ
        /// </summary>
        public bool m_soundState { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public vButton()
        {
            m_id = uint.MaxValue;                           // とりあえずあり得ない値にしておく
            m_pushSound = Properties.Resources._Sound_01;   // 音の選択機能はなしで...
            m_player = new SoundPlayer(m_pushSound);        // とりあえずボタン１つに１つのプレイヤーオブジェクト
            m_soundState = false;                           // デフォルトは音を鳴らさない
        }

        /// <summary>
        /// ヒットテスト
        /// </summary>
        /// <param name="now">テスト対象座標</param>
        /// <returns>ヒットした場合、trueを返す</returns>
        public bool hitTest(Point now)
        {
            // とりあえず中心座標からの距離で判断
            if ((m_point.X - now.X) * (m_point.X - now.X) + (m_point.Y - now.Y) * (m_point.Y - now.Y) <= ((radius + 18) * (radius + 18)))
            {
                if (m_soundState)
                {
                    m_player.Play();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// ボタンイメージの描画
        /// </summary>
        /// <param name="e">ペイントイベント</param>
        public virtual void drawImage(PaintEventArgs e)
        {
            if (this.m_id == uint.MaxValue)
            {
                e.Graphics.DrawImage(m_image_off, m_point.X - radius, m_point.Y - radius);
            }
            else
            {
                e.Graphics.DrawImage(m_image_on, m_point.X - radius, m_point.Y - radius);
            }
        }
    }

    /// <summary>
    /// アナログスティック
    /// </summary>
    public class vStick : vButton
    {
        public const int scope = 50;

        public Point m_home { get; set; }

        public vStick()
        {
            radius = 32;
        }

        public void SetScope(Point point)
        {
            // 座標がスコープ内か？の判断
            //if ((m_home.X - point.X) * (m_home.X - point.X) + (m_home.Y - point.Y) * (m_home.Y - point.Y) <= (scope * scope))
            //{
            //    m_point = point;
            //}
            //else
            //{
            //    // はみ出た場合、スコープ内座標に丸める
            //    // 中心点とタッチ座標のラジアン角を求める
            //    double rad = Math.Atan2((double)(m_home.Y - point.Y), (double)(m_home.X - point.X));
            //    double x = (double)scope * Math.Cos(rad);
            //    double y = (double)scope * Math.Sin(rad);
            //    m_point = new Point(m_home.X - (int)x, m_home.Y - (int)y);
            //}
            int x = point.X;
            int y = point.Y;
            if ((m_home.X - scope) >= point.X)
            {
                x = m_home.X - scope;
            }
            else if ((m_home.X + scope) <= point.X)
            {
                x = m_home.X + scope;
            }
            if ((m_home.Y - scope) >= point.Y)
            {
                y = m_home.Y - scope;
            }
            else if ((m_home.Y + scope) <= point.Y)
            {
                y = m_home.Y + scope;
            }
            m_point = new Point(x, y);
        }

        public long GetAxisY()
        {
            return ((m_home.Y + scope - m_point.Y) * 100) / ((scope - 5) * 2);
        }

        public long GetAxisX()
        {
            return ((m_home.X + scope - m_point.X) * 100) / ((scope - 5) * 2);
        }

        public void SetHomePosition()
        {
            m_point = m_home;
        }
    }

    /// <summary>
    /// 連射ボタン
    /// </summary>
    public class vBarrageButton : vButton
    {
        /// <summary>
        /// 連射モード時のボタン画像
        /// </summary>
        public Image m_image_Barrage { set; get; }

        /// <summary>
        /// 連射モードフラグ
        /// </summary>
        public bool m_bBarrageOn { set; get; }

        /// <summary>
        /// 仮想ゲームパッド
        /// </summary>
        protected DeviceControl m_devCon;

        /// <summary>
        /// ボタンID
        /// </summary>
        protected uint m_buttonId;

        /// <summary>
        /// フォーム
        /// </summary>
        protected Form m_form;

        /// <summary>
        /// 連射タイマー
        /// </summary>
        protected System.Windows.Forms.Timer m_timer;

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="devCon">vJoyオブジェクト</param>
        /// <param name="form">メインフォーム</param>
        /// <param name="id">ボタン識別ID</param>
        public vBarrageButton(ref DeviceControl devCon, Form form, uint id)
        {
            m_devCon = devCon;
            m_form = form;
            m_buttonId = id;
            m_timer = new System.Windows.Forms.Timer();
        }

        /// <summary>
        /// タッチパネルボタンON処理
        /// </summary>
        /// <param name="now">座標</param>
        /// <returns>true:ヒットした false:ヒットしなかった</returns>
        public bool PointerDown(Point now)
        {
            bool bRet = base.hitTest(now);
            if (bRet)
            {
                if (m_timer.Enabled)
                {
                    m_timer.Tick -= new EventHandler(OnTimerEvent);
                    m_timer.Enabled = false;
                    m_bBarrageOn = false;
                }
                else
                {
                    m_timer.Interval = 2000; // 2秒
                    m_timer.Tick += new EventHandler(OnTimerEvent);
                    m_timer.Enabled = true;
                }
            }
            return bRet;
        }

        /// <summary>
        /// 
        /// </summary>
        public void PointerUp()
        {
            if (m_timer.Enabled && !m_bBarrageOn)
            {
                m_timer.Tick -= new EventHandler(OnTimerEvent);
                m_timer.Enabled = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnTimerEvent(object sender, EventArgs e)
        {
            // 連射モードON
            if (!m_bBarrageOn)
            {
                m_bBarrageOn = true;
                m_form.Invalidate();
                if (m_soundState)
                {
                    m_player.PlaySync();
                    m_player.Play();

                }
            }
            m_timer.Interval = 100; // 0.1秒
            m_devCon.PushButton(m_buttonId);
            m_devCon.FreeButton(m_buttonId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public override void drawImage(PaintEventArgs e)
        {
            if (m_bBarrageOn)
            {
                e.Graphics.DrawImage(m_image_Barrage, m_point.X - radius, m_point.Y - radius);
            }
            else
            {
                base.drawImage(e);
            }
        }
    }

    public partial class vGamePadForm : Form
    {
        public static vGamePadForm _Form = null;

        private const int WS_EX_NOACTIVATE = 0x8000000;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x202;

        private const int WM_POINTERUPDATE = 0x0245;
        private const int WM_POINTERDOWN = 0x0246;
        private const int WM_POINTERUP = 0x0247;

        private const int WM_DISPLAYCHANGE = 0x007E;                // 画面回転に対応する

        private static int GET_X_LPARAM(IntPtr lParam)
        {
            return (int)lParam & 0xFFFF;
        }
        private static int GET_Y_LPARAM(IntPtr lParam)
        {
            return (int)lParam >> 16;
        }
        private static uint GET_POINTERID_WPARAM(IntPtr wParam)
        {
            return (uint)wParam & 0xFFFF;
        }

        private vButton[] m_buttonArray = new vButton[8];
        private vButton[] m_crossArray = new vButton[4];
        private vStick[] m_stickArray = new vStick[2];
        private vBarrageButton[] m_barrageArray = new vBarrageButton[4];
        private vButton m_soundState;
        private vButton m_softKeyboard;
        //private vButton m_dqxMove;
        private string m_cmdLine = null;

        private DeviceControl m_devCon;

        private Button Button1;
        private Button Button2;

        private GraphicsPath m_path;

        private InformationForm m_InfotmationForm = null;
        private ConfigrationForm m_ConfigForm = null;

        public vGamePadForm()
        {
            InitializeComponent();
        }

        public vGamePadForm(ref DeviceControl devCon)
        {
            InitializeComponent();
            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int baseWidth = this.ClientSize.Width; // 786

            m_devCon = devCon;
            m_cmdLine = Environment.ExpandEnvironmentVariables("%ProgramFiles%") + "\\Common Files\\microsoft shared\\ink\\TabTip.exe";

            m_barrageArray[0] = new vBarrageButton(ref devCon, this, 0);
            m_barrageArray[0].m_image_off = Properties.Resources._01_0;
            m_barrageArray[0].m_image_on = Properties.Resources._01_1;
            m_barrageArray[0].m_image_Barrage = Properties.Resources._01_2;

            m_barrageArray[1] = new vBarrageButton(ref devCon, this, 1);
            m_barrageArray[1].m_image_off = Properties.Resources._02_0;
            m_barrageArray[1].m_image_on = Properties.Resources._02_1;
            m_barrageArray[1].m_image_Barrage = Properties.Resources._02_2;

            m_barrageArray[2] = new vBarrageButton(ref devCon, this, 2);
            m_barrageArray[2].m_image_off = Properties.Resources._03_0;
            m_barrageArray[2].m_image_on = Properties.Resources._03_1;
            m_barrageArray[2].m_image_Barrage = Properties.Resources._03_2;

            m_barrageArray[3] = new vBarrageButton(ref devCon, this, 3);
            m_barrageArray[3].m_image_off = Properties.Resources._04_0;
            m_barrageArray[3].m_image_on = Properties.Resources._04_1;
            m_barrageArray[3].m_image_Barrage = Properties.Resources._04_2;

            m_buttonArray[0] = new vButton();
            m_buttonArray[0].m_image_off = Properties.Resources._05_0;
            m_buttonArray[0].m_image_on = Properties.Resources._05_1;

            m_buttonArray[1] = new vButton();
            m_buttonArray[1].m_image_off = Properties.Resources._06_0;
            m_buttonArray[1].m_image_on = Properties.Resources._06_1;

            m_buttonArray[2] = new vButton();
            m_buttonArray[2].m_image_off = Properties.Resources._07_0;
            m_buttonArray[2].m_image_on = Properties.Resources._07_1;

            m_buttonArray[3] = new vButton();
            m_buttonArray[3].m_image_off = Properties.Resources._08_0;
            m_buttonArray[3].m_image_on = Properties.Resources._08_1;

            m_buttonArray[4] = new vButton();
            m_buttonArray[4].m_image_off = Properties.Resources._09_0;
            m_buttonArray[4].m_image_on = Properties.Resources._09_1;

            m_buttonArray[5] = new vButton();
            m_buttonArray[5].m_image_off = Properties.Resources._10_0;
            m_buttonArray[5].m_image_on = Properties.Resources._10_1;

            m_buttonArray[6] = new vButton();
            m_buttonArray[6].m_image_off = Properties.Resources._11_0;
            m_buttonArray[6].m_image_on = Properties.Resources._11_1;

            m_buttonArray[7] = new vButton();
            m_buttonArray[7].m_image_off = Properties.Resources._12_0;
            m_buttonArray[7].m_image_on = Properties.Resources._12_1;

            m_crossArray[0] = new vButton();
            m_crossArray[0].m_image_off = Properties.Resources.up_0;
            m_crossArray[0].m_image_on = Properties.Resources.up_1;

            m_crossArray[1] = new vButton();
            m_crossArray[1].m_image_off = Properties.Resources.right_0;
            m_crossArray[1].m_image_on = Properties.Resources.right_1;

            m_crossArray[2] = new vButton();
            m_crossArray[2].m_image_off = Properties.Resources.down_0;
            m_crossArray[2].m_image_on = Properties.Resources.down_1;

            m_crossArray[3] = new vButton();
            m_crossArray[3].m_image_off = Properties.Resources.left_0;
            m_crossArray[3].m_image_on = Properties.Resources.left_1;

            m_stickArray[0] = new vStick();
            m_stickArray[0].m_image_off = Properties.Resources.stick_b_0;
            m_stickArray[0].m_image_on = Properties.Resources.stick_b_1;

            m_stickArray[1] = new vStick();
            m_stickArray[1].m_image_off = Properties.Resources.stick_b_0;
            m_stickArray[1].m_image_on = Properties.Resources.stick_b_1;

            m_soundState = new vButton();
            m_soundState.m_image_off = Properties.Resources.Sound_OFF;
            m_soundState.m_image_on = Properties.Resources.Sound_ON;
            m_soundState.m_soundState = true;

            m_softKeyboard = new vButton();
            m_softKeyboard.m_image_off = Properties.Resources.Keyboard;
            m_softKeyboard.m_image_on = Properties.Resources.Keybord_ON;

            //m_dqxMove = new vButton();
            //m_dqxMove.m_image_off = Properties.Resources.DQX_0;
            //m_dqxMove.m_image_on = Properties.Resources.DQX_1;
            //m_dqxMove.m_soundState = true;

            m_devCon.MoveStick(0, 50, 50);
            m_devCon.MoveStick(1, 50, 50);

            this.Button1 = new System.Windows.Forms.Button();
            this.Button1.Name = "button1";
            this.Button1.Size = new System.Drawing.Size(44, 22);
            this.Button1.Text = "";
            this.Button1.BackColor = System.Drawing.Color.Red;

            this.Button2 = new System.Windows.Forms.Button();
            this.Button2.Name = "button2";
            this.Button2.Size = new System.Drawing.Size(22, 22);
            this.Button2.Text = "";
            this.Button2.BackColor = System.Drawing.Color.Red;

            this.Button1.MouseDown += Button1_MouseDown;
            this.Button1.MouseMove += Button1_MouseMove;
            this.Button1.MouseUp += Button1_MouseUp;
            this.Button2.Click += Button2_Click;

            OnDisplayChange(Screen.PrimaryScreen.Bounds.Width, 0);

            SetRegion(Properties.Settings.Default.Skeleton);

            this.Controls.Add(this.Button1);
            this.Controls.Add(this.Button2);

            m_ConfigForm = new ConfigrationForm();
            m_ConfigForm.Show();

            m_InfotmationForm = new InformationForm();

            _Form = this;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("仮想ゲームパッド(vGamePad)を終了しますか？", "vGamePadメッセージ", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                if (m_ConfigForm != null)
                {
                    m_ConfigForm.Close();
                }
                if (m_InfotmationForm != null)
                {
                    m_InfotmationForm.Close();
                }
                this.Close();
            }
        }

        private Point mousePoint;

        private void Button1_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                mousePoint = new Point(e.X, e.Y);
                this.BackColor = System.Drawing.Color.White;
                if ( Properties.Settings.Default.Skeleton )
                {
                    this.Region = null;
                }
            }
        }

        private void Button1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Top += e.Y - mousePoint.Y;
            }
        }

        void Button1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if ( Screen.PrimaryScreen.Bounds.Bottom < this.Top + this.Height )
                {
                    this.Top = Screen.PrimaryScreen.Bounds.Bottom - this.Height;
                }
                mousePoint = new Point(e.X, e.Y);
                this.BackColor = System.Drawing.Color.DarkGray;
                if (Properties.Settings.Default.Skeleton)
                {
                    this.Region = new Region(m_path);
                }
            }
        }

        protected void OnDisplayChange(int horizontal, int vertical)
        {
            this.Width = horizontal;
            int baseWidth = this.ClientSize.Width; // 786
            m_barrageArray[0].m_point = new Point(baseWidth - 100, 120);
            m_barrageArray[1].m_point = new Point(baseWidth - 50, 170);
            m_barrageArray[2].m_point = new Point(baseWidth - 100, 220);
            m_barrageArray[3].m_point = new Point(baseWidth - 150, 170);

            m_buttonArray[0].m_point = new Point(baseWidth - 150, 50);
            m_buttonArray[1].m_point = new Point(baseWidth - 80, 50);
            m_buttonArray[2].m_point = new Point(220, 50);
            m_buttonArray[3].m_point = new Point(baseWidth - 220, 50);
            m_buttonArray[4].m_point = new Point(-450, -50);
            m_buttonArray[5].m_point = new Point(210, 240);
            m_buttonArray[6].m_point = new Point(-550, -50);
            m_buttonArray[7].m_point = new Point(baseWidth - 210, 240);

            m_crossArray[0].m_point = new Point(100, 100);
            m_crossArray[1].m_point = new Point(150, 150);
            m_crossArray[2].m_point = new Point(100, 200);
            m_crossArray[3].m_point = new Point(50, 150);

            m_stickArray[0].m_point = new Point(300, 150);
            m_stickArray[0].m_home = new Point(300, 150);
            m_stickArray[1].m_point = new Point(baseWidth - 300, 150);
            m_stickArray[1].m_home = new Point(baseWidth - 300, 150);

            m_soundState.m_point = new Point(baseWidth / 2 + 40, 240);
            m_softKeyboard.m_point = new Point(baseWidth / 2 - 40, 240);
            //m_dqxMove.m_point = new Point(baseWidth / 2, 50);

            this.Button1.Location = new System.Drawing.Point(2, 2);
            this.Button2.Location = new System.Drawing.Point(baseWidth - 2 - 22, 2);

            if (m_ConfigForm != null)
            {
                m_ConfigForm.SetPostion(horizontal,vertical);
            }
            if (m_InfotmationForm != null)
            {
                m_InfotmationForm.SetPostion(horizontal, vertical);
            }

            m_path = new GraphicsPath();

            m_path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
            m_path.AddRectangle(new Rectangle(0, 0, 48, 26));
            m_path.AddRectangle(new Rectangle(this.Width - 26, 0, 26, 26));
            m_path.AddEllipse(new Rectangle(baseWidth - 100 - 33, 120 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 50 - 33, 170 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 100 - 33, 220 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 150 - 33, 170 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 150 - 33, 50 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 80 - 33, 50 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(220 - 33, 50 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 220 - 33, 50 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(210 - 33, 240 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth - 210 - 33, 240 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(100 - 33, 100 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(150 - 33, 150 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(100 - 33, 200 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(50 - 33, 150 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth / 2 - 40 - 33, 240 - 33, 64, 64));
            m_path.AddEllipse(new Rectangle(baseWidth / 2 + 40 - 33, 240 - 33, 64, 64));

            m_path.AddPie(300 - (32 + 50), 150 - (32 + 50), 32 * 2, 32 * 2, 180, 90);
            m_path.AddPie(300 - (32 + 50), 150 + (50 - 32), 32 * 2, 32 * 2, 90, 90);
            m_path.AddPie(300 + 50 - 32, 150 - (32 + 50), 32 * 2, 32 * 2, 270, 90);
            m_path.AddPie(300 + 50 - 32, 150 + (50 - 32), 32 * 2, 32 * 2, 0, 90);
            m_path.AddRectangle(new Rectangle(300 - (32 + 50) + 32, 150 - (32 + 50), (32 + 50) * 2 - 64, (32 + 50) * 2));
            m_path.AddRectangle(new Rectangle(300 - (32 + 50), 150 - (32 + 50) + 32, (32 + 50) * 2, (32 + 50) * 2 - 64));

            m_path.AddPie(baseWidth - 300 - (32 + 50), 150 - (32 + 50), 32 * 2, 32 * 2, 180, 90);
            m_path.AddPie(baseWidth - 300 - (32 + 50), 150 + (50 - 32), 32 * 2, 32 * 2, 90, 90);
            m_path.AddPie(baseWidth - 300 + 50 - 32, 150 - (32 + 50), 32 * 2, 32 * 2, 270, 90);
            m_path.AddPie(baseWidth - 300 + 50 - 32, 150 + (50 - 32), 32 * 2, 32 * 2, 0, 90);
            m_path.AddRectangle(new Rectangle(baseWidth - 300 - (32 + 50) + 32, 150 - (32 + 50), (32 + 50) * 2 - 64, (32 + 50) * 2));
            m_path.AddRectangle(new Rectangle(baseWidth - 300 - (32 + 50), 150 - (32 + 50) + 32, (32 + 50) * 2, (32 + 50) * 2 - 64));
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                //case WM_LBUTTONDOWN:        // 本来は WM_POINTERDOWN メッセージを処理する
                case WM_POINTERDOWN:
                    // POINTERDOWNはスクリーン座標
                    Point pointer = this.PointToClient(new Point(GET_X_LPARAM(m.LParam), GET_Y_LPARAM(m.LParam)));
                    // アナログスティックの判定
                    for (uint i = 0; i < m_stickArray.Length; i++)
                    {
                        // ヒットした場合、GET_POINTERID_WPARAMでポインターIDを保存する
                        if (m_stickArray[i].hitTest(pointer))
                        {
                            m_stickArray[i].m_id = GET_POINTERID_WPARAM(m.WParam);
                            /*
                             * m_stickArray[i].MoveStick();
                             */
                            m_devCon.MoveStick(
                                i,
                                (long)m_stickArray[i].GetAxisX(),
                                (long)m_stickArray[i].GetAxisY());
                            break;
                        }
                    }
                    // 1～4のボタン判定
                    for (uint i = 0; i < m_barrageArray.Length; i++)
                    {
                        if(m_barrageArray[i].PointerDown(pointer))
                        {
                            m_barrageArray[i].m_id = GET_POINTERID_WPARAM(m.WParam);
                            this.m_devCon.PushButton(i);
                            break;
                        }
                    }
                    // 5～12のボタン判定
                    for (uint i = 0; i < m_buttonArray.Length; i++)
                    {
                        // ヒットした場合、GET_POINTERID_WPARAMでポインターIDを保存する
                        if (m_buttonArray[i].hitTest(pointer))
                        {
                            m_buttonArray[i].m_id = GET_POINTERID_WPARAM(m.WParam);
                            /* m_buttonArray[i].PushButton();*/
                            // ボタンON
                            this.m_devCon.PushButton(i+4);
                            break;
                        }
                    }
                    // ＋キーボタンの判定
                    for (uint i = 0; i < m_crossArray.Length; i++)
                    {
                        // ヒットした場合、GET_POINTERID_WPARAMでポインターIDを保存する
                        if (m_crossArray[i].hitTest(pointer))
                        {
                            m_crossArray[i].m_id = GET_POINTERID_WPARAM(m.WParam);
                            /* m_crossArray[i].PushCross();*/
                            // ボタンON
                            this.m_devCon.PushCross(i);
                            break;
                        }
                    }

                    // SoundON/OFFボタンの判定
                    if (m_soundState.hitTest(pointer))
                    {
                        //bool soundState;
                        //if (m_soundState.m_id == 1)
                        //{
                        //    m_soundState.m_id = uint.MaxValue;
                        //    soundState = false;
                        //}
                        //else
                        //{
                        //    m_soundState.m_id = 1;
                        //    soundState = true;
                        //}
                        //for (uint i = 0; i < m_buttonArray.Length; i++)
                        //{
                        //    m_buttonArray[i].m_soundState = soundState;
                        //}
                        //for (uint i = 0; i < m_stickArray.Length; i++)
                        //{
                        //    m_stickArray[i].m_soundState = soundState;
                        //}
                        //for (uint i = 0; i < m_barrageArray.Length; i++)
                        //{
                        //    m_barrageArray[i].m_soundState = soundState;
                        //}
                        //for (uint i = 0; i < m_crossArray.Length; i++)
                        //{
                        //    m_crossArray[i].m_soundState = soundState;
                        //}
                    }

                    // キーボード起動
                    if (m_softKeyboard.hitTest(pointer))
                    {
                        m_softKeyboard.m_id = GET_POINTERID_WPARAM(m.WParam);
                        try
                        {
                            System.Diagnostics.Process[] ps =
                                System.Diagnostics.Process.GetProcessesByName("DQXGame");   // ドラクエ10のプロセス名を指定する
                            if (ps.Length == 1)
                            {
                                Microsoft.VisualBasic.Interaction.AppActivate(ps[0].Id);
                            }
                            ps = System.Diagnostics.Process.GetProcessesByName("DQXLauncher");   // ドラクエ10のプロセス名を指定する                     
                            if (ps.Length == 1)
                            {
                                Microsoft.VisualBasic.Interaction.AppActivate(ps[0].Id);
                            }
                        }
                        catch
                        {

                        }
                        System.Diagnostics.Process hProc = null;
                        // C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe
                        hProc = System.Diagnostics.Process.Start(m_cmdLine);
                        hProc.Close();
                    }

                    // DQX移動調整ボタン
                    //if (m_dqxMove.hitTest(pointer))
                    //{
                    //    m_dqxMove.m_id = GET_POINTERID_WPARAM(m.WParam);
                    //    Thread thread = new Thread(new ThreadStart(SetDQXWindowPos));
                    //    thread.Start();
                    //}

                    this.Invalidate();
                    break;
                //              case WM_LBUTTONUP:          // 本来は WM_POINTERUP メッセージを処理する
                case WM_POINTERUP:
                    // ポインターIDを取得
                    uint id = GET_POINTERID_WPARAM(m.WParam);
                    // アナログスティックの判定
                    for (uint i = 0; i < m_stickArray.Length; i++)
                    {
                        if (m_stickArray[i].m_id == id)
                        {
                            m_stickArray[i].m_id = uint.MaxValue;
                            m_stickArray[i].SetHomePosition();
                            m_devCon.MoveStick(
                                i,
                                (long)m_stickArray[i].GetAxisX(),
                                (long)m_stickArray[i].GetAxisY());
                            break;
                        }
                    }

                    // 1～12のボタンIDをチェック
                    for (uint i = 0; i < m_barrageArray.Length; i++ )
                    {
                        if (m_barrageArray[i].m_id == id)
                        {
                            m_barrageArray[i].m_id = uint.MaxValue;
                            m_barrageArray[i].PointerUp();
                            this.m_devCon.FreeButton(i);
                            break;
                        }
                    }
                    for (uint i = 0; i < m_buttonArray.Length; i++)
                    {
                        if (m_buttonArray[i].m_id == id)
                        {
                            m_buttonArray[i].m_id = uint.MaxValue;
                            // ボタンOFF
                            this.m_devCon.FreeButton(i + 4);
                            break;
                        }
                    }
                    // ＋キーボタンの判定
                    for (uint i = 0; i < m_crossArray.Length; i++)
                    {
                        // ヒットした場合、GET_POINTERIDでポインターIDを保存する
                        if (m_crossArray[i].m_id == id)
                        {
                            m_devCon.FreeCross(i);
                            m_crossArray[i].m_id = uint.MaxValue;
                            break;
                        }
                    }

                    // キーボード起動
                    if (m_softKeyboard.m_id == id)
                    {
                        m_softKeyboard.m_id = uint.MaxValue;
                    }

                    // ドラクエ10画面移動結果
                    //if (m_dqxMove.m_id == id)
                    //{
                    //    m_dqxMove.m_id = uint.MaxValue;
                    //}

                    this.Invalidate();
                    break;
                //              case WM_MOUSEMOVE:          // 本来は WM_POINTERUPDATE メッセージを処理する
                case WM_POINTERUPDATE:
                    // ポインターIDを取得
                    id = GET_POINTERID_WPARAM(m.WParam);
                    for (uint i = 0; i < m_stickArray.Length; i++)
                    {
                        if (m_stickArray[i].m_id == id)
                        {
                            pointer = this.PointToClient(new Point(GET_X_LPARAM(m.LParam), GET_Y_LPARAM(m.LParam)));
                            m_stickArray[i].SetScope(pointer);
                            m_devCon.MoveStick(
                                i,
                                (long)m_stickArray[i].GetAxisX(),
                                (long)m_stickArray[i].GetAxisY());
                            break;
                        }
                    }
                    this.Invalidate();
                    break;
                case WM_DISPLAYCHANGE:
                    // 縦横の確認を行う
                    // 画面リサイズに伴うボタン配置の見直し
                    OnDisplayChange(GET_X_LPARAM(m.LParam), GET_Y_LPARAM(m.LParam));
                    if ( Properties.Settings.Default.Skeleton )
                    {
                        this.Region = new Region(this.m_path);
                    }
                    // ウィンドウ内に収まらない場合の確認
                    // 基本的に左下の座標が確実に入るように設定する
                    if (GET_Y_LPARAM(m.LParam) < this.Top + this.Height)
                    {
                        this.Top = GET_Y_LPARAM(m.LParam) - this.Height;
                    }
                    this.Invalidate();
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 1～12のボタン表示
            for (int i = 0; i < m_barrageArray.Length; i++)
            {
                m_barrageArray[i].drawImage(e);
            }
            for (int i = 0; i < m_buttonArray.Length; i++)
            {
                m_buttonArray[i].drawImage(e);
            }

            // ＋字キーの表示
            for (int i = 0; i < m_crossArray.Length; i++)
            {
                m_crossArray[i].drawImage(e);
            }

            // アナログスティックの表示
            for (int i = 0; i < m_stickArray.Length; i++)
            {
                m_stickArray[i].drawImage(e);
            }

            // サウンドON/OFFの表示
            m_soundState.drawImage(e);

            // キーボードON
            m_softKeyboard.drawImage(e);

            // ドラクエ10 画面調整
            //m_dqxMove.drawImage(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                if (!base.DesignMode)
                {
                    p.ExStyle = p.ExStyle | (WS_EX_NOACTIVATE);
                }
                return p;
            }
        }

        public void SetRegion(bool value)
        {
            if (value)
            {
                this.Region = new Region(m_path);
            }
            else
            {
                this.Region = null;
            }
        }

        public void SetSoundStatus(bool value)
        {
            for (uint i = 0; i < m_buttonArray.Length; i++)
            {
                m_buttonArray[i].m_soundState = value;
            }
            for (uint i = 0; i < m_stickArray.Length; i++)
            {
                m_stickArray[i].m_soundState = value;
            }
            for (uint i = 0; i < m_barrageArray.Length; i++)
            {
                m_barrageArray[i].m_soundState = value;
            }
            for (uint i = 0; i < m_crossArray.Length; i++)
            {
                m_crossArray[i].m_soundState = value;
            }
            m_softKeyboard.m_soundState = value;
        }

        static public void ChangeSetting(string SettingName, bool value)
        {
            if (_Form != null)
            {
                if ( SettingName == "Skeleton" )
                {
                    _Form.SetRegion(value);
                }
                else if ( SettingName == "Sound")
                {
                    _Form.SetSoundStatus(value);
                }
            }
        }
    }
}
