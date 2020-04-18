using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Compiler
{
    public static class work
    {
        /// <summary>
        /// コンパイラとしての最大トラック数
        /// </summary>
        public static int MAX_WORK_CHANNEL = 22;

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
        public static int FMLIB = 0;// 0x6000 w

        public static int[] tcnt = new int[MAX_WORK_CHANNEL]; //0x8c10 w
        public static int[] lcnt = new int[MAX_WORK_CHANNEL]; //0x8c12 w
                
        public static int pcmFlag = 0;//0x8c10+10*4 w

        public static int JCLOCK = 0;//0x8c90 w



        public static int LOOPSP = 0;//0xf260 w ﾙｰﾌﾟｽﾀｯｸ

        public static int MDATA = 0;// 0xf320 w
        public static int DATTBL = 0;// 0xf324 w
        public static int OCTAVE = 0;// 0xf326 b
        public static int SIFTDAT = 0;// 0xf327 b
        public static int SIFTDA2 = 0;// 0xf327 b
        public static int CLOCK = 0;// 0xf328 b
        public static int ERRLINE = 0;//0xf32e w
        public static int COMNOW = 0;// 0xf330 b
        public static int COUNT = 0;// 0xf331 b

        public static int VOLINT = 0;// 0xfxxx b
        public static int ESCAPE = 0;//
        public static int MINUSF = 0;//
        public static int BEFRST = 0;// 0xfxxx b
        public static int TIEFG = 0;// 0xfxxx b
        public static int OTONUM = 0;// 0xfxxx b
        public static int VOLUME = 0;// 0xfxxx b
        public static int ENDADR = 0;// 0xfxxx w
        public static int OCTINT = 0;// 0xfxxx w

        public static int VICADR = 0;// 0xE300 w

        public static string titleFmt = "[  MUCOM88 Ver:0.0  ]  Address:0000-0000(0000)         [ 00:00 ] MODE:NORMAL  "; // 0xf3c8 b
        public static string title = "[  MUCOM88 Ver:0.0  ]  Address:0000-0000(0000)         [ 00:00 ] MODE:NORMAL  "; // 0xf3c8 b
        //public static byte fmvoiceCnt = 0;//0xf320+50
        public static byte[] LFODAT = new byte[] { 1, 0, 0, 0, 0, 0, 0 };

        public static byte LINCFG = 0;
        public static int ADRSTC = 0;
        public static byte VPCO=1;//dummyで1としている
        public static byte OctaveUDFLG = 0;
        public static byte VolumeUDFLG = 0;
        public static int REPCOUNT = 0;
        public static int TV_OFS = 0;
        public static int POINTC = 0;// LOOPSTART ADR ｶﾞ ｾｯﾃｲｻﾚﾃｲﾙ ADR
        public static byte MACFG = 0;//0>< AS MACRO PRC

        public static int TST2_VAL = 0xc000;

        public static int HEXFG = 0;

        public static byte SECCOM { get; internal set; }
        public static byte[] BEFTONE { get; internal set; } = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public static int BEFCO { get; internal set; }
        public static int com { get; internal set; }
        public static byte BFDAT { get; internal set; }
        public static byte VDDAT { get; internal set; }
        public static int LINE { get; internal set; }
        public static int JPLINE { get; internal set; } = -1;
        public static int BEFMD { get; internal set; }
        public static int FRQBEF { get; internal set; }
        public static int PSGMD { get; internal set; }
        public static int KEYONR { get; internal set; }
        public static int bufStartPtr { get; internal set; }
        public static int[] bufCount { get; internal set; } = new int[MAX_WORK_CHANNEL];
        public static int JPCOL { get; internal set; }
        public static List<int> JCHCOM { get; internal set; }

        public static int MU_NUM = 0;// 0xC200 b ｺﾝﾊﾟｲﾙﾁｭｳ ﾉ MUSICﾅﾝﾊﾞｰ
        public static int OTODAT = 1;// 0xc201 w FMｵﾝｼｮｸ ｶﾞ ｶｸﾉｳｻﾚﾙ ｱﾄﾞﾚｽﾄｯﾌﾟ ｶﾞ ﾊｲｯﾃｲﾙ
        public static int SSGDAT = 3;// 0xc203 w SSG...
        public static int MU_TOP = 5;// 0xc205 w ﾐｭｰｼﾞｯｸ ﾃﾞｰﾀ(ｱﾄﾞﾚｽﾃｰﾌﾞﾙ ﾌｸﾑ) ｽﾀｰﾄ ｱﾄﾞﾚｽ
        public static CompilerInfo compilerInfo = null;
        public static int quantize=0;
        public static int beforeQuantize=0;
    }
}
