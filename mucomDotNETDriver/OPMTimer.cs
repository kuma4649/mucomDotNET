using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Driver
{
    //
    //  OPM timer エミュレーション
    //
    //


    public class OPMTimer : FMTimer
    {

        public OPMTimer(int renderingFreq, int opmMasterClock) : base(renderingFreq, opmMasterClock)
        {
            step = opmMasterClock / 64.0 / 1.0 / (double)renderingFreq;
        }

        public override bool WriteReg(byte adr, byte data)
        {
            switch (adr)
            {
                case 0x10:
                    TimerA &= 0x3;
                    TimerA |= (data << 2);
                    return true;
                case 0x11:
                    TimerA &= 0x3fc;
                    TimerA |= (data & 3);
                    return true;
                case 0x12:
                    // TimerB
                    TimerB = (256 - (int)data) << (10 - 6);
                    return true;
                case 0x14:
                    // タイマー制御レジスタ
                    TimerReg = data & 0x8F;
                    StatReg &= 0xFF - ((data >> 4) & 3);
                    return true;
            }
            return false;
        }

    }
}
