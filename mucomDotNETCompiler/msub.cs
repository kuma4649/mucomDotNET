using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Compiler
{
    public class Msub
    {
        private work work = null;
        private readonly MUCInfo mucInfo;
        public Muc88 muc88;
        private iEncoding enc = null;

        public byte[] SCORE = {
            0,0,0,0,0,0
        };

        public byte[] FCOMS = new byte[]{// COMMANDs
         0x6c // 'l'	LIZM
        ,0x6f //,'o'	OCTAVE
        ,0x44 //,'D'	DETUNE
        ,0x76 //,'v'	VOLUME
        ,0x40 //,'@'	SOUND COLOR
        ,0x3e //,'>'	OCTAVE UP
        ,0x3c //,'<'	OCTAVE DOWN
        ,0x29 //,')'	VOLUME UP
        ,0x28 //,'('	VOLUME DOWN
        ,0x26 //,'&'	TIE
        ,0x79 //,'y'	REGISTER WRITE
        ,0x4d //,'M'	MODURATION(LFO)
        ,0x72 //,'r'	REST
        ,0x5b //,'['	LOOP START
        ,0x5d //,']'	LOOP END
        ,0x53 //,'S'	SE DETUNE
        ,0x4c //,'L'	JUMP RESTART ADR
        ,0x71 //,'q'	COMMAND OF 'q'
        ,0x45 //,'E'	SOFT ENV
        ,0x50 //,'P'	MIX PORT
        ,0x77 //,'w'	NOIZE WAVE
        ,0x74 //,'t'	TEMPO(DIRECT CLOCK)
        ,0x43 //,'C'	SET CLOCK
        ,0x21 //,'!'	COMPILE END
        ,0x4b //,'K'	KEY SHIFT
        ,0x2f //,'/'	REPEAT JUMP
        ,0x56 //,'V'	TOTAL VOLUME OFFSET
        ,0x5c //,'\'    BEFORE CODE
        //,0x73 //,'s'	HARD ENVE SET
        ,0x6d //,'m'	HARD ENVE PERIOD
        ,0x6b //,'k'	KEY SHIFT 2
        ,0x73 //,'s'	KEY ON REVISE
        ,0x25 //,'%'	SET LIZM(DIRECT CLOCK)
        ,0x70 //,'p'	STEREO PAN
        ,0x48 //,'H'	HARD LFO
        ,0x54 //,'T'	TEMPO
        ,0x4a //,'J'	TAG SET & JUMP TO TAG
        ,0x3b //,';'	ﾁｭｳﾔｸ ﾖｳ
        ,0x52 //,'R'	ﾘﾊﾞｰﾌﾞ
        ,0x2a //,'*'	MACRO
        ,0x3a //,':'	RETURN
        ,0x5e //,'^'	&ﾄ ｵﾅｼﾞ
        ,0x7c //,'|'	ｼｮｳｾﾂ
        ,0x7d //,'}'	ﾏｸﾛｴﾝﾄﾞ
        ,0x7b //,'{'	ﾎﾟﾙﾀﾒﾝﾄｽﾀｰﾄ
        ,0x23 //,'#'	FLAG SET
        ,0
        };

        public byte[] TONES = new byte[] {
             0x63,0 //  'c' ,0
            ,0x64,2 //  'd' ,2
            ,0x65,4 //  'e' ,4
            ,0x66,5 //  'f' ,5
            ,0x67,7 //  'g' ,7
            ,0x61,9 //  'a' ,9
            ,0x62,11//   'b' ,11
        };

        public Msub(work work, MUCInfo mucInfo,iEncoding enc)
        {
            this.work = work;
            this.mucInfo = mucInfo;
            this.enc = enc;
        }

        public int REDATA(Tuple<int,string> lin,ref int srcCPtr)
        {
            mucInfo.ErrSign = false;

            for (int i = 0; i < SCORE.Length; i++) SCORE[i] = 0;
            int degit = 5;   // 5ｹﾀ ﾏﾃﾞ

            work.HEXFG = 0;
            work.MINUSF = 0;

        //READ0:			// FIRST CHECK
            char ch;

            do
            {
                if (lin.Item2.Length == srcCPtr)
                {
                    srcCPtr++;
                    mucInfo.Carry = true; // NON DATA
                    return 0;
                }
                ch = lin.Item2.Length > srcCPtr ? lin.Item2[srcCPtr] : (char)0;
                srcCPtr++;
            } while (ch == ' ');

            if (ch == '$')//0x24
            {
                work.HEXFG = 1;
                srcCPtr++;
                goto READ7;
            }

            if (ch == '-')//0x2d
            {
                ch = lin.Item2.Length > srcCPtr ? lin.Item2[srcCPtr] : (char)0;
                srcCPtr++;
                if (ch < '0' || ch > '9')//0x30 0x39
                {
                    goto READE;//0ｲｼﾞｮｳ ﾉ ｷｬﾗｸﾀﾅﾗ ﾂｷﾞ
                }
                work.MINUSF = 1;   // SET MINUS FLAG
                goto READ7;
            }

            if (ch < '0' || ch > '9')//0x30 0x39
            {
                goto READE;//0ｲｼﾞｮｳ ﾉ ｷｬﾗｸﾀﾅﾗ ﾂｷﾞ
            }
            goto READ7;

        READ7:
            srcCPtr--;
            do
            {
                ch = lin.Item2.Length > srcCPtr ? lin.Item2[srcCPtr] : (char)0;
                //Z80.A = Mem.LD_8(Z80.HL);       // SECOND CHECK
                if (work.HEXFG == 0)
                {
                    goto READC;
                }

                if (ch >= 'a' && ch <= 'f')
                {
                    ch -= (char)32;
                }
            //READG:
                if (ch >= 'A' && ch <= 'F')
                {
                    ch -= (char)7;
                    goto READF;
                }
            READC:
                if (ch < '0' || ch > '9')
                {
                    goto READ1;//9ｲｶﾅﾗ ﾂｷﾞ
                }
            READF:

                SCORE[0] = SCORE[1];
                SCORE[1] = SCORE[2];
                SCORE[2] = SCORE[3];
                SCORE[3] = SCORE[4];
                SCORE[4] = SCORE[5];

                ch -= (char)0x30;// A= 0 - 9
                SCORE[4] = (byte)ch;
                srcCPtr++; // NEXT TEXT
                degit--;

                if (lin.Item2.Length == srcCPtr) goto READ1;

            } while (degit > 0);

            ch = lin.Item2.Length > srcCPtr ? lin.Item2[srcCPtr] : (char)0; // THIRD CHECK
            if (ch < '0' || ch > '9')
            {
                goto READ1;//9ｲｶﾅﾗ ﾂｷﾞ
            }
        //READ8:
            mucInfo.Carry = false;
            mucInfo.ErrSign = true;// ERROR SIGN
            return 0;//RET	; 7ｹﾀｲｼﾞｮｳ ﾊ ｴﾗｰ

        READ1:
            int a = 0;
            if (work.HEXFG == 1)
            {
                for (int i = 1; i < 5; i++)
                {
                    a *= 16;
                    a += SCORE[i];
                }
                goto READA;
            }
        //READD:
            for (int i = 0; i < 5; i++)
            {
                a *= 10;
                a += SCORE[i];
            }

            if (work.MINUSF != 0)
            {// CHECK MINUS FLAG
                a = -a;
            }
        READA:
            mucInfo.Carry = false;
            return a;//    RET
        READE:
            work.SECCOM = (byte)ch;
            mucInfo.Carry = true; // NON DATA
            return 0;
        }

        public bool MCMP_DE(string strDE, Tuple<int, string> lin, ref int srcCPtr)
        {
            try
            {
                string trgDE = strDE.Substring(0, strDE.IndexOf("\0"));
                if (trgDE.Length < 1) return false;

                byte[] bHL = new byte[trgDE.Length];
                for (int i = 0; i < trgDE.Length; i++)
                {
                    bHL[i] = (byte)(lin.Item2.Length > srcCPtr ? lin.Item2[srcCPtr] : 0);
                    srcCPtr++;
                }
                string trgHL = enc.GetStringFromUtfArray(bHL);// Encoding.UTF8.GetString(bHL);
                if (trgHL == trgDE)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }

        public void MWRIT2(MmlDatum dat)
        {
            //Console.WriteLine("{0:x2}", dat.dat);
            mucInfo.bufDst.Set(work.MDATA++, dat);

            if (work.MDATA - work.bufStartPtr > 0xffff)
            {
                throw new MucException(
                    msg.get("E0200")
                    , mucInfo.row, mucInfo.col);
            }

            muc88.DispHex4(work.MDATA, 36);
        }

        public void MWRITE(MmlDatum cmdNo, MmlDatum cmdDat)
        {
            //Common.WriteLine("{0:x2}", cmdNo);
            mucInfo.bufDst.Set(work.MDATA++, cmdNo);
            //Common.WriteLine("{0:x2}", cmdDat);
            mucInfo.bufDst.Set(work.MDATA++, cmdDat);

            if (work.MDATA - work.bufStartPtr > 0xffff)
            {
                throw new MucException(
                    msg.get("E0200")
                    , mucInfo.row, mucInfo.col);
            }

            muc88.DispHex4(work.MDATA, 36);
        }

        public int ERRT(Tuple<int, string> lin, ref int ptr,string cmdMsg)
        {
            ptr++;
            int n = REDATA(lin, ref ptr);
            if (mucInfo.Carry)//数値読み取れなかった
            {
                throw new MucException(
                    string.Format(msg.get("E0201"), cmdMsg)
                    , mucInfo.row, mucInfo.col);
            }
            else
            {
                if (mucInfo.ErrSign)
                {
                    //ERRORIF();
                    return -1;
                }
            }

            return n;
        }

        public int FMCOMC(char c)
        {
            for (int i = 0; i < FCOMS.Length; i++)
            {
                if (FCOMS[i] == 0)
                {
                    break;
                }
                if (FCOMS[i] == c)
                {
                    //Common.WriteLine("{0}", c);
                    return i + 1;
                }
            }
            //Common.WriteLine("{0}!", c);
            return 0;
        }

        public byte STTONE()
        {
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;

            Log.WriteLine(LogLevel.TRACE, c.ToString());

            for (int i = 0; i < 7; i++)
            {
                if (c == TONES[i * 2])
                {
                    mucInfo.Carry = false;
                    return TONEXT((byte)i);
                }
            }

            mucInfo.Carry = true;
            return 0;
        }

        private byte TONEXT(byte n)
        {
            n = TONES[n * 2 + 1];
            int o = work.OCTAVE;

            ++mucInfo.srcCPtr;
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;

            if (c == '+')
            {
                if (n == 11)// KEY='b'?
                {
                    n = 0xff;
                    o++;
                    if (o == 8)
                    {
                        o = 7;
                    }
                }
                n++;
            }
            else if (c == '-')
            {
                if (n == 0)
                {
                    n = 12;
                    o--;
                    if (o < 0)
                    {
                        o = 0;
                    }
                }
                n--;
            }
            else
            {
                mucInfo.srcCPtr--;
            }

            KEYSIFT(ref o, ref n);
            mucInfo.Carry = false;
            return (byte)(((o & 0xf) << 4) | (n & 0xf));
        }

        public void KEYSIFT(ref int oct,ref byte n)
        {
            int shift = (sbyte)work.SIFTDAT + (sbyte)work.SIFTDA2;
            if (shift == 0) return;

            //mucInfo.Carry = (oct * 12 + n > 0xff);
            n = (byte)(oct * 12 + n);

            oct = (n + shift) / 12;
            n = (byte)((n + shift) % 12);
        }

        /// <summary>
        /// 音長のよみとり
        /// </summary>
        /// <returns>
        /// 0...normal
        /// -1...WARNING
        /// </returns>
        public int STLIZM(Tuple<int,string> lin,ref int ptr,out byte clk)
        {
            char c = ptr < lin.Item2.Length
                ? lin.Item2[ptr]
                : (char)0;
            int n;
            clk = 0;

            if (c == '%')
            {
                ptr++;
                n = REDATA(lin, ref ptr);
                if (mucInfo.Carry)//数値読み取れなかった
                {
                    ptr--;
                    throw new MucException(msg.get("E0499"), lin.Item1, ptr);//ERRORSN
                }
                if (mucInfo.ErrSign)
                {
                    throw new MucException(msg.get("E0500"), lin.Item1, ptr);//ERRORIF
                }

                clk = (byte)n;
                if (n < 0 || n > 255)
                {
                    return -1;
                }
                return 0;
            }

            int w = 0;
            n = REDATA(lin, ref ptr);
            if (n < 0 || n > 255)
            {
                w = -1;
            }
            n = (byte)n;

            if (mucInfo.Carry)//数値読み取れなかった
            {
                ptr--;
                n = work.COUNT;
            }
            else
            {
                if (mucInfo.ErrSign)
                {
                    throw new MucException(msg.get("E0501"), lin.Item1, ptr);//ERRORSN
                }
                if (work.CLOCK < n)//clock以上の細かい音符は指定できないようにしている
                {
                    throw new MucException(string.Format(msg.get("E0502"), n), lin.Item1, ptr);//ERRORIF
                    //CLOCK<E ﾃﾞ ERROR
                }

                n = work.CLOCK / n;//clockに変換
            }

            int a = n;
            do
            {
                c = ptr < lin.Item2.Length
                    ? lin.Item2[ptr]
                    : (char)0;
                if (c != '.') break;
                ptr++;
                a /= 2;
                n += a;
            } while (true);

            if (n > 255)
            {
                throw new MucException(string.Format(msg.get("E0503"), n), lin.Item1, ptr);//ERROROF
            }

            clk = (byte)n;
            return w;
        }


        public void ERRSN()
        {
            throw new NotImplementedException();
        }

    }
}
