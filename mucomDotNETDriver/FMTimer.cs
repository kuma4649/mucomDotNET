using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Driver
{
    public class FMTimer
    {
        public int TimerA;        // タイマーAのオーバーフロー設定値
        protected double TimerAcounter;  // タイマーAのカウンター値
        public int TimerB;            // タイマーBのオーバーフロー設定値
        protected double TimerBcounter;  // タイマーBのカウンター値
        public int TimerReg;       // タイマー制御レジスタ (下位4ビット+7ビット)
        public double step;

        public int StatReg { get; set; }        // ステータスレジスタ (下位2ビット)
        public Action CsmKeyOn;

        public FMTimer(int renderingFreq, int masterClock)
        {
        }

        public virtual void timer()
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

        public virtual bool WriteReg(byte adr, byte data)
        {
            return false;
        }
    }
}
