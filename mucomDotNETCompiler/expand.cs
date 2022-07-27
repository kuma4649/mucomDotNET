using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Compiler
{
    public class expand
    {
        public const string NumPattern = "0123456789+-.$abcdefABCDEF";

        private work work = null;
        private MUCInfo mucInfo;
        public Msub msub = null;
        public smon smon = null;
        public Muc88 muc88 = null;
        //public ushort[] FNUMB = new ushort[] {
        //    0x26A,0x28F,0x2B6,0x2DF,0x30B,0x339,0x36A,0x39E,
        //    0x3D5,0x410,0x44E,0x48F
        //};

        //public ushort[] SNUMB = new ushort[] {
        //    0x0EE8,0x0E12,0x0D48,0x0C89,0x0BD5,0x0B2B,0x0A8A,0x09F3,
        //    0x0964,0x08DD,0x085E,0x07E6
        //};

        public ushort[][] FNUMB = new ushort[2][]{
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
        public ushort[][] SNUMB = new ushort[2][]{
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
        public short[][] FNUMBopm = new short[2][]{
                new short[] {
                 0x0000    ,0x0040    ,0x0080    ,0x00c0
                ,0x0100    ,0x0140    ,0x0180    ,0x01c0
                ,0x0200    ,0x0240    ,0x0280    ,0x02c0
                },
                new short[] {
                 0x0000-59-64 ,0x0040-59-64 ,0x0080-59-64 ,0x00c0-59-64
                ,0x0100-59-64 ,0x0140-59-64 ,0x0180-59-64 ,0x01c0-59-64
                ,0x0200-59-64 ,0x0240-59-64 ,0x0280-59-64 ,0x02c0-59-64
                }
        };

        public expand(work work, MUCInfo mucInfo)
        {
            this.work = work;
            this.mucInfo = mucInfo;
        }

        private bool warningToneFormatFlag = false;

        public void FVTEXT(int vn)
        {
            int fvfg;
            int fmlib1 = 1;// 0x6001;
            bool found = false;
            bool warningFlag ;

            for (int i = 0; i < mucInfo.basSrc.Count; i++)
            {
                if (mucInfo.basSrc[i] == null) continue;
                if (mucInfo.basSrc[i].Item2 == null) continue;
                if (mucInfo.basSrc[i].Item2.Length < 4) continue;
                if (mucInfo.basSrc[i].Item2[0] != ' ') continue;
                if (mucInfo.basSrc[i].Item2[2] != '@') continue;

                int srcCPtr = 3;
                fvfg = '\0';
                if (mucInfo.basSrc[i].Item2[srcCPtr] == '%')
                {
                    srcCPtr++;
                    fvfg = '%';
                }
                else if (mucInfo.basSrc[i].Item2[srcCPtr] == 'M'|| mucInfo.basSrc[i].Item2[srcCPtr] == 'm')
                {
                    srcCPtr++;
                    fvfg = 'M';
                }

                int n = msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                if (mucInfo.Carry || mucInfo.ErrSign)
                {
                    muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                }
                if (n != vn) continue;

                found = true;

                if (fvfg == '%')
                {
                    // ---	%(25BYTEｼｷ) ﾉ ﾄｷ ﾉ ﾖﾐｺﾐ	---
                    for (int row = 0; row < 6; row++)
                    {
                        i++;
                        srcCPtr = 1;
                        for (int col = 0; col < 4; col++)
                        {
                            byte v = (byte)msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                            if (mucInfo.Carry || mucInfo.ErrSign)
                            {
                                if(!warningToneFormatFlag) muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                                warningToneFormatFlag = true;
                            }
                            if (skipSpaceAndTab(i, ref srcCPtr))
                            {
                                if (!warningToneFormatFlag) muc88.WriteWarning(msg.get("W0800"), i, srcCPtr);//mucom88で読み込めない恐れあり
                                warningToneFormatFlag = true;
                            }
                            if (NumPattern.IndexOf(getMoji(i, srcCPtr)) < 0)
                                srcCPtr++;// SKIP','
                            mucInfo.mmlVoiceDataWork.Set(
                                fmlib1++
                                , v
                                );
                        }
                    }

                    i++;
                    srcCPtr = 2;
                    mucInfo.mmlVoiceDataWork.Set(
                        fmlib1
                        , (byte)msub.REDATA(mucInfo.basSrc[i], ref srcCPtr)
                        );
                }
                else if (fvfg == 'M')
                {
                    //// --	42ﾊﾞｲﾄﾍﾞｰｼｯｸﾎｳｼｷﾉﾄｷﾉ ﾖﾐｺﾐ	--

                    List<byte> voi = new List<byte>();

                    i++;
                    srcCPtr = 2;
                    int fb = msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                    if (mucInfo.Carry || mucInfo.ErrSign)
                    {
                        muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                    }
                    srcCPtr++;
                    int alg = msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                    if (mucInfo.Carry || mucInfo.ErrSign)
                    {
                        muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                    }
                    srcCPtr++;

                    voi.Add((byte)fb);
                    voi.Add((byte)alg);

                    for (int row = 0; row < 4; row++)
                    {
                        i++;
                        srcCPtr = 1;
                        for (int col = 0; col < 10; col++)
                        {
                            byte v = (byte)msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                            if (mucInfo.Carry || mucInfo.ErrSign)
                            {
                                muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                            }
                            if (skipSpaceAndTab(i, ref srcCPtr))
                            {
                                if (!warningToneFormatFlag) muc88.WriteWarning(msg.get("W0800"), i, srcCPtr);//mucom88で読み込めない恐れあり
                                warningToneFormatFlag = true;
                            }
                            if (NumPattern.IndexOf(getMoji(i, srcCPtr)) < 0)
                                srcCPtr++;// SKIP','
                            voi.Add((byte)v);
                        }
                    }

                    smon.CONVERTopm(voi);//42BYTE->25BYTE
                }
                else
                {
                    //// --	38ﾊﾞｲﾄﾍﾞｰｼｯｸﾎｳｼｷﾉﾄｷﾉ ﾖﾐｺﾐ	--

                    i++;
                    srcCPtr = 2;
                    int fb = msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                    if (mucInfo.Carry || mucInfo.ErrSign)
                    {
                        muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                    }
                    srcCPtr++;
                    int alg = msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                    if (mucInfo.Carry || mucInfo.ErrSign)
                    {
                        muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                    }
                    srcCPtr++;

                    for (int row = 0; row < 4; row++)
                    {
                        i++;
                        srcCPtr = 1;
                        for (int col = 0; col < 9; col++)
                        {
                            byte v = (byte)msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                            if (mucInfo.Carry || mucInfo.ErrSign)
                            {
                                muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                            }

                            if (skipSpaceAndTab(i, ref srcCPtr))
                            {
                                if (!warningToneFormatFlag) muc88.WriteWarning(msg.get("W0800"), i, srcCPtr);//mucom88で読み込めない恐れあり
                                warningToneFormatFlag = true;
                            }
                            if (NumPattern.IndexOf(getMoji(i, srcCPtr)) < 0)
                                srcCPtr++;// SKIP','

                            mucInfo.mmlVoiceDataWork.Set(
                                fmlib1++
                                , v
                                );
                        }
                    }
                    mucInfo.mmlVoiceDataWork.Set(fmlib1++, (byte)fb);
                    mucInfo.mmlVoiceDataWork.Set(fmlib1, (byte)alg);
                    //    Z80.HL = 0x6001;
                    smon.CONVERT();//38BYTE->25BYTE
                }

                break;
            }

            mucInfo.Carry = false;
            if (!found) mucInfo.Carry = true;
            return;


        }

        public void SSGTEXT()
        {
            for (int i = 0; i < mucInfo.basSrc.Count; i++)
            {
                if (mucInfo.basSrc[i] == null) continue;
                if (mucInfo.basSrc[i].Item2 == null) continue;
                if (mucInfo.basSrc[i].Item2.Length < 4) continue;
                if (mucInfo.basSrc[i].Item2[0] != ' ') continue;
                if (mucInfo.basSrc[i].Item2[2] != '@') continue;

                int srcCPtr = 3;
                if (mucInfo.basSrc[i].Item2[srcCPtr] == 'W' || mucInfo.basSrc[i].Item2[srcCPtr] == 'w')
                {
                    srcCPtr++;
                    SSGWaveDefine(ref i, ref srcCPtr);
                }
            }

            work.useSSGVoice.Clear();
            return;
        }

        private void SSGWaveDefine(ref int srcRow, ref int srcCPtr)
        {
            //定義番号を得る
            int n = msub.REDATA(mucInfo.basSrc[srcRow], ref srcCPtr);
            if (mucInfo.Carry || mucInfo.ErrSign)
            {
                muc88.WriteWarning(msg.get("Wxxxx"), srcRow, srcCPtr);//フォーマットが不正です。スペースやカンマをチェックせよ
            }
            if (!work.useSSGVoice.Contains(n)) return;

            byte[] v = new byte[64];
            for (int row = 0; row < 4; row++)
            {

                srcRow++;
                if (mucInfo.basSrc.Count == srcRow)
                {
                    throw new MucException(
                        msg.get("E0800")
                        , srcRow, srcCPtr);//フォーマットが不正です。スペースやカンマをチェックせよ
                }

                srcCPtr = 1;
                for (int col = 0; col < 16; col++)
                {
                    v[row * 16 + col] = (byte)msub.REDATA(mucInfo.basSrc[srcRow], ref srcCPtr);
                    if (mucInfo.Carry || mucInfo.ErrSign)
                    {
                        throw new MucException(
                            msg.get("E0800")
                            , srcRow, srcCPtr);//フォーマットが不正です。スペースやカンマをチェックせよ
                    }

                    if (skipSpaceAndTab(srcRow, ref srcCPtr))
                    {
                        if (!warningToneFormatFlag) muc88.WriteWarning(msg.get("W0800"), srcRow, srcCPtr);//mucom88で読み込めない恐れあり
                        warningToneFormatFlag = true;
                    }
                    if (NumPattern.IndexOf(getMoji(srcRow, srcCPtr)) < 0)
                        srcCPtr++;// SKIP','
                }
            }

            if (mucInfo.ssgVoice.ContainsKey(n))
            {
                mucInfo.ssgVoice.Remove(n);
            }
            mucInfo.ssgVoice.Add(n, v);

        }

        private bool skipSpaceAndTab(int srcRow, ref int srcCPtr)
        {
            bool ret = false;
            char c = getMoji(srcRow, srcCPtr);

            while (c == ' ' || c == 0x9)
            {
                srcCPtr++;
                ret = true;
                c = getMoji(srcRow, srcCPtr);
            }

            return ret;
        }

        private char getMoji(int srcRow,int srcCPtr)
        {
            char c = srcCPtr < mucInfo.basSrc[srcRow].Item2.Length
                ? mucInfo.basSrc[srcRow].Item2[srcCPtr]
                : (char)0;
            return c;
        }

        // **	ﾎﾟﾙﾀﾒﾝﾄ ｹｲｻﾝ	**
        // IN:	HL<={CG}ﾀﾞｯﾀﾗ GﾉﾃｷｽﾄADR
        // EXIT:	DE<=Mｺﾏﾝﾄﾞﾉ 3ﾊﾞﾝﾒ ﾉ ﾍﾝｶﾘｮｳ
        //	Zﾌﾗｸﾞ=1 ﾅﾗ ﾍﾝｶｼﾅｲ
        public int CULPTM(int chipIndex, byte startNote, byte endNote, byte clk)
        {
            int depth = CULP2Ex(chipIndex, startNote, endNote) / (byte)(clk >> 0);
            //int depth = CULP2(note) / (byte)(work.BEFCO >> 0);//Mem.LD_8(BEFCO + 1); ?

            if (!mucInfo.Carry) return depth;
            mucInfo.Carry = false;
            return -depth;//    RET
        }

        public double CULPTMex(int chipIndex, byte startNote, byte endNote, byte clk)
        {
            double depth = CULP2Ex(chipIndex, startNote, endNote) / (double)clk;

            if (!mucInfo.Carry) return depth;
            mucInfo.Carry = false;
            return -depth;//    RET
        }

        public byte GetEndNote()
        {
            int DE = work.MDATA;
            byte endNote = msub.STTONE();
            work.MDATA = DE;
            if (mucInfo.Carry)
            {
                mucInfo.Carry = true;//  SCF
                return 0;//    RET
            }
            return endNote;
        }

        public int GetDiffNote(byte startNote, byte endNote)
        {
            return CTONE(endNote) - CTONE(startNote);
        }

        private int CULP2Ex(int chipIndex, byte startNote, byte endNote)
        {
            int HL;
            bool up;

            //EXIT:	HL<=ﾍﾝｶﾊﾝｲ
            //	CY ﾅﾗ ｻｶﾞﾘﾊｹｲ
            //	Z ﾅﾗ ﾍﾝｶｾｽﾞ

            bool CULP2_Ptn = false;
            Muc88.ChannelType tp = muc88.CHCHK();
            if (tp == Muc88.ChannelType.SSG)
            {
                CULP2_Ptn = true;
            }

            int C = startNote & 0x0F;//KEY
            if (!CULP2_Ptn)
            {
                if (chipIndex < 2) work.FRQBEF = FNUMB[0][C];
                else if (chipIndex < 4) work.FRQBEF = FNUMB[1][C];
                else work.FRQBEF = FNUMBopm[0][C];
            }
            else
            {
                if (chipIndex < 2) work.FRQBEF = SNUMB[0][C];
                else if (chipIndex < 4) work.FRQBEF = SNUMB[1][C];
            }

            int noteNum = CTONE(endNote) - CTONE(startNote);
            if (noteNum == 0) return 0;//変化なし
            if (noteNum >= 0)
            {
                up = !CULP2_Ptn;
            }
            else
            {
                noteNum = (byte)-noteNum;
                up = CULP2_Ptn;
            }

            if (up)
            {
                //	BEFTONE<NOWTONE (ｱｶﾞﾘ)
                //HL = CULC(1.059463f, 33031, noteNum) - work.FRQBEF;//FACC=629C0781(1.05946)  33031(0x8107)
                HL = CULC(1.059463f, work.FRQBEF, noteNum) - work.FRQBEF;//FACC=629C0781(1.05946)  33031(0x8107)
                mucInfo.Carry = false;
                return HL;
            }

            // BEFTONE>NOWTONE(ｻｶﾞﾘ)
            //HL = work.FRQBEF - CULC(0.943874f, 32881, noteNum);//FACC=BBA17180(0.943874)  32881(0x8071)
            HL = work.FRQBEF - CULC(0.943874f, work.FRQBEF, noteNum);//FACC=BBA17180(0.943874)  32881(0x8071)
            mucInfo.Carry = true;
            return HL;
        }



        // **	ﾎﾟﾙﾀﾒﾝﾄ ｹｲｻﾝ	**
        // IN:	HL<={CG}ﾀﾞｯﾀﾗ GﾉﾃｷｽﾄADR
        // EXIT:	DE<=Mｺﾏﾝﾄﾞﾉ 3ﾊﾞﾝﾒ ﾉ ﾍﾝｶﾘｮｳ
        //	Zﾌﾗｸﾞ=1 ﾅﾗ ﾍﾝｶｼﾅｲ
        public int CULPTM(int chipIndex)
        {
            int DE = work.MDATA;
            byte note = msub.STTONE();
            work.MDATA = DE;
            if (mucInfo.Carry)
            {
                mucInfo.Carry = true;//  SCF
                return 0;//    RET
            }

            int depth = CULP2(chipIndex,note) / work.BEFCO;//Mem.LD_8(BEFCO + 1); ?

            if (!mucInfo.Carry) return depth;
            mucInfo.Carry = false;
            return -depth;//    RET
        }

        private int CULP2(int chipIndex,byte note)
        {
            int HL;
            bool up;

            //EXIT:	HL<=ﾍﾝｶﾊﾝｲ
            //	CY ﾅﾗ ｻｶﾞﾘﾊｹｲ
            //	Z ﾅﾗ ﾍﾝｶｾｽﾞ

            bool CULP2_Ptn = false;
            Muc88.ChannelType tp = muc88.CHCHK();
            if (tp == Muc88.ChannelType.SSG)
            {
                CULP2_Ptn = true;
            }

            int C = work.BEFTONE[0] & 0x0F;//KEY
            //if (!CULP2_Ptn) work.FRQBEF = FNUMB[C];
            //else work.FRQBEF = SNUMB[C];
            if (!CULP2_Ptn)
            {
                if (chipIndex < 2) work.FRQBEF = FNUMB[0][C];
                else if (chipIndex < 4) work.FRQBEF = FNUMB[1][C];
                else work.FRQBEF = FNUMBopm[0][C];
            }
            else
            {
                if (chipIndex < 2) work.FRQBEF = SNUMB[0][C];
                else if (chipIndex < 4) work.FRQBEF = SNUMB[1][C];
            }

            int noteNum = CTONE(note) - CTONE(work.BEFTONE[0]);
            if (noteNum == 0) return 0;//変化なし
            if (noteNum >= 0)
            {
                up = !CULP2_Ptn;
            }
            else
            {
                noteNum = (byte)-noteNum;
                up = CULP2_Ptn;
            }

            if (up)
            {
                //	BEFTONE<NOWTONE (ｱｶﾞﾘ)
                //HL = CULC(1.059463f, 33031, noteNum) - work.FRQBEF;//FACC=629C0781(1.05946)  33031(0x8107)
                HL = CULC(1.059463f, work.FRQBEF, noteNum) - work.FRQBEF;//FACC=629C0781(1.05946)  33031(0x8107)
                mucInfo.Carry = false;
                return HL;
            }

            // BEFTONE>NOWTONE(ｻｶﾞﾘ)
            //HL = work.FRQBEF - CULC(0.943874f, 32881, noteNum);//FACC=BBA17180(0.943874)  32881(0x8071)
            HL = work.FRQBEF - CULC(0.943874f, work.FRQBEF, noteNum);//FACC=BBA17180(0.943874)  32881(0x8071)
            mucInfo.Carry = true;
            return HL;
        }

        private int CULC(float facc, int frq, int amul)
        {
            float frqbef = (float)frq;
            for (int count = 0; count < amul; count++)
            {
                frqbef *= facc;
            }
            return (ushort)(int)frqbef;
        }

        public byte CTONE(byte a)
        {
            return (byte)((a & 0x0f) + ((a & 0xf0) >> 4) * 12);
        }

    }
}
