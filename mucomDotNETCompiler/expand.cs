using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace mucomDotNET.Compiler
{
    public class expand
    {
        private work work = null;
        private MUCInfo mucInfo;
        public Msub msub = null;
        public smon smon = null;
        public Muc88 muc88 = null;
        public ushort[] FNUMB = new ushort[] {
            0x26A,0x28F,0x2B6,0x2DF,0x30B,0x339,0x36A,0x39E,
            0x3D5,0x410,0x44E,0x48F
        };

        public ushort[] SNUMB = new ushort[] {
            0x0EE8,0x0E12,0x0D48,0x0C89,0x0BD5,0x0B2B,0x0A8A,0x09F3,
            0x0964,0x08DD,0x085E,0x07E6
        };
        public expand(work work, MUCInfo mucInfo)
        {
            this.work = work;
            this.mucInfo = mucInfo;
        }

        public void FVTEXT(int vn)
        {
            int fvfg = 0;
            int fmlib1 = 1;// 0x6001;
            bool found = false;

            for (int i = 0; i < mucInfo.basSrc.Count; i++)
            {
                if (mucInfo.basSrc[i] == null) continue;
                if (mucInfo.basSrc[i].Item2 == null) continue;
                if (mucInfo.basSrc[i].Item2.Length < 4) continue;
                if (mucInfo.basSrc[i].Item2[0] != ' ') continue;
                if (mucInfo.basSrc[i].Item2[2] != '@') continue;

                int srcCPtr = 3;
                if (mucInfo.basSrc[i].Item2[srcCPtr] == '%')
                {
                    srcCPtr++;
                    fvfg = '%';
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

                    for(int row = 0; row < 6; row++)
                    {
                        i++;
                        srcCPtr = 1;
                        for(int col = 0; col < 4; col++)
                        {
                            byte v = (byte)msub.REDATA(mucInfo.basSrc[i], ref srcCPtr);
                            if(mucInfo.Carry || mucInfo.ErrSign)
                            {
                                muc88.WriteWarning(msg.get("W0409"), i, srcCPtr);
                            }
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

        // **	ﾎﾟﾙﾀﾒﾝﾄ ｹｲｻﾝ	**
        // IN:	HL<={CG}ﾀﾞｯﾀﾗ GﾉﾃｷｽﾄADR
        // EXIT:	DE<=Mｺﾏﾝﾄﾞﾉ 3ﾊﾞﾝﾒ ﾉ ﾍﾝｶﾘｮｳ
        //	Zﾌﾗｸﾞ=1 ﾅﾗ ﾍﾝｶｼﾅｲ
        public int CULPTM(byte startNote, byte endNote, byte clk)
        {
            int depth = CULP2Ex(startNote, endNote) / (byte)(clk >> 0);
            //int depth = CULP2(note) / (byte)(work.BEFCO >> 0);//Mem.LD_8(BEFCO + 1); ?

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

        private int CULP2Ex(byte startNote, byte endNote)
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
            if (!CULP2_Ptn) work.FRQBEF = FNUMB[C];
            else work.FRQBEF = SNUMB[C];

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
        public int CULPTM()
        {
            int DE = work.MDATA;
            byte note = msub.STTONE();
            work.MDATA = DE;
            if (mucInfo.Carry)
            {
                mucInfo.Carry = true;//  SCF
                return 0;//    RET
            }

            int depth = CULP2(note) / work.BEFCO;//Mem.LD_8(BEFCO + 1); ?

            if (!mucInfo.Carry) return depth;
            mucInfo.Carry = false;
            return -depth;//    RET
        }

        private int CULP2(byte note)
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
            if (!CULP2_Ptn) work.FRQBEF = FNUMB[C];
            else work.FRQBEF = SNUMB[C];

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
