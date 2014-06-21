using System;
using System.Windows.Forms;

namespace vGamePad
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 仮想ゲームパッドの生成
            DeviceControl devCon = new DeviceControl();

            // 利用可能か確認する
            bool ret = devCon.Initialize();
            if (ret == false)
            {
                MessageBox.Show("vJoyが利用できる環境ではありません。", "vGamePadエラー");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new vGamePadForm(ref devCon));
        }
    }
}
