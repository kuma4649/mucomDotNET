using System;
using System.Collections.Generic;
using System.Text;
using mucomDotNET.Common;

namespace mucomDotNET.Compiler
{
    public class smon
    {
        private MUCInfo mucInfo;

        public smon(MUCInfo mucInfo)
        {
            this.mucInfo = mucInfo;
        }

        public void CONVERT()
        {
            // 9列x4行を4列９行に入れ替える
            byte[] vbuf = new byte[40];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    byte b = mucInfo.mmlVoiceDataWork.Get(row * 9 + col + 1);
                    vbuf[col * 4 + row] = b;
                }
            }
            vbuf[36] = mucInfo.mmlVoiceDataWork.Get(37);
            vbuf[38] = mucInfo.mmlVoiceDataWork.Get(38); //次のOPEXにて37に移動するよ

            //OPEX()
            for (int col = 0; col < 10; col++)
            {
                byte b = vbuf[col * 4 + 1];
                vbuf[col * 4 + 1] = vbuf[col * 4 + 2];
                vbuf[col * 4 + 2] = b;
            }

            //GETPARA
            int a;
            for (int row = 0; row < 4; row++)
            {
                //MUL/DT
                a = vbuf[row + 32] * 16 + vbuf[row + 28];
                mucInfo.mmlVoiceDataWork.Set(row + 1, (byte)a);

                //TL
                mucInfo.mmlVoiceDataWork.Set(row + 5, (byte)vbuf[row + 20]);

                //KS/AR
                a = vbuf[row + 24] * 64 + vbuf[row];
                mucInfo.mmlVoiceDataWork.Set(row + 9, (byte)a);

                //DR
                mucInfo.mmlVoiceDataWork.Set(row + 13, (byte)vbuf[row + 4]);

                //SR
                mucInfo.mmlVoiceDataWork.Set(row + 17, (byte)vbuf[row + 8]);

                //SL/RR
                a = vbuf[row + 16] * 16 + vbuf[row + 12];
                mucInfo.mmlVoiceDataWork.Set(row + 21, (byte)a);
            }

            a = vbuf[36] * 8 + vbuf[37];
            mucInfo.mmlVoiceDataWork.Set(25, (byte)a);

            ////OPEX() 要らないと思う
            //for (int col = 0; col < 10; col++)
            //{
            //byte b = vbuf[col * 4 + 1];
            //vbuf[col * 4 + 1] = vbuf[col * 4 + 2];
            //vbuf[col * 4 + 2] = b;
            //}
        }

        public void CONVERTopm(List<byte> voi)
        {
            int a;
            for (int row = 0; row < 4; row++)
            {
                int op = row;
                op = (op == 1 ? 2 : (op == 2 ? 1 : op));

                //DT/MUL
                a = voi[op * 10 + 2 + 8] * 16 + voi[op * 10 + 2 + 7];
                mucInfo.mmlVoiceDataWork.Set(row + 1, (byte)a);

                //TL
                mucInfo.mmlVoiceDataWork.Set(row + 5, (byte)voi[op * 10 + 2 + 5]);

                //KS/AR
                a = voi[op * 10 + 2 + 6] * 64 + voi[op * 10 + 2 + 0];
                mucInfo.mmlVoiceDataWork.Set(row + 9, (byte)a);

                //DR
                mucInfo.mmlVoiceDataWork.Set(row + 13, (byte)voi[op * 10 + 2 + 1]);

                //DT2/SR
                a = voi[op * 10 + 2 + 9] * 64 + voi[op * 10 + 2 + 2];
                mucInfo.mmlVoiceDataWork.Set(row + 17, (byte)a);

                //SL/RR
                a = voi[op * 10 + 2 + 4] * 16 + voi[op * 10 + 2 + 3];
                mucInfo.mmlVoiceDataWork.Set(row + 21, (byte)a);
            }

            a = voi[0] * 8 + voi[1];
            mucInfo.mmlVoiceDataWork.Set(25, (byte)a);

        }
    }
}
