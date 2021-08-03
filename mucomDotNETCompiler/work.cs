using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Compiler
{
    public class work
    {
        public const int MAXChips = 4;
        public const int MAXCH = 11;
        public const int MAXPG = 10;

        //使用しない！
        //public const int T_CLK = 0x8C10;
        //public const int BEFMD = T_CLK + 4 * 11 + 1;//+1ｱﾏﾘ
        //public const int PTMFG = BEFMD + 2;
        //public const int PTMDLY = PTMFG + 1;
        //public const int TONEADR = PTMDLY + 2;
        //public const int SPACE = TONEADR + 2;//2*6BYTE ｱｷ ｶﾞ ｱﾙ
        //public const int DEFVOICE = SPACE + 2 * 6;
        //public const int DEFVSSG = DEFVOICE + 32;
        //public const int JCLOCK = DEFVSSG + 32;
        //public const int JPLINE = JCLOCK + 2;
        //
        public int FMLIB = 0;// 0x6000 w

        public int[][][] tcnt;// MAXCH]; //0x8c10 w
        public int[][][] lcnt;// MAXCH]; //0x8c12 w
        public int[][][] loopPoint;// MAXCH]; //0x8c12 w

        public int pcmFlag = 0;//0x8c10+10*4 w
        public int JCLOCK = 0;//0x8c90 w
        public int LOOPSP = 0;//0xf260 w ﾙｰﾌﾟｽﾀｯｸ
        public int MDATA = 0;// 0xf320 w
        public int DATTBL = 0;// 0xf324 w
        public int OCTAVE = 0;// 0xf326 b
        public int SIFTDAT = 0;// 0xf327 b
        public int SIFTDA2 = 0;// 0xf327 b
        public int CLOCK = 0;// 0xf328 b
        public int ERRLINE = 0;//0xf32e w
        //public int COMNOW = 0;// 0xf330 b
        public int COUNT = 0;// 0xf331 b
        public int VOLINT = 0;// 0xfxxx b
        public int ESCAPE = 0;//
        public int MINUSF = 0;//
        public int BEFRST = 0;// 0xfxxx b
        public int TIEFG = 0;// 0xfxxx b
        public int[] OTONUM = new int[MAXChips];// 0xfxxx b
        public int VOLUME = 0;// 0xfxxx b
        public int ENDADR = 0;// 0xfxxx w
        public int OCTINT = 0;// 0xfxxx w
        public int VICADR = 0;// 0xE300 w
        public string titleFmt = "[  MUCOM88 Ver:0.0  ]  Address:0000-0000(0000)         [ 00:00 ] MODE:NORMAL  "; // 0xf3c8 b
        public string title = "[  MUCOM88 Ver:0.0  ]  Address:0000-0000(0000)         [ 00:00 ] MODE:NORMAL  "; // 0xf3c8 b
        //public byte fmvoiceCnt = 0;//0xf320+50
        public byte[] LFODAT = new byte[] { 1, 0, 0, 0, 0, 0, 0 };
        public byte LINCFG = 0;
        public int ADRSTC = 0;
        public byte VPCO = 1;//dummyで1としている
        public byte OctaveUDFLG = 0;
        public byte VolumeUDFLG = 0;
        public int REPCOUNT = 0;
        public int TV_OFS = 0;
        public int POINTC = 0;// LOOPSTART ADR ｶﾞ ｾｯﾃｲｻﾚﾃｲﾙ ADR
        public byte MACFG = 0;//0>< AS MACRO PRC

        public int TST2_VAL = 0xc000;

        public int HEXFG = 0;

        public byte SECCOM { get; internal set; }
        public byte[] BEFTONE { get; internal set; } = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public int BEFCO { get; internal set; }
        public int com { get; internal set; }
        public byte BFDAT { get; internal set; }
        public byte VDDAT { get; internal set; }
        public int LINE { get; internal set; }
        public int JPLINE { get; internal set; } = -1;
        public int BEFMD { get; internal set; }
        public int FRQBEF { get; internal set; }
        public int PSGMD { get; internal set; }
        public int KEYONR { get; internal set; }
        public int bufStartPtr { get; internal set; }
        public int[][][] bufCount { get; internal set; }
        public int JPCOL { get; internal set; }
        public List<int> JCHCOM { get; internal set; }
        public bool rhythmRelMode { get; internal set; } = false;
        public bool rhythmPanMode { get; internal set; } = false;
        public int rhythmInstNum { get; internal set; } = 0;
        public bool isEnd { get; internal set; } = false;

        public int MU_NUM = 0;// 0xC200 b ｺﾝﾊﾟｲﾙﾁｭｳ ﾉ MUSICﾅﾝﾊﾞｰ
        public int OTODAT = 1;// 0xc201 w FMｵﾝｼｮｸ ｶﾞ ｶｸﾉｳｻﾚﾙ ｱﾄﾞﾚｽﾄｯﾌﾟ ｶﾞ ﾊｲｯﾃｲﾙ
        public int SSGDAT = 3;// 0xc203 w SSG...
        public int MU_TOP = 5;// 0xc205 w ﾐｭｰｼﾞｯｸ ﾃﾞｰﾀ(ｱﾄﾞﾚｽﾃｰﾌﾞﾙ ﾌｸﾑ) ｽﾀｰﾄ ｱﾄﾞﾚｽ
        public CompilerInfo compilerInfo = null;
        public int quantize = 0;
        public int beforeQuantize = 0;
        public int pageNow = 0;
        public int backupMDATA = 0;
        public int lastMDATA = 0;
        public int latestNote = 0;// 最後に解析したのが音符の場合は1、休符の場合は2、初期値は0

        public byte porSW = 0;
        public sbyte porDelta = 0;
        public int porTime = 0;
        public int porOldNote = -1;
        public byte porPin = 0;

        /// <summary>
        /// 各チップのindex
        /// </summary>
        public int ChipIndex = 0;

        /// <summary>
        /// 各チップの割当チャンネル
        /// 曲全体のトラックはCOMNOWを参照
        /// </summary>
        public int CHIP_CH = 0;


        public work()
        {
            ClearCounter();
        }

        public void ClearCounter()
        {
            if (tcnt == null) tcnt = new int[MAXChips][][];
            if (lcnt == null) lcnt = new int[MAXChips][][];
            if (loopPoint == null) loopPoint = new int[MAXChips][][];
            if (bufCount == null) bufCount = new int[MAXChips][][];

            for (int i = 0; i < MAXChips; i++)
            {
                if (tcnt[i] == null) tcnt[i] = new int[MAXCH][];
                if (lcnt[i] == null) lcnt[i] = new int[MAXCH][];
                if (loopPoint[i] == null) loopPoint[i] = new int[MAXCH][];
                if (bufCount[i] == null) bufCount[i] = new int[MAXCH][];

                for (int j = 0; j < MAXCH; j++)
                {
                    if (tcnt[i][j] == null) tcnt[i][j] = new int[MAXPG];
                    if (lcnt[i][j] == null) lcnt[i][j] = new int[MAXPG];
                    if (loopPoint[i][j] == null) loopPoint[i][j] = new int[MAXPG];
                    if (bufCount[i][j] == null) bufCount[i][j] = new int[MAXPG];

                    for (int k = 0; k < MAXPG; k++)
                    {
                        tcnt[i][j][k] = 0;
                        lcnt[i][j][k] = 0;
                        loopPoint[i][j][k] = 0;
                        bufCount[i][j][k] = 0;
                    }
                }
            }
        }


        // 各チップのトラック
        // ABCDEFGHIJK OPNA x1
        // LMNOPQRSTUV      x2
        // abcdefghijk      x3
        // lmnopqrstuv      x4

        public string Tracks = "ABCDEFGHIJKLMNOPQRSTUVabcdefghijklmnopqrstuv";

        public bool SetChipValueFromTrackCharacter(char ch)
        {
            int no = GetTrackNo(ch);
            if (no < 0) return false;//知らない文字
            ChipIndex = no / MAXCH;
            CHIP_CH = no % MAXCH;
            return true;//セットできた
        }

        public char GetTrackCharacterFromChipValue(int chipIndex, int CHIP_CH)
        {
            int no = chipIndex * MAXCH + CHIP_CH;
            if (no < 0 || no >= Tracks.Length) return char.MinValue;
            return Tracks[no];
        }

        public int GetTrackNo(char ch)
        {
            return Tracks.IndexOf(ch);
        }

        public char GetTrackCharacter(int num)
        {
            return Tracks[num];
        }

        /// <summary>
        /// トラックとして使用できる文字ではない
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsNotTrackCharacter(char c)
        {
            return Tracks.IndexOf(c) < 0;
        }
    }
}
