using mucomDotNET.Common;
using System;
using System.Text;
using musicDriverInterface;
using System.Collections.Generic;
using System.Drawing;

namespace mucomDotNET.Compiler
{
    public class Muc88
    {
        public Msub msub = null;
        public expand expand = null;

        /// <summary>
        /// 最大チャンネル数
        /// </summary>
        internal static readonly int MAXCH = 22;
        private readonly MUCInfo mucInfo;
        private readonly Func<EnmFCOMPNextRtn>[] COMTBL;
        //private readonly int errLin = 0;
        private iEncoding enc = null;

        public Muc88(MUCInfo mucInfo,iEncoding enc)
        {
            this.enc = enc;
            this.mucInfo = mucInfo;
            COMTBL = new Func<EnmFCOMPNextRtn>[]
            {
              SETLIZ
            , SETOCT
            , SETDT
            , SETVOL
            , SETCOL
            , SETOUP
            , SETODW
            , SETVUP
            , SETVDW
            , SETTIE
            , SETREG
            , SETMOD
            , SETRST
            , SETLPS
            , SETLPE
            , SETSEorSETHE
            , SETJMP
            , SETQLG
            , SETSEV
            , SETMIX
            , SETWAV
            , TIMERB
            , SETCLK
            , COMOVR
            //, SETKST
            , SETKS2
            , SETRJP
            , TOTALV
            , SETBEF
            //, SETHE
            , SETHEP
            , SETKST
            , SETKON
            , SETDCO
            , SETLR
            , SETHLF
            , SETTMP
            , SETTAG
            , SETMEM
            , SETRV
            , SETMAC
            , STRET  //RETで戻る!
            , SETTI2 //ret code:fcomp13
            , SETSYO
            , ENDMAC
            , SETPTM
            , SETFLG
            };
        }

        private EnmFCOMPNextRtn SETHE()
        {
            if (CHCHK() != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0521")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0504"));
            mucInfo.srcCPtr = ptr;
            if (mucInfo.ErrSign || n < 0 || n > 7)
                throw new MucException(
                    msg.get("E0505")
                    , mucInfo.row, mucInfo.col);

            mucInfo.isDotNET = true;
            msub.MWRITE(new MmlDatum(0xff), new MmlDatum(0xf1));
            msub.MWRIT2(new MmlDatum((byte)(n + 8)));

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETHEP()
        {
            if (CHCHK() != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0520")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0506"));
            mucInfo.srcCPtr = ptr;
            if (mucInfo.ErrSign || n < 0 || n > 65535)
                throw new MucException(
                    msg.get("E0507")
                    , mucInfo.row, mucInfo.col);

            mucInfo.isDotNET = true;
            msub.MWRITE(new MmlDatum(0xff), new MmlDatum(0xf2));
            msub.MWRITE(new MmlDatum((byte)n), new MmlDatum((byte)(n >> 8)));

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETSEorSETHE()
        {
            if (CHCHK() != ChannelType.SSG) return SETSE();//SSG以外ならスロットディチューンコマンドとして動作

            return SETHE();//SSGなら
        }

        // *	ﾏｸﾛｾｯﾄ*

