using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{

    //
    //  OPNA timer エミュレーション
    //
    //


    public class OPNATimer
    {
        private int TimerA;        // タイマーAのオーバーフロー設定値
        private double TimerAcounter;  // タイマーAのカウンター値
        private int TimerB;            // タイマーBのオーバーフロー設定値
        private double TimerBcounter;  // タイマーBのカウンター値
        private int TimerReg;       // タイマー制御レジスタ (下位4ビット+7ビット)
        private double step;

        public int StatReg { get; private set; }        // ステータスレジスタ (下位2ビット)
        public Action CsmKeyOn;

        public OPNATimer(int renderingFreq, int opnaMasterClock)
        {
            step = opnaMasterClock / 72.0 / 2.0 / (double)renderingFreq;
        }

        public void timer()
        {
            if ((TimerReg & 0x01) != 0)
            {   // TimerA 動作中
                TimerAcounter += step;
                if (TimerAcounter >= (1024 - TimerA))
                {
                    StatReg |= ((TimerReg >> 2) & 0x01);
                    TimerAcounter -= (1024 - TimerA);
                    //if ((TimerReg & 0x80) != 0) CsmKeyOn?.Invoke();
                }
            }

            if ((TimerReg & 0x02) != 0)
            {   // TimerB 動作中
                TimerBcounter += step;
                if (TimerBcounter >= TimerB)
                {
                    StatReg |= ((TimerReg >> 2) & 0x02);
                    TimerBcounter -= TimerB;
                }
            }
        }

        public void WriteReg(byte adr, byte data)
        {
            switch (adr)
            {
                // TimerA
                case 0x24:
                    TimerA &= 0x3;
                    TimerA |= (data << 2);
                    break;
                case 0x25:
                    TimerA &= 0x3fc;
                    TimerA |= (data & 3);
                    break;

                case 0x26:
                    // TimerB
                    TimerB = (256 - data) << 4;
                    break;

                case 0x27:
                    // タイマー制御レジスタ
                    TimerReg = data & 0x8F;
                    StatReg &= 0xFF - ((data >> 4) & 3);
                    break;
            }
        }

    }
}
