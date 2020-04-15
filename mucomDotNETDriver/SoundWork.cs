using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{
    /// <summary>
    /// オリジナルに存在するワークはここに定義する
    /// </summary>
    public class SoundWork
    {
        public CHDAT[] CHDAT = new CHDAT[]
        {
            new CHDAT()//FM Ch1
            ,new CHDAT()//FM Ch2
            ,new CHDAT()//FM Ch3

            ,new CHDAT()//SSG Ch1
            ,new CHDAT()//SSG Ch2
            ,new CHDAT()//SSG Ch3

            ,new CHDAT()//Drums Ch

            ,new CHDAT()//FM Ch4
            ,new CHDAT()//FM Ch5
            ,new CHDAT()//FM Ch6

            ,new CHDAT()//ADPCM Ch

            // ２つ目
            ,new CHDAT()//FM Ch1
            ,new CHDAT()//FM Ch2
            ,new CHDAT()//FM Ch3

            ,new CHDAT()//SSG Ch1
            ,new CHDAT()//SSG Ch2
            ,new CHDAT()//SSG Ch3

            ,new CHDAT()//Drums Ch

            ,new CHDAT()//FM Ch4
            ,new CHDAT()//FM Ch5
            ,new CHDAT()//FM Ch6

            ,new CHDAT()//ADPCM Ch

        };

        public byte[] PREGBF = null;
        public byte[] INITPM = null;
        public ushort[] DETDAT = null;
        public byte[] DRMVOL = null;
        public byte[] DrmPanEnable = null;
        public byte[] DrmPanMode = null;
        public byte[] DrmPanCounter = null;
        public byte[] DrmPanCounterWork = null;
        public byte[] DrmPanValue = null;
        public byte[] OP_SEL = null;
        public byte[] TYPE1 = null;
        public byte[] TYPE2 = null;
        public byte DMY = 0;       //DB    8
        public ushort[] FNUMB = null;
        public ushort[] SNUMB = null;
        public ushort[] PCMNMB = null;
        public byte[] SSGDAT = null;

        public int MUSNUM { get; internal set; }
        public int C2NUM { get; internal set; }
        public int CHNUM { get; internal set; }
        public int PVMODE { get; internal set; }
        public uint MU_TOP { get; internal set; } = 5;
        public byte TIMER_B { get; internal set; }
        public uint TB_TOP { get; internal set; }
        public int NOTSB2 { get; internal set; }
        public byte PLSET1_VAL { get; internal set; }
        public byte PLSET2_VAL { get; internal set; }
        public int PCMLR { get; internal set; }
        public int FMPORT { get; internal set; }
        public int SSGF1 { get; internal set; }
        public int DRMF1 { get; internal set; }
        public int PCMFLG { get; internal set; }
        public int READY { get; internal set; } = 0xff;
        public int RHYTHM { get; internal set; }
        public uint DELT_N { get; internal set; }
        public uint FNUM { get; internal set; }
        public uint FMSUB8_VAL { get; internal set; }
        public byte FPORT_VAL { get; internal set; } = 0xa4;
        public byte PCMNUM { get; internal set; }
        public byte P_OUT { get; internal set; }
        public int STTADR { get; internal set; }
        public int ENDADR { get; internal set; }
        public byte TOTALV { get; internal set; }
        public int OTODAT { get; internal set; } = 1;
        public byte LFOP6_VAL { get; internal set; }
        public byte FLGADR { get; internal set; }
        public int NEWFNM { get; internal set; }
        public ushort RANDUM { get; internal set; } = 0;
        public int KEY_FLAG { get; internal set; } = 0;
        public int PORTOFS { get; internal set; }

        // **	PMS/AMS/LR DATA	**
        public byte[] PALDAT = new byte[] {
            0x0C0,
            0x0C0,
            0x0C0,
            0x0,// DUMMY
            0x0C0,
            0x0C0,
            0x0C0
        };

        // **	ﾎﾞﾘｭｰﾑ ﾃﾞｰﾀ   **

        public byte[] FMVDAT = new byte[]{// ﾎﾞﾘｭｰﾑ ﾃﾞｰﾀ(FM)
        0x36,0x33,0x30,0x2D,
        0x2A,0x28,0x25,0x22,//  0,  1,  2,  3
        0x20,0x1D,0x1A,0x18,//  4,  5,  6,  7
        0x15,0x12,0x10,0x0D,//  8,  9, 10, 11
        0x0a,0x08,0x05,0x02 // 12, 13, 14, 15
        };

        public byte[] CRYDAT = new byte[]{// ｷｬﾘｱ / ﾓｼﾞｭﾚｰﾀ ﾉ ﾃﾞｰﾀ
        0x08,
        0x08,// ｶｸ ﾋﾞｯﾄ ｶﾞ ｷｬﾘｱ/ﾓｼﾞｭﾚｰﾀ ｦ ｱﾗﾜｽ
        0x08,//
        0x08,// Bit=1 ｶﾞ ｷｬﾘｱ
        0x0C,//      0 ｶﾞ ﾓｼﾞｭﾚｰﾀ
        0x0E,//
        0x0E,// Bit0=OP 1 , Bit1=OP 2 ... etc
        0x0F
        };

        internal void Init() {
            InitValue(0);
            InitValue(11);

            PREGBF = new byte[9];
            INITPM = new byte[] { 0, 0, 0, 0, 0, 56, 0, 0, 0 };
            DETDAT = new ushort[4] { 0, 0, 0, 0 };
            DRMVOL = new byte[6] { 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0 };
            DrmPanCounter = new byte[6] { 0, 0, 0, 0, 0, 0 };
            DrmPanCounterWork = new byte[6] { 0, 0, 0, 0, 0, 0 };
            DrmPanEnable = new byte[6] { 0, 0, 0, 0, 0, 0 };
            DrmPanMode = new byte[6] { 0, 0, 0, 0, 0, 0 };
            DrmPanValue = new byte[6] { 0, 0, 0, 0, 0, 0 };
            OP_SEL = new byte[4] { 0xa6, 0xac, 0xad, 0xae };
            DMY = 8;
            TYPE1 = new byte[] { 0x032, 0x044, 0x046 };
            TYPE2 = new byte[] { 0x0AA, 0x0A8, 0x0AC };
            FNUMB = new ushort[] {
                 0x026A    ,0x028F    ,0x02B6    ,0x02DF
                ,0x030B    ,0x0339    ,0x036A    ,0x039E
                ,0x03D5    ,0x0410    ,0x044E    ,0x048F
            };
            SNUMB = new ushort[] {
                0x0EE8    ,0x0E12    ,0x0D48    ,0x0C89
                ,0x0BD5    ,0x0B2B    ,0x0A8A    ,0x09F3
                ,0x0964    ,0x08DD    ,0x085E    ,0x07E6
            };
            PCMNMB = new ushort[] {
                0x49BA+200,0x4E1C+200,0x52C1+200,0x57AD+200
                ,0x5CE4+200,0x626A+200,0x6844+200,0x6E77+200
                ,0x7509+200,0x7BFE+200,0x835E+200,0x8B2D+200
            };
            SSGDAT = new byte[]{
                255,255,255,255,0,255 // E
                ,255,255,255,200,0,10
                ,255,255,255,200,1,10
                ,255,255,255,190,0,10
                ,255,255,255,190,1,10
                ,255,255,255,170,0,10
                ,40,70,14,190,0,15
                ,120,030,255,255,0,10
                ,255,255,255,225,8,15
                ,255,255,255,1,255,255
                ,255,255,255,200,8,255
                ,255,255,255,220,20,8
                ,255,255,255,255,0,10
                ,255,255,255,255,0,10
                ,120,80,255,255,0,255
                ,255,255,255,220,0,255 // 6*16
            };

        }

        private void InitValue(int ofs) {
            CHDAT[ofs + 0].lengthCounter = 1;
            CHDAT[ofs + 0].instrumentNumber = 24;
            CHDAT[ofs + 0].volume = 10;

            CHDAT[ofs + 1].lengthCounter = 1;
            CHDAT[ofs + 1].instrumentNumber = 24;
            CHDAT[ofs + 1].volume = 10;
            CHDAT[ofs + 1].channelNumber = 1;

            CHDAT[ofs + 2].lengthCounter = 1;
            CHDAT[ofs + 2].instrumentNumber = 24;
            CHDAT[ofs + 2].volume = 10;
            CHDAT[ofs + 2].channelNumber = 2;


            CHDAT[ofs + 3].lengthCounter = 1;
            CHDAT[ofs + 3].instrumentNumber = 0;
            CHDAT[ofs + 3].volume = 8;
            CHDAT[ofs + 3].volReg = 8;
            CHDAT[ofs + 3].channelNumber = 0;

            CHDAT[ofs + 4].lengthCounter = 1;
            CHDAT[ofs + 4].instrumentNumber = 0;
            CHDAT[ofs + 4].volume = 8;
            CHDAT[ofs + 4].volReg = 9;
            CHDAT[ofs + 4].channelNumber = 2;

            CHDAT[ofs + 5].lengthCounter = 1;
            CHDAT[ofs + 5].instrumentNumber = 0;
            CHDAT[ofs + 5].volume = 8;
            CHDAT[ofs + 5].volReg = 10;
            CHDAT[ofs + 5].channelNumber = 4;


            CHDAT[ofs + 6].lengthCounter = 1;
            CHDAT[ofs + 6].volume = 10;
            CHDAT[ofs + 6].channelNumber = 2;


            CHDAT[ofs + 7].lengthCounter = 1;
            CHDAT[ofs + 7].volume = 10;
            CHDAT[ofs + 7].channelNumber = 2;

            CHDAT[ofs + 8].lengthCounter = 1;
            CHDAT[ofs + 8].volume = 10;
            CHDAT[ofs + 8].channelNumber = 2;

            CHDAT[ofs + 9].lengthCounter = 1;
            CHDAT[ofs + 9].volume = 10;
            CHDAT[ofs + 9].channelNumber = 2;


            CHDAT[ofs + 10].lengthCounter = 1;
            CHDAT[ofs + 10].volume = 10;
            CHDAT[ofs + 10].channelNumber = 2;
        }
    }

    public class CHDAT
    {
        public MmlDatum[] mData = null;

        public int lengthCounter = 1;//DB	1	        ; LENGTH ｶｳﾝﾀｰ      IX+ 0
        public int instrumentNumber = 24;//DB	24	        ; ｵﾝｼｮｸ ﾅﾝﾊﾞｰ		1
        public uint dataAddressWork = 0;//DW	0	        ; DATA ADDRES WORK	2,3
        public int dataTopAddress = -1;//DW	0	        ; DATA TOP ADDRES	4,5
        public int volume = 10;//DB	10	        ; VOLUME DATA		6
        public int softEnvelopeFlag = 0;
        //			        ; bit 4 = attack flag
        //                  ; bit 5 = decay flag
        //                  ; bit 6 = sustain flag
        //                  ; bit 7 = soft envelope flag
        public int algo = 0;//DB	0	        ; ｱﾙｺﾞﾘｽﾞﾑ No.      7(FM)
        public int volReg = 0;//DB	8   	    ; VOL.REG.No.       7
        public int channelNumber = 0;//DB    0	        ; ﾁｬﾝﾈﾙ ﾅﾝﾊﾞｰ          	8
        public int detune = 0;//DW	0	        ; ﾃﾞﾁｭｰﾝ DATA		9,10
        public int TLlfo = 0;//DB	0	        ; for TLLFO		11
        public int softEnvelopeCounter = 0;//DB	0   	    ; SOFT ENVE COUNTER	11
        public int reverb = 0;//DB	0	        ; for ﾘﾊﾞｰﾌﾞ		12
        //public int[] softEnvelopeDummy = new int[5];//DS	5	        ; SOFT ENVE DUMMY	13-17
        public int[] softEnvelopeParam = new int[6];//SOFT ENVE		12-17    //KUMA:  12:AL 13:AR 14:DR 15:SR 16:SL 17:RR
        public int reverbVol = 0;// rev vol? 17
        public int quantize = 0;//DB	0	        ; qｵﾝﾀｲｽﾞ		18
        public int lfoDelay = 0;//DB	0	        ; LFO DELAY		19
        public int lfoDelayWork = 0;//DB	0	        ; WORK			20
        public int lfoCounter = 0;//DB	0	        ; LFO COUNTER		21
        public int lfoCounterWork = 0;//DB	0	        ; WORK			22
        public int lfoDelta = 0;//DW	0	        ; LFO ﾍﾝｶﾘｮｳ 2BYTE	23,24
        public int lfoDeltaWork = 0;//DW	0	        ; WORK			25,26
        public int lfoPeak = 0;//DB	0	        ; LFO PEAK LEVEL	27
        public int lfoPeakWork = 0;//DB	0	        ; WORK			28
        public int fnum = 0;//DB	0	        ; FNUM1 DATA		29
        public int bfnum2 = 0;//DB	0	        ; B/FNUM2 DATA		30
        public bool lfoflg = false;//DB	00000001B	        ; bit7=LFO FLAG	31
        public bool keyoffflg = false;//			        ; bit6=KEYOFF FLAG
        public bool lfoContFlg = false;//                   ; 5=LFO CONTINUE FLAG
        public bool tieFlg = false;//			            ; 4=TIE FLAG
        public bool muteFlg = false;//                      ; 3=MUTE FLAG
        public bool lfo1shotFlg = false;//                  ; 2=LFO 1SHOT FLAG
        public bool loopEndFlg = false;//			        ; 0=1LOOPEND FLAG

        public int beforeCode = 0;//DB 	0               ; BEFORE CODE		32
        public bool hardEnveFlg = false;//              ; bit   7=HardEnvelope FLAG   33
        public bool tlLfoflg = false;//DB	0	        ; bit	6=TL LFO FLAG     
        public bool reverbFlg = false;//			          ; 5=REVERVE FLAG
        public bool reverbMode = false;//                     ; 4=REVERVE MODE
        public byte hardEnvelopValue = 0;//                   ; 0-3=hardware Envelope value 
        public int returnAddress = 0;//DW	0	        ; ﾘﾀｰﾝｱﾄﾞﾚｽ	34,35
        public int reserve = 0;//DB	0,0         ; 36,37 (ｱｷ)

        public byte panEnable = 0;//DB ? ;パーン 38
        public byte panMode = 0;//DB ? ;パーン モード 39
        public byte panCounterWork = 0;//DB ? ;パーン カウンター 40
        public byte panCounter = 0;//DB ? ;パーン カウンター 41
        public byte panValue = 0;//DB ? ;パーン 値 42

        public bool musicEnd { get; internal set; }
        public byte TLlfoSlot { get; internal set; }
        public bool SSGTremoloFlg { get; internal set; }
        public int SSGTremoloVol { get; internal set; }
        public int loopCounter { get; internal set; }

        public bool KeyOnDelayFlag = false;
        public byte keyOnSlot = 0xf0;
        public byte[] KD = new byte[4];
        public byte[] KDWork = new byte[4];
    }
}