        public EnmFCOMPNextRtn SETMAC()
        {
            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (n > 0xff || n < 0)
            {
                WriteWarning(msg.get("W0400"), mucInfo.row, mucInfo.col);
            }
            n &= 0xff;
            mucInfo.srcCPtr = ptr;

            //戻り先を記憶
            mucInfo.bufMacStack.Set(work.ADRSTC, mucInfo.srcCPtr);//col
            mucInfo.bufMacStack.Set(work.ADRSTC + 1, mucInfo.srcLinPtr + 1);//line row
            work.ADRSTC += 2;

            //飛び先を取得
            mucInfo.srcCPtr = mucInfo.bufMac.Get(n * 2 + 0);
            mucInfo.srcLinPtr = mucInfo.bufMac.Get(n * 2 + 1);
            if (mucInfo.srcLinPtr == 0)
            {
                throw new MucException(msg.get("E0400"), mucInfo.row, mucInfo.col);
            }
            mucInfo.srcLinPtr--;
            mucInfo.lin = mucInfo.basSrc[mucInfo.srcLinPtr];

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn ENDMAC()
        {
            work.ADRSTC -= 2;
            if (work.ADRSTC < 0)
            {
                throw new MucException(
                    msg.get("E0401")
                    , mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = mucInfo.bufMacStack.Get(work.ADRSTC);
            mucInfo.srcLinPtr = mucInfo.bufMacStack.Get(work.ADRSTC + 1);
            mucInfo.bufMacStack.Set(work.ADRSTC, 0);//clear col
            mucInfo.bufMacStack.Set(work.ADRSTC + 1, 0);//clear line row
            if (mucInfo.srcLinPtr == 0)
            {
                throw new MucException(
                    msg.get("E0402")
                    , mucInfo.row, mucInfo.col);
            }
            mucInfo.srcLinPtr--;
            mucInfo.lin = mucInfo.basSrc[mucInfo.srcLinPtr];

            return EnmFCOMPNextRtn.fcomp1;
        }

        private void skipSpaceAndTab()
        {
            char c = getMoji();

            while (c == ' ' || c == 0x9)
            {
                mucInfo.srcCPtr++;
                c = getMoji();
            }
        }

        private char getMoji()
        {
            return mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
        }

        private EnmFCOMPNextRtn SETPTM()
        {
            LinePos lp = new LinePos(mucInfo.fnSrc, mucInfo.row, mucInfo.col, mucInfo.srcCPtr);

            mucInfo.srcCPtr++;
            byte before_note = msub.STTONE();//KUMA:オクターブ情報などを含めた音符情報に変換
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0403")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            int befco = FC162p_clock(before_note);//SET TONE&LIZ
            int qbefco = befco - work.quantize;

            skipSpaceAndTab();

            char c = getMoji();
            if (c == 0)
            {
                throw new MucException(
                    msg.get("E0404")
                    , mucInfo.row, mucInfo.col);
            }

            if (c == '}')//0x7d
            {
                //到達点を指定することなくポルタメント終了している場合はエラー
                throw new MucException(
                    msg.get("E0405")
                    , mucInfo.row, mucInfo.col);
            }

            bool pflg = false;
            while (c == '>' || c == '<' || c == ' ' || c == 0x9)
            {
                if (c == '>')//0x3e
                {
                    if (pflg) mucInfo.isDotNET = true;
                    pflg = true;
                    SOU1();
                }
                else if (c == '<')//0x3c
                {
                    if (pflg) mucInfo.isDotNET = true;
                    pflg = true;
                    SOD1();
                }
                else
                    mucInfo.srcCPtr++;

                c = getMoji();
            }

            byte after_note = msub.STTONE();//KUMA:オクターブ情報などを含めた音符情報に変換
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0404")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;

            skipSpaceAndTab();

            c = getMoji();//KUMA:次のコマンドを取得

            if (c != '}')//0x7d
            {
                throw new MucException(
                    msg.get("E0407")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            lp.length = mucInfo.srcCPtr - lp.length;

            int beftone = expand.CTONE(before_note);
            int noteNum = expand.CTONE(after_note) - beftone;
            int sign = Math.Sign(noteNum);
            noteNum = Math.Abs(noteNum);// +1;

            //26を超えたら分割が必要?

            int noteDiv = noteNum / 2 + 1; //2おんずつ
            if (noteDiv > befco) noteDiv = befco;
            int noteStep = noteNum / noteDiv * sign;
            int noteMod = (noteNum % noteDiv) * sign;
            int clock = befco / noteDiv;
            int clockMod = befco % noteDiv;

            List<Tuple<int, int, int, int>> lstPrt = new List<Tuple<int, int, int, int>>();
            int stNote = 0;
            for (int i = 0; i < noteDiv; i++)
            {
                int edNote = stNote + noteStep + ((noteMod > 0 ? 1 : 0)) * sign;
                if (clock + (clockMod > 0 ? 1 : 0) != 0)
                {
                    int n = clock + (clockMod > 0 ? 1 : 0);
                    Tuple<int, int, int, int> p = new Tuple<int, int, int, int>(
                        stNote + beftone
                        , edNote + beftone
                        , n
                        , qbefco > n ? n : qbefco
                        );
                    lstPrt.Add(p);
                    qbefco -= n;
                    qbefco = Math.Max(qbefco, 0);
                }
                else
                {
                    Tuple<int, int, int,int> p = new Tuple<int, int, int,int>(
                        lstPrt[lstPrt.Count - 1].Item1
                        , edNote + beftone
                        , lstPrt[lstPrt.Count - 1].Item3
                        , qbefco > lstPrt[lstPrt.Count - 1].Item3 ? lstPrt[lstPrt.Count - 1].Item3 : qbefco
                        );
                    qbefco -= lstPrt[lstPrt.Count - 1].Item3;
                    qbefco = Math.Max(qbefco, 0);
                    lstPrt[lstPrt.Count - 1] = p;
                }

                stNote = edNote;// + sign;
                noteMod--;
                clockMod--;
            }

            bool tie = true;
            work.beforeQuantize = work.quantize;

            for (int i = 0; i < lstPrt.Count; i++)
            {
                Tuple<int, int, int, int> it = lstPrt[i];
                if (i == lstPrt.Count - 1) tie = false;
                byte st = (byte)(((it.Item1 / 12) << 4) | ((it.Item1 % 12) & 0xf));
                byte ed = (byte)(((it.Item2 / 12) << 4) | ((it.Item2 % 12) & 0xf));
                WritePortament(st, ed, (byte)it.Item3, (byte)(it.Item3 - it.Item4), tie);
            }

            PortamentEnd();

            return EnmFCOMPNextRtn.fcomp1;

        }

        private void WritePortament(byte note, byte endNote, byte clk,byte q, bool tie)
        {
            int depth = expand.CULPTM(note, endNote, clk);//KUMA:DEPTHを計算
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0406")
                    , mucInfo.row, mucInfo.col);
            }

            PortamentStart(depth);
            FC162p_write(note, clk,q, tie);
        }

        private void PortamentStart(int depth)
        {
            msub.MWRIT2(new MmlDatum(0xf4));// PTMDAT;
            msub.MWRIT2(new MmlDatum(0x00));
            msub.MWRIT2(new MmlDatum(0x01));
            msub.MWRIT2(new MmlDatum(0x01));
            work.BEFMD = work.MDATA;//KUMA:DEPTHの書き込み位置を退避
            //work.MDATA += 2;
            msub.MWRIT2(new MmlDatum((byte)(depth & 0xff)));
            msub.MWRIT2(new MmlDatum((byte)(depth >> 8)));

            msub.MWRIT2(new MmlDatum(0xff));//KUMA:回数(255回)を書き込む
        }

        private void PortamentEnd()
        {
            msub.MWRIT2(new MmlDatum(0xf4));//KUMA:2個目のMコマンド作成開始
            byte a = work.LFODAT[0];//KUMA:現在のLFOのスイッチを取得
            a--;
            if (a == 0)//KUMA:OFF(1)の場合はSTP1で2個めのMコマンドへOFF(1)を書き込む
            {
                msub.MWRIT2(new MmlDatum(0x01));
            }
            else
            {
                msub.MWRIT2(new MmlDatum(0x00));//KUMA:ON(0)の場合は2個めのMコマンドへON(0)を書き込む
                msub.MWRIT2(new MmlDatum(work.LFODAT[1]));//KUMA:残りの現在のLFOの設定5byteをそのまま２個目のMコマンドへコピー
                msub.MWRIT2(new MmlDatum(work.LFODAT[2]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[3]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[4]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[5]));
            }

            if (work.beforeQuantize != work.quantize)
                msub.MWRITE(new MmlDatum(0xf3), new MmlDatum((byte)work.quantize));// COM OF 'q'

        }

        private void FC162p_write(int note, byte clk,byte q, bool tie)
        {
            //if (tie)
            //{
            //    msub.MWRIT2(new MmlDatum(0xfd));
            //}
            TCLKSUB(clk);// ﾄｰﾀﾙｸﾛｯｸ ｶｻﾝ

            if (work.beforeQuantize != q)
            {
                msub.MWRITE(new MmlDatum(0xf3), new MmlDatum(q));// COM OF 'q'
                work.beforeQuantize = q;
            }

            if (q < clk)
            {
                FCOMP17(note, clk);// note
                if (tie)
                {
                    msub.MWRIT2(new MmlDatum(0xfd)); // tie
                }
            }
            else
            {
                //rest
                while (clk > 0x6f)
                {
                    clk -= 0x6f;
                    msub.MWRIT2(new MmlDatum((byte)(0b1110_1111)));
                }
                work.BEFRST = clk;
                msub.MWRIT2(new MmlDatum((byte)(clk | 0b1000_0000)));// SET REST FLAG
            }
        }



        private EnmFCOMPNextRtn SETPTM2()
        {
            msub.MWRIT2(new MmlDatum(0xf4));// PTMDAT;
            msub.MWRIT2(new MmlDatum(0x00));
            msub.MWRIT2(new MmlDatum(0x01));
            msub.MWRIT2(new MmlDatum(0x01));
            work.BEFMD = work.MDATA;//KUMA:DEPTHの書き込み位置を退避
            work.MDATA += 2;
            msub.MWRIT2(new MmlDatum(0xff));//KUMA:回数(255回)を書き込む

            mucInfo.srcCPtr++;

            byte note = msub.STTONE();//KUMA:オクターブ情報などを含めた音符情報に変換
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0403")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            FC162p(note);//SET TONE&LIZ

            msub.MWRIT2(new MmlDatum(0xf4));//KUMA:2個目のMコマンド作成開始
            byte a = work.LFODAT[0];//KUMA:現在のLFOのスイッチを取得
            a--;
            if (a == 0)//KUMA:OFF(1)の場合はSTP1で2個めのMコマンドへOFF(1)を書き込む
            {
                msub.MWRIT2(new MmlDatum(0x01));
            }
            else
            {
                msub.MWRIT2(new MmlDatum(0x00));//KUMA:ON(0)の場合は2個めのMコマンドへON(0)を書き込む
                msub.MWRIT2(new MmlDatum(work.LFODAT[1]));//KUMA:残りの現在のLFOの設定5byteをそのまま２個目のMコマンドへコピー
                msub.MWRIT2(new MmlDatum(work.LFODAT[2]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[3]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[4]));
                msub.MWRIT2(new MmlDatum(work.LFODAT[5]));
            }

            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;

            if (c == 0)
            {
                throw new MucException(
                    msg.get("E0404")
                    , mucInfo.row, mucInfo.col);
            }
            if (c == '}')//0x7d
            {
                throw new MucException(
                    msg.get("E0405")
                    , mucInfo.row, mucInfo.col);
            }

            bool pflg = false;
            while (c == '>' || c == '<' || c == ' ' || c == 0x9)
            {
                if (c == '>')//0x3e
                {
                    if (pflg) mucInfo.isDotNET = true;
                    pflg = true;
                    SOU1();
                }
                else if (c == '<')//0x3c
                {
                    if (pflg) mucInfo.isDotNET = true;
                    pflg = true;
                    SOD1();
                }
                else
                    mucInfo.srcCPtr++;

                c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                    mucInfo.lin.Item2[mucInfo.srcCPtr]
                    : (char)0;
            }

            int depth = expand.CULPTM();//KUMA:DEPTHを計算
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0406")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.bufDst.Set(work.BEFMD, new MmlDatum((byte)depth));//KUMA:DE(DEPTH)を書き込む
            mucInfo.bufDst.Set(work.BEFMD + 1, new MmlDatum((byte)(depth >> 8)));

            mucInfo.srcCPtr++;
            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;//KUMA:次のコマンドを取得

            if (c != '}')//0x7d
            {
                throw new MucException(
                    msg.get("E0407")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            return EnmFCOMPNextRtn.fcomp1;
        }

        // *	ﾘﾊﾞｰﾌﾞ*

        private EnmFCOMPNextRtn SETRV()
        {
            ChannelType tp = CHCHK();
            if (tp != ChannelType.FM && tp != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0408")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;

            byte dat = 0xf3;
            if (c == 'm')//0x6d
            {
                dat = 0xf4;
            }
            else if (c == 'F')//0x46
            {
                dat = 0xf5;
            }
            else
            {
                mucInfo.srcCPtr--;
            }

            msub.MWRITE(new MmlDatum(0xff), new MmlDatum(dat));
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0409"));
            mucInfo.srcCPtr = ptr;
            msub.MWRIT2(new MmlDatum((byte)n));

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETTMP()
        {
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0410")); // T
            mucInfo.srcCPtr = ptr;
            if ((byte)n == 0)
            {
                throw new MucException(
                    msg.get("E0411")
                    , mucInfo.row, mucInfo.col);
            }

            int HL = (25600 - 346 * (byte)(60000 / (work.CLOCK / 4 * (byte)n) + 1)) / 100;
            if (HL <= 0)
            {
                HL = 1;
            }

            return TIMEB2((byte)HL);
        }


        // *	FLAGDATA SET	*

        private EnmFCOMPNextRtn SETFLG()
        {
            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry)
            {
                n = 0xFF;
            }
            msub.MWRITE(new MmlDatum(0xf9), new MmlDatum((byte)n));

            return EnmFCOMPNextRtn.fcomp1;
        }

        // *	ｼｮｳｾﾂﾏｰｸ*

        private EnmFCOMPNextRtn SETSYO()
        {
            mucInfo.srcCPtr++;

            return EnmFCOMPNextRtn.fcomp1;
        }

        // *	ﾁｭｳﾔｸ*

        private EnmFCOMPNextRtn SETMEM()
        {
            mucInfo.srcCPtr = mucInfo.lin.Item2.Length;
            return EnmFCOMPNextRtn.fcomp1;//KUMA:NextLine
        }


        // *	SET TAG & JUMP TO TAG*

        private EnmFCOMPNextRtn SETTAG()
        {

            work.JCLOCK = work.tcnt[work.COMNOW];
            work.JCHCOM = new List<int>();
            work.JCHCOM.Add(work.COMNOW);
            if (work.POINTC >= 0)
            {
                int p = work.POINTC;
                while (p >= 0)
                {
                    work.JCLOCK += mucInfo.bufLoopStack.Get(p + 6) + mucInfo.bufLoopStack.Get(p + 7) * 0x100;
                    p -= 10;
                }
            }
            work.JPLINE = mucInfo.row;
            work.JPCOL = mucInfo.col;

            mucInfo.srcCPtr++;

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	HARD LFO	*

        private EnmFCOMPNextRtn SETHLF()
        {
            ChannelType tp = CHCHK();
            if (tp != ChannelType.FM)
            {
                throw new MucException(
                    msg.get("E0412")
                    , mucInfo.row, mucInfo.col);
            }

            msub.MWRIT2(new MmlDatum(0xfc));

            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0413"));
            msub.MWRIT2(new MmlDatum((byte)n));

            n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0413"));
            msub.MWRIT2(new MmlDatum((byte)n));

            n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0413"));
            mucInfo.srcCPtr = ptr;
            msub.MWRIT2(new MmlDatum((byte)n));

            return EnmFCOMPNextRtn.fcomp1;
        }

        // *	STEREO PAN	*

        private EnmFCOMPNextRtn SETLR()
        {
            ChannelType tp = CHCHK();
            if (tp == ChannelType.SSG)
            {
                WriteWarning(msg.get("W0407"), mucInfo.row, mucInfo.col);
            }

            int ptr = mucInfo.srcCPtr;
            //Ryhthm の内訳
            // bit0～3 rythmType R:5 T:4 H:3 C:2 S:1 B:0
            // bit4～7 パン 1:右, 2:左, 3:中央 4:右オート 5:左オート 6:ランダム

            int v = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0414"));
            int n = (byte)v;
            int rn = (byte)((tp == ChannelType.RHYTHM) ? (n >> 4) : n);
            mucInfo.srcCPtr = ptr;
            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;

            //1-6の範囲外の時 -> エラー
            if (rn < 1 || rn > 6)
            {
                //使用可能な値の範囲外
                throw new MucException(string.Format(msg.get("E0524"), v), mucInfo.row, mucInfo.col);
            }

            //4-6なのに,が無いとき -> エラー
            if (c != ',' && rn > 3 && rn < 7)
            {
                //4～6の場合は値2が必須
                throw new MucException(msg.get("E0525"), mucInfo.row, mucInfo.col);
            }

            //1-3なのに,がある時 -> 警告扱い
            if (c == ',' && rn > 0 && rn < 4)
            {
                WriteWarning(msg.get("W0415"), mucInfo.row, mucInfo.col);
            }

            //
            List<object> args = new List<object>();
            args.Add(n);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , tp == ChannelType.FM ? "FM" : (tp == ChannelType.SSG ? "SSG" : (tp == ChannelType.RHYTHM ? "RHYTHM" : "ADPCM"))
                , "YM2608", 0, 0, work.COMNOW); ;

            msub.MWRITE(
                new MmlDatum(enmMMLType.Pan, args, lp, 0xf8)
                , new MmlDatum((byte)n));// COM OF 'p'

            ////
            //AMD98
            if (c == ',')//0x2c
            {
                //1-6の範囲内ならwait値を取得する
                mucInfo.isDotNET = true;
                mucInfo.srcCPtr++;
                ptr = mucInfo.srcCPtr;
                int n2 = msub.REDATA(mucInfo.lin, ref ptr);
                mucInfo.srcCPtr = ptr;

                //4-6の範囲内の場合のみwait値を出力する
                if (rn > 3 && rn < 7)
                {

                    //wait値が範囲外の時 ->エラー
                    //if (n2 < 1 || n2 > work.CLOCK || (byte)n2 < 1)
                    if (n2 < 1 || n2 > 255 || (byte)n2 < 1)
                    {
                        //throw new MucException(string.Format(msg.get("E0526"), work.CLOCK, v), mucInfo.row, mucInfo.col);
                        throw new MucException(string.Format(msg.get("E0526"), 255, v), mucInfo.row, mucInfo.col);
                    }

                    if (tp == ChannelType.SSG) return EnmFCOMPNextRtn.fcomp1;//KUMA:互換の為。。。(SSGパートではnoise周波数設定コマンドとして動作)

                    msub.MWRIT2(new MmlDatum((byte)n2));// ２こめ
                }
            }

            ////

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	DIRECT COUNT	*

        private EnmFCOMPNextRtn SETDCO()
        {
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0415"));
            mucInfo.srcCPtr = ptr;
            work.COUNT = (byte)n;

            return EnmFCOMPNextRtn.fcomp12;
        }

        // *	SET HARD ENVE TYPE/FLAG*

        //private enmFCOMPNextRtn SETHE()
        //{
        //    mucInfo.srcCPtr++;
        //    msub.MWRITE(new MmlDatum(0xff,0xf1);// 2nd COM

        //    int ptr = mucInfo.srcCPtr;
        //    int n = msub.REDATA(mucInfo.lin, ref ptr);
        //    mucInfo.srcCPtr = ptr;
        //    msub.MWRIT2(new MmlDatum((byte)n);

        //    return enmFCOMPNextRtn.fcomp1;
        //}

        //private enmFCOMPNextRtn SETHEP()
        //{
        //    mucInfo.srcCPtr++;
        //    msub.MWRITE(new MmlDatum(0xff, 0xf2);

        //    int ptr = mucInfo.srcCPtr;
        //    int n = msub.REDATA(mucInfo.lin, ref ptr);
        //    mucInfo.srcCPtr = ptr;
        //    msub.MWRITE(new MmlDatum((byte)n, (byte)(n >> 8));// 2ﾊﾞｲﾄﾃﾞｰﾀ ｶｸ

        //    return enmFCOMPNextRtn.fcomp1;
        //}

        // *	BEFORE CODE	*

        private EnmFCOMPNextRtn SETBEF()
        {
            int ptr;
            int n;

            mucInfo.srcCPtr++;
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != '=') //0x3d
            {
                msub.MWRITE(new MmlDatum(0xfb), new MmlDatum((byte)-work.VDDAT));
                msub.MWRIT2(new MmlDatum((byte)work.BEFCO));
                TCLKSUB(work.BEFCO);

                msub.MWRIT2(new MmlDatum(work.BEFTONE[work.BFDAT]));
                msub.MWRITE(new MmlDatum(0xfb), new MmlDatum(work.VDDAT));

                return EnmFCOMPNextRtn.fcomp1;
            }

            ptr = mucInfo.srcCPtr;
            n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0416"));
            mucInfo.srcCPtr = ptr;

            if (n >= 10)
            {
                throw new MucException(
                     msg.get("E0417")
                    , mucInfo.row, mucInfo.col);
            }

            if (n == 0)
            {
                n++;
            }
            n--;

            work.BFDAT = (byte)n;

            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != ',')//0x2c
            {
                return EnmFCOMPNextRtn.fcomp1;
            }

            ptr = mucInfo.srcCPtr;
            n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0416"));
            mucInfo.srcCPtr = ptr;

            work.VDDAT = (byte)n;
            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	TOTAL VOLUME	*

        private EnmFCOMPNextRtn TOTALV()
        {
            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0418"));
            mucInfo.srcCPtr = ptr;

            work.TV_OFS = (byte)n;

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	REPEAT JUMP	**

        private EnmFCOMPNextRtn SETRJP()
        {
            mucInfo.srcCPtr++;

            msub.MWRIT2(new MmlDatum(0xfe));

            int HL = work.POINTC + 4;
            if (HL < 0)
            {
                throw new MucException(
                    msg.get("E0523")
                    , mucInfo.row, mucInfo.col);
            }

            if ((mucInfo.bufLoopStack.Get(HL) | mucInfo.bufLoopStack.Get(HL + 1)) != 0)
            {
                throw new MucException(
                    msg.get("E0419")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.bufLoopStack.Set(HL, (byte)work.MDATA);
            mucInfo.bufLoopStack.Set(HL + 1, (byte)(work.MDATA >> 8));
            HL += 4;
            work.MDATA += 2;

            mucInfo.bufLoopStack.Set(HL, (byte)work.tcnt[work.COMNOW]);
            mucInfo.bufLoopStack.Set(HL + 1, (byte)(work.tcnt[work.COMNOW] >> 8));

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *    KEY ON REVISE * added
        public EnmFCOMPNextRtn SETKON()
        {
            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0420"));
            mucInfo.srcCPtr = ptr;
            work.KEYONR = (byte)n;

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	KEY SHIFT(k)	*

        private EnmFCOMPNextRtn SETKST()
        {
            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0421"));
            mucInfo.srcCPtr = ptr;
            work.SIFTDAT = (byte)n;

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	KEY SHIFT(K)	*

        public EnmFCOMPNextRtn SETKS2()
        {
            char ch = mucInfo.lin.Item2.Length > (mucInfo.srcCPtr + 1) ? mucInfo.lin.Item2[mucInfo.srcCPtr + 1] : (char)0;
            if (ch == 'D')
            {
                return SETKeyOnDelay();
            }

            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0422"));
            mucInfo.srcCPtr = ptr;
            work.SIFTDA2 = (byte)n;

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETKeyOnDelay()
        {
            if (CHCHK() != ChannelType.FM)
            {
                throw new MucException(msg.get("E0518"), mucInfo.row, mucInfo.col);
            }

            int ptr = mucInfo.srcCPtr + 1;
            int[] n = new int[4];

            for (int i = 0; i < 4; i++)
            {
                n[i] = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0516"));
                if (n[i] > work.CLOCK) throw new MucException(string.Format(msg.get("E0517"), n[i]), mucInfo.lin.Item1, ptr);//ERRORIF
                if (n[i] < 0) throw new MucException(string.Format(msg.get("E0518"), n[i]), mucInfo.lin.Item1, ptr);//ERRORIF
            }

            mucInfo.srcCPtr = ptr;
            mucInfo.isDotNET = true;
            msub.MWRITE(new MmlDatum(0xff), new MmlDatum(0xf6));
            msub.MWRITE(new MmlDatum((byte)n[0]), new MmlDatum((byte)n[1]));
            msub.MWRITE(new MmlDatum((byte)n[2]), new MmlDatum((byte)n[3]));

            return EnmFCOMPNextRtn.fcomp1;
        }


        // *	!	*
        private EnmFCOMPNextRtn COMOVR()
        {
            return EnmFCOMPNextRtn.comovr;
        }

        private EnmFCOMPNextRtn STRET()
        {
            return EnmFCOMPNextRtn.comovr;
        }

        // **	TEMPO(TIMER_B) SET**

        private EnmFCOMPNextRtn TIMERB()
        {
            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0423"));
            mucInfo.srcCPtr = ptr;

            return TIMEB2((byte)n);
        }

        private EnmFCOMPNextRtn TIMEB2(byte n)
        {
            mucInfo.bufDst.Set(work.DATTBL - 1, new MmlDatum(n));// TIMER_B ﾆ ｱﾜｾﾙ

            if (work.COMNOW >= 3 && work.COMNOW < 6)
            {
                WriteWarning(msg.get("W0412"), mucInfo.row, mucInfo.col);
                return EnmFCOMPNextRtn.fcomp1;
            }
            if (n >= 253 && n <= 255)
            {
                WriteWarning(msg.get("W0413"), mucInfo.row, mucInfo.col);
            }

            msub.MWRITE(new MmlDatum(0xfa), new MmlDatum(0x26));
            msub.MWRIT2(new MmlDatum(n));
            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	NOIZE WAVE	**

        private EnmFCOMPNextRtn SETWAV()
        {

            ChannelType tp = CHCHK();
            if (tp != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0424")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0425"));
            mucInfo.srcCPtr = ptr;

            msub.MWRITE(new MmlDatum(0xf8), new MmlDatum((byte)n));// COM OF 'w'

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	MIX PORT	**

        private EnmFCOMPNextRtn SETMIX()
        {

            ChannelType tp = CHCHK();
            if (tp != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0426")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = (byte)msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0427"));
            mucInfo.srcCPtr = ptr;

            if (n == 1)
            {
                n = 8;
            }
            else if (n == 2)
            {
                n = 1;
            }
            else if (n == 3)
            {
                n = 0;
            }
            else if (n == 0)
            {
                n = 9;
            }
            else
            {
                throw new MucException(
                    msg.get("E0428")
                    , mucInfo.row, mucInfo.col);
            }

            msub.MWRITE(new MmlDatum(0xf7), new MmlDatum((byte)n));// COM OF 'P'
            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	SOFT ENVELOPE	**

        private EnmFCOMPNextRtn SETSEV()
        {

            ChannelType tp = CHCHK();
            if (tp != ChannelType.SSG)
            {
                throw new MucException(
                    msg.get("E0429")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0430"));
            mucInfo.srcCPtr = ptr;

            msub.MWRITE(new MmlDatum(0xfa), new MmlDatum((byte)n));// COM OF 'E'

            return SETSE1(5, "E0512", "E0513");//ﾉｺﾘ 5 PARAMETER
        }

        // **	Q**

        private EnmFCOMPNextRtn SETQLG()
        {
            int ptr;
            ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0431"));
            mucInfo.srcCPtr = ptr;
            work.quantize = n;
            msub.MWRITE(new MmlDatum(0xf3), new MmlDatum((byte)n));// COM OF 'q'

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	JUMP ADDRESS SET**

        private EnmFCOMPNextRtn SETJMP()
        {
            mucInfo.srcCPtr++;

            int HL = work.MDATA;
            int DE = HL - work.MU_TOP;
            int c = work.COMNOW;
            HL = work.DATTBL + c * 4 + 2;

            mucInfo.bufDst.Set(HL, new MmlDatum((byte)DE));
            HL++;
            mucInfo.bufDst.Set(HL, new MmlDatum((byte)(DE >> 8)));

            work.lcnt[c] = work.tcnt[c] + 1;// +1('L'ﾌﾗｸﾞﾉ ｶﾜﾘ)

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	SE DETUNE ﾉ ｾｯﾃｲ	**

        private EnmFCOMPNextRtn SETSE()
        {

            if (work.COMNOW != 2)
            {
                // 3 Ch ｲｶﾞｲﾅﾗ ERROR
                throw new MucException(
                    msg.get("E0432")
                    , mucInfo.row, mucInfo.col);
            }

            int ptr;
            string errCode_Fmt = "E0434";
            string errCode_Val = "E0435";
            int[] n = new int[4];

            ptr = mucInfo.srcCPtr;
            n[0] = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0433"));
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry) throw new MucException(msg.get(errCode_Fmt), mucInfo.row, mucInfo.col);
            if (mucInfo.ErrSign) throw new MucException(msg.get(errCode_Val), mucInfo.row, mucInfo.col);

            for (int i = 1; i < 4; i++)
            {
                char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
                if (c != ',') throw new MucException(msg.get(errCode_Fmt), mucInfo.row, mucInfo.col);
                mucInfo.srcCPtr++;
                ptr = mucInfo.srcCPtr;
                n[i] = msub.REDATA(mucInfo.lin, ref ptr);
                mucInfo.srcCPtr = ptr;
                if (mucInfo.Carry) throw new MucException(msg.get(errCode_Fmt), mucInfo.row, mucInfo.col);
                if (mucInfo.ErrSign) throw new MucException(msg.get(errCode_Val), mucInfo.row, mucInfo.col);
            }

            //値の範囲をチェック
            bool is16bit = false;
            for (int i = 0; i < 4; i++)
            {
                if (n[i] == (byte)n[i]) continue;
                is16bit = true;
                break;
            }

            if (!mucInfo.isDotNET && !is16bit)
            {
                msub.MWRIT2(new MmlDatum(0xf7));// COM OF 'S'
                for (int i = 0; i < 4; i++)
                    msub.MWRIT2(new MmlDatum((byte)n[i]));
                mucInfo.needNormalMucom = true;//既存のmucomであることを求めるフラグ
            }
            else
            {
                //既存のフォーマットを既に使っている時はエラーとする
                if (!mucInfo.isDotNET && mucInfo.needNormalMucom)
                    throw new MucException(msg.get("E0527"), mucInfo.row, mucInfo.col);

                mucInfo.isDotNET = true;
                msub.MWRIT2(new MmlDatum(0xf7));// COM OF 'S'
                for (int i = 0; i < 4; i++)
                {
                    msub.MWRIT2(new MmlDatum((byte)n[i]));
                    msub.MWRIT2(new MmlDatum((byte)(n[i] >> 8)));
                }
                mucInfo.needNormalMucom = true;
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETSE1(int b ,string errCode_Fmt,string errCode_Val)
        {
            do
            {
                char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ?
                    mucInfo.lin.Item2[mucInfo.srcCPtr]
                    : (char)0;
                if (c != ',')// 0x2c
                {
                    throw new MucException(
                        msg.get(errCode_Fmt)//E0434
                        , mucInfo.row, mucInfo.col);
                }

                mucInfo.srcCPtr++;
                int ptr = mucInfo.srcCPtr;
                int n = msub.REDATA(mucInfo.lin, ref ptr);
                mucInfo.srcCPtr = ptr;

                if (mucInfo.Carry)
                {
                    // NONDATA ﾅﾗERROR
                    throw new MucException(
                        msg.get(errCode_Fmt)//"E0434"
                        , mucInfo.row, mucInfo.col);
                }

                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get(errCode_Val)//"E0435"
                        , mucInfo.row, mucInfo.col);
                }

                msub.MWRIT2(new MmlDatum((byte)n));// SET DATA ONLY
                b--;
            } while (b != 0);

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETLPE()
        {
            int ptr;

            ptr = mucInfo.srcCPtr;
            int rep = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0436"));
            mucInfo.srcCPtr = ptr;
            if (rep < 0 || rep > 255)
            {
                WriteWarning(string.Format(msg.get("W0416"), rep), mucInfo.row, mucInfo.col);
            }

            msub.MWRIT2(new MmlDatum(0xf6));	// WRITE COM OF LOOP
            msub.MWRIT2(new MmlDatum((byte)rep)); // WRITE LOOP Co.
            int adr = work.MDATA;
            msub.MWRIT2(new MmlDatum((byte)rep)); // WRITE LOOP Co. (SPEAR)

            if (work.POINTC < work.LOOPSP)
            {
                throw new MucException(msg.get("E0437"), mucInfo.row, mucInfo.col);
            }

            int loopStackPtr = work.POINTC;
            work.POINTC -= 10;

            int n = mucInfo.bufLoopStack.Get(loopStackPtr)
                + mucInfo.bufLoopStack.Get(loopStackPtr + 1) * 0x100;
            adr -= n;

            mucInfo.bufDst.Set(n, new MmlDatum((byte)adr));// RSKIP JP ADR
            mucInfo.bufDst.Set(n + 1, new MmlDatum((byte)(adr >> 8)));

            int m = mucInfo.bufLoopStack.Get(loopStackPtr + 2)
                + mucInfo.bufLoopStack.Get(loopStackPtr + 3) * 0x100;// HL ﾊ LOOP ｦ ｶｲｼｼﾀ ｱﾄﾞﾚｽ

            int loopRetOfs = work.MDATA - m;//LOOP RET ADR OFFSET
            mucInfo.bufDst.Set(work.MDATA, new MmlDatum((byte)loopRetOfs));
            mucInfo.bufDst.Set(work.MDATA + 1, new MmlDatum((byte)(loopRetOfs >> 8)));// WRITE RET ADR OFFSET
            work.MDATA += 2;

            m = mucInfo.bufLoopStack.Get(loopStackPtr + 4);
            m += mucInfo.bufLoopStack.Get(loopStackPtr + 5) * 0x100;
            if (m != 0)
            {
                int DE = work.MDATA - 4;
                DE -= m;// loopStackPtr + 4;//DE as OFFSET
                mucInfo.bufDst.Set(m, new MmlDatum((byte)DE));// loopStackPtr + 4, (byte)DE);
                mucInfo.bufDst.Set(m + 1, new MmlDatum((byte)(DE >> 8))); //loopStackPtr + 5, (byte)(DE >> 8));
            }

            work.REPCOUNT--;

            int backupAdr = mucInfo.bufLoopStack.Get(loopStackPtr + 6)
                + mucInfo.bufLoopStack.Get(loopStackPtr + 7) * 0x100;
            backupAdr += work.tcnt[work.COMNOW] * (rep - 1);

            int ofs = mucInfo.bufLoopStack.Get(loopStackPtr + 8)
                + mucInfo.bufLoopStack.Get(loopStackPtr + 9) * 0x100;
            if (ofs == 0)
            {
                // '/'ﾊ ｶｯｺﾅｲﾆ ﾂｶﾜﾚﾃﾅｲ
                ofs = work.tcnt[work.COMNOW];
            }

            backupAdr += ofs;
            work.tcnt[work.COMNOW] = backupAdr;

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	LOOP START	**

        private EnmFCOMPNextRtn SETLPS()
        {

            mucInfo.srcCPtr++;

            msub.MWRIT2(new MmlDatum(0xf5));// COM OF LOOPSTART
            work.POINTC += 10;

            mucInfo.bufLoopStack.Set(work.POINTC + 0, (byte)work.MDATA);// SAVE REWRITE ADR
            mucInfo.bufLoopStack.Set(work.POINTC + 1, (byte)(work.MDATA >> 8));

            work.MDATA += 2;

            mucInfo.bufLoopStack.Set(work.POINTC + 2, (byte)work.MDATA);// SAVE LOOP START ADR
            mucInfo.bufLoopStack.Set(work.POINTC + 3, (byte)(work.MDATA >> 8));
            mucInfo.bufLoopStack.Set(work.POINTC + 4, 0);
            mucInfo.bufLoopStack.Set(work.POINTC + 5, 0);
            mucInfo.bufLoopStack.Set(work.POINTC + 6, (byte)work.tcnt[work.COMNOW]);
            mucInfo.bufLoopStack.Set(work.POINTC + 7, (byte)(work.tcnt[work.COMNOW] >> 8));
            mucInfo.bufLoopStack.Set(work.POINTC + 8, 0);
            mucInfo.bufLoopStack.Set(work.POINTC + 9, 0);

            work.tcnt[work.COMNOW] = 0;//ﾄｰﾀﾙ ｸﾛｯｸ ｸﾘｱ

            work.REPCOUNT++;
            if (work.REPCOUNT > 16)
            {
                throw new MucException(
                    msg.get("E0438")
                    , mucInfo.row, mucInfo.col);
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	REST**

        private EnmFCOMPNextRtn SETRST()
        {
            int ptr;
            int kotae;

            mucInfo.srcCPtr++;
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;

            if (c == '%')// 0x25
            {
                mucInfo.srcCPtr++;
                ptr = mucInfo.srcCPtr;
                kotae = msub.REDATA(mucInfo.lin, ref ptr);
                if (kotae < 0 || kotae > 255)
                {
                    WriteWarning(string.Format(msg.get("W0403"), kotae), mucInfo.row, mucInfo.col);
                }
                kotae = (byte)kotae;
                mucInfo.srcCPtr = ptr;
                if (mucInfo.Carry)
                {
                    kotae = (byte)work.COUNT;
                    mucInfo.srcCPtr--;
                    c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                        ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                        : (char)0;
                    if (c == '.')// 0x2e
                    {
                        //厳密には挙動が違いますが、この文法は使用できないことを再現させるため
                        throw new MucException(
                            msg.get("E0439")
                            , mucInfo.row, mucInfo.col);
                        //mucInfo.srcCPtr++;
                        //kotae += (byte)(kotae >> 1);// /2
                    }
                }
                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get("E0439")
                        , mucInfo.row, mucInfo.col);
                }
            }
            else
            {
                ptr = mucInfo.srcCPtr;
                kotae = msub.REDATA(mucInfo.lin, ref ptr);
                if (kotae < 0 || kotae > 255)
                {
                    WriteWarning(string.Format(msg.get("W0403"), kotae), mucInfo.row, mucInfo.col);
                }
                kotae = (byte)kotae;
                if (kotae != 0) kotae = (byte)(work.CLOCK / kotae);
                mucInfo.srcCPtr = ptr;
                if (mucInfo.Carry)
                {
                    if (c == '^' || c == '&')
                    {
                        WriteWarning(msg.get("W0401"), mucInfo.row, mucInfo.col);
                    }
                    kotae = (byte)work.COUNT;
                    mucInfo.srcCPtr--;
                }
                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get("E0439")
                        , mucInfo.row, mucInfo.col);
                }

                c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                    : (char)0;
                if (c == '.')// 0x2e
                {
                    mucInfo.srcCPtr++;
                    kotae += (kotae >> 1);// /2
                    if (kotae < 0 || kotae > 255)
                    {
                        WriteWarning(string.Format(msg.get("W0403"), kotae), mucInfo.row, mucInfo.col);
                    }
                    kotae = (byte)kotae;

                    c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                        ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                        : (char)0;
                    if (c == '.')
                    {
                        WriteWarning(msg.get("W0402"), mucInfo.row, mucInfo.col);
                    }
                }
            }

            work.tcnt[work.COMNOW] += kotae;

            if (work.BEFRST != 0)// ｾﾞﾝｶｲｶｳﾝﾀ ﾜｰｸ(ﾌﾗｸﾞ)
            {
                kotae += work.BEFRST;
                if (kotae < 0 || kotae > 255)
                {
                    WriteWarning(string.Format(msg.get("W0403"), kotae), mucInfo.row, mucInfo.col);
                }
                kotae = (byte)kotae;
                work.MDATA--;
            }

            List<object> args = new List<object>();
            args.Add(kotae);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row
                , mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , ""
                , "YM2608", 0, 0
                , work.COMNOW);


            while (kotae > 0x6f)
            {
                kotae -= 0x6f;
                msub.MWRIT2(new MmlDatum(
                    enmMMLType.Rest
                    , args
                    , lp
                    , (byte)(0b1110_1111)
                    ));
            }
            work.BEFRST = kotae;
            msub.MWRIT2(new MmlDatum(
                    enmMMLType.Rest
                    , args
                    , lp
                    , (byte)(kotae | 0b1000_0000)
                    ));// SET REST FLAG

            return EnmFCOMPNextRtn.fcomp12;
        }

        private EnmFCOMPNextRtn SETMOD()
        {
            int ix = 0;

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry)
            {
                return SETMO2();// NONDATA ﾅﾗ 2nd COM PRC
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0440")
                    , mucInfo.row, mucInfo.col);
            }

            work.LFODAT[ix] = 0;
            msub.MWRIT2(new MmlDatum(0xf4));// COM OF 'M'
            msub.MWRIT2(new MmlDatum(0x00));// 2nd COM
            work.LFODAT[ix + 1] = (byte)n;
            msub.MWRIT2(new MmlDatum((byte)n));// SET DELAY

            // --	ｶｳﾝﾀ ｾｯﾄ	--
            //SETMO1:
            char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != ',')//0x2c
            {
                throw new MucException(
                    msg.get("E0441")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0441")
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0440")
                    , mucInfo.row, mucInfo.col);
            }

            work.LFODAT[ix + 2] = (byte)n;
            msub.MWRIT2(new MmlDatum((byte)n));// SET DATA ONLY
                                             // --	SET VECTOR	--
            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != ',')//0x2c
            {
                throw new MucException(
                    msg.get("E0441")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0441")
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0440")
                    , mucInfo.row, mucInfo.col);
            }

            ChannelType tp = CHCHK();
            if (tp == ChannelType.SSG)
            {
                n = -n;//ssgの場合はdeの符号を反転
            }

            msub.MWRIT2(new MmlDatum((byte)n));
            work.LFODAT[ix + 3] = (byte)n;
            msub.MWRIT2(new MmlDatum((byte)(n >> 8)));
            work.LFODAT[ix + 4] = (byte)(n >> 8);

            // --	SET DEPTH	--
            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != ',')//0x2c
            {
                throw new MucException(
                    msg.get("E0441")
                    , mucInfo.row, mucInfo.col);
            }

            ptr = mucInfo.srcCPtr;
            n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0442"));
            mucInfo.srcCPtr = ptr;
            work.LFODAT[ix + 5] = (byte)n;
            msub.MWRIT2(new MmlDatum((byte)n));// SET DATA ONLY

            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c != ',')//0x2c
            {
                return EnmFCOMPNextRtn.fcomp1;
            }

            ptr = mucInfo.srcCPtr;
            msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0442"));
            mucInfo.srcCPtr = ptr;

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETMO2()
        {
            int iy;
            int ptr;
            int n;

            //mucInfo.srcCPtr++;
            // COM OF 'M'
            msub.MWRIT2(new MmlDatum(0xf4));// COM ONLY
            if (work.SECCOM == 'F')//0x46
            {
                ptr = mucInfo.srcCPtr;
                n = msub.REDATA(mucInfo.lin, ref ptr);
                mucInfo.srcCPtr = ptr;
                if (mucInfo.Carry)
                {
                    throw new MucException(
                        msg.get("E0443")
                        , mucInfo.row, mucInfo.col);
                }
                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get("E0444")
                        , mucInfo.row, mucInfo.col);
                }

                if (n != 0 && n != 1)
                {
                    throw new MucException(
                        msg.get("E0445")
                        , mucInfo.row, mucInfo.col);
                }
                n++;// SECOND COM
                work.LFODAT[0] = (byte)n;
                msub.MWRIT2(new MmlDatum((byte)n));// 'MF0' or 'MF1'
                return EnmFCOMPNextRtn.fcomp1;
            }

            //MO4:
            iy = 1;
            if (work.SECCOM == 0x57)//'W'
            {
                return MODP2(iy, 3, msg.get("E0446"));// COM OF 'MW'
            }

            //M05:
            iy++;
            if (work.SECCOM == 0x43)//'C'
            {
                return MODP2(iy, 4, msg.get("E0447"));// 'MC'
            }

            //M06:
            iy++;
            if (work.SECCOM == 0x4c)//'L'
            {
                // 'ML'
                work.LFODAT[0] = 5;
                msub.MWRIT2(new MmlDatum(0x5));

                ptr = mucInfo.srcCPtr;
                n = msub.REDATA(mucInfo.lin, ref ptr);
                mucInfo.srcCPtr = ptr;
                if (mucInfo.Carry)
                {
                    throw new MucException(
                        msg.get("E0448")
                        , mucInfo.row, mucInfo.col);
                }
                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get("E0449")
                        , mucInfo.row, mucInfo.col);
                }
                work.LFODAT[iy] = (byte)n;
                work.LFODAT[iy + 1] = (byte)(n >> 8);
                work.LFODAT[iy + 2] = 0;
                msub.MWRITE(new MmlDatum((byte)n), new MmlDatum((byte)(n >> 8)));

                return EnmFCOMPNextRtn.fcomp1;
            }
            //M07:
            iy += 3;
            if (work.SECCOM == 0x44)//'D'
            {
                return MODP2(iy, 6, msg.get("E0450"));//'D'
            }
            //M08:
            if (work.SECCOM != 0x54)//'T'
            {
                throw new MucException(
                    msg.get("E0451")
                    , mucInfo.row, mucInfo.col);
            }

            ChannelType tp = CHCHK();

            //MT はFMではTL LFO , SSGでは音量LFOスイッチ

            //if (tp == ChannelType.SSG)
            //{
            //    throw new MucException(
            //        msg.get("E0452")
            //        , mucInfo.row, mucInfo.col);
            //}

            mucInfo.isDotNET = true;
            ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0453")
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0454")
                    , mucInfo.row, mucInfo.col);
            }

            msub.MWRITE(new MmlDatum(7), new MmlDatum((byte)n));
            if ((byte)n == 0 || tp== ChannelType.SSG)
            {
                return EnmFCOMPNextRtn.fcomp1;
            }

            //M083:
            mucInfo.srcCPtr++;
            ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0455")
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0456")
                    , mucInfo.row, mucInfo.col);
            }

            msub.MWRIT2(new MmlDatum((byte)n));

            return EnmFCOMPNextRtn.fcomp1;
        }

        // **	PARAMETER SET	**

        // IN: A<= COM No.

        public EnmFCOMPNextRtn MODP2(int iy, byte a, string typ)
        {
            work.LFODAT[0] = a;
            msub.MWRIT2(new MmlDatum(a));

            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;
            if (mucInfo.Carry)
            {
                throw new MucException(
                    string.Format(msg.get("E0457"), typ)
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    string.Format(msg.get("E0458"), typ)
                    , mucInfo.row, mucInfo.col);
            }

            work.LFODAT[iy] = (byte)n;
            msub.MWRIT2(new MmlDatum((byte)n));

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETREG()
        {
            ChannelType tp = CHCHK();
            if (tp == ChannelType.SSG)
            {
                WriteWarning(msg.get("W0406"), mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                mucInfo.srcCPtr--;
                return SETR2();
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0459")
                    , mucInfo.row, mucInfo.col);
            }
            if (0xb2 < n)
            {
                throw new MucException(
                    msg.get("E0460")
                    , mucInfo.row, mucInfo.col);
            }

            return SETR1(n);
        }

        private EnmFCOMPNextRtn SETR1(int n)
        {
            msub.MWRITE(new MmlDatum(0xfa), new MmlDatum((byte)n));// COM OF 'y'

            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
            if (c != ',')//0x2c
            {
                throw new MucException(
                    msg.get("E0461")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0462")
                    , mucInfo.row, mucInfo.col);
            }
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0463")
                    , mucInfo.row, mucInfo.col);
            }

            msub.MWRIT2(new MmlDatum((byte)n));// SET DATA ONLY

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETR2()
        {
            // --	yXX(ﾓｼﾞﾚﾂ),OpNo.,DATA	--

            int i = 0;
            int ptr;

            do
            {
                ptr = mucInfo.srcCPtr;
                mucInfo.Carry = msub.MCMP_DE(DMDAT[i], mucInfo.lin, ref ptr);
                if (mucInfo.Carry)
                {
                    mucInfo.srcCPtr = ptr;
                    break;
                }
                i++;
            } while (i < 7);
            if (i == 7)
            {
                throw new MucException(
                    msg.get("E0464")
                    , mucInfo.row, mucInfo.col);
            }

            byte SETR9_VAL = (byte)(i * 16);
            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
            if (c != ',')//0x2c
            {
                throw new MucException(
                    msg.get("E0461")
                    , mucInfo.row, mucInfo.col);
            }

            ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0465"));// ｵﾍﾟﾚｰﾀｰ No.
            mucInfo.srcCPtr = ptr;

            if (n == 0 || n > 4)
            {
                throw new MucException(
                    msg.get("E0466")
                    , mucInfo.row, mucInfo.col);
            }

            // op2<->op3
            if (n == 3)
            {
                n = 2;
            }
            else if (n == 2)
            {
                n = 3;
            }

            n--;
            n *= 4;

            byte ch = (byte)work.COMNOW;

            if (ch >= 11) ch -= 11;

            // 範囲外(0未満、10超過) or SSG
            if (ch < 0 || (ch >= 3 && ch < 7) || ch >= 10)
            {
                throw new MucException(
                    string.Format(msg.get("E0467"), (char)('A' + ch))
                    , mucInfo.row, mucInfo.col);
            }
            if (ch > 6) ch -= 7;

            n += ch;// Op*4+CH No.
            n += 0x30 + SETR9_VAL;
            return SETR1(n);
        }

        public string[] DMDAT = new string[]
        {
            "DM\0"
            ,"TL\0"
            ,"KA\0"
            ,"DR\0"
            ,"SR\0"
            ,"SL\0"
            ,"SE\0"
        };

        private EnmFCOMPNextRtn SETTIE()
        {
            SETTI1();
            return EnmFCOMPNextRtn.fcomp12;
        }

        public void SETTI1()
        {
            mucInfo.srcCPtr++;
            work.TIEFG = 0xfd;
            msub.MWRIT2(new MmlDatum(0xfd));
        }

        public EnmFCOMPNextRtn SETTI2()
        {
            SETTI1();
            byte a = work.BEFTONE[0];

            return FCOMP13(a);
        }


        private EnmFCOMPNextRtn SETVUP()
        {
            if (work.VolumeUDFLG != 0)
            {
                return SVD2();
            }
            return SVU2();
        }

        public EnmFCOMPNextRtn SETVDW()
        {
            if (work.VolumeUDFLG != 0)
            {
                return SVU2();
            }
            return SVD2();
        }

        public EnmFCOMPNextRtn SVU2()
        {

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0468")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                mucInfo.srcCPtr--;
                n = 1;// ﾍﾝｶ 1
            }
            work.VOLUME += n;
            msub.MWRITE(new MmlDatum(0xfb), new MmlDatum((byte)n));// COM OF ')'

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETODW()
        {
            SOD1();
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0469")
                    , mucInfo.row, mucInfo.col);
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        public EnmFCOMPNextRtn SVD2()
        {

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0470")
                    , mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                mucInfo.srcCPtr--;
                n = 1;// ﾍﾝｶ 1
            }
            work.VOLUME -= n;
            msub.MWRITE(new MmlDatum(0xfb), new MmlDatum((byte)-n));// ')' ﾉ ﾊﾝﾀｲ ﾊ '('

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETOUP()
        {

            SOU1();
            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0471")
                    , mucInfo.row, mucInfo.col);
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        public void SOU1()
        {
            mucInfo.srcCPtr++;

            if (work.OctaveUDFLG != 0)
            {
                SOD2();
                return;
            }
            SOU2();
        }

        public void SOU2()
        {
            if (work.OCTAVE == 7)
            {
                mucInfo.Carry = true;
                return;
            }

            work.OCTAVE++;
            mucInfo.Carry = false;
        }

        public void SOD1()
        {
            mucInfo.srcCPtr++;

            if (work.OctaveUDFLG != 0)
            {
                SOU2();
                return;
            }
            SOD2();
        }

        public void SOD2()
        {
            if (work.OCTAVE == 0)
            {
                mucInfo.Carry = true;
                return;
            }
            work.OCTAVE--;
            mucInfo.Carry = false;
        }

        private EnmFCOMPNextRtn SETVOL()
        {
            if (work.COMNOW == 10)
            {
                return SETVOL_ADPCM();
            }

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0472")
                    , mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                n = work.VOLINT;
            }

            n = (byte)n;
            work.VOLUME = n;
            work.VOLINT = n;

            ChannelType tp = CHCHK();
            List<object> args = new List<object>();
            args.Add(n);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , tp == ChannelType.FM ? "FM" : "SSG"
                , "YM2608", 0, 0, work.COMNOW);

            if (work.COMNOW != 6)
            {
                n += work.TV_OFS;
                if (work.COMNOW < 3 || work.COMNOW > 6)
                {
                    n += 4;
                }

                msub.MWRITE(
                    new MmlDatum(enmMMLType.Volume, args, lp, 0xf1)
                    , new MmlDatum((byte)n));// COM OF 'v'
                return EnmFCOMPNextRtn.fcomp1;
            }

            return SETVOL_Rhythm(n);
        }

        private EnmFCOMPNextRtn SETVOL_Rhythm(int n)
        {
            List<object> args = new List<object>();
            args.Add(n);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , "RHYTHM"
                , "YM2608", 0, 0, work.COMNOW);

            // -	DRAM V. -
            n += work.TV_OFS;
            msub.MWRITE(new MmlDatum(enmMMLType.Volume, args, lp, 0xf1), new MmlDatum((byte)n));

            for (int i = 0; i < 6; i++)
            {
                char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
                if (c != ',')//','
                {
                    throw new MucException(
                        msg.get("E0473")
                        , mucInfo.row, mucInfo.col);
                }

                int ptr = mucInfo.srcCPtr;
                n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0474"));
                if (mucInfo.ErrSign)
                    throw new MucException(
                        msg.get("E0475")
                        , mucInfo.row, mucInfo.col);
                mucInfo.srcCPtr = ptr;

                msub.MWRIT2(new MmlDatum((byte)n));
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETVOL_ADPCM()
        {

            // -	PCMVOL	-
            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0476")
                    , mucInfo.row, mucInfo.col);
            mucInfo.srcCPtr = ptr;

            if (!mucInfo.Carry)
            {
                if (mucInfo.ErrSign)
                {
                    throw new MucException(
                        msg.get("E0476")
                        , mucInfo.row, mucInfo.col);
                }

                List<object> args = new List<object>();
                args.Add(n);
                LinePos lp = new LinePos(
                    mucInfo.fnSrcOnlyFile
                    , mucInfo.row, mucInfo.col
                    , mucInfo.srcCPtr - mucInfo.col + 1
                    , "ADPCM"
                    , "YM2608", 0, 0, work.COMNOW);

                n = (byte)n;
                work.VOLUME = n;
                work.VOLINT = n;
                n += work.TV_OFS + 4;
                if ((byte)n >= 246 && (byte)n <= 251 && mucInfo.VM == 0)
                {
                    WriteWarning(msg.get("W0411"), mucInfo.row, mucInfo.col);
                }
                msub.MWRITE(new MmlDatum(enmMMLType.Volume, args, lp, 0xf1), new MmlDatum((byte)n));
                return EnmFCOMPNextRtn.fcomp1;
            }

            msub.MWRITE(new MmlDatum(0xff), new MmlDatum(0xf0));
            mucInfo.srcCPtr--;
            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
            if (c != 'm')// vm command
            {
                throw new MucException(
                    string.Format(msg.get("E0477"), c)
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            ptr = mucInfo.srcCPtr;
            n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0478")
                    , mucInfo.row, mucInfo.col);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0479")
                    , mucInfo.row, mucInfo.col);
            }

            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0478")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.VM = n;
            msub.MWRIT2(new MmlDatum((byte)n));// SET DATA ONLY

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETDT()
        {
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0480"));
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0481")
                    , mucInfo.row, mucInfo.col);
            mucInfo.srcCPtr = ptr;

            List<object> args = new List<object>();
            args.Add(n);

            ChannelType tp = CHCHK();
            if (tp == ChannelType.SSG)
            {
                n = -n;
            }

            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , tp == ChannelType.FM ? "FM" : (tp == ChannelType.SSG ? "SSG" : (tp == ChannelType.RHYTHM ? "RHYTHM" : "ADPCM"))
                , "YM2608", 0, 0, work.COMNOW); ;
            msub.MWRITE(new MmlDatum(
                 enmMMLType.Detune
                 , args
                 , lp
                 , 0xf2
                ), new MmlDatum((byte)n));// COM OF 'D'
            msub.MWRIT2(new MmlDatum((byte)(n >> 8)));

