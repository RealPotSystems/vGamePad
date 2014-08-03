using System;
using System.Drawing;
using System.Windows.Forms;
using WiimoteLib;

namespace BalanceWiiBoardPad
{
    public partial class Form1 : Form
    {
        Wiimote wm = new Wiimote();

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                this.wm.Connect();
            }
            catch (Exception)
            {
                return;
            }

            this.wm.WiimoteChanged += wm_WiimoteChanged;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                this.wm.Disconnect();
            }
            catch (Exception)
            {

            }
        }

        private void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs e)
        {
            //WiimoteStateの値を取得
            WiimoteState ws = e.WiimoteState;

            //ピクチャーボックスへ描画
            this.DrawForms(ws);

            //ラベル

            //重さ(Kg)表示
            this.label1.Text = ws.BalanceBoardState.WeightKg + "kg";
            //重心のX座標表示
            this.label2.Text = "X:" +
                ws.BalanceBoardState.CenterOfGravity.X;
            //重心のY座標表示
            this.label3.Text = "Y:" +
                ws.BalanceBoardState.CenterOfGravity.Y;
        }

        private void DrawForms(WiimoteState ws)
        {
            //pictureBox1のグラフィックスを取得
            Graphics g = this.pictureBox1.CreateGraphics();
            g.Clear(Color.Black);     //画面を黒色にクリア

            //X、Y座標の計算
            float x =
                (wm.WiimoteState.BalanceBoardState.CenterOfGravity.X
                + 20.0f) * 10;    //表示位置(X座標)を求める
            float y =
                (wm.WiimoteState.BalanceBoardState.CenterOfGravity.Y
                + 12.0f) * 10;    //表示位置(Y座標)を求める

            //赤色でマーカを描写
            g.FillEllipse(Brushes.Red, x, y, 10, 10);

            g.Dispose();  //グラフィックスを開放
        }

    }
}
