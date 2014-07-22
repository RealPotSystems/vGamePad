using System;
using vJoyInterfaceWrap;

namespace vGamePad
{
    /// <summary>
    /// DQXに特化してvJoyの機能をまとめるI/Fも実装する
    /// </summary>
    public class DeviceControl : IDisposable
    {
        /// <summary>
        /// ＋字キーの状態
        /// </summary>
        public enum Pov
        {
            NORTH   = 0,    // ↑
            EAST    = 1,    // →
            SOUTH   = 2,    // ↓
            WEST    = 3,    // ←
            NEUTRAL = -1,   // ニュートラル
        }

        /// <summary>
        /// vJoyオブジェクト
        /// </summary>
        static private vJoy joystick = new vJoy();

        /// <summary>
        /// デバイスIDは1固定
        /// </summary>
        static private UInt32 rID = 1;

        /// <summary>
        /// 軸の最大値
        /// </summary>
        private long m_nAxisMax = long.MaxValue;

        /// <summary>
        /// 軸の最小値
        /// </summary>
        private long m_nAxisMin = long.MinValue;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DeviceControl( UInt32 id = 1 )
        {
            rID = id;   // rIDをコンストラクタで変更は可能
        }

        /// <summary>
        /// vJoyの初期化
        /// </summary>
        /// <returns>true</returns>
        /// <returns>false</returns>
        public bool Initialize()
        {
            // vJoyドライバーのテスト
            // 失敗した場合、falseを返す
            bool result = joystick.vJoyEnabled();
            if ( result == false )
            {
                return false;
            }
            // vJoy仮想デバイスのテスト
            VjdStat status = joystick.GetVJDStatus(rID);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:  // すでに他プログラムがvJoy仮想デバイスを占有
                case VjdStat.VJD_STAT_BUSY: // 〃
                case VjdStat.VJD_STAT_MISS: // インストールされていない or 無効な状態
                    return false;
                case VjdStat.VJD_STAT_FREE: // 利用可能
                    break;
            }

            // vJoy仮想デバイスの設定値確認
            // 今回、仕様としては、
            //    12ボタン
            //    ＋字キー
            //    アナログスティック ２本
            // としています
            // ※将来的には自動的に設定するよう変更

            // ボタン数のチェック
            Int32 nBtn = joystick.GetVJDButtonNumber(rID);
            if (nBtn != 12)
            {
                return false;
            }
            // ＋字キーの有無チェック
            Int32 nDPov = joystick.GetVJDDiscPovNumber(rID);
            if (nDPov != 1)
            {
                return false;
            }
            Int32 nCPov = joystick.GetVJDContPovNumber(rID);
            if (nCPov != 0)
            {
                return false;
            }
            // アナログスティックのチェック
            // 全ての機能の有無はチェックせず必要条件のみ確認
            // 左アナログスティック←→
            result = joystick.GetVJDAxisExist(rID, HID_USAGES.HID_USAGE_X);
            if (result == false)
            {
                return false;
            }
            // 左アナログスティック↑↓
            result = joystick.GetVJDAxisExist(rID, HID_USAGES.HID_USAGE_Y);
            if (result == false)
            {
                return false;
            }
            // 右アナログスティック←→
            result = joystick.GetVJDAxisExist(rID, HID_USAGES.HID_USAGE_Z);
            if (result == false)
            {
                return false;
            }
            // 右アナログスティック↑↓
            result = joystick.GetVJDAxisExist(rID, HID_USAGES.HID_USAGE_RZ);
            if (result == false)
            {
                return false;
            }

            joystick.GetVJDAxisMax(rID, HID_USAGES.HID_USAGE_X, ref m_nAxisMax);
            joystick.GetVJDAxisMin(rID, HID_USAGES.HID_USAGE_X, ref m_nAxisMin);

            // 接続
            joystick.AcquireVJD(rID);

            joystick.ResetVJD(rID);

            return true;
        }

        /// <summary>
        /// ボタンON
        /// </summary>
        /// <param name="n">対象のボタンID</param>
        public void PushButton(uint n)   // 0 ~ 11
        {
            joystick.SetBtn(true, rID, n + 1);
        }

        /// <summary>
        /// ボタンOFF
        /// </summary>
        /// <param name="n">対象のボタンID</param>
        public void FreeButton(uint n)
        {
            joystick.SetBtn(false, rID, n + 1);
        }

        /// <summary>
        /// 方向キー
        /// </summary>
        /// <param name="n">0:上 1:右 2:下 3:左</param>
        public void PushCross(uint n)    // 0 ~ 3
        {
            joystick.SetDiscPov((Int32)n, rID, 1);
//          joystick.SetDiscPov((Int32)Pov.NEUTRAL, rID, 1);
        }

        /// <summary>
        /// 方向キーのフリー
        /// </summary>
        /// <param name="n">0:上 1:右 2:下 3:左</param>
        public void FreeCross(uint n)
        {
            joystick.SetDiscPov((Int32)Pov.NEUTRAL, rID, 1);
        }

        /// <summary>
        /// アナログスティックの移動
        /// </summary>
        /// <param name="n">0:アナログスティック(左) 1:アナログスティック(右)</param>
        /// <param name="x">上下</param>
        /// <param name="y">左右</param>
        public void MoveStick(uint n, long x, long y)
        {
            HID_USAGES usageX = HID_USAGES.HID_USAGE_X;
            HID_USAGES usageY = HID_USAGES.HID_USAGE_Y;
            switch (n)
            {
                case 0:
                    usageX = HID_USAGES.HID_USAGE_X;
                    usageY = HID_USAGES.HID_USAGE_Y;
                    break;
                case 1:
                    usageX = HID_USAGES.HID_USAGE_Z;
                    usageY = HID_USAGES.HID_USAGE_RZ;
                    break;
            }
            x = 100 - x;
            y = 100 - y;
            joystick.SetAxis((int)(m_nAxisMax * x) / 100, rID, usageX);
            joystick.SetAxis((int)(m_nAxisMax * y ) / 100, rID, usageY);
        }

        /// <summary>
        /// バージョン文字列作成
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string strTemp = joystick.GetvJoyManufacturerString() + joystick.GetvJoyProductString() + joystick.GetvJoySerialNumberString();
            return strTemp;
        }

        /// <summary>
        /// 念のため実装
        /// </summary>
        public void Dispose()
        {
            joystick.RelinquishVJD(rID);
        }
    }
}
