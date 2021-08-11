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
        public List<List< CHDAT>> CHDAT = new List<List<CHDAT>>()
        {
            new List<CHDAT>(){
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
            },
            new List<CHDAT>(){
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
            },
            new List<CHDAT>(){
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
            },
            new List<CHDAT>(){
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
            },
            new List<CHDAT>(){
                new CHDAT()//FM Ch1
                ,new CHDAT()//FM Ch2
                ,new CHDAT()//FM Ch3
                ,new CHDAT()//FM Ch4
                ,new CHDAT()//FM Ch5
                ,new CHDAT()//FM Ch6
                ,new CHDAT()//FM Ch7
                ,new CHDAT()//FM Ch8
                ,null
                ,null
                ,null
            }
        };

        public byte[] PREGBF = null;
        public byte[] INITPM = null;
        public ushort[][] DETDAT = new ushort[4][] { null, null, null, null };
        public byte[][] DRMVOL = new byte[4][] { null, null, null, null };
        public byte[][] DrmPanEnable = new byte[4][] { null, null, null, null };
        public byte[][] DrmPanMode = new byte[4][] { null, null, null, null };
        public byte[][] DrmPanCounter = new byte[4][] { null, null, null, null };
        public byte[][] DrmPanCounterWork = new byte[4][] { null, null, null, null };
        public byte[][] DrmPanValue = new byte[4][] { null, null, null, null };
        public byte[] OP_SEL = null;
        public byte[] TYPE1 = null;
        public byte[] TYPE2 = null;
        public byte DMY = 0;       //DB    8
        public ushort[][] FNUMB = null;
        public short[][] FNUMBopm = null;
        public ushort[][] SNUMB = null;
        public ushort[][] PCMNMB = null;
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
        public int[] PCMLR { get; internal set; } = new int[6];
        public int FMPORT { get; internal set; }
        public int SSGF1 { get; internal set; }
        public int DRMF1 { get; internal set; }
        public int PCMFLG { get; internal set; }
        public int READY { get; internal set; } = 0xff;
        public int RHYTHM { get; internal set; }
        public uint[] DELT_N { get; internal set; } = new uint[4];
        public uint FNUM { get; internal set; }
        public uint FMSUB8_VAL { get; internal set; }
        public byte FPORT_VAL { get; internal set; } = 0xa4;
        public byte PCMNUM { get; internal set; }
        public byte P_OUT { get; internal set; }
        public int[] STTADR { get; internal set; } = new int[4];
        public int[] ENDADR { get; internal set; } = new int[4];
        public byte TOTALV { get; internal set; }
        public int OTODAT { get; internal set; } = 1;
        public byte LFOP6_VAL { get; internal set; }
        public byte FLGADR { get; internal set; }
        public int NEWFNM { get; internal set; }
        public ushort RANDUM { get; internal set; } = 0;
        public int KEY_FLAG { get; internal set; } = 0;
        public int currentChip { get; internal set; }
        public int currentCh { get; internal set; }
        public int[][] PCMaSTTADR { get; internal set; } = new int[2][] { new int[6], new int[6] };
        public int[][] PCMaENDADR { get; internal set; } = new int[2][] { new int[6], new int[6] };

        // **	PMS/AMS/LR DATA	**
        public byte[] PALDAT = new byte[] {
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0,
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0,
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0,
            0x00,0x00,0x00,0x00,0x00, 0x00,0x00,0x00,0x00,0x00, // DUMMY
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0,
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0,
            0xC0,0xC0,0xC0,0xC0,0xC0, 0xC0,0xC0,0xC0,0xC0,0xC0
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

        internal void Init()
        {
            for (int chipIndex = 0; chipIndex < 5; chipIndex++)
            {
                for (int i = 0; i < CHDAT[chipIndex][0].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][0].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][0].PGDAT[i].instrumentNumber = 24;
                    CHDAT[chipIndex][0].PGDAT[i].volume = 10;
                }

                for (int i = 0; i < CHDAT[chipIndex][1].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][1].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][1].PGDAT[i].instrumentNumber = 24;
                    CHDAT[chipIndex][1].PGDAT[i].volume = 10;
                    CHDAT[chipIndex][1].PGDAT[i].channelNumber = 1;
                }

                for (int i = 0; i < CHDAT[chipIndex][2].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][2].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][2].PGDAT[i].instrumentNumber = 24;
                    CHDAT[chipIndex][2].PGDAT[i].volume = 10;
                    CHDAT[chipIndex][2].PGDAT[i].channelNumber = 2;
                }

                for (int i = 0; i < CHDAT[chipIndex][3].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][3].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][3].PGDAT[i].instrumentNumber = 0;
                    CHDAT[chipIndex][3].PGDAT[i].volume = 8;
                    CHDAT[chipIndex][3].PGDAT[i].volReg = 8;
                    CHDAT[chipIndex][3].PGDAT[i].channelNumber = 0;
                }

                for (int i = 0; i < CHDAT[chipIndex][4].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][4].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][4].PGDAT[i].instrumentNumber = 0;
                    CHDAT[chipIndex][4].PGDAT[i].volume = 8;
                    CHDAT[chipIndex][4].PGDAT[i].volReg = 9;
                    CHDAT[chipIndex][4].PGDAT[i].channelNumber = 2;
                }

                for (int i = 0; i < CHDAT[chipIndex][5].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][5].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][5].PGDAT[i].instrumentNumber = 0;
                    CHDAT[chipIndex][5].PGDAT[i].volume = 8;
                    CHDAT[chipIndex][5].PGDAT[i].volReg = 10;
                    CHDAT[chipIndex][5].PGDAT[i].channelNumber = 4;
                }

                for (int i = 0; i < CHDAT[chipIndex][6].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][6].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][6].PGDAT[i].volume = 10;
                    CHDAT[chipIndex][6].PGDAT[i].channelNumber = 2;
                }

                for (int i = 0; i < CHDAT[chipIndex][7].PGDAT.Count; i++)
                {
                    CHDAT[chipIndex][7].PGDAT[i].lengthCounter = 1;
                    CHDAT[chipIndex][7].PGDAT[i].volume = 10;
                    CHDAT[chipIndex][7].PGDAT[i].channelNumber = 2;
                }

                if (CHDAT[chipIndex][8] != null)
                {
                    for (int i = 0; i < CHDAT[chipIndex][8].PGDAT.Count; i++)
                    {
                        CHDAT[chipIndex][8].PGDAT[i].lengthCounter = 1;
                        CHDAT[chipIndex][8].PGDAT[i].volume = 10;
                        CHDAT[chipIndex][8].PGDAT[i].channelNumber = 2;
                    }
                }

                if (CHDAT[chipIndex][9] != null)
                {
                    for (int i = 0; i < CHDAT[chipIndex][9].PGDAT.Count; i++)
                    {
                        CHDAT[chipIndex][9].PGDAT[i].lengthCounter = 1;
                        CHDAT[chipIndex][9].PGDAT[i].volume = 10;
                        CHDAT[chipIndex][9].PGDAT[i].channelNumber = 2;
                    }
                }

                if (CHDAT[chipIndex][10] != null)
                {
                    for (int i = 0; i < CHDAT[chipIndex][10].PGDAT.Count; i++)
                    {
                        CHDAT[chipIndex][10].PGDAT[i].lengthCounter = 1;
                        CHDAT[chipIndex][10].PGDAT[i].volume = 10;
                        CHDAT[chipIndex][10].PGDAT[i].channelNumber = 2;
                    }
                }
            }

            PREGBF = new byte[9];
            INITPM = new byte[] { 0, 0, 0, 0, 0, 56, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                DETDAT[i] = new ushort[4] { 0, 0, 0, 0 };
                DRMVOL[i] = new byte[6] { 0xc0, 0xc0, 0xc0, 0xc0, 0xc0, 0xc0 };
                DrmPanCounter[i] = new byte[6] { 0, 0, 0, 0, 0, 0 };
                DrmPanCounterWork[i] = new byte[6] { 0, 0, 0, 0, 0, 0 };
                DrmPanEnable[i] = new byte[6] { 0, 0, 0, 0, 0, 0 };
                DrmPanMode[i] = new byte[6] { 0, 0, 0, 0, 0, 0 };
                DrmPanValue[i] = new byte[6] { 0, 0, 0, 0, 0, 0 };
            }
            OP_SEL = new byte[4] { 0xa6, 0xac, 0xad, 0xae };
            DMY = 8;
            TYPE1 = new byte[] { 0x032, 0x044, 0x046 };
            TYPE2 = new byte[] { 0x0AA, 0x0A8, 0x0AC };
            FNUMB = new ushort[2][]{
                new ushort[] {
                 0x026A    ,0x028F    ,0x02B6    ,0x02DF
                ,0x030B    ,0x0339    ,0x036A    ,0x039E
                ,0x03D5    ,0x0410    ,0x044E    ,0x048F
                },
                new ushort[] {
                 0x0269    ,0x028E    ,0x02b4    ,0x02De
                ,0x0309    ,0x0337    ,0x0368    ,0x039c
                ,0x03d3    ,0x040e    ,0x044b    ,0x048d
                }
            };
            FNUMBopm = new short[2][]{
                new short[] {
                 0x0000    ,0x0040    ,0x0080    ,0x00c0
                ,0x0100    ,0x0140    ,0x0180    ,0x01c0
                ,0x0200    ,0x0240    ,0x0280    ,0x02c0
                },
                new short[] {
                 0x0000-59 ,0x0040-59 ,0x0080-59 ,0x00c0-59
                ,0x0100-59 ,0x0140-59 ,0x0180-59 ,0x01c0-59
                ,0x0200-59 ,0x0240-59 ,0x0280-59 ,0x02c0-59
                }
            };
            SNUMB = new ushort[2][]{
                new ushort[] {
                    0x0EE8    ,0x0E12    ,0x0D48    ,0x0C89
                    ,0x0BD5    ,0x0B2B    ,0x0A8A    ,0x09F3
                    ,0x0964    ,0x08DD    ,0x085E    ,0x07E6
                },
                new ushort[] {
                    0x0EEe    ,0x0E18    ,0x0D4d    ,0x0C8e
                    ,0x0BDa    ,0x0B30    ,0x0A8f    ,0x09F7
                    ,0x0968    ,0x08e1    ,0x0861    ,0x07E9
                }
            };
            PCMNMB = new ushort[2][]{
                new ushort[] {
                    0x49BA+200,0x4E1C+200,0x52C1+200,0x57AD+200
                    ,0x5CE4+200,0x626A+200,0x6844+200,0x6E77+200
                    ,0x7509+200,0x7BFE+200,0x835E+200,0x8B2D+200
                },
                new ushort[] {
                    0x49BA+200,0x4E1C+200,0x52C1+200,0x57AD+200
                    ,0x5CE4+200,0x626A+200,0x6844+200,0x6E77+200
                    ,0x7509+200,0x7BFE+200,0x835E+200,0x8B2D+200
                }
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
    }

    public class CHDAT
    {
        public List<PGDAT> PGDAT = new List<PGDAT>();

        public int keyOnCh { get; internal set; }
        public int currentPageNo { get; internal set; }
    }

    public class PGDAT
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
        public int feedback = 0;
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
        public byte panValue = 3;//DB ? ;パーン 値 42

        public bool musicEnd { get; internal set; }
        public byte TLlfoSlot { get; internal set; }
        public bool SSGTremoloFlg { get; internal set; }
        public int SSGTremoloVol { get; internal set; }
        public int loopCounter { get; internal set; }
        public int pageNo { get; internal set; }

        public bool KeyOnDelayFlag = false;
        public byte keyOnSlot = 0xf0;
        public byte[] KD = new byte[4];
        public byte[] KDWork = new byte[4];
        public byte backupMIXPort { get; internal set; } = 0x38;
        public byte backupNoiseFrq { get; internal set; } = 0;
        public byte backupHardEnv { get; internal set; } = 0;
        public byte backupHardEnvFine { get; internal set; } = 0;
        public byte backupHardEnvCoarse { get; internal set; } = 0;
        public byte[] v_tl = new byte[4] { 0, 0, 0, 0 };
    }
}