            char c =
                mucInfo.lin.Item2.Length > mucInfo.srcCPtr
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            mucInfo.srcCPtr++;
            if (c != 0x2b)//'+'
            {
                c = (char)0;
                mucInfo.srcCPtr--;
            }
            msub.MWRIT2(new MmlDatum((byte)c));

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETLIZ()
        {
            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr+1 ? mucInfo.lin.Item2[mucInfo.srcCPtr+1] : (char)0;
            if (c == '%')//0x2c
            {
                mucInfo.srcCPtr++;
                return SETDCO();
            }

            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0482"));
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0483")
                    );
            mucInfo.srcCPtr = ptr;

            if (work.CLOCK < n)
            {
                throw new MucException(
                    string.Format(msg.get("E0484"), work.CLOCK, n)
                    , mucInfo.row, mucInfo.col);
            }

            work.COUNT = work.CLOCK / n;
            int cnt = work.COUNT;
            int bf = 0;

            do
            {
                c =
                    mucInfo.lin.Item2.Length > mucInfo.srcCPtr
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                    : (char)0;
                if (c != 0x2e)//'.'
                    break;

                mucInfo.srcCPtr++;
                cnt >>= 1;
                bf += cnt;
            } while (true);

            work.COUNT += bf;

            return EnmFCOMPNextRtn.fcomp12;
        }

        private EnmFCOMPNextRtn SETOCT()
        {

            mucInfo.srcCPtr++;
            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0485")
                    , mucInfo.row, mucInfo.col);
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                n = work.OCTINT;
                n++;
            }

            n--;
            if (n >= 8)
            {
                // OCTAVE > 8?
                throw new MucException(
                    msg.get("E0486")
                    , mucInfo.row, mucInfo.col);
            }

            work.OCTAVE = n;
            work.OCTINT = n;

            return EnmFCOMPNextRtn.fcomp1;
        }

        private EnmFCOMPNextRtn SETCOL()
        {

            //音色名による指定か
            mucInfo.srcCPtr++;
            char c = mucInfo.lin.Item2.Length > mucInfo.srcCPtr
                ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                : (char)0;
            if (c == '\"')
            {
                SETVN();//文字列による指定
                return EnmFCOMPNextRtn.fcomp1;
            }

            //mucInfo.srcCPtr++;
            //c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
            //if (c == '=')
            //{
            //    throw new MucException(
            //        msg.get("E0487")
            //        , mucInfo.row, mucInfo.col);
            //}
            //mucInfo.srcCPtr -= 2;
            mucInfo.srcCPtr--;

            //数値を取得する
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0488"));
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0489")
                    , mucInfo.row, mucInfo.col);
            mucInfo.srcCPtr = ptr;

            ChannelType tp = CHCHK();
            if (tp == ChannelType.SSG)
            {
                //音色番号チェック
                if (n < 0 || n > 15)
                {
                    throw new MucException(
                        string.Format(msg.get("E0508"), n)
                        , mucInfo.row, mucInfo.col);
                }
            }

            c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length ? mucInfo.lin.Item2[mucInfo.srcCPtr] : (char)0;
            if (c == '=')
            {
                return SETSSGPreset(n);
            }
                        
            //通常の音色指定

            if (tp == ChannelType.SSG)
            {
                return STCL5(n);//SSG
            }
            if (tp == ChannelType.FM)
            {
                //音色番号チェック
                if (n == 0 || n == 1)
                {
                    WriteWarning(msg.get("W0410"), mucInfo.row, mucInfo.col);
                }

                STCL2(n);//FM
                return EnmFCOMPNextRtn.fcomp1;
            }

            return STCL72(n);//RHY&PCM
        }

        private EnmFCOMPNextRtn SETSSGPreset(int n)
        {
            msub.MWRITE(new MmlDatum(0xfc), new MmlDatum((byte)n));//@= command

            int ptr;
            ptr = mucInfo.srcCPtr;
            n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0509"));
            mucInfo.srcCPtr = ptr;
            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0510")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.isDotNET = true;
            msub.MWRIT2(new MmlDatum((byte)n));//一つ目のパラメータをセット

            return SETSE1(5, "E0510", "E0511");
        }

        public void SETVN()
        {
            ChannelType tp = CHCHK();
            if (tp != ChannelType.FM)
            {
                throw new MucException(
                    msg.get("E0490")
                    , mucInfo.row, mucInfo.col);
            }

            mucInfo.srcCPtr++;
            int voiBPtr = 0x20 + 26;// 0x6020 + 26;
            int num = 1;
            //var unicodeByte = Encoding.Unicode.GetBytes(mucInfo.lin.Item2);
            var sjis = enc.GetSjisArrayFromString(mucInfo.lin.Item2);
            // Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding("shift_jis"), unicodeByte);

            do
            {
                int srcPtr = mucInfo.srcCPtr;
                int voiPtr = voiBPtr;

                int chcnt = 6;
                do
                {
                    byte v = (mucInfo.voiceData != null && mucInfo.voiceData.Length > voiPtr) ? (byte)mucInfo.voiceData[voiPtr] : (byte)0;
                    byte s = sjis[srcPtr];
                    if (v != s)
                    {
                        //Common.WriteLine("{0:X06} v:{1} s:{2}", voiPtr, (char)mucInfo.voiceData[voiPtr], (char)mucInfo.lin.Item2[srcPtr]);
                        break;
                    }

                    srcPtr++;
                    voiPtr++;
                    if (sjis[srcPtr] == 0x22)//'"'
                    {
                        if (chcnt != 1 && mucInfo.voiceData[voiPtr] != 0x20)
                        {
                            throw new MucException(
                                msg.get("E0491")
                                , mucInfo.row, mucInfo.col);
                        }

                        mucInfo.srcCPtr = srcPtr + 1;
                        STCL2(num);
                        return;
                    }

                    chcnt--;
                    if (chcnt == 0)
                    {
                        throw new MucException(
                            msg.get("E0491")
                            , mucInfo.row, mucInfo.col);
                    }
                } while (chcnt != 0);

                voiBPtr += 32;
                num++;

            } while (num != 256);

            throw new MucException(
                msg.get("E0491")
                , mucInfo.row, mucInfo.col);
        }

        public void STCL2(int num)      // FM
        {
            num++;

            List<object> args = new List<object>();
            args.Add(0);//dummy
            args.Add(num - 1);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , "FM"
                , "YM2608", 0, 0, work.COMNOW);

            int voiceIndex = CCVC(num, mucInfo.bufDefVoice);// --	VOICE ｶﾞ ﾄｳﾛｸｽﾞﾐｶ?	--
            if (voiceIndex != -1)
            {
                msub.MWRITE(
                    new MmlDatum(enmMMLType.Instrument, args, lp, 0xf0)
                    , new MmlDatum((byte)(voiceIndex - 1))
                    );
                return;
            }

            voiceIndex = CWVC(num, mucInfo.bufDefVoice);// --	WORK ﾆ ｱｷ ｶﾞ ｱﾙｶ?	--
            if (voiceIndex != -1)
            {

                msub.MWRITE(
                    new MmlDatum(enmMMLType.Instrument, args, lp, 0xf0)
                    , new MmlDatum((byte)(voiceIndex - 1))
                    );
                return;
            }

            throw new MucException(
                msg.get("E0492")
                , mucInfo.row, mucInfo.col);
        }

        public EnmFCOMPNextRtn STCL5(int num)
        {

            List<object> args = new List<object>();
            args.Add(0);//dummy
            args.Add(num);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , "SSG"
                , "YM2608", 0, 0, work.COMNOW);

            msub.MWRITE(
                new MmlDatum(enmMMLType.Instrument, args, lp, 0xf0)
                , new MmlDatum((byte)num));

            if (work.PSGMD != 0)
            {
                return EnmFCOMPNextRtn.fcomp1;
            }

            int HL = (byte)num * 16;
            msub.MWRIT2(new MmlDatum(0xf7));
            msub.MWRIT2(new MmlDatum(SSGLIB[HL]));
            HL += 8;
            if (SSGLIB[HL] == 1)
            {
                return EnmFCOMPNextRtn.fcomp1;
            }

            int DE = work.MDATA;
            mucInfo.bufDst.Set(DE, new MmlDatum(0xf4));
            DE++;
            for (int b = 0; b < 6; b++)
            {
                mucInfo.bufDst.Set(DE++, new MmlDatum(SSGLIB[HL + b]));
                work.LFODAT[b] = SSGLIB[HL + b];
            }
            work.MDATA = DE;

            return EnmFCOMPNextRtn.fcomp1;
        }

        public EnmFCOMPNextRtn STCL72(int num)
        {
            ChannelType tp = CHCHK();

            List<object> args = new List<object>();
            args.Add(0);//dummy
            args.Add(num);
            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row, mucInfo.col
                , mucInfo.srcCPtr - mucInfo.col + 1
                , tp == ChannelType.ADPCM ? "ADPCM" : "RHYTHM"
                , "YM2608", 0, 0, work.COMNOW);

            msub.MWRITE(new MmlDatum(enmMMLType.Instrument, args, lp, 0xf0), new MmlDatum((byte)num));

            if (tp == ChannelType.ADPCM) work.pcmFlag = 1;

            return EnmFCOMPNextRtn.fcomp1;
        }

        public byte[] SSGLIB = new byte[] {
            8,0			// ﾉｰﾏﾙ
            ,255,255,255,255,0,255 // E
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ(1)
            ,255,255,255,200,0,10
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ(2)
            ,255,255,255,200,1,10
            ,1,0,0,0,0,0,0,0,

            8,0			//ｺﾅﾐ+LFO(1)
            ,255,255,255,190,0,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ｺﾅﾐ+LFO(2)
            ,255,255,255,190,1,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ｺﾅﾐ(3)
            ,255,255,255,170,0,10
            ,1,0,0,0,0,0,0,0,

            //5
            8,0			//ｾｶﾞ ﾀｲﾌﾟ
            ,40,70,14,190,0,15
            ,0,16,1,24,0,5,0,0,

            8,0			//ｽﾄﾘﾝｸﾞ ﾀｲﾌﾟ
            ,120,030,255,255,0,10
            ,0,16,1,25,0,4,0,0,

            8,0			//ﾋﾟｱﾉ･ﾊｰﾌﾟ ﾀｲﾌﾟ
            ,255,255,255,225,8,15
            ,1,0,0,0,0,0,0,0,

            1,0			//ｸﾛｰｽﾞ ﾊｲﾊｯﾄ
            ,255,255,255,1,255,255
            ,1,0,0,0,0,0,0,0,

            1,0			//ｵｰﾌﾟﾝ ﾊｲﾊｯﾄ
            ,255,255,255,200,8,255
            ,1,0,0,0,0,0,0,0,

            //10
            8,0			//ｼﾝｾﾀﾑ･ｼﾝｾｷｯｸ
            ,255,255,255,220,20,8
            ,0,1,1,0x2C,1,0x0FF,0,0,

            8,0			//UFO
            ,255,255,255,255,0,10
            ,0,1,1,0x70,0x0FE,4,0,0,

            8,0			//FALLING
            ,255,255,255,255,0,10
            ,0,1,1,0x50,00,255,0,0,

            8,0			//ﾎｲｯｽﾙ
            ,120,80,255,255,0,255
            ,0,1,1,06,0x0FF,1,0,0,

            8,0			//BOM!
            ,255,255,255,220,0,255
            ,0,1,1,0xB8,0x0B,255,0,0

        };

        // --	VOICE ｶﾞ ﾄｳﾛｸｽﾞﾐｶ?	--

        public int CCVC(int num, AutoExtendList<int> buf)
        {
            for (int b = 0; b < 256; b++)
            {
                if (num == buf.Get(b))
                {
                    return b + 1;// VOICE ﾊ ｽﾃﾞﾆ ﾄｳﾛｸｽﾞﾐ
                }
            }

            return -1;
        }

        // --	WORK ﾆ ｱｷ ｶﾞ ｱﾙｶ?	--

        public int CWVC(int num, AutoExtendList<int> buf)
        {
            for (int b = 0; b < 256; b++)
            {
                if (0 == buf.Get(b))
                {
                    // WORK ﾆ ｱｷ ｱﾘ
                    buf.Set(b, num);
                    return b + 1;
                }
            }

            return -1;//空き無し
        }

        public ChannelType CHCHK()
        {
            var CurrentChannel = work.COMNOW;
            if (CurrentChannel >= 11) CurrentChannel -= 11;

            if (CurrentChannel >= 3 && CurrentChannel < 6)
            {
                return ChannelType.SSG;
            }

            if (CurrentChannel == 6) return ChannelType.RHYTHM;

            if (CurrentChannel < 10)
            {
                return ChannelType.FM;
            }

            return ChannelType.ADPCM;
        }

        public enum ChannelType
        {
            FM, SSG, RHYTHM, ADPCM
        }

        private EnmFCOMPNextRtn SETCLK()
        {
            int ptr = mucInfo.srcCPtr;
            int n = msub.ERRT(mucInfo.lin, ref ptr, msg.get("E0493"));
            if (mucInfo.ErrSign)
                throw new MucException(
                    msg.get("E0494")
                    , mucInfo.row, mucInfo.col);

            int len = ptr - mucInfo.srcCPtr;
            mucInfo.srcCPtr = ptr;

            if (mucInfo.isIDE)
            {
                mucInfo.isDotNET = true;
                List<object> args = new List<object>();
                args.Add(n);
                LinePos lp = new LinePos(
                    mucInfo.fnSrcOnlyFile
                    ,mucInfo.row
                    ,mucInfo.col
                    ,len
                    ,""
                    ,"YM2608"
                    ,0,0
                    ,work.COMNOW
                    );
                msub.MWRITE(
                    new MmlDatum(
                        enmMMLType.ClockCounter
                        , args
                        , lp
                        , 0xff
                        )
                    , new MmlDatum(0xff));
            }

            work.CLOCK = n;
            return EnmFCOMPNextRtn.fcomp1;
        }

        internal int GetErrorLine()
        {
            return mucInfo.row;// errLin;
        }

        internal int COMPIL()
        {
            work.COMNOW = 0;
            work.ADRSTC = 0;
            work.VPCO = 0;

            work.JPLINE = -1;
            work.JPCOL = -1;

            INIT();
            if (work.LINCFG != 0)
            {
                return COMPI3();
            }
            for (int i = 0; i < work.MAXCH; i++)
            {
                work.tcnt[i] = 0;
                work.lcnt[i] = 0;
            }
            work.pcmFlag = 0;
            work.JCLOCK = 0;
            work.JCHCOM = null;

            return COMPI3();
        }

        public int COMPI3()
        {
            work.DATTBL = work.MU_TOP + 1;
            work.MDATA = work.MU_TOP + (MAXCH * 4) + 3;// 11ch * 4byte + 3byte    (DATTBLの大きさ?)
            work.REPCOUNT = 0;
            work.title = work.titleFmt;

            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 0, new MmlDatum((byte)(work.MDATA - work.DATTBL + 1)));
            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 1, new MmlDatum((byte)((work.MDATA - work.DATTBL + 1) >> 8)));

            work.POINTC = work.LOOPSP - 10;// LOOP ﾖｳ ｽﾀｯｸ

            //Z80.HL = 1;// TEXT START ADR
            CSTART();

            return mucInfo.ErrSign ? -1 : 0;
        }

        public void CSTART()
        {
            Log.WriteLine(LogLevel.TRACE, msg.get("I0400"));
            work.MACFG = 0xff;
            COMPST();//KUMA:先ずマクロの解析

            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 0, new MmlDatum((byte)(work.MDATA - work.DATTBL + 1)));
            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 1, new MmlDatum((byte)((work.MDATA - work.DATTBL + 1) >> 8)));

            mucInfo.srcCPtr = 0;
            mucInfo.srcLinPtr = 0;

            CSTART2();
        }

        public void CSTART2()
        {
            do
            {

                work.MACFG = 0x00;
                work.bufStartPtr = work.MDATA;

                COMPST();
                if (mucInfo.ErrSign) return;

                CMPEND();// ﾘﾝｸ ﾎﾟｲﾝﾀ = 0->BASIC END
                if (mucInfo.ErrSign) return;

                work.bufCount[work.COMNOW - 1] = work.MDATA - work.bufStartPtr;

            } while (work.COMNOW != work.MAXCH);
        }

        public void COMPST()
        {
            do
            {
                mucInfo.srcLinPtr++;
                if (mucInfo.basSrc.Count == mucInfo.srcLinPtr) return;
                mucInfo.lin = mucInfo.basSrc[mucInfo.srcLinPtr];
                if (mucInfo.lin.Item2.Length < 1) continue;
                mucInfo.srcCPtr = 0;

                Log.WriteLine(LogLevel.TRACE, string.Format(msg.get("I0401"), mucInfo.lin.Item1));
                work.ERRLINE = mucInfo.lin.Item1;

                if (work.MACFG == 0xff)
                {
                    MACPRC();//マクロの解析
                    continue;
                }

                char c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr++]
                    : (char)0;

                if (work.ADRSTC > 0)
                {
                    //        goto CST3;//ﾏｸﾛﾁｭｳﾅﾗ ﾍｯﾀﾞﾁｪｯｸﾊﾟｽ
                }

                if (c == 0)
                {
                    //goto RECOM
                    continue;
                }

                if (c < 'A' || c > ('A' + work.MAXCH))
                {
                    //goto RECOM
                    continue;
                }

                char ch = c;
                c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr]
                    : (char)0;
                if (c == 0)
                {
                    //goto RECOM
                    continue;
                }

                if ((ch - 'A') != work.COMNOW)
                {
                    // ｹﾞﾝｻﾞｲ ｺﾝﾊﾟｲﾙﾁｭｳ ﾉ ﾁｬﾝﾈﾙ
                    // ﾃﾞﾅｹﾚ ﾊﾞ ﾂｷﾞﾉｷﾞｮｳ
                    // goto RECOM;
                    continue;
                }

                Log.WriteLine(LogLevel.TRACE, string.Format(msg.get("I0402"), ch));

                EnmFMCOMPrtn ret = FMCOMP();// TO FM COMPILE
                if (mucInfo.ErrSign) break;

                if (ret == EnmFMCOMPrtn.nextPart)
                {
                    break;
                }

                //RECOM:
                // ﾘﾝｸﾎﾟｲﾝﾀ ｻｲｾｯﾄ
                // ﾂｷﾞ ﾉ ｷﾞｮｳﾍ
            } while (true);
        }

        public void MACPRC()
        {
            mucInfo.row = mucInfo.lin.Item1;
            mucInfo.col = mucInfo.srcCPtr + 1;

            //Z80.C = 0x2a;// '*'
            //Z80.DE = 0x0C000;//VRAMSTAC
            work.TST2_VAL = 0x0c000;
            MPC1();
        }

        public void MPC1()
        {
            mucInfo.Carry = false;

            char ch = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr++] : (char)0;
            if (ch != '#')//0x23
            {
                mucInfo.Carry = true;
                return;
            }

            do
            {
                ch = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr++] : (char)0;
            } while (ch == ' ');//0x20

            if (ch == 0 || ch == ';' || ch == '{')//0x3b 0x7b 0x2a
            {
                mucInfo.Carry = true;
                return;
            }
            if (ch != '*')
            {
                mucInfo.Carry = true;
                return;
            }

            int ptr = mucInfo.srcCPtr;
            int n = msub.REDATA(mucInfo.lin, ref ptr);// MAC NO.
            mucInfo.srcCPtr = ptr;

            if (mucInfo.Carry)
            {
                throw new MucException(
                    msg.get("E0495")
                    , mucInfo.row, mucInfo.col);
            }

            if (mucInfo.ErrSign)
            {
                throw new MucException(
                    msg.get("E0496")
                    , mucInfo.row, mucInfo.col);
            }

            ch = mucInfo.lin.Item2.Length > mucInfo.srcCPtr ? mucInfo.lin.Item2[mucInfo.srcCPtr++] : (char)0;
            if (ch != '{')//0x7b
            {
                throw new MucException(
                    msg.get("E0497")
                    , mucInfo.row, mucInfo.col);
            }

            if (work.MACFG == 0xff)
            {
                TOSTAC(n);
            }

            mucInfo.Carry = false;
        }

        public void WriteWarning(string wmsg, int row, int col)
        {
            work.compilerInfo.warningList.Add(new Tuple<int, int, string>(row, col, wmsg));
            Log.WriteLine(LogLevel.WARNING, string.Format(msg.get("E0300"), row, col, wmsg));
        }

        public void WriteWarning(string wmsg)
        {
            work.compilerInfo.warningList.Add(new Tuple<int, int, string>(-1, -1, wmsg));
            Log.WriteLine(LogLevel.WARNING, string.Format(msg.get("E0300"), "-", "-", wmsg));
        }

        // *	MACRO STAC	*

        public void TOSTAC(int n)
        {
            if (n > 0xff || n < 0)
            {
                WriteWarning(msg.get("W0400"), mucInfo.row, mucInfo.col);
            }
            n &= 0xff;
            n *= 2;
            if (work.TST2_VAL == 0xc000)
            {
                mucInfo.bufMac.Set(n, mucInfo.srcCPtr);
                mucInfo.bufMac.Set(n + 1, mucInfo.srcLinPtr + 1);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void CMPEND()
        {
            mucInfo.ErrSign = false;

            if (work.tcnt[work.COMNOW] != 0 && mucInfo.bufDst.Get(work.DATTBL + 4 * work.COMNOW + 2)!=null)
            {
                goto CMPE2;
            }

            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 2, new MmlDatum(0));
            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 3, new MmlDatum(0));

        CMPE2:
            mucInfo.bufDst.Set(work.MDATA++, new MmlDatum(0));   // SET END MARK = 0

            work.COMNOW++;	// Ch.=Ch.+ 1

            //↓TBLSET();相当
            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 0, new MmlDatum((byte)(work.MDATA - work.DATTBL + 1)));
            mucInfo.bufDst.Set(work.DATTBL + 4 * work.COMNOW + 1, new MmlDatum((byte)((work.MDATA - work.DATTBL + 1) >> 8)));

            if (work.COMNOW == work.MAXCH)
            {
                CMPEN1();
                return;
            }

            // TEXT START ADR
            mucInfo.srcCPtr = 0;
            mucInfo.srcLinPtr = 0;

            INIT();

            if (work.REPCOUNT != 0)
            {
                throw new MucException(
                    msg.get("E0498")
                    , mucInfo.row, mucInfo.col);
            }

            work.REPCOUNT = 0;
            work.TV_OFS = 0;
            work.SIFTDAT = 0;
            work.SIFTDA2 = 0;
            work.KEYONR = 0;
            work.BEFRST = 0;
            work.TIEFG = 0;

        }

        /// <summary>
        /// パートごとに呼ばれる初期化処理
        /// </summary>
        public void INIT()
        {
            work.LFODAT[0] = 1;
            work.OCTAVE = 5;
            work.COUNT = 24;
            work.CLOCK = 128;
            work.VOLUME = 0;
            work.OCTINT = 0;// 検証結果による値。おそらく実機では不定
            work.compilerInfo = new CompilerInfo();

            work.OctaveUDFLG = 0;
            work.VolumeUDFLG = 0;
            if (mucInfo.invert.ToLower() == "on")
            {
                work.OctaveUDFLG = 1;
                work.VolumeUDFLG = 1;
            }
            work.quantize = 0;//KUMA: ポルタメントむけq値保存
        }

        public void CMPEN1()
        {
            if (work.REPCOUNT != 0)
            {
                throw new MucException(
                    msg.get("E0498")
                    , mucInfo.row, mucInfo.col);
            }

            work.ENDADR = work.MDATA;
            work.OTODAT = work.MDATA - work.MU_NUM;

            //START ADRは0固定なのでわざわざ表示を更新する必要なし
            //string h = Convert.ToString(0, 16);//START ADRは0固定

            VOICECONV1();
        }

        public void VOICECONV1()
        {
            // --   25 BYTE VOICE DATA ﾉ ｾｲｾｲ   --
            mucInfo.useOtoAdr = work.ENDADR;
            work.ENDADR++;
            work.VICADR = work.ENDADR;

            int dvPtr = 0;// work.DEFVOICE;
            int B = 256;//FM:256色
            int useOto = 0;

            do
            {
                int vn = mucInfo.bufDefVoice.Get(dvPtr);   // GET VOICE NUMBER

                //DEBUG
                //vn = 1;

                if (vn == 0)
                {
                    break;
                }

                vn += work.VPCO;
                dvPtr++;
                vn--;
                expand.FVTEXT(vn); //KUMA:MML中で音色定義されているナンバーかどうか探す

                byte[] bufVoi = mucInfo.voiceData;
                int vAdr;

                if (mucInfo.Carry)
                {
                    //vAdr = GETADR(vn);
                    vAdr = vn * 32 + work.FMLIB;
                    vAdr++;// HL= VOICE INDEX
                    //KUMA:見つからなかった
                }
                else
                {
                    vAdr = work.FMLIB + 1;
                    bufVoi = mucInfo.mmlVoiceDataWork.GetByteArray();
                }

                //mucomDotNET 独自
                if (bufVoi == null)
                {
                    throw new MucException(msg.get("E0519"), -1, -1);
                }

                ////
                //TLチェック処理
                ////

                //24:FB/CON
                // 7:CON bit0-2
                int wAlg = bufVoi[vAdr + 12 + 4 + 8] & 7;
                // 4-7:TL ope1-4
                int[] wTL = new int[]{
                bufVoi[vAdr + 4] & 0x7f//op1
                ,bufVoi[vAdr + 6] & 0x7f//op2 6がop2
                ,bufVoi[vAdr + 5] & 0x7f//op3
                ,bufVoi[vAdr + 7] & 0x7f//op4
                };
                int[] wCar = new int[]
                {
                    8 , 8 , 8 , 8 , 10 , 14 , 14 , 15
                };
                for (int i = 0; i < 4; i++)
                {
                    if ((wCar[wAlg] & (1 << i)) == 0) continue;
                    if (wTL[i] != 0)
                    {
                        WriteWarning(string.Format(msg.get("W0408"), vn, i + 1, wAlg, wTL[0], wTL[1], wTL[2], wTL[3]));
                        break;
                    }
                }

                //KUMA:最初の12byte分の音色データをコピー

                int adr = work.ENDADR;

                for (int i = 0; i < 12; i++)
                {
                    mucInfo.bufDst.Set(adr, new MmlDatum(bufVoi[vAdr]));
                    adr++;
                    vAdr++;
                }

                //KUMA:次の4byte分の音色データをbit7(AMON)を立ててコピー
                for (int i = 0; i < 4; i++)
                {
                    mucInfo.bufDst.Set(adr, new MmlDatum((byte)(bufVoi[vAdr] | 0b1000_0000)));// SET AMON FLAG
                    adr++;
                    vAdr++;
                }

                for (int i = 0; i < 9; i++)
                {
                    mucInfo.bufDst.Set(adr, new MmlDatum(bufVoi[vAdr]));
                    adr++;
                    vAdr++;
                }

                work.ENDADR = adr;

                useOto++;
                B--;
            } while (B != 0);

            work.OTONUM = useOto;// ﾂｶﾜﾚﾃﾙ ｵﾝｼｮｸ ﾉ ｶｽﾞ
            mucInfo.bufDst.Set(mucInfo.useOtoAdr, new MmlDatum((byte)useOto));

            work.SSGDAT = work.ENDADR - work.MU_NUM;

            int n = work.ENDADR;
            int pos = 36;
            DispHex4(n, pos);

            n = work.ENDADR - work.MU_NUM;
            pos = 41;
            DispHex4(n, pos);

            work.ESCAPE = 1;
        }

        public void DispHex4(int n, int pos)
        {
            string h = "000" + Convert.ToString(n, 16);//PR END ADR
            h = h.Substring(h.Length - 4, 4);
            byte[] data = enc.GetSjisArrayFromString(h);// System.Text.Encoding.GetEncoding("shift_jis").GetBytes(h);
            for (int i = 0; i < data.Length; i++) mucInfo.bufTitle.Set(pos + i, data[i]);
        }

        public enum EnmFMCOMPrtn
        {
            normal,
            error,
            nextPart
        }

        public EnmFMCOMPrtn FMCOMP()
        {
            char c;

            //Channel表記の後は必ず空白1文字が必要。
            // 以下のような表記はOK
            // Akumajho cde
            // A cde と同じ意
            do
            {
                c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr++]
                    : (char)0;
                if (c == 0) return EnmFMCOMPrtn.normal;
            } while (c != ' ');//ONE SPACE?

            mucInfo.ErrSign = false;
            EnmFCOMPNextRtn ret = EnmFCOMPNextRtn.fcomp1;

            do
            {
                switch (ret)
                {
                    case EnmFCOMPNextRtn.comprc:
                        ret = COMPRC();
                        break;
                    case EnmFCOMPNextRtn.fcomp1:
                        ret = FCOMP1();
                        break;
                    case EnmFCOMPNextRtn.fcomp12:
                        ret = FCOMP12();
                        break;
                        //case enmFCOMPNextRtn.fcomp13:
                        //ret = FCOMP13();
                        //break;
                }

                if (ret == EnmFCOMPNextRtn.occuredERROR)
                {
                    mucInfo.ErrSign = true;
                    return EnmFMCOMPrtn.error;
                }
                if (ret == EnmFCOMPNextRtn.comovr)
                {
                    return EnmFMCOMPrtn.nextPart;
                }

            } while (ret != EnmFCOMPNextRtn.NextLine);

            return EnmFMCOMPrtn.normal;
        }

        public EnmFCOMPNextRtn FCOMP1()
        {
            work.BEFRST = 0;
            work.TIEFG = 0;
            return EnmFCOMPNextRtn.fcomp12;
        }

        public EnmFCOMPNextRtn FCOMP12()
        {
            char c;
            do
            {
                c = mucInfo.srcCPtr < mucInfo.lin.Item2.Length
                    ? mucInfo.lin.Item2[mucInfo.srcCPtr++]
                    : (char)0;
                if (c == 0)// DATA END?
                    return EnmFCOMPNextRtn.NextLine;// ﾂｷﾞ ﾉ ｷﾞｮｳﾍ
            } while (c == ' ');//CHECK SPACE

            mucInfo.srcCPtr--;
            work.com = msub.FMCOMC(c);// COM CHECK

            mucInfo.row = mucInfo.lin.Item1;
            mucInfo.col = mucInfo.srcCPtr + 1;

            if (mucInfo.skipPoint != Point.Empty)
            {
                if (mucInfo.skipPoint.Y == mucInfo.row)
                {
                    mucInfo.skipChannel = work.COMNOW;

                    if (mucInfo.skipPoint.X <= mucInfo.col)
                    {
                        SETTAG();
                        mucInfo.srcCPtr--;
                        mucInfo.skipPoint = Point.Empty;
                        mucInfo.skipChannel = -1;
                    }
                }

                if (mucInfo.skipPoint.Y < mucInfo.row && mucInfo.skipChannel == work.COMNOW)
                {
                    SETTAG();
                    mucInfo.srcCPtr--;
                    mucInfo.skipPoint = Point.Empty;
                    mucInfo.skipChannel = -1;
                }
            }

            if (work.com != 0)
            {
                return EnmFCOMPNextRtn.comprc;
            }

            //音符の解析
            byte note = msub.STTONE();
            if (mucInfo.Carry)
            {
                return EnmFCOMPNextRtn.occuredERROR;
            }

            //音符が直前と同じで、タイフラグがたっているか
            if (note == work.BEFTONE[0] && work.TIEFG != 0)
            {
                mucInfo.srcCPtr++;
                return FCOMP13(note);
            }
            else
            {
                mucInfo.srcCPtr++;
                FC162(note);
            }

            return EnmFCOMPNextRtn.fcomp1;
        }

        private void FC162(int note)
        {
            int ptr = mucInfo.srcCPtr;
            byte clk;
            int ret = msub.STLIZM(mucInfo.lin, ref ptr, out clk);// LIZM SET
            if (ret < 0)
            {
                WriteWarning(msg.get("W0405"), mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;
            clk = FCOMP1X(clk);
            TCLKSUB(clk);// ﾄｰﾀﾙｸﾛｯｸ ｶｻﾝ

            FCOMP17(note, clk);

        }

        private void FC162p(int note)
        {
            int ptr = mucInfo.srcCPtr;
            byte clk;
            int ret = msub.STLIZM(mucInfo.lin, ref ptr, out clk);// LIZM SET
            if (ret < 0)
            {
                WriteWarning(msg.get("W0405"), mucInfo.row, mucInfo.col);
            }
            if (clk > 128)
            {
                WriteWarning(string.Format(msg.get("W0414"), clk), mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;
            clk = FCOMP1X(clk);
            TCLKSUB(clk);// ﾄｰﾀﾙｸﾛｯｸ ｶｻﾝ
            FCOMP17(note, clk);

        }

        private byte FC162p_clock(int note)
        {
            int ptr = mucInfo.srcCPtr;
            byte clk;
            int ret = msub.STLIZM(mucInfo.lin, ref ptr, out clk);// LIZM SET
            if (ret < 0)
            {
                WriteWarning(msg.get("W0405"), mucInfo.row, mucInfo.col);
            }
            if (clk > 128)
            {
                WriteWarning(string.Format(msg.get("W0414"), clk), mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;
            clk = FCOMP1X(clk);
            //TCLKSUB(clk);// ﾄｰﾀﾙｸﾛｯｸ ｶｻﾝ
            //FCOMP17(note, clk);
            return clk;
        }


        private EnmFCOMPNextRtn FCOMP13(int note)
        {
            work.MDATA -= 3;

            int ptr = mucInfo.srcCPtr;
            byte clk;
            int ret = msub.STLIZM(mucInfo.lin, ref ptr, out clk);
            if (ret < 0)
            {
                WriteWarning(msg.get("W0405"), mucInfo.row, mucInfo.col);
            }
            mucInfo.srcCPtr = ptr;
            clk = FCOMP1X(clk);
            TCLKSUB(clk);
            int n = clk;
            n += work.BEFCO;
            if (n > 0xff)
            {
                n -= 127;
                msub.MWRIT2(new MmlDatum(127));
                msub.MWRIT2(new MmlDatum((byte)note));
                msub.MWRIT2(new MmlDatum(0xfd));
            }
            clk = (byte)n;
            FCOMP17(note, clk);

            return EnmFCOMPNextRtn.fcomp1;
        }

        public byte FCOMP1X(byte clk)
        {
            int n = clk;
            n += work.KEYONR;
            work.KEYONR = (sbyte)-work.KEYONR;
            if (n < 0 || n > 255)
            {
                WriteWarning(string.Format(msg.get("W0404"), n), mucInfo.row, mucInfo.col);
            }
            clk = (byte)n;
            return clk;
        }


        private void TCLKSUB(int clk)
        {
            work.tcnt[work.COMNOW] += clk;
        }

        public void FCOMP17(int note, int clk)
        {
            //Mem.LD_8(BEFCO + 1, Z80.A); //?

            if (clk < 128)
            {
                FCOMP2(note, clk);
                return;
            }

            List<object> args = new List<object>();
            args.Add(note);
            args.Add(clk);
            ChannelType tp = CHCHK();

            int ptr = mucInfo.srcCPtr;
            if (mucInfo.lin.Item2.Length > ptr - 1)
                while (ptr > 1 && mucInfo.lin.Item2[ptr - 1] == ' ') ptr--;

            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row
                , mucInfo.col
                , ptr - mucInfo.col + 1
                , tp == ChannelType.FM ? "FM" : (tp == ChannelType.SSG ? "SSG" : (tp == ChannelType.RHYTHM ? "RHYTHM" : "ADPCM"))
                , "YM2608"
                , 0
                , 0
                , work.COMNOW
                );

            // --	ｶｳﾝﾄ ｵｰﾊﾞｰ ｼｮﾘ	--
            clk -= 127;
            msub.MWRIT2(new MmlDatum(
                enmMMLType.Note
                , args
                , lp
                , 127
                ));// FIRST COUNT
            msub.MWRIT2(new MmlDatum((byte)note));// TONE DATA
            msub.MWRIT2(new MmlDatum(0xfd));// COM OF COUNT OVER(SOUND)
            work.BEFCO = clk;// RESTORE SECOND COUNT
            msub.MWRIT2(new MmlDatum((byte)clk));

            for (int i = 0; i < 8; i++)
                work.BEFTONE[8 - i] = work.BEFTONE[7 - i];

            work.BEFTONE[0] = (byte)note;
            msub.MWRIT2(new MmlDatum((byte)note));
        }

        // --	ﾉｰﾏﾙ ｼｮﾘ	--
        public void FCOMP2(int note, int clk)
        {
            List<object> args = new List<object>();
            args.Add(note);
            args.Add(clk);
            ChannelType tp = CHCHK();

            int ptr = mucInfo.srcCPtr;
            if (mucInfo.lin.Item2.Length > ptr - 1)
                while (ptr > 1 && mucInfo.lin.Item2[ptr - 1] == ' ') ptr--;

            LinePos lp = new LinePos(
                mucInfo.fnSrcOnlyFile
                , mucInfo.row
                , mucInfo.col
                , ptr - mucInfo.col + 1
                , tp == ChannelType.FM ? "FM" : (tp == ChannelType.SSG ? "SSG" : (tp == ChannelType.RHYTHM ? "RHYTHM" : "ADPCM"))
                , "YM2608"
                , 0
                , 0
                , work.COMNOW
                );

            mucInfo.bufDst.Set(work.MDATA++, new MmlDatum(
                enmMMLType.Note
                , args
                , lp
                , (byte)clk
                ));// SAVE LIZM
            work.BEFCO = clk;

            mucInfo.bufDst.Set(work.MDATA++, new MmlDatum((byte)note));// SAVE TONE

            for (int i = 0; i < 8; i++)
                work.BEFTONE[8 - i] = work.BEFTONE[7 - i];
            work.BEFTONE[0] = (byte)note;
        }

        public EnmFCOMPNextRtn COMPRC()
        {
            Func<EnmFCOMPNextRtn> act = COMTBL[work.com - 1];
            if (act == null)
            {
                return EnmFCOMPNextRtn.occuredERROR;
            }

            Log.WriteLine(LogLevel.TRACE, act.Method.ToString());

            return act();
        }

    }

    public enum EnmFCOMPNextRtn
    {
        Unknown,
        NextLine,
        comprc,
        occuredERROR,
        fcomp1,
        fcomp12,
        fcomp13,
        comovr
    }

}
