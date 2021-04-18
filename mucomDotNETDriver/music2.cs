using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{
    public class Music2
    {
        public const int MAXCH = 11;
        // **	FM CONTROL COMMAND(s)   **
        public Action[] FMCOM = null;
        public Action[] FMCOM2 = null;
        public Action[] LFOTBL = null;
        // **   PSG COMMAND TABLE**
        public Action[] PSGCOM = null;

        private Work work;
        private Action<ChipDatum> WriteOPNAPRegister = null;
        private Action<ChipDatum> WriteOPNASRegister = null;
        private Action<ChipDatum> WriteOPNBPRegister = null;
        private Action<ChipDatum> WriteOPNBSRegister = null;

        private byte[] autoPantable = new byte[] { 2, 3, 1, 3 };


        public Music2(Work work, Action<ChipDatum> WriteOPNAPRegister, Action<ChipDatum> WriteOPNASRegister, Action<ChipDatum> WriteOPNBPRegister, Action<ChipDatum> WriteOPNBSRegister)
        {
            this.work = work;
            this.WriteOPNAPRegister = WriteOPNAPRegister;
            this.WriteOPNASRegister = WriteOPNASRegister;
            this.WriteOPNBPRegister = WriteOPNBPRegister;
            this.WriteOPNBSRegister = WriteOPNBSRegister;
            initMusic2();
            initAryDRIVE();
        }

        internal bool notSoundBoard2;

        public void MSTART(int musicNumber)
        {
            lock (work.SystemInterrupt)
            {

                work.soundWork.MUSNUM = musicNumber;
                AKYOFF();
                SSGOFF();
                WORKINIT();

                CHK();//added
                INT57();
                ENBL();
                TO_NML();

                work.Status = 1;
            }
        }

        public void MSTOP()
        {
            lock (work.SystemInterrupt)
            {

                AKYOFF();
                SSGOFF();

                if(work.Status>0) work.Status = 0;

            }
        }

        public void FDO()
        {
            lock (work.SystemInterrupt)
            {



            }
        }

        public object RETW()
        {
            lock (work.SystemInterrupt)
            {



            }
            return null;
        }

        public void EFC()
        {
            lock (work.SystemInterrupt)
            {
                //work.SystemInterrupt = true;



                //work.SystemInterrupt = false;
            }
        }

        public void Rendering()
        {
            if (work.Status == 0) return;

            lock (work.SystemInterrupt)
            {
                //work.SystemInterrupt = true;
                work.timer.timer();
                work.timeCounter++;
                if ((work.timer.StatReg & 3) != 0)
                {
                    PL_SND();
                }
                //work.SystemInterrupt = false;
            }
        }

        public void SkipCount(int count)
        {
            lock (work.SystemInterrupt)
            {
                //work.SystemInterrupt = true;
                for (int c = 0; c < 4; c++)
                {
                    for (int i = 0; i < work.soundWork.CHDAT[c].Count; i++)
                    {
                        for (int j = 0; j < work.soundWork.CHDAT[c][i].PGDAT.Count; j++)
                            work.soundWork.CHDAT[c][i].PGDAT[j].muteFlg = true;
                    }
                }

                while (count > 0)
                {
                    PL_SND();
                    count--;
                }

                for (int c = 0; c < 4; c++)
                {
                    for (int i = 0; i < work.soundWork.CHDAT[c].Count; i++)
                        for (int j = 0; j < work.soundWork.CHDAT[c][i].PGDAT.Count; j++)
                            work.soundWork.CHDAT[c][i].PGDAT[j].muteFlg = false;
                    //work.SystemInterrupt = false;
                }
            }
        }


        public void initMusic2()
        {
            SetFMCOMTable();
            SetLFOTBL();
            SetPSGCOM();
            SetSoundWork();

        }

        public void SetFMCOMTable()
        {
            FMCOM = new Action[] {
                OTOPST        // 0xF0 - ｵﾝｼｮｸ ｾｯﾄ    '@'
                ,VOLPST       // 0xF1 - VOLUME SET   'v'
                ,FRQ_DF       // 0xF2 - DETUNE(ｼｭｳﾊｽｳ ｽﾞﾗｼ) 'D'
                ,SETQ         // 0xF3 - SET COMMAND 'q'
                ,LFOON        // 0xF4 - LFO SET
                ,REPSTF       // 0xF5 - REPEAT START SET  '['
                ,REPENF       // 0xF6 - REPEAT END SET    ']'
                ,MDSET        // 0xF7 - FMｵﾝｹﾞﾝ ﾓｰﾄﾞｾｯﾄ  KUMA:'S'スロットディチューンコマンド
                // ,STEREO    // 0xF8 - STEREO MODE
                ,STEREO_AMD98 // 0xF8 - STEREO MODE  'p'
                ,FLGSET       // 0xF9 - FLAG SET
                ,W_REG        // 0xFA - COMMAND OF   'y'
                ,VOLUPF       // 0xFB - VOLUME UP    ')'
                ,HLFOON       // 0xFC - HARD LFO
                ,TIE          // (CANT USE)
                ,RSKIP        // 0xFE - REPEAT JUMP'/'
                ,SECPRC       // 0xFF - to second com
            };

            FMCOM2 = new Action[] {
                PVMCHG         // 0xFF 0xF0 - PCM VOLUME MODE
                ,HRDENV	       // 0xFF 0xF1 - HARD ENVE SET 's'  -> 'S'(kuma)
                ,ENVPOD        // 0xFF 0xF2 - HARD ENVE PERIOD 'm'
                ,REVERVE       // 0xFF 0xF3 - ﾘﾊﾞｰﾌﾞ
                ,REVMOD	       // 0xFF 0xF4 - ﾘﾊﾞｰﾌﾞﾓｰﾄﾞ
                ,REVSW	       // 0xFF 0xF5 - ﾘﾊﾞｰﾌﾞ ｽｲｯﾁ
                ,SetKeyOnDelay // 0xFF 0xF6 - キーオンディレイ 'KD' n1,n2,n3,n4
                ,NTMEAN        // 0xFF 0xF7
                ,NTMEAN        // 0xFF 0xF8
                ,NTMEAN        // 0xFF 0xF9
                ,NTMEAN        // 0xFF 0xFA
                ,NTMEAN        // 0xFF 0xFB
                ,NTMEAN        // 0xFF 0xFC
                ,NTMEAN        // 0xFF 0xFD
                ,NTMEAN        // 0xFF 0xFE
                ,NOP           // 0xFF 0xFF
            };
        }

        private void NOP()
        {
            DummyOUT();
        }

        public void SetLFOTBL()
        {
            LFOTBL = new Action[]{
                LFOOFF
                , LFOON2
                , SETDEL
                , SETCO
                , SETVC2
                , SETPEK
                , TLLFOorSSGTremolo
            };
        }

        public void SetPSGCOM()
        {
            PSGCOM = new Action[] {
                 OTOSSG// 0xF0 - ｵﾝｼｮｸ ｾｯﾄ         '@'
                ,PSGVOL// 0xF1 - VOLUME SET
                ,FRQ_DF// 0xF2 - DETUNE
                ,SETQ  // 0xF3 - COMMAND OF        'q'
                ,LFOON // 0xF4 - LFO
                ,REPSTF// 0xF5 - REPEAT START SET  '['
                ,REPENF// 0xF6 - REPEAT END SET    ']'
                ,NOISE // 0xF7 - MIX PORT          'P'
                ,NOISEW// 0xF8 - NOIZE PARAMATER   'w'
                ,FLGSET// 0xF9 - FLAG SET
                ,ENVPST// 0xFA - SOFT ENVELOPE     'E'
                ,VOLUPS// 0xFB - VOLUME UP    ')'
                ,OTOSET// 0xFC - ｵﾝｼｮｸﾃｲｷﾞ   '@='
                ,TIE   // 0x
                ,RSKIP // 0x
                ,SECPRC// 0xFF - to sec com
            };
        }

        public void SetSoundWork()
        {
            work.Init();
        }

        /// <summary>
        /// AllKeYOFF(fm only)
        /// </summary>
        private void AKYOFF()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int e = 0; e < 7; e++)
                {
                    ChipDatum dat = new ChipDatum(0, 0x28, (byte)e);
                    WriteRegister(i, dat);
                }
            }
        }

        // **	SSG ALL SOUND OFF	**

        private void SSGOFF()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int b = 0; b < 3; b++)
                {
                    ChipDatum dat = new ChipDatum(0, (byte)(0x8 + b), 0x0);
                    WriteRegister(i, dat);
                }
            }
        }

        // **   VOLUME OR FADEOUT etc RESET**

        public void WORKINIT()
        {
            work.soundWork.C2NUM = 0;
            work.soundWork.CHNUM = 0;
            work.soundWork.PVMODE = 0;

            work.soundWork.KEY_FLAG = 0;
            work.soundWork.RANDUM = (ushort)System.DateTime.Now.Ticks;

            if (work.header.mupb != null)
            {
                WORKINITExtendFormat();
                return;
            }

            int num = work.soundWork.MUSNUM;
            work.mDataAdr = work.soundWork.MU_TOP;


            for (int i = 0; i < num; i++)
            {
                work.mDataAdr += 1 + MAXCH * 4;
                work.mDataAdr = work.soundWork.MU_TOP + Cmn.getLE16(work.mData, (uint)work.mDataAdr);
            }

            work.soundWork.TIMER_B = (work.mData[work.mDataAdr] != null) ? ((byte)work.mData[work.mDataAdr].dat) : (byte)200;
            work.soundWork.TB_TOP = ++work.mDataAdr;

            int ch = 0;// (CH1DATのこと)
            for (ch = 0; ch < 6; ch++)
            {
                FMINIT(0, ch);
                //ch++;//オリジナルは　ix+=WKLENG だが、配列化しているので。
            }

            work.soundWork.CHNUM = 0;
            ch = 6;//DRAMDAT
            FMINIT(0, ch);

            work.soundWork.CHNUM = 0;
            //ix = 7;//CHADAT
            for (ch = 7; ch < 7 + 4; ch++)
            {
                FMINIT(0, ch);
                //オリジナルは　ix+=WKLENG だが、配列化しているので。
            }

            work.fmVoiceAtMusData = GetVoiceDataAtMusData();

            work.mData = null;
        }

        public void WORKINITExtendFormat()
        {
            work.mDataAdr = 0;
            work.soundWork.TIMER_B = 200;
            work.soundWork.TB_TOP = 0;

            int ch;// (CH1DATのこと)
            for (int c = 0; c < 4; c++)
            {
                work.soundWork.CHNUM = 0;
                for (ch = 0; ch < 6; ch++) FMINITex(c,ch);
            }

            for (int c = 0; c < 4; c++)
            {
                work.soundWork.CHNUM = 0;
                ch = 6;//DRAMDAT
                FMINITex(c,ch);
            }

            for (int c = 0; c < 4; c++)
            {
                work.soundWork.CHNUM = 0;
                for (ch = 7; ch < 7 + 4; ch++) FMINITex(c,ch);
            }

            List<byte> buf = new List<byte>();
            if (work.header.mupb.instruments != null && work.header.mupb.instruments.Length > 0 && work.header.mupb.instruments[0].data != null)
            {
                for (int i = 0; i < work.header.mupb.instruments[0].data.Length; i++)
                {
                    buf.Add((byte)work.header.mupb.instruments[0].data[i].dat);
                }
            }
            work.fmVoiceAtMusData = buf.ToArray();

            work.mData = null;
        }

        private byte[] GetVoiceDataAtMusData()
        {
            int otodat = work.mData[1].dat + work.mData[2].dat * 0x100 + work.weight;
            int voiCnt = work.mData[otodat].dat;
            List<byte> buf = new List<byte>();
            buf.Add((byte)work.mData[otodat++].dat);
            for (int i = 0; i < voiCnt * 25; i++)
            {
                buf.Add((byte)work.mData[otodat + i].dat);
            }
            return buf.ToArray();
        }

        private void FMINIT(int chipIndex,int ch)
        {
            work.soundWork.CHDAT[chipIndex][ch] = new CHDAT();
            work.soundWork.CHDAT[chipIndex][ch].PGDAT = new List<PGDAT>();
            work.soundWork.CHDAT[chipIndex][ch].PGDAT.Add(new PGDAT());
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].lengthCounter = 1;
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].volume = 0;
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].musicEnd = false;

            // ---	POINTER ﾉ ｻｲｾｯﾃｲ	---
            uint stPtr =  Cmn.getLE16(work.mData, work.soundWork.TB_TOP);
            int lpPtr = (int)Cmn.getLE16(work.mData, work.soundWork.TB_TOP + 2);
            if (lpPtr == 0) lpPtr = -1;

            //次のチャンネル
            int nCPtr= (int)Cmn.getLE16(work.mData, work.soundWork.TB_TOP + 4);

            List<MmlDatum> bf = new List<MmlDatum>();
            int length = (int)(nCPtr - stPtr + (nCPtr < stPtr ? 0x10000 : 0));
            for (int i = 0; i < length; i++)
            {
                bf.Add(work.mData[work.soundWork.MU_TOP + work.weight + stPtr + i]);
            }
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].mData = bf.ToArray();

            if (nCPtr < stPtr)
            {
                work.weight += 0x1_0000;
            }

            //work.soundWork.CHDAT[ch].dataAddressWork
            //    = (uint)(work.soundWork.MU_TOP + stPtr + work.weight);//ix 2,3
            //work.soundWork.CHDAT[ch].dataTopAddress
            //    = (uint)(lpPtr != 0 ? (work.soundWork.MU_TOP + lpPtr + work.weight) : 0);//ix 4,5
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].dataAddressWork
                = (uint)0;//ix 2,3
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].dataTopAddress
                = (int)(lpPtr != -1 ? (lpPtr - stPtr) : -1);//ix 4,5


            work.soundWork.C2NUM++;
            work.soundWork.TB_TOP += 4;
            if (work.soundWork.CHNUM > 2)
            {
                // ---   FOR SSG   ---
                work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].volReg = work.soundWork.CHNUM + 5;//ix 7
                work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].channelNumber = (work.soundWork.CHNUM - 3) * 2;//ix 8
                work.soundWork.CHNUM++;
                return;
            }
            work.soundWork.CHDAT[chipIndex][ch].PGDAT[0].channelNumber = work.soundWork.CHNUM;//ix 8
            work.soundWork.CHNUM++;
        }

        private void FMINITex(int chipIndex,int ch)
        {
            work.soundWork.CHDAT[chipIndex][ch] = new CHDAT();
            work.soundWork.CHDAT[chipIndex][ch].PGDAT = new List<PGDAT>();
            work.soundWork.CHDAT[chipIndex][ch].keyOnCh = -1;//KUMA:初期化不要だが念のため
            work.soundWork.CHDAT[chipIndex][ch].currentPageNo = 0;//KUMA:初期カレントは0ページ

            MupbInfo.ChipDefine.chipPart partInfo = work.header.mupb.chips[chipIndex].parts[ch];

            for (int i = 0; i < partInfo.pages.Length; i++)
            {
                MupbInfo.PageDefine pageInfo = partInfo.pages[i];

                PGDAT pg = new PGDAT();
                pg.pageNo = i;
                work.soundWork.CHDAT[chipIndex][ch].PGDAT.Add(pg);
                pg.lengthCounter = 1;
                pg.volume = 0;
                pg.musicEnd = false;
                pg.dataAddressWork = 0;//ix 2,3
                pg.dataTopAddress = pageInfo.loopPoint;
                pg.mData = pageInfo.data;
                pg.channelNumber = work.soundWork.CHNUM;//ix 8
                if (work.soundWork.CHNUM > 2)
                {
                    // ---   FOR SSG   ---
                    pg.volReg = work.soundWork.CHNUM + 5;//ix 7
                    pg.channelNumber = (work.soundWork.CHNUM - 3) * 2;//ix 8
                }
                //リズムＣｈの場合は音色番号を0x3fにセットして全てのドラム音を有効にする
                if ( ch == 6)
                {
                    pg.instrumentNumber = 0x3f;
                }
            }

            work.soundWork.C2NUM++;
            work.soundWork.TB_TOP += 4;
            work.soundWork.CHNUM++;
        }

        /// <summary>
        /// サウンドボード2のチェックと割り込みベクタ、ポートの設定
        /// (割り込みベクタ、ポートの設定は不要)
        /// </summary>
        private void CHK()
        {
            work.soundWork.NOTSB2 = notSoundBoard2 ? 1 : 0;
        }

        // **	ﾜﾘｺﾐ ﾉ ﾚﾍﾞﾙ ｿﾉﾀ ｼｮｷｾｯﾃｲ ｦ ｵｺﾅｳ**

        private void INT57()
        {

            //割り込み系の設定は不要

            TO_NML();
            MONO();
            AKYOFF();// ALL KEY OFF
            SSGOFF();

            for (int c = 0; c < 4; c++)
            {
                ChipDatum dat;
                if (c < 2)
                {
                    dat = new ChipDatum(0, 0x29, 0x83);// CH 4-6 ENABLE
                    WriteRegister(c, dat);
                }

                for (int b = 0; b < 6; b++)
                {
                    dat = new ChipDatum(0, (byte)b, 0x00);// CH 4-6 ENABLE
                    WriteRegister(c, dat);
                }

                dat = new ChipDatum(0, 7, 0b0011_1000);
                WriteRegister(c, dat);
            }

            // PSGﾊﾞｯﾌｧ ｲﾆｼｬﾗｲｽﾞ
            for (int i = 0; i < work.soundWork.INITPM.Length; i++)
            {
                work.soundWork.PREGBF[i] = work.soundWork.INITPM[i];
            }

        }

        //private void TO_NML()
        //{
        //    work.soundWork.PLSET1_VAL = 0x38;
        //    work.soundWork.PLSET2_VAL = 0x3a;

        //    OPNAData dat = new OPNAData(0, 0x27, 0x3a);
        //    WriteOPNARegister(dat);
        //}

        // **	ALL MONORAL / H.LFO OFF	***

        private void MONO()
        {
            ChipDatum dat;
            work.soundWork.FMPORT = 0;
            for (int b = 0; b < 3; b++)
            {
                dat = new ChipDatum(0, (byte)(0xb4 + b), 0xc0);//fm 1-3
                WriteRegister(work.soundWork.currentChip, dat);
            }

            for (int b = 0; b < 6; b++)
            {
                dat = new ChipDatum(0, (byte)(0x18 + b), 0xc0);//rhythm
                WriteRegister(work.soundWork.currentChip, dat);
            }

            work.soundWork.FMPORT = 4;
            for (int b = 0; b < 3; b++)
            {
                dat = new ChipDatum(1, (byte)(0xb4 + b), 0xc0);//fm 4-6
                WriteRegister(work.soundWork.currentChip, dat);
            }

            work.soundWork.FMPORT = 0;
            dat = new ChipDatum(0, 0x22, 0x00);//lfo freq control
            WriteRegister(work.soundWork.currentChip, dat);
            dat = new ChipDatum(0, 0x12, 0x00);//rhythm test data
            WriteRegister(work.soundWork.currentChip, dat);


            for (int b = 0; b < 7; b++)
            {
                for (int c = 0; c < 10; c++)
                    work.soundWork.PALDAT[b * 10 + c] = 0xc0;
            }

            work.soundWork.PCMLR[work.soundWork.currentChip] = 3;
        }

        private void WriteRegister(int id, ChipDatum dat)
        {
            switch (id)
            {
                case 0:
                    WriteOPNAPRegister(dat);
                    break;
                case 1:
                    WriteOPNASRegister(dat);
                    break;
                case 2:
                    WriteOPNBPRegister(dat);
                    break;
                case 3:
                    WriteOPNBSRegister(dat);
                    break;
            }
        }

        // **	ﾐｭｰｼﾞｯｸ ﾜﾘｺﾐ ENABLE**

        public void ENBL()
        {
            STTMB(work.soundWork.TIMER_B);// SET Timer-B

            //割り込みベクタリセット不要
            //Z80.A = M_VECTR;
            //Z80.C = Z80.A;
            //Z80.A = PC88.IN(Z80.C);
            //Z80.A &= 0x7F;
            //PC88.OUT(Z80.C, Z80.A);
        }

        // **	Timer-B ｶｳﾝﾀ･ｾｯﾄ ﾙｰﾁﾝ   **
        // IN: E<= TIMER_B COUNTER

        private void STTMB(byte e)
        {
            ChipDatum dat = new ChipDatum(0, 0x26, e);
            WriteRegister(0, dat);

            dat = new ChipDatum(0, 0x27, 0x78);
            WriteRegister(0, dat);

            dat = new ChipDatum(0, 0x27, 0x7a);
            WriteRegister(0, dat);

            //割り込みレベルリセット不要
            //Z80.A = 5;
            //PC88.OUT(0xe4, Z80.A);
        }









        // **	MUSIC MAIN	**

        public void PL_SND()
        {
            ChipDatum dat = new ChipDatum(0, 0x27, work.soundWork.PLSET1_VAL);//  TIMER-OFF DATA
            WriteRegister(0, dat);
            dat = new ChipDatum(0, 0x27, work.soundWork.PLSET2_VAL);//  TIMER-ON DATA
            WriteRegister(0, dat);

            DRIVE();
            //FDOUT();

            int n = 0;
            for (int c = 0; c < 4; c++)
            {
                for (int i = 0; i < 11; i++)
                {
                    int p = 0;
                    for (int j = 0; j < work.soundWork.CHDAT[c][i].PGDAT.Count; j++)
                    {
                        if (work.soundWork.CHDAT[c][i].PGDAT[j].musicEnd) p++;
                    }
                    if (p == work.soundWork.CHDAT[c][i].PGDAT.Count) n++;
                    //if (work.soundWork.CHDAT[i].PGDAT[0].musicEnd) n++;
                }
            }
            if (n == 11 * 4)
                work.Status = 0;
        }

        // **	CALL FM		**

        private Tuple<string, int, int, int, int, Action>[] aryDRIVE;
        private void initAryDRIVE()
        {
            aryDRIVE = new Tuple<string, int, int, int, int, Action>[]
            {
                new Tuple<string, int, int, int, int, Action>("----- FM 1  " , 0,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- FM 2  " , 0,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- FM 3  " , 0,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- SSG 1 " , 0,0xff,0,0x00 , SSGENT),
                new Tuple<string, int, int, int, int, Action>("----- SSG 2 " , 0,0xff,0,0x00 , SSGENT),
                new Tuple<string, int, int, int, int, Action>("----- SSG 3 " , 0,0xff,0,0x00 , SSGENT),
                new Tuple<string, int, int, int, int, Action>("----- Ryhthm" , 0,0x00,1,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- FM 4  " , 4,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- FM 5  " , 4,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- FM 6  " , 4,0x00,0,0x00 , FMENT),
                new Tuple<string, int, int, int, int, Action>("----- ADPCM " , 0,0x00,0,0xff , FMENT)
            };
        }

        public void DRIVE()
        {
            int n = 0;

            for (int c = 0; c < 4; c++)
            {
                work.soundWork.currentChip = c;

                for (int i = 0; i < aryDRIVE.Length; i++)
                {
                    Log.WriteLine(LogLevel.TRACE, aryDRIVE[i].Item1);

                    //KUMA:フラグ系パラメータのセット
                    work.soundWork.FMPORT = aryDRIVE[i].Item2;
                    work.soundWork.SSGF1 = aryDRIVE[i].Item3;
                    work.soundWork.DRMF1 = aryDRIVE[i].Item4;
                    work.soundWork.PCMFLG = aryDRIVE[i].Item5;

                    work.cd = work.soundWork.CHDAT[c][i];//KUMA:カレントのパートワーク切り替え
                    work.cd.keyOnCh = -1;//KUMA:発音ページ情報をリセット
                    work.rhythmOR[c] = 0;
                    work.rhythmORKeyOff[c] = 0;

                    int m = 0;
                    for (int j = 0; j < work.cd.PGDAT.Count; j++)
                    {
                        work.pg = work.cd.PGDAT[j];//KUMA:カレントのページワーク切り替え
                        if (!work.pg.musicEnd)
                        {
                            if (work.pg.muteFlg) work.soundWork.READY = 0x00; //KUMA: 0x08(bit3)=MUTE FLAG

                            aryDRIVE[i].Item6();//KUMA:パートごとの処理をコール

                            if (work.pg.muteFlg) work.soundWork.READY = 0xff;//KUMA: 0x08(bit3)=MUTE FLAG
                        }
                        //KUMA:終了パートのカウント
                        if ((work.pg.dataTopAddress == -1 && work.pg.loopEndFlg)
                            || work.pg.loopCounter >= work.maxLoopCount) m++;
                    }
                    if (m == work.cd.PGDAT.Count)
                        n++;

                    //リズム音源のキーオンオフ制御
                    if (c < 2)
                    {
                        if (work.rhythmORKeyOff[c] != 0)
                            PSGOUT(0x10, (byte)((work.rhythmORKeyOff[c] & 0b0011_1111) | 0x80));// KEY OFF
                        if (work.rhythmOR[c] != 0)
                            PSGOUT(0x10, (byte)(work.rhythmOR[c] & 0b0011_1111));// KEY ON
                    }
                    else
                    {
                        if (work.rhythmORKeyOff[c] != 0)
                            PCMOUT(1, 0x0, (byte)((work.rhythmORKeyOff[c] & 0b0011_1111) | 0x80));// KEY OFF
                        if (work.rhythmOR[c] != 0)
                            PCMOUT(1, 0x0, (byte)(work.rhythmOR[c] & 0b0011_1111));// KEY ON
                    }
                }
            }

            if (work.maxLoopCount == -1) n = 0;
            if (n == MAXCH * 4) MSTOP();
        }


        public void FMENT()
        {
            PANNING();//AMD98
            KeyOnDelaying();

            FMSUB();
            PLLFO();
        }

        public void SSGENT()
        {
            SSGSUB();
            PLLFO();
        }


        //**	FM ｵﾝｹﾞﾝ ﾆ ﾀｲｽﾙ ｴﾝｿｳ ﾙｰﾁﾝ	**

        public void FMSUB()
        {
            //work.carry = false;
            work.pg.lengthCounter--;
            work.pg.lengthCounter = (byte)work.pg.lengthCounter;
            if (work.pg.lengthCounter == 0)
            {
                FMSUB1();
                return;
            }

            if ((byte)work.pg.lengthCounter > (byte)work.pg.quantize)
            {
                //if(!work.carry)
                return;
            }

            //FMSUB0

            if (work.pg.mData[work.pg.dataAddressWork].dat == 0xfd) return;// COUNT OVER ?

            //    BIT	5,(IX+33)
            if (work.pg.reverbFlg)//KUMA: 0x20(0b0010_0000)(bit5) = REVERVE FLAG  
            {
                FS2();
                return;
            }

            if (work.cd.currentPageNo == work.pg.pageNo)
                KEYOFF();
        }

        public void FS2()
        {
            STV2((byte)((byte)(work.pg.volume + work.pg.reverbVol) >> 1));
            work.pg.keyoffflg = true;
        }

        public void STV2(byte c)
        {
            byte e = work.soundWork.FMVDAT[c];// GET VOLUME DATA
            byte d = (byte)(0x40 + work.pg.channelNumber);// GET PORT No.

            if (work.pg.algo >= 8) return;//KUMA: オリジナルはチェック無し

            c = work.soundWork.CRYDAT[work.pg.algo];

            for (int b = 0; b < 4; b++)
            {
                if ((c & (1 << b)) != 0) PSGOUT((byte)(d + b * 4), e);// ｷｬﾘｱ ﾅﾗ PSGOUT ﾍ
            }
        }

        public void PSGOUT(byte d, byte e)
        {
            byte port = 0;
            if (d >= 0x30)
            {
                if (work.soundWork.FMPORT != 0)
                {
                    port = 1;
                }
            }
            
            //if (d >= 0x10 && d <= 0x1d)
            //{
            //    Console.WriteLine("{0:x} {1:x}", d, e);
            //}

            ChipDatum dat = new ChipDatum(port, d, e, 0, work.crntMmlDatum);
            WriteRegister(work.soundWork.currentChip, dat);
        }

        public void DummyOUT()
        {
            ChipDatum dat = new ChipDatum(-1,-1,-1, 0, work.crntMmlDatum);
            WriteRegister(work.soundWork.currentChip, dat);
        }


        // **	KEY-OFF ROUTINE		**

        public void KEYOFF()
        {
            if (work.soundWork.PCMFLG != 0)
            {
                PCMEND();
                return;
            }

            if (work.soundWork.DRMF1 != 0)
            {
                if (work.header.mupb == null)
                {
                    // --	ﾘｽﾞﾑ ｵﾝｹﾞﾝ ﾉ ｷｰｵﾌ	--
                    PSGOUT(0x10, (byte)((work.soundWork.RHYTHM & 0b0011_1111) | 0x80));// GET RETHM PARAMETER
                }
                else
                {
                    work.rhythmORKeyOff[work.soundWork.currentChip] |= (work.pg.instrumentNumber & 0b0011_1111);
                }
                return;
            }

            work.pg.KDWork[0] = 0;
            work.pg.KDWork[1] = 0;
            work.pg.KDWork[2] = 0;
            work.pg.KDWork[3] = 0;
            PSGOUT(0x28, (byte)(work.soundWork.FMPORT + work.pg.channelNumber));//  KEY-OFF

        }

        public void PCMEND()
        {
            if (work.soundWork.currentChip > 1)
            {
                PCMEND2610();
                return;
            }

            if (work.cd.currentPageNo != work.pg.pageNo) return;

            PCMOUT(0x0b, 0x00);
            PCMOUT(0x01, 0x00);
            PCMOUT(0x00, 0x21);
        }

        public void PCMEND2610()
        {
            if (work.cd.currentPageNo != work.pg.pageNo) return;

            PCMOUT(0,0x1b, 0x00);
            PCMOUT(0,0x11, 0x00);
            PCMOUT(0,0x10, 0x21);
        }

        // ***	ADPCM OUT	***

        public void PCMOUT(byte d, byte e)
        {
            ChipDatum dat = new ChipDatum(1, d, e, 0, work.crntMmlDatum);
            WriteRegister(work.soundWork.currentChip, dat);
        }
        public void PCMOUT(byte p,byte d, byte e)
        {
            ChipDatum dat = new ChipDatum(p, d, e, 0, work.crntMmlDatum);
            WriteRegister(work.soundWork.currentChip, dat);
        }

        // **	SET NEW SOUND**

        public void FMSUB1()
        {
            work.pg.keyoffflg = true;
            if (work.pg.mData[work.pg.dataAddressWork].dat != 0x0FD)// COUNT OVER?
            {
                FMSUBC(work.pg.dataAddressWork);
                return;
            }

            work.pg.keyoffflg = false;            // RES KEYOFF FLAG
            FMSUBC(work.pg.dataAddressWork + 1);
        }

        public void FMSUBC(uint hl)
        {

            byte a;
            bool nrFlg = false;
            do
            {
                Log.WriteLine(LogLevel.TRACE, string.Format("{0:x}", hl + 0xc200));
                a = (byte)work.pg.mData[hl].dat;
                //* 00H as end
                while (a == 0)// ﾃﾞｰﾀ ｼｭｳﾘｮｳ ｦ ｼﾗﾍﾞﾙ
                {
                    work.pg.loopEndFlg = true;

                    if (work.pg.dataTopAddress == -1 || nrFlg)
                    {
                        FMEND(hl);//* DATA TOP ADRESS ｶﾞ 0000H ﾃﾞ BGM
                        return; // ﾉ ｼｭｳﾘｮｳ ｦ ｹｯﾃｲ ｿﾚ ｲｶﾞｲﾊ ｸﾘｶｴｼ
                    }
                    hl = (uint)work.pg.dataTopAddress;
                    a = (byte)work.pg.mData[hl].dat;// GET FLAG & LENGTH
                    work.pg.loopCounter++;
                    if (work.pg.loopCounter > work.nowLoopCounter) work.nowLoopCounter = work.pg.loopCounter;
                    nrFlg = true;
                }

                //演奏情報退避
                work.crntMmlDatum = work.pg.mData[hl];

                // **	SET LENGTH	**
                hl++;
                if (a < 0xf0) break;

                // DATA ｶﾞ ｺﾏﾝﾄﾞ ﾅﾗ FMSUBA ﾍ
                // **	ｻﾌﾞ･ｺﾏﾝﾄﾞ ﾉ ｹｯﾃｲ**
                //FMSUBA
                a &= 0xf;// A=COMMAND No.(0-F)
                work.hl = hl;
                FMCOM[a]();
                hl = work.hl;
            } while (true);

            nrFlg = false;
            work.pg.lengthCounter = a & 0x7f;// SET WAIT COUNTER


            if ((a & 0x80) != 0) //BIT7(ｷｭｳﾌ ﾌﾗｸﾞ)
            {
                work.crntMmlDatum = work.pg.mData[hl - 1];
                // **	SET F-NUMBER**
                work.pg.dataAddressWork = hl;// SET NEXT SOUND DATA ADD

                if (work.pg.reverbMode)
                {
                    if (work.cd.currentPageNo == work.pg.pageNo)
                        KEYOFF();
                    return;
                }
                if (work.pg.reverbFlg)
                {
                    FS2();
                    return;
                }
                if (work.cd.currentPageNo == work.pg.pageNo)
                    KEYOFF();
                return;
            }

            if (work.cd.keyOnCh != -1 && work.cd.keyOnCh != work.pg.pageNo)// !work.pg.keyoffflg)
            {
                work.pg.dataAddressWork = hl + 1;// SET NEXT SOUND DATA ADD
                return;
            }

            if (work.cd.currentPageNo != work.pg.pageNo)
            {
                //切り替え処理
                RestoreOTOPST();
                restoreSTEREO_AMD98();
            }

            //カレントページ情報セット
            work.cd.currentPageNo = work.pg.pageNo;

            // ｵﾝﾌﾟ ﾅﾗ FMSUB5 ﾍ
            if (work.pg.keyoffflg)
            {
                KEYOFF();
            }

            if (work.soundWork.PLSET1_VAL != 0x78)//効果音モードでは無い場合
            {
                FMSUB4(hl);
                return;
            }

            if (work.soundWork.FMPORT != 0)
            {
                FMSUB4(hl);
                return;
            }

            if (work.pg.channelNumber == 2)//CH=3?
            {
                EXMODE(hl);
                return;
            }

            FMSUB4(hl);
        }

        // **	ｴﾝｿｳ ｵﾜﾘ	**

        public void FMEND(uint hl)
        {
            work.pg.musicEnd = true;
            work.pg.dataAddressWork = hl;

            if (work.soundWork.PCMFLG != 0)
            {
                PCMEND();
                return;
            }
            if (work.cd.currentPageNo == work.pg.pageNo)
                KEYOFF();
        }

        public void FMSUB4(uint hl)
        {
            byte a, b;
            work.carry = false;

            a = (byte)work.pg.mData[hl].dat;// A=BLOCK(OCTAVE-1 ) & KEY CODE DATA
            work.pg.dataAddressWork = hl + 1;// SET NEXT SOUND DATA ADD
            if  (!work.pg.keyoffflg // CHECK KEYOFF FLAG
                && work.pg.beforeCode == a)// GET BEFORE CODE DATA
            {
                work.carry = true;
                return;
            }

            work.pg.beforeCode = a;

            if (work.soundWork.PCMFLG != 0)
            {
                //PCMGFQ:
                hl = (uint)(work.soundWork.PCMNMB[work.soundWork.currentChip/2][a & 0b0000_1111] + work.pg.detune);
                a >>= 4;
                b = a;
                //ASUB7:
                while (b != 0)
                {
                    hl >>= 1;
                    b--;
                }
                //ASUB72:
                work.soundWork.DELT_N[work.soundWork.currentChip] = hl;
                if (!work.pg.keyoffflg)
                {
                    LFORST();
                }
                LFORST2();
                PLAY();
                return;//戻り値がcarry
            }

            if (work.soundWork.DRMF1 == 0)
            {
                //FMGFQ:
                hl = work.soundWork.FNUMB[work.soundWork.currentChip / 2][a & 0xf];// GET KEY CODE(C, C+, D...B)
                hl |= (ushort)((a & 0x70) << 7);// GET BLOCK DATA
                                                // A4-A6 ﾎﾟｰﾄ ｼｭﾂﾘｮｸﾖｳ ﾆ ｱﾜｾﾙ
                                                // GET FNUM2
                                                // A= KEY CODE & FNUM HI

                hl = (uint)(hl + work.pg.detune);// GET DETUNE DATA
                                                 // DETUNE PLUS

                if (!work.pg.tlLfoflg)
                {
                    work.pg.fnum = (int)hl;// FOR LFO
                                           // FOR LFO
                    work.soundWork.FNUM = hl;
                }
                if (work.pg.keyoffflg)
                {
                    LFORST();
                }
                LFORST2();
                //FMSUB8:
                FMSUB6(hl, work.soundWork.FMSUB8_VAL);//戻り値がcarry
                return;
            }

            //DRMFQ:
            if (!work.pg.keyoffflg)
            {
                return;
            }
            DKEYON();//戻り値がcarry
        }

        public void FMSUB6(uint hl, uint bc)
        {
            if (work.isDotNET)
            {
                hl = AddDetuneToFNum((ushort)hl, (short)(ushort)bc);
            }
            else
            {
                hl += bc;// BLOCK/FNUM1&2 DETUNE PLUS(for SE MODE)
            }

            byte e = (byte)(hl >> 8);// BLOCK/F-NUMBER2 DATA
                                     //FPORT:
            byte d = work.soundWork.FPORT_VAL;// 0x0A4;// PORT A4H
            d += (byte)work.pg.channelNumber;
            PSGOUT(d, e);

            d -= 4;
            e = (byte)hl;// F-NUMBER1 DATA
                         //FMSUB7:
            PSGOUT(d, e);
            KEYON();
            work.carry = false;
        }

        private ushort AddDetuneToFNum(ushort fnum, short detune)
        {
            int block = (byte)((fnum >> 11) & 7);
            int fnum11b = fnum & 0x7ff;

            fnum11b += detune;
            if (detune < 0)
            {
                while (fnum11b < 0x26a)
                {
                    if (block == 0)
                    {
                        if (fnum11b < 0) fnum11b = 0;
                        break;
                    }
                    fnum11b += 0x26a;
                    block--;
                }
            }
            else
            {
                while (fnum11b > 0x26a * 2)
                {
                    if (block == 7)
                    {
                        if (fnum11b > 0x7ff) fnum11b = 0x7ff;
                        break;
                    }
                    fnum11b -= 0x26a;
                    block++;
                }
            }

            return (ushort)(((block & 7) << 11) | (fnum11b & 0x7ff));
        }

        // **	SE MODE ﾉ DETUNE ｾｯﾃｲ**

        public void EXMODE(uint hl)
        {
            work.soundWork.FMSUB8_VAL = (ushort)work.soundWork.DETDAT[0];
            FMSUB4(hl);// SET OP1
            if (work.carry)
            {
                work.soundWork.FMSUB8_VAL = 0;//忘れずに戻す
                return;
            }

            hl = 1;
            byte a = 0x0AA;//  A = CH3 F-NUM2 OP1 PORT - 2
                           //EXMLP:
            while (a != 0xad)//END PORT+1
            {
                work.soundWork.FPORT_VAL = a;
                a++;
                //HLSTC0:
                FMSUB6(work.soundWork.FNUM, (ushort)work.soundWork.DETDAT[hl++]);// SET OP2-OP4
            }

            work.soundWork.FPORT_VAL = 0xa4;
            //BRESET:
            work.soundWork.FMSUB8_VAL = 0;
            //  RET TO MAIN
        }

        // ---	RESET PEAK L.&DELAY	---

        public void LFORST()
        {
            work.pg.lfoDelayWork = work.pg.lfoDelay;// LFO DELAY ﾉ ｻｲｾｯﾃｲ
            work.pg.lfoContFlg = false;            // RESET LFO CONTINE FLAG
        }

        public void LFORST2()
        {
            work.pg.lfoPeakWork = work.pg.lfoPeak >> 1;// LFO PEAK LEVEL ｻｲ ｾｯﾃｲ
            work.pg.lfoDeltaWork = work.pg.lfoDelta;// ﾍﾝｶﾘｮｳ ｻｲｾｯﾃｲ
            if (!work.pg.tlLfoflg)
            {
                return;
            }
            work.pg.fnum = work.pg.TLlfo;
            work.pg.bfnum2 = 0;
        }

        // ***	ADPCM PLAY	***

        // IN:(STTADR)<=ｻｲｾｲ ｽﾀｰﾄ ｱﾄﾞﾚｽ
        //	   (ENDADR)  <=ｻｲｾｲ ｴﾝﾄﾞ ｱﾄﾞﾚｽ
        //	   (DELT_N)<=ｻｲｾｲ ﾚｰﾄ

        public void PLAY()
        {
            if (work.soundWork.currentChip > 1)
            {
                PLAY2610();
                return;
            }

            if (work.cd.keyOnCh != -1)
                return;//KUMA:既に他のページが発音中の場合は処理しない
            work.cd.keyOnCh = work.pg.pageNo;

            if (work.soundWork.READY == 0) return;

            PCMOUT(0x0b, 0x00);
            PCMOUT(0x01, 0x00);
            PCMOUT(0x00, 0x21);
            PCMOUT(0x10, 0x08);
            PCMOUT(0x10, 0x80);// INIT
            PCMOUT(0x02, (byte)work.soundWork.STTADR[work.soundWork.currentChip]);// START ADR
            PCMOUT(0x03, (byte)(work.soundWork.STTADR[work.soundWork.currentChip] >> 8));
            PCMOUT(0x04, (byte)work.soundWork.ENDADR[work.soundWork.currentChip]);// END ADR
            PCMOUT(0x05, (byte)(work.soundWork.ENDADR[work.soundWork.currentChip] >> 8));
            PCMOUT(0x09, (byte)work.soundWork.DELT_N[work.soundWork.currentChip]);// ｻｲｾｲ ﾚｰﾄ ｶｲ
            PCMOUT(0x0a, (byte)(work.soundWork.DELT_N[work.soundWork.currentChip] >> 8));// ｻｲｾｲ ﾚｰﾄ ｼﾞｮｳｲ
            PCMOUT(0x00, 0xa0);

            byte e = (byte)(work.soundWork.TOTALV * 4 + work.pg.volume);
            if (e >= 250)
            {
                e = 0;
            }
            //PL1:
            if (work.soundWork.PVMODE != 0)
            {
                e += (byte)work.pg.volReg;
            }
            //PL2:
            PCMOUT(0xb, e);// VOLUME

            e = (byte)((work.soundWork.PCMLR[work.soundWork.currentChip] & 3) << 6);
            PCMOUT(0x01, e);// 1 bit TYPE, L&R OUT

            // ｼﾝｺﾞｳﾀﾞｽ
            work.soundWork.P_OUT = work.soundWork.PCMNUM;
        }

        public void PLAY2610()
        {
            if (work.cd.keyOnCh != -1)
                return;//KUMA:既に他のページが発音中の場合は処理しない
            work.cd.keyOnCh = work.pg.pageNo;

            if (work.soundWork.READY == 0) return;

            PCMOUT(0,0x1b, 0x00);
            PCMOUT(0,0x11, 0x00);
            PCMOUT(0,0x10, 0x21);
            PCMOUT(0,0x1c, 0x08);
            PCMOUT(0,0x1c, 0x80);// INIT
            PCMOUT(0,0x12, (byte)(work.soundWork.STTADR[work.soundWork.currentChip] >> 0));// START ADR
            PCMOUT(0,0x13, (byte)(work.soundWork.STTADR[work.soundWork.currentChip] >> 8));
            PCMOUT(0,0x14, (byte)(work.soundWork.ENDADR[work.soundWork.currentChip] >> 0));// END ADR
            PCMOUT(0,0x15, (byte)(work.soundWork.ENDADR[work.soundWork.currentChip] >> 8));
            PCMOUT(0,0x19, (byte)(work.soundWork.DELT_N[work.soundWork.currentChip] >> 0));// ｻｲｾｲ ﾚｰﾄ ｶｲ
            PCMOUT(0,0x1a, (byte)(work.soundWork.DELT_N[work.soundWork.currentChip] >> 8));// ｻｲｾｲ ﾚｰﾄ ｼﾞｮｳｲ
            PCMOUT(0, 0x10, 0xa0);

            byte e = (byte)(work.soundWork.TOTALV * 4 + work.pg.volume);
            if (e >= 250)
            {
                e = 0;
            }
            //PL1:
            if (work.soundWork.PVMODE != 0)
            {
                e += (byte)work.pg.volReg;
            }
            //PL2:
            PCMOUT(0,0x1b, e);// VOLUME

            e = (byte)((work.soundWork.PCMLR[work.soundWork.currentChip] & 3) << 6);
            PCMOUT(0,0x11, e);// 1 bit TYPE, L&R OUT

            // ｼﾝｺﾞｳﾀﾞｽ
            work.soundWork.P_OUT = work.soundWork.PCMNUM;
        }

        public void SetADPCM_AAddress(int ach)
        {

            PCMOUT(1, (byte)(0x10 + ach), (byte)(work.soundWork.PCMaSTTADR[work.soundWork.currentChip - 2][ach] >> 0));// START ADR
            PCMOUT(1, (byte)(0x18 + ach), (byte)(work.soundWork.PCMaSTTADR[work.soundWork.currentChip - 2][ach] >> 8));
            PCMOUT(1, (byte)(0x20 + ach), (byte)(work.soundWork.PCMaENDADR[work.soundWork.currentChip - 2][ach] >> 0));// END ADR
            PCMOUT(1, (byte)(0x28 + ach), (byte)(work.soundWork.PCMaENDADR[work.soundWork.currentChip - 2][ach] >> 8));

        }

        public void SetADPCM_A_InstrumentAddress(int ach, int i)
        {

            if (work.pcmTables[work.soundWork.currentChip + 2] == null) return;
            if (work.pcmTables[work.soundWork.currentChip + 2].Length < 1) return;

            if (i >= work.pcmTables[work.soundWork.currentChip + 2].Length) return;
            work.soundWork.PCMaSTTADR[work.soundWork.currentChip - 2][ach] = work.pcmTables[work.soundWork.currentChip + 2][i].Item2[0];//start address
            work.soundWork.PCMaENDADR[work.soundWork.currentChip - 2][ach] = work.pcmTables[work.soundWork.currentChip + 2][i].Item2[1];//end address

        }

        // **   ﾘｽﾞﾑ ｵﾝｹﾞﾝ ﾉ ｷｰｵﾝ   **

        public void DKEYON()
        {
            if (work.soundWork.READY == 0) return;
            if (work.header.mupb == null)
            {
                PSGOUT(0x10, (byte)(work.soundWork.RHYTHM & 0b0011_1111));// KEY ON
            }
            else
            {
                work.rhythmOR[work.soundWork.currentChip] |= (work.pg.instrumentNumber & 0b0011_1111);

                //アドレス送信
                if (work.soundWork.currentChip > 1)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if ((work.pg.instrumentNumber & (1 << i)) != 0)
                        {
                            SetADPCM_A_InstrumentAddress(i, work.pg.beforeCode);
                            SetADPCM_AAddress(i);
                        }
                    }
                }
            }
        }

        // **	KEY-ON ROUTINE   **

        public void KEYON()
        {
            if (work.soundWork.READY == 0) return;
            if (work.cd.keyOnCh != -1) return;//KUMA:既に他のページが発音中の場合は処理しない

            byte a = 0x04;
            if (work.soundWork.FMPORT == 0)
            {
                a = 0x00;
            }

            if (!work.pg.KeyOnDelayFlag)
            {
                a += work.pg.keyOnSlot;
            }
            else
            {
                work.pg.keyOnSlot = 0x00;
                if (work.pg.KD[0] == 0) work.pg.keyOnSlot += 0x10;
                if (work.pg.KD[1] == 0) work.pg.keyOnSlot += 0x20;
                if (work.pg.KD[2] == 0) work.pg.keyOnSlot += 0x40;
                if (work.pg.KD[3] == 0) work.pg.keyOnSlot += 0x80;
                a += work.pg.keyOnSlot;

                work.pg.KDWork[0] = work.pg.KD[0];
                work.pg.KDWork[1] = work.pg.KD[1];
                work.pg.KDWork[2] = work.pg.KD[2];
                work.pg.KDWork[3] = work.pg.KD[3];
            }

            //発音ページ情報セット
            work.cd.keyOnCh = work.pg.pageNo;

            //KEYON2:
            a += (byte)work.pg.channelNumber;
            PSGOUT(0x28, a);//KEY-ON

            if (work.pg.reverbFlg)
            {
                STVOL();
            }
        }

        // **	ﾎﾞﾘｭｰﾑ**

        public void STVOL()
        {
            //STV1
            byte c = (byte)(work.soundWork.TOTALV + work.pg.volume);// INPUT VOLUME
            if (c >= 20)
            {
                c = 0;
            }
            //STV12:
            STV2(c);
        }




        public void RestoreOTOPST()
        {
            if (work.soundWork.PCMFLG != 0)
            {
                restoreOTOPCM();
                return;
            }

            if (work.soundWork.DRMF1 != 0)
            {
                restoreOTODRM();
                return;
            }

            STENV();
            STVOL();
        }

        // **	ｵﾝｼｮｸ ｾｯﾄ ﾒｲﾝ**

        public void OTOPST()
        {
            if (work.soundWork.PCMFLG != 0)
            {
                OTOPCM();
                return;
            }

            if (work.soundWork.DRMF1 != 0)
            {
                OTODRM();
                return;
            }

            work.pg.instrumentNumber = work.pg.mData[work.hl++].dat;

            if (work.cd.currentPageNo != work.pg.pageNo) return;//KUMA:カレントページの場合のみ音色を変更する

            STENV();
            STVOL();
        }

        public void OTODRM()
        {
            DummyOUT();
            work.soundWork.RHYTHM = work.pg.mData[work.hl++].dat;// SET RETHM PARA
            work.pg.instrumentNumber = work.soundWork.RHYTHM;
        }

        public void restoreOTODRM()
        {
            DummyOUT();
            work.soundWork.RHYTHM = work.pg.instrumentNumber;
        }

        public void OTOPCM()
        {
            if (work.cd.currentPageNo != work.pg.pageNo)
            {
                work.pg.instrumentNumber = (byte)work.pg.mData[work.hl++].dat-1;
                return;
            }
            DummyOUT();
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.soundWork.PCMNUM = a;
            a--;
            work.pg.instrumentNumber = a;

            if (work.pcmTables != null && work.pcmTables[work.soundWork.currentChip] != null && work.pcmTables[work.soundWork.currentChip].Length > a)
            {
                work.soundWork.STTADR[work.soundWork.currentChip] = work.pcmTables[work.soundWork.currentChip][a].Item2[0];//start address
                work.soundWork.ENDADR[work.soundWork.currentChip] = work.pcmTables[work.soundWork.currentChip][a].Item2[1];//end address
            }

            if (work.soundWork.PVMODE == 0) return;

            work.pg.volume = (byte)work.pcmTables[work.soundWork.currentChip][a].Item2[3];
        }

        public void restoreOTOPCM()
        {
            DummyOUT();
            work.soundWork.PCMNUM = (byte)(work.pg.instrumentNumber + 1);
            byte a = (byte)work.pg.instrumentNumber;

            if (work.pcmTables != null
                && work.pcmTables[work.soundWork.currentChip] != null
                && work.pcmTables[work.soundWork.currentChip].Length > a)
            {
                work.soundWork.STTADR[work.soundWork.currentChip] = work.pcmTables[work.soundWork.currentChip][a].Item2[0];//start address
                work.soundWork.ENDADR[work.soundWork.currentChip] = work.pcmTables[work.soundWork.currentChip][a].Item2[1];//end address
            }

            if (work.soundWork.PVMODE == 0) return;

            work.pg.volume = (byte)work.pcmTables[work.soundWork.currentChip][a].Item2[3];
        }

        // **	ｵﾝｼｮｸ ｾｯﾄ ｻﾌﾞﾙｰﾁﾝ(FM)  **

        public void STENV()
        {
            if (work.cd.currentPageNo == work.pg.pageNo)
                KEYOFF();

            byte a = (byte)(0x80 + work.pg.channelNumber);
            byte e = 0xf;
            byte b = 4;
            //ENVLP:
            do
            {
                PSGOUT(a, e);// ﾘﾘｰｽ(RR) ｶｯﾄ ﾉ ｼｮﾘ
                a += 4;
                b--;
            } while (b != 0);

            // ﾜｰｸ ｶﾗ ｵﾝｼｮｸ ﾅﾝﾊﾞｰ ｦ ｴﾙ
            //STENV0:
            int hl = work.pg.instrumentNumber * 25;// HL=*25
            //hl += work.mData[work.soundWork.OTODAT].dat + work.mData[work.soundWork.OTODAT + 1].dat * 0x100 + 1;// HL ﾊ ｵﾝｼｮｸﾃﾞｰﾀ ｶｸﾉｳ ｱﾄﾞﾚｽ
            //hl += work.soundWork.MUSNUM;
            hl++;//音色数を格納している為いっこずらす

            //STENV1:
            byte d = 0x30;// START=PORT 30H
            d += (byte)work.pg.channelNumber;// PLUS CHANNEL No.
                                             //STENV2:
            byte c = 6;// 6 PARAMATER(Det/Mul, Total, KS/AR, DR, SR, SL/RR)
            do
            {
                b = 4;// 4 OPERATER
                      //STENV3:
                do
                {
                    // GET DATA
                    //PSGOUT(d, work.mData[hl++].dat);
                    PSGOUT(d, work.fmVoiceAtMusData[hl++]);
                    d += 4;// SKIP BLANK PORT
                    b--;
                } while (b != 0);
                c--;
            } while (c != 0);

            //e = work.mData[hl].dat;// GET FEEDBACK/ALGORIZM
            e = work.fmVoiceAtMusData[hl];// GET FEEDBACK/ALGORIZM
            // GET ALGORIZM
            work.pg.algo = e & 0x07;// STORE ALGORIZM
            // GET ALGO SET ADDRES
            d = (byte)(0xb0 + work.pg.channelNumber);// CH PLUS
            PSGOUT(d, e);
        }

        // **	ﾎﾞﾘｭｰﾑ ｾｯﾄ	**

        public void VOLPST()
        {
            DummyOUT();
            if (work.soundWork.PCMFLG != 0)
            {
                PCMVOL();
                return;
            }

            if (work.soundWork.DRMF1 != 0)
            {
                VOLDRM();
                return;
            }

            work.pg.volume = work.pg.mData[work.hl++].dat;
            STVOL();
        }

        public void PCMVOL()
        {
            byte e = (byte)work.pg.mData[work.hl++].dat;
            if (work.soundWork.PVMODE != 0)
            {
                work.pg.volReg = e;
                return;
            }
            work.pg.volume = e;
        }

        public void VOLDRM()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;

            if (work.isDotNET)
            {
                if ((a & 0x80) != 0)
                {
                    VOLDRMn((byte)(a & 0x1f));
                    return;
                }
            }

            work.pg.volume = a;
            DVOLSET();
            //VOLDR1:
            byte b = 6;
            int de = 0;// work.soundWork.DRMVOL;
                       //VOLDR2:
            do
            {
                a = (byte)(work.soundWork.DRMVOL[work.soundWork.currentChip][de] & 0b1100_0000);
                a |= (byte)work.pg.mData[work.hl++].dat;
                work.soundWork.DRMVOL[work.soundWork.currentChip][de++] = a;
                if (work.soundWork.currentChip < 2)
                    PSGOUT((byte)(0x18 - b + 6), a);
                else
                    PCMOUT(1, (byte)(0x8 - b + 6), a);
                b--;
            } while (b != 0);
        }

        public void VOLDRMn(byte a)
        {
            int inst = work.pg.instrumentNumber;
            for(int i = 0; i < 6; i++)
            {
                if(((inst >> i) & 1) != 0)
                {
                    byte b = (byte)((work.soundWork.DRMVOL[work.soundWork.currentChip][i] & 0b1100_0000) | a);
                    work.soundWork.DRMVOL[work.soundWork.currentChip][i] = b;
                    if (work.soundWork.currentChip < 2)
                        PSGOUT((byte)(0x18 + i), b);
                    else
                        PCMOUT(1, (byte)(0x8 + i), b);
                }
            }
        }

        // --   SET TOTAL RHYTHM VOL	--

        public void DVOLSET()
        {
            byte d = 0x11;
            byte a = (byte)work.pg.volume;
            a &= 0b0011_1111;
            a = (byte)(work.soundWork.TOTALV * 5 + a);
            if (a >= 64)
            {
                a = 0;
            }
            //DV2:
            if (work.soundWork.currentChip < 2)
                PSGOUT(d, a);
            else
                PCMOUT(1, 0x1, a);
        }

        // **	ﾃﾞﾁｭｰﾝ ｾｯﾄ	**

        public void FRQ_DF()
        {
            
            DummyOUT();
            work.pg.beforeCode = 0;// DETUNE ﾉ ﾊﾞｱｲﾊ BEFORE CODE ｦ CLEAR
            int de = work.pg.mData[work.hl].dat + work.pg.mData[work.hl + 1].dat * 0x100;
            work.hl += 2;
            byte a = (byte)work.pg.mData[work.hl++].dat;
            if (a != 0)
            {
                de += work.pg.detune;
            }
            //FD2:
            work.pg.detune = de;
            if (work.soundWork.PCMFLG == 0)
            {
                return;
            }

            if (work.cd.currentPageNo != work.pg.pageNo) return;

            ushort hl = (ushort)work.soundWork.DELT_N[work.soundWork.currentChip];
            hl += (ushort)de;
            if (work.soundWork.currentChip < 2)
            {
                PCMOUT(0x09, (byte)hl);
                PCMOUT(0x0a, (byte)(hl >> 8));
            }
            else
            {
                PCMOUT(0,0x19, (byte)hl);
                PCMOUT(0,0x1a, (byte)(hl >> 8));
            }
        }

        // **	SET Q COMMAND**

        public void SETQ()
        {
            work.pg.quantize = work.pg.mData[work.hl++].dat;
        }

        // **	SOFT LFO SET(RESET) **

        public void LFOON()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;// GET SUB COMMAND
            if (a != 0)
            {
                a--;//LFOTBL;
                LFOTBL[a]();
                return;
            }
            SETDEL();
            SETCO();
            SETVCT();
            SETPEK();
            work.pg.lfoflg = true;// SET LFO FLAG
        }

        public void SETDEL()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.pg.lfoDelay = a;
            work.pg.lfoDelayWork = a;
        }

        public void SETCO()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.pg.lfoCounter = a;
            work.pg.lfoCounterWork = a;
        }

        public void SETVCT()
        {
            byte e = (byte)work.pg.mData[work.hl++].dat;
            byte d = (byte)work.pg.mData[work.hl++].dat;

            work.pg.lfoDelta = e + d * 0x100;
            work.pg.lfoDeltaWork = e + d * 0x100;
        }

        public void SETPEK()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;

            work.pg.lfoPeak = a;//SET PEAK LEVEL
            a >>= 1;
            work.pg.lfoPeakWork = a;
        }

        public void LFOOFF()
        {
            work.pg.lfoflg = false;// RESET LFO
        }

        public void LFOON2()
        {
            work.pg.lfoflg = true;// LFOON
        }

        public void SETVC2()
        {
            SETVCT();
            LFORST();
        }

        public void TLLFOorSSGTremolo()
        {
            if (work.soundWork.SSGF1 == 0)
            {
                TLLFO();
                return;
            }

            SSGTremolo();
        }

        public void TLLFO()
        {

            byte a = (byte)work.pg.mData[work.hl++].dat;
            if (a == 0)
            {
                work.pg.tlLfoflg = false;
                return;
            }

        //TLL2:
            work.pg.TLlfoSlot = a;
            work.pg.tlLfoflg = true;
            a = (byte)work.pg.mData[work.hl++].dat;
            work.pg.fnum = a;
            work.pg.bfnum2 = 0;
            work.pg.TLlfo = a;
        }

        public void SSGTremolo()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            if (a == 0)
            {
                work.pg.SSGTremoloFlg = false;
                work.pg.SSGTremoloVol = 0;
                return;
            }

            work.pg.SSGTremoloFlg = true;
            work.pg.SSGTremoloVol = 0;
        }

        // **	ﾘﾋﾟｰﾄ ｽﾀｰﾄ ｾｯﾄ**

        public void REPSTF()
        {
            byte e = (byte)work.pg.mData[work.hl++].dat;
            byte d = (byte)work.pg.mData[work.hl++].dat;//DE as REWRITE ADR OFFSET +1

            int hl = (int)work.hl;
            hl -= 2;
            hl += e + d * 0x100;
            byte a = (byte)work.pg.mData[hl--].dat;
            work.pg.mData[hl].dat = a;
        }

        // **	ﾘﾋﾟｰﾄ ｴﾝﾄﾞ ｾｯﾄ(FM) **

        public void REPENF()
        {
            byte a = (byte)(work.pg.mData[work.hl].dat - (byte)1);// DEC REPEAT Co.
            work.pg.mData[work.hl].dat--;

            if (a == 0)
            {
                //REPENF2();
                work.pg.mData[work.hl].dat = work.pg.mData[work.hl + 1].dat;
                work.hl += 4;
                return;
            }

            work.hl += 2;

            byte e = (byte)work.pg.mData[work.hl++].dat;
            byte d = (byte)work.pg.mData[work.hl--].dat;

            //a &= a;
            work.hl -= (uint)(e + d * 0x100);
        }

        // **	SE DETUNE SET SUB ROUTINE**

        public void MDSET()
        {
            TO_EFC();

            if (work.isDotNET)
            {
                for (int bc = 0; bc < 4; bc++)
                {
                    byte l = (byte)work.pg.mData[work.hl++].dat;
                    byte m = (byte)work.pg.mData[work.hl++].dat;
                    work.soundWork.DETDAT[bc] = (ushort)(l + m * 0x100);
                }
            }
            else
            {
                for (int bc = 0; bc < 4; bc++)
                {
                    byte a = (byte)work.pg.mData[work.hl++].dat;
                    work.soundWork.DETDAT[bc] = a;
                }
            }
        }

        // **	CHANGE SE MODE**

        public void TO_NML()
        {
            work.soundWork.PLSET1_VAL = 0x38;
            TNML2(0x3a);
        }

        public void TO_EFC()
        {
            work.soundWork.PLSET1_VAL = 0x78;
            TNML2(0x7a);
        }

        public void TNML2(byte a)
        {
            work.soundWork.PLSET2_VAL = a;
            PSGOUT(0x27, a);
        }

        // **	STEREO**

        public void STEREO()
        {
            if (work.soundWork.DRMF1 != 0)
            {
                goto STE2;
            }
            if (work.soundWork.PCMFLG != 0)
            {
                work.soundWork.PCMLR[work.soundWork.currentChip] = work.pg.mData[work.hl++].dat;
                return;
            }
            //STER2:
            byte a = (byte)work.pg.mData[work.hl++].dat;
            byte c = (byte)(((a >> 2) & 0x3f) | (a << 6));
            byte d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
            a = (byte)(0x0B4 + work.pg.channelNumber);

            if (work.cd.currentPageNo == work.pg.pageNo)
                PSGOUT(a, d);

            return;
        STE2:
            byte dat = (byte)work.pg.mData[work.hl++].dat;
            c = dat;
            dat &= 0b0000_1111;
            a = work.soundWork.DRMVOL[work.soundWork.currentChip][dat];
            a = (byte)(((c << 2) & 0b1100_0000) | (a & 0b0001_1111));
            work.soundWork.DRMVOL[work.soundWork.currentChip][dat] = a;

            if (work.cd.currentPageNo == work.pg.pageNo)
            {
                if (work.soundWork.currentChip < 2)
                    PSGOUT((byte)(dat + 0x18), a);
                else
                    PCMOUT(1, (byte)(dat + 0x8), a);
            }

        }

        public void STEREO_AMD98()
        {
            byte a,c,d;
            if (work.soundWork.DRMF1 != 0)
            {
                STEREO_AMD98_RHYTHM();
                return;
            }

            if (work.soundWork.PCMFLG != 0)
            {
                STEREO_AMD98_ADPCM();
                return;
            }

            a = (byte)work.pg.mData[work.hl++].dat;
            work.pg.panMode = a;
            if (a >= 4)
            {
                goto STE012;
            }

            DummyOUT();

            //既存処理
            c = (byte)(((a >> 2) & 0x3f) | (a << 6));//右ローテート2回(左6回のほうがC#的にはシンプル)
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
            a = (byte)(0x0B4 + work.pg.channelNumber);

            if (work.cd.currentPageNo == work.pg.pageNo)
                PSGOUT(a, d);

            work.pg.panEnable = 0;//パーン禁止
            return;

        STE012:
            work.pg.panEnable |= 1;//パーン許可
            //work.pg.panMode = a;
            work.pg.panCounterWork = (byte)work.pg.mData[work.hl].dat;
            work.pg.panCounter = (byte)work.pg.mData[work.hl].dat;
            work.hl++;
            switch (a)
            {
                case 4:
                    work.pg.panValue = a = 2; //LEFT index
                    break;
                case 5:
                    work.pg.panValue = a = 0; //RIGHT index
                    break;
                default:
                    work.pg.panValue = a = 1; //CENTER index
                    break;
            }

            a = (byte)autoPantable[a];

            c = (byte)(a << 6);
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
            a = (byte)(0x0B4 + work.pg.channelNumber);

            if (work.cd.currentPageNo == work.pg.pageNo)
                PSGOUT(a, d);

        }

        public void STEREO_AMD98_RHYTHM()
        {
            DummyOUT();

            // bit0～3 rythmType RTHCSB
            // bit4～7 パン(1:右, 2:左, 3:中央 4:右オート 5:左オート 6:ランダム)を指定する。
            byte a = (byte)(work.pg.mData[work.hl].dat >> 4);
            byte b = (byte)(work.pg.mData[work.hl].dat & 0xf);
            work.hl++;
            byte c;
            if (b >= 6) return;

            if (a < 4)
            {
                //既存処理
                c = work.soundWork.DRMVOL[work.soundWork.currentChip][b];
                a = (byte)(((a << 6) & 0b1100_0000) | (c & 0b0001_1111));
                work.soundWork.DRMVOL[work.soundWork.currentChip][b] = a;
                if (work.cd.currentPageNo == work.pg.pageNo)
                {
                    if (work.soundWork.currentChip < 2)
                        PSGOUT((byte)(b + 0x18), a);
                    else
                        PCMOUT(1, (byte)(b + 0x8), a);
                }
                work.soundWork.DrmPanEnable[work.soundWork.currentChip][b] = 0;//パーン禁止
                return;
            }

            work.soundWork.DrmPanEnable[work.soundWork.currentChip][b] |= 1;//パーン許可
            work.soundWork.DrmPanMode[work.soundWork.currentChip][b] = a;
            work.soundWork.DrmPanCounterWork[work.soundWork.currentChip][b] = (byte)work.pg.mData[work.hl].dat;
            work.soundWork.DrmPanCounter[work.soundWork.currentChip][b] = (byte)work.pg.mData[work.hl].dat;
            work.hl++;

            switch (a)
            {
                case 4:
                    work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 2;
                    break;
                case 5:
                    work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 0;
                    break;
                default:
                    work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 1;
                    break;
            }

            a = (byte)autoPantable[a];
            c = work.soundWork.DRMVOL[work.soundWork.currentChip][b];
            a = (byte)(((a << 6) & 0b1100_0000) | (c & 0b0001_1111));
            work.soundWork.DRMVOL[work.soundWork.currentChip][b] = a;
            if (work.cd.currentPageNo == work.pg.pageNo)
                if (work.cd.currentPageNo == work.pg.pageNo)
                {
                    if (work.soundWork.currentChip < 2)
                        PSGOUT((byte)(b + 0x18), a);
                    else
                        PCMOUT(1, (byte)(b + 0x8), a);
                }
        }

        public void STEREO_AMD98_ADPCM()
        {
            DummyOUT();

            byte a = (byte)work.pg.mData[work.hl++].dat;

            if (a < 4)
            {
                //既存処理
                work.soundWork.PCMLR[work.soundWork.currentChip] = a;
                work.pg.panValue = a;
                work.pg.panEnable = 0;//パーン禁止
                return;
            }

            work.pg.panEnable |= 1;//パーン許可
            work.pg.panMode = a;
            work.pg.panCounterWork = (byte)work.pg.mData[work.hl].dat;
            work.pg.panCounter = (byte)work.pg.mData[work.hl].dat;
            work.hl++;

            switch (a)
            {
                case 4:
                    work.pg.panValue = a = 2;
                    break;
                case 5:
                    work.pg.panValue = a = 0;
                    break;
                default:
                    work.pg.panValue = a = 1;
                    break;
            }

            a = (byte)autoPantable[a];
            work.soundWork.PCMLR[work.soundWork.currentChip] = a;

            if (work.soundWork.currentChip < 2)
            {
                if (work.cd.currentPageNo == work.pg.pageNo)
                    PCMOUT(0x01, (byte)(a << 6));
            }
            else
            {
                if (work.cd.currentPageNo == work.pg.pageNo)
                    PCMOUT(0,0x11, (byte)(a << 6));
            }
        }

        public void restoreSTEREO_AMD98()
        {
            byte a, c, d;
            if (work.soundWork.DRMF1 != 0)
            {
                restoreSTEREO_AMD98_RHYTHM();
                return;
            }

            if (work.soundWork.PCMFLG != 0)
            {
                restoreSTEREO_AMD98_ADPCM();
                return;
            }

            a = work.pg.panMode;
            if (a >= 4)
            {
                goto STE012;
            }

            DummyOUT();

            //既存処理
            c = (byte)(((a >> 2) & 0x3f) | (a << 6));//右ローテート2回(左6回のほうがC#的にはシンプル)
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
            a = (byte)(0x0B4 + work.pg.channelNumber);
            PSGOUT(a, d);
            work.pg.panEnable = 0;//パーン禁止
            return;

        STE012:
            work.pg.panEnable |= 1;//パーン許可
            work.pg.panCounterWork = work.pg.panCounter;
            switch (a)
            {
                case 4:
                    work.pg.panValue = a = 2; //LEFT index
                    break;
                case 5:
                    work.pg.panValue = a = 0; //RIGHT index
                    break;
                default:
                    work.pg.panValue = a = 1; //CENTER index
                    break;
            }

            a = (byte)autoPantable[a];

            c = (byte)(a << 6);
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
            a = (byte)(0x0B4 + work.pg.channelNumber);

            PSGOUT(a, d);

        }

        public void restoreSTEREO_AMD98_RHYTHM()
        {
            DummyOUT();

            for (byte b = 0; b < 6; b++)
            {
                byte a = work.soundWork.DRMVOL[work.soundWork.currentChip][b];
                if (work.soundWork.DrmPanMode[work.soundWork.currentChip][b] < 4)
                {
                    if (work.cd.currentPageNo == work.pg.pageNo)
                    {
                        if (work.soundWork.currentChip < 2)
                            PSGOUT((byte)(b + 0x18), a);
                        else
                            PCMOUT(1, (byte)(b + 0x8), a);
                    }
                    work.soundWork.DrmPanEnable[work.soundWork.currentChip][b] = 0;//パーン禁止
                    continue;
                }

                work.soundWork.DrmPanEnable[work.soundWork.currentChip][b] |= 1;//パーン許可
                work.soundWork.DrmPanCounterWork[work.soundWork.currentChip][b] = work.soundWork.DrmPanCounter[work.soundWork.currentChip][b];

                switch (a)
                {
                    case 4:
                        work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 2;
                        break;
                    case 5:
                        work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 0;
                        break;
                    default:
                        work.soundWork.DrmPanValue[work.soundWork.currentChip][b] = a = 1;
                        break;
                }

                a = (byte)autoPantable[a];
                byte c = work.soundWork.DRMVOL[work.soundWork.currentChip][b];
                a = (byte)(((a << 6) & 0b1100_0000) | (c & 0b0001_1111));
                work.soundWork.DRMVOL[work.soundWork.currentChip][b] = a;
                if (work.cd.currentPageNo == work.pg.pageNo)
                {
                    if (work.soundWork.currentChip < 2)
                        PSGOUT((byte)(b + 0x18), a);
                    else
                        PCMOUT(1, (byte)(b + 0x8), a);
                }
            }

            // bit0～3 rythmType RTHCSB
            // bit4～7 パン(1:右, 2:左, 3:中央 4:右オート 5:左オート 6:ランダム)を指定する。
        }

        public void restoreSTEREO_AMD98_ADPCM()
        {
            DummyOUT();
            byte a = (byte)work.pg.panMode;
            if (a < 4)
            {
                work.pg.panEnable = 0;//パーン禁止
                a = work.pg.panValue;
                work.soundWork.PCMLR[work.soundWork.currentChip] = a;
                if (work.soundWork.currentChip < 2)
                    PCMOUT(0x01, (byte)(a << 6));
                else
                    PCMOUT(0,0x11, (byte)(a << 6));
                return;
            }

            work.pg.panEnable |= 1;//パーン許可
            work.pg.panCounterWork = work.pg.panCounter;

            switch (a)
            {
                case 4:
                    work.pg.panValue = a = 2;
                    break;
                case 5:
                    work.pg.panValue = a = 0;
                    break;
                default:
                    work.pg.panValue = a = 1;
                    break;
            }

            a = (byte)autoPantable[a];
            work.soundWork.PCMLR[work.soundWork.currentChip] = a;
            if (work.soundWork.currentChip < 2)
                PCMOUT(0x01, (byte)(a << 6));
            else
                PCMOUT(0,0x11, (byte)(a << 6));
        }

        public void PANNING()
        {
            if (work.soundWork.DRMF1 != 0)
            {
                PANNING_RHYTHM();
                return;
            }

            if ((work.pg.panEnable & 1) == 0) return;
            if ((--work.pg.panCounterWork) != 0) return;

            work.pg.panCounterWork = work.pg.panCounter;//; カウンター再設定

            if (work.pg.panMode == 4 || work.pg.panMode == 5)
            {
                //LEFT / RIGHT
                byte ah = work.pg.panValue;
                ah++;
                if (ah == autoPantable.Length)
                {
                    ah = 0;
                }
                work.pg.panValue = ah;//ah : 0～
            }
            else
            {
                //RANDOM
                ushort ax;
                do
                {
                    ax = work.soundWork.RANDUM;
                    ax *= 5;
                    ax += 0x1993;
                    work.soundWork.RANDUM = ax;
                    ax &= 0x0300;
                } while (ax == 0);
                work.pg.panValue = (byte)(ax >> 8);//ah : 1～3
            }

            byte a, c, d;
            a = (work.pg.panMode == 4 || work.pg.panMode == 5) ? autoPantable[work.pg.panValue] : work.pg.panValue;

            List<object> args = new List<object>();
            args.Add((int)a);

            LinePos lp = new LinePos(null,"", -1, -1, -1
                , work.soundWork.SSGF1 == 0 ? "FM" : "ADPCM"
                , "YM2608", 0, 0, work.pg.channelNumber);
            work.crntMmlDatum = new MmlDatum(enmMMLType.Pan, args, lp, 0);
            DummyOUT();

            if (work.soundWork.PCMFLG == 0)
            {
                c = (byte)(((a >> 2) & 0x3f) | (a << 6));//右ローテート2回(左6回のほうがC#的にはシンプル)
                d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo];
                d = (byte)((d & 0b0011_1111) | c);
                work.soundWork.PALDAT[work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo] = d;
                a = (byte)(0x0B4 + work.pg.channelNumber);

                if (work.cd.currentPageNo == work.pg.pageNo)
                    PSGOUT(a, d);

                return;
            }

            work.soundWork.PCMLR[work.soundWork.currentChip] = a;
            c = (byte)((a << 6) & 0xc0);

            if (work.soundWork.currentChip < 2)
            {
                if (work.cd.currentPageNo == work.pg.pageNo)
                    PCMOUT(0x01, c);
            }
            else
            {
                if (work.cd.currentPageNo == work.pg.pageNo)
                    PCMOUT(0,0x11, c);
            }
        }

        public void PANNING_RHYTHM()
        {
            for (int n = 0; n < 6; n++)
            {
                if ((work.soundWork.DrmPanEnable[work.soundWork.currentChip][n] & 1) == 0) continue;
                if ((--work.soundWork.DrmPanCounterWork[work.soundWork.currentChip][n]) != 0) continue;

                work.soundWork.DrmPanCounterWork[work.soundWork.currentChip][n] = work.soundWork.DrmPanCounter[work.soundWork.currentChip][n];//; カウンター再設定

                if (work.soundWork.DrmPanMode[work.soundWork.currentChip][n] == 4|| work.soundWork.DrmPanMode[work.soundWork.currentChip][n] == 5)
                {
                    //LEFT / RIGHT
                    byte ah = work.soundWork.DrmPanValue[work.soundWork.currentChip][n];
                    ah++;
                    if (ah == autoPantable.Length)
                    {
                        ah = 0;
                    }
                    work.soundWork.DrmPanValue[work.soundWork.currentChip][n] = ah;//ah : 0～
                }
                else
                {
                    //RANDOM
                    ushort ax;
                    do
                    {
                        ax = work.soundWork.RANDUM;
                        ax *= 5;
                        ax += 0x1993;
                        work.soundWork.RANDUM = ax;
                        ax &= 0x0300;
                    } while (ax == 0);
                    work.soundWork.DrmPanValue[work.soundWork.currentChip][n] = (byte)(ax >> 8);//ah : 1～3
                }

                byte a = (work.soundWork.DrmPanMode[work.soundWork.currentChip][n] == 4 || work.soundWork.DrmPanMode[work.soundWork.currentChip][n] == 5) 
                    ? autoPantable[work.soundWork.DrmPanValue[work.soundWork.currentChip][n]] 
                    : work.soundWork.DrmPanValue[work.soundWork.currentChip][n];

                byte c = work.soundWork.DRMVOL[work.soundWork.currentChip][n];
                a = (byte)(((work.soundWork.DrmPanValue[work.soundWork.currentChip][n] << 6) & 0b1100_0000) | (c & 0b0001_1111));
                work.soundWork.DRMVOL[work.soundWork.currentChip][n] = a;

                if (work.cd.currentPageNo == work.pg.pageNo)
                    if (work.cd.currentPageNo == work.pg.pageNo)
                    {
                        if (work.soundWork.currentChip < 2)
                            PSGOUT((byte)(n + 0x18), a);
                        else
                            PCMOUT(1, (byte)(n + 0x8), a);
                    }

            }
        }

        // **	ﾌﾗｸﾞｾｯﾄ**

        public void FLGSET()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.soundWork.FLGADR = a;
        }

        // **   WRITE REG   **

        public void W_REG()
        {
            byte d = (byte)work.pg.mData[work.hl++].dat;
            byte e = (byte)work.pg.mData[work.hl++].dat;
            PSGOUT(d, e);
        }

        // **	VOLUME UP & DOWN**

        public void VOLUPF()
        {
            if (work.soundWork.DRMF1 != 0)
            {
                byte n = (byte)work.pg.mData[work.hl++].dat;

                if (work.isDotNET)
                {
                    if ((n&0x80)!=0)
                    {
                        VOLUPF_Rhythm(n);
                        return;
                    }
                }

                work.pg.volume += (sbyte)n;
                DVOLSET();
                return;
            }

            work.pg.volume += work.pg.mData[work.hl++].dat;

            if (work.soundWork.PCMFLG != 0)
            {
                return;
            }

            STVOL();
        }

        public void VOLUPF_Rhythm(byte a)
        {
            int inst = work.pg.instrumentNumber;
            for (int i = 0; i < 6; i++)
            {
                if (((inst >> i) & 1) != 0)
                {
                    byte b = (byte)((sbyte)((a & 0x3f) | ((a & 0x40) != 0 ? 0xc0 : 0)) + (work.soundWork.DRMVOL[work.soundWork.currentChip][i] & 0x3f));
                    b = (byte)((work.soundWork.DRMVOL[work.soundWork.currentChip][i] & 0b1100_0000) | b);
                    work.soundWork.DRMVOL[work.soundWork.currentChip][i] = b;
                    if (work.cd.currentPageNo == work.pg.pageNo)
                    {
                        if (work.soundWork.currentChip < 2)
                            PSGOUT((byte)(i + 0x18), b);
                        else
                            PCMOUT(1, (byte)(i + 0x8), b);
                    }
                }
            }
        }

        // **	HARD LFO SET**

        public void HLFOON()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;// FREQ CONT
            a |= 0b0000_1000;
            PSGOUT(0x22, a);

            byte c = (byte)work.pg.mData[work.hl++].dat;// PMS
            c = (byte)(c | (work.pg.mData[work.hl++].dat << 4));// AMS+PMS
            int de = work.soundWork.FMPORT + work.pg.channelNumber * 10 + work.pg.pageNo; // PALDAT;
            a = (byte)((work.soundWork.PALDAT[de] & 0b1100_0000) | c);
            work.soundWork.PALDAT[de] = a;
            PSGOUT((byte)(0xb4 + work.pg.channelNumber), a);
        }

        public void TIE()
        {
            work.pg.keyoffflg = false;
        }

        // **	ﾘﾋﾟｰﾄ ｽｷｯﾌﾟ	**

        public void RSKIP()
        {
            byte e = (byte)work.pg.mData[work.hl++].dat;
            byte d = (byte)work.pg.mData[work.hl++].dat;

            uint hl = work.hl;
            hl -= 2;
            hl += (uint)(e + d * 0x100);

            byte a = (byte)work.pg.mData[hl].dat;
            a--;// LOOP ｶｳﾝﾀ = 1 ?
            if (a == 0)
            {
                hl += 4;// HL = JUMP ADR
                work.hl = hl;
                return;
            }

        }

        public void SECPRC()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            a &= 0xf;// A=COMMAND No.(0-F)

            FMCOM2[a]();
        }

        public void NTMEAN() { }

        // **	PCM VMODE CHANGE**

        public void PVMCHG()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.soundWork.PVMODE = a;
        }

        // **	ﾘﾊﾞｰﾌﾞ**

        public void REVERVE()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            work.pg.reverbVol = a;
            //RV1:
            work.pg.reverbFlg = true;
        }

        public void REVSW()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            if (a != 0)
            {
                //goto RV1;
                work.pg.reverbFlg = true;
                return;
            }
            work.pg.reverbFlg = false;

            //if (work.idx >= 3 && work.idx <= 5) return;
            if (work.soundWork.SSGF1 != 0) return;

            STVOL();
        }

        public void REVMOD()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;
            if (a != 0)
            {
                work.pg.reverbMode = true;
                return;
            }
            //RM2:
            work.pg.reverbMode = false;
        }




        // **   PSG ｵﾝｼｮｸｾｯﾄ   **

        public void OTOSSG()
        {
            DummyOUT();

            byte a = (byte)work.pg.mData[work.hl++].dat;

            //OTOCAL
            int ptr = 0;// SSGDAT;
            ptr = a * 6;
            //ENVPST();
            for (int i = 0; i < 6; i++)
            {
                work.pg.softEnvelopeParam[i] = work.soundWork.SSGDAT[ptr + i];
            }
            work.pg.volume = work.pg.volume | 0b1001_0000;

        }

        public void OTOSET()
        {
            byte a = (byte)work.pg.mData[work.hl++].dat;

            //OTOCAL
            int ptr = 0;// SSGDAT;
            ptr = a * 6;

            for (int i = 0; i < 6; i++)
            {
                work.soundWork.SSGDAT[ptr + i] = (byte)work.pg.mData[work.hl++].dat;
            }

        }

        // **   ｴﾝﾍﾞﾛｰﾌﾟ ﾊﾟﾗﾒｰﾀ ｾｯﾄ**

        public void ENVPST()
        {
            for (int i = 0; i < 6; i++)
            {
                work.pg.softEnvelopeParam[i] = work.pg.mData[work.hl++].dat;
            }
            work.pg.volume = work.pg.volume | 0b1001_0000;// ｴﾝﾍﾞﾌﾗｸﾞ ｱﾀｯｸﾌﾗｸﾞ ｾｯﾄ

        }

        // **	PSG VOLUME	**

        public void PSGVOL()
        {
            DummyOUT();
            work.pg.hardEnveFlg = false;
            byte e = (byte)(work.pg.volume & 0b1111_0000);
            byte c = (byte)work.pg.mData[work.hl].dat;
            PV1(c, e);
        }

        public void PV1(byte c, byte e)
        {
            byte a = work.soundWork.TOTALV;
            a += c;
            if (a < 16)
            {
                goto PV2;
            }
            a = 0;
        PV2:
            a |= e;
            work.hl++;
            work.pg.volume = a;
        }

        // **   MIX PORT CONTROL**

        public void NOISE()
        {
            work.pg.backupMIXPort = (byte)work.pg.mData[work.hl++].dat;
            if (work.pg.pageNo != work.cd.currentPageNo) return;

            tNOISE();
        }
        
        public void restoreNOISE()
        {
            tNOISE();
        }

        public void tNOISE()
        {
            byte c = work.pg.backupMIXPort;
            byte b = (byte)work.pg.channelNumber;
            byte e = work.soundWork.PREGBF[5];
            b >>= 1;
            b++;
            byte d = b;
            byte a = 0b0111_1011;
            //NOISE1:
            do
            {
                a = (byte)((a << 1) | (a >> 7));
                b--;
            } while (b != 0);
            a &= e;
            e = a;
            a = c;
            b = d;
            a = (byte)((a >> 1) | (a << 7));
            //NOISE2:
            do
            {
                a = (byte)((a << 1) | (a >> 7));
                b--;
            } while (b != 0);
            a |= e;
            d = 7;
            e = a;
            PSGOUT(d, e);
            work.soundWork.PREGBF[5] = e;
        }


        // **   ﾉｲｽﾞ ｼｭｳﾊｽｳ   **

        public void NOISEW()
        {
            work.pg.backupNoiseFrq = (byte)work.pg.mData[work.hl++].dat;
            if (work.pg.pageNo != work.cd.currentPageNo) return;

            tNOISEW();

        }
        public void restoreNOISEW()
        {
            tNOISEW();
        }
        public void tNOISEW()
        {
            byte e = (byte)work.pg.backupNoiseFrq;
            PSGOUT(6, e);
            work.soundWork.PREGBF[4] = e;
        }

        // **	SSG VOLUME UP & DOWN**

        public void VOLUPS()
        {
            byte d = (byte)work.pg.mData[work.hl++].dat;
            if (!work.pg.hardEnveFlg)
            {
                byte a = (byte)work.pg.volume;
                byte e = a;
                a &= 0b0000_1111;
                a += d;
                if (a >= 16)
                {
                    return;
                }
                d = a;
                a = e;
                a &= 0b1111_0000;
                a |= d;
                work.pg.volume = a;
            }
        }

        // **	LFO ﾙｰﾁﾝ	**

        public void PLLFO()
        {
            if (work.pg.pageNo != work.cd.currentPageNo) return;

            // ---	FOR FM & SSG LFO	---
            if (!work.pg.lfoflg)
            {
                return;
            }
            uint hl = work.pg.dataAddressWork;
            hl--;
            byte a = (byte)work.pg.mData[hl].dat;
            if (a == 0xf0)
            {
                return;//  ｲｾﾞﾝ ﾉ ﾃﾞｰﾀ ｶﾞ '&' ﾅﾗ RET
            }
            if (!work.pg.lfoContFlg)
            {
                // **	LFO INITIARIZE   **
                LFORST();
                LFORST2();
                work.pg.lfoCounterWork = work.pg.lfoCounter;
                work.pg.lfoContFlg = true;// SET CONTINUE FLAG
            }
            //CTLFO:
            if (work.pg.lfoDelayWork == 0)//delayが完了していたら次の処理へ
            {
                CTLFO1();
                return;
            }
            work.pg.lfoDelayWork--;//delayのカウントダウン
        }

        public void CTLFO1()
        {
            work.pg.lfoCounterWork--;// ｶｳﾝﾀ
            if (work.pg.lfoCounterWork != 0)
            {
                return;
            }
            work.pg.lfoCounterWork = work.pg.lfoCounter;//ｶｳﾝﾀ ｻｲ ｾｯﾃｲ
            if (work.pg.lfoPeakWork == 0)//  GET PEAK LEVEL COUNTER(P.L.C)
            {
                work.pg.lfoDeltaWork = -work.pg.lfoDeltaWork;// WAVE ﾊﾝﾃﾝ
                work.pg.lfoPeakWork = work.pg.lfoPeak;//  P.L.C ｻｲ ｾｯﾃｲ
            }
            //PLLFO1:
            work.pg.lfoPeakWork--;// P.L.C.-1
            int hl = work.pg.lfoDeltaWork;
            PLS2(hl);
        }

        public void PLS2(int hl)
        {
            if (work.soundWork.PCMFLG == 0)
            {
                PLSKI2(hl);
                return;
            }

            hl += (int)work.soundWork.DELT_N[work.soundWork.currentChip];
            work.soundWork.DELT_N[work.soundWork.currentChip] = (uint)hl;

            if (work.soundWork.currentChip < 2)
            {
                PCMOUT(0x09, (byte)hl);
                PCMOUT(0x0a, (byte)(hl >> 8));
            }
            else
            {
                PCMOUT(0,0x19, (byte)hl);
                PCMOUT(0,0x1a, (byte)(hl >> 8));
            }
        }

        public void PLSKI2(int hl)
        {
            if (work.soundWork.SSGF1 != 0 && work.pg.SSGTremoloFlg)
            {
                work.pg.SSGTremoloVol += hl;
                //Console.WriteLine(work.pg.SSGTremoloVol);
                return;
            }

            if (work.soundWork.SSGF1 == 0)
            {
                //KUMA:FMの時はリミットチェック処理

                int num = work.pg.fnum & 0x7ff;
                int blk = work.pg.fnum >> 11;
                short dlt = (short)(ushort)hl;
                //Console.Write("b:{0} num:{1:x} -> +{2}", blk, num, dlt);

                num += dlt;

                if (dlt > 0)
                {
                    //0x26a == Note C
                    while (num > 0x26a * 2 && blk < 7)
                    {
                        blk++;
                        num -= 0x26a;
                    }
                }
                else
                {
                    while (num < 0x26a && blk > 0)
                    {
                        blk--;
                        num += 0x26a;
                    }
                }

                //Console.WriteLine(" -> b:{0} num:{1:x}",blk,num);

                hl = (blk << 11) | num;
            }
            else
            {
                //KUMA:SSGの時は既存の処理

                int de = work.pg.fnum;// GET FNUM1
                // GET B/FNUM2
                hl += de;//  HL= NEW F-NUMBER
                hl = (ushort)hl;
            }

            work.pg.fnum = hl;// SET NEW F-NUM1
                              // SET NEW F-NUM2

            if (work.soundWork.SSGF1 == 0)
            {
                LFOP5(hl);
                return;
            }

            // ---	FOR SSG LFO	---
            byte a = (byte)work.pg.beforeCode;// GET KEY CODE&OCTAVE
            a >>= 4;
            if (a != 0)//  OCTAVE=1?
            {
                byte b = a;
                //SNUMGETL:
                do
                {
                    hl >>= 1;
                    b--;
                } while (b != 0);
            }
            //SSLFO2:
            byte e = (byte)hl;
            byte d = (byte)work.pg.channelNumber;
            PSGOUT(d, e);
            d++;
            e = (byte)(hl >> 8);
            PSGOUT(d, e);
        }

        // ---	FOR FM LFO	---

        public void LFOP5(int hl)
        {
            if (work.pg.tlLfoflg)
            {
                LFOP6(hl);
                return;
            }

            if ((work.pg.channelNumber & 0x02) == 0)//  CH=3?
            {
                PLLFO2(hl);// NOT CH3 THEN PLLFO2
                return;
            }

            if (work.soundWork.PLSET1_VAL != 0x78)
            {
                PLLFO2(hl);// NOT SE MODE
                return;
            }

            work.soundWork.NEWFNM = hl;
            //LFOP4:
            hl = 0;
            int iy = 0;
            byte b = 4;
            //LFOP3:
            do
            {
                int fnum = work.soundWork.NEWFNM + work.soundWork.DETDAT[hl++];

                byte d = work.soundWork.OP_SEL[iy++];
                byte e = (byte)(fnum >> 8);
                PSGOUT(d, e);

                d -= 4;
                e = (byte)fnum;
                PSGOUT(d, e);

                b--;
            } while (b != 0);

        }

        public void PLLFO2(int hl)
        {
            byte d = 0xa4;//  PORT A4H
            d += (byte)work.pg.channelNumber;
            byte e = (byte)(hl >> 8);
            PSGOUT(d, e);

            d -= 4;
            e = (byte)hl;// F-NUMBER1 DATA
            PSGOUT(d, e);
        }

        public void LFOP6(int hl)
        {
            byte c = work.pg.TLlfoSlot;//.soundWork.LFOP6_VAL;
            
            byte d = 0x40;
            d += (byte)work.pg.channelNumber;
            byte e = (byte)hl;

            if ((c & 0x01) != 0)
            {
                PSGOUT(d, e);
                d += 4;
            }
            if ((c & 0x04) != 0)
            {
                PSGOUT(d, e);
                d += 4;
            }
            if ((c & 0x02) != 0)
            {
                PSGOUT(d, e);
                d += 4;
            }
            if ((c & 0x08) == 0)
            {
                return;
            }
            PSGOUT(d, e);
            d += 4;
        }





        //SSG:
        // **	SSG ｵﾝｹﾞﾝｴﾝｿｳ ﾙｰﾁﾝ**

        public void SSGSUB()
        {
            //work.cd = work.soundWork.CHDAT[work.idx];
            //work.pg = work.cd.PGDAT[0];
            work.hl = work.pg.dataAddressWork;

            work.pg.lengthCounter = (byte)(work.pg.lengthCounter - 1);
            if (work.pg.lengthCounter == 0)
            {
                SSSUB7();
                return;
            }

            if (work.pg.lengthCounter != work.pg.quantize )
            {
                SSSUB0();
                return;
            }

            if (work.pg.mData[work.pg.dataAddressWork].dat == 0xfd)//COUNT OVER?
            {
                goto SSUB0;
            }
            SSSUBA();// TO REREASE
            return;//    RET
        SSUB0:
            work.pg.keyoffflg = false;//SET TIE FLAG(たぶんキーオフをリセット)
            SSSUB0();
        }

        public void SSSUB0()
        {
            if (work.pg.pageNo != work.cd.currentPageNo)
                return;

            if ((work.pg.volume & 0x80) == 0)// ENVELOPE CHECK
            {
                return;
            }

            SOFENV();

            if (work.pg.SSGTremoloFlg)
            {
                work.A_Reg = (byte)Math.Max(Math.Min((work.A_Reg + work.pg.SSGTremoloVol / 16), 15), 0);
            }
            

            byte e = work.A_Reg;
            if (work.soundWork.READY == 0)
            {
                e = 0;
            }
            if (work.soundWork.KEY_FLAG != 0xff)
            {
                byte d = (byte)work.pg.volReg;
                if (work.pg.pageNo == work.cd.currentPageNo) PSGOUT(d, e);
            }
            return;
        }

        public void SSSUB7()
        {
            work.hl = work.pg.dataAddressWork;
            if (work.pg.mData[work.hl].dat == 0xfd)//COUNT OVER?
            {
                //SSUB1:
                work.pg.keyoffflg = false;//SET TIE FLAG(たぶんキーオフをリセット)
                work.hl++;
                SSSUBB();
                return;
            }
            //SSSUBE:
            work.pg.keyoffflg = true;
            SSSUBB();
        }

        // **	KEY OFF ｼﾞ ﾉ RR ｼｮﾘ	**

        public void SSSUBA()
        {
            // --	HARD ENV.KEY OFF   --
            if (work.pg.hardEnveFlg)
            {
                if (work.pg.pageNo == work.cd.currentPageNo) PSGOUT((byte)work.pg.volReg, 0);//SSG KEY OFF
            }

            // --	SOFT ENV.KEY OFF   --

            if (work.pg.reverbFlg)
            {
                work.pg.keyoffflg = false;
                SSSUB0();
                return;
            }

            //SSUBAC:
            if ((work.pg.volume & 0x80) == 0)
            {
                SSSUB3(0);// ﾘﾘｰｽ ｼﾞｬﾅｹﾚﾊﾞ SSSUB3
                return;
            }
            work.pg.volume &= 0b1000_1111;// STATE 4 (ﾘﾘｰｽ)
            SOFEV9();
            SSSUB3(work.A_Reg);
        }

        public void SSSUBB()
        {
            work.crntMmlDatum = work.pg.mData[work.hl];

            byte a;
            do
            {
                if (work.pg.mData[work.hl].dat == 0)// CHECK END MARK
                {
                    work.pg.loopEndFlg = true;
                    // HL=DATA TOP ADD
                    if (work.pg.dataTopAddress == -1)
                    {
                        SSGEND();
                        return;
                    }
                    work.hl = (uint)work.pg.dataTopAddress;
                    work.pg.loopCounter++;
                    if (work.pg.loopCounter > work.nowLoopCounter) work.nowLoopCounter = work.pg.loopCounter;
                }

                //演奏情報退避
                work.crntMmlDatum = work.pg.mData[work.hl];

                //SSSUB1:
                //SSSUB2:
                a = (byte)work.pg.mData[work.hl++].dat;// INPUT FLAG &LENGTH
                if (a < 0xf0) break;
                //COMMAND OF PSG?
                //SSSUB8();
                a &= 0xf;// A=COMMAND No.(0-F)
                PSGCOM[a]();

            } while (true);

            bool carry = ((a & 0x80) != 0);
            a &= 0x7f;// CY=REST FLAG

            work.pg.lengthCounter = a;//  SET WAIT COUNTER
                                      //  ｷｭｳﾌ ﾅﾗ SSSUBA
            if (carry)
            {
                work.crntMmlDatum = work.pg.mData[work.hl - 1];
                SSSUBA();
                SETPT();
                return;
            }

            // **	SET FINE TUNE & COARSE TUNE	**

            //SSSUB6:
            a = (byte)work.pg.mData[work.hl++].dat;// LOAD OCT & KEY CODE
            byte b, c;
            if (!work.pg.keyoffflg)
            {
                c = a;
                b = (byte)work.pg.beforeCode;
                a -= b;
                if (a == 0)
                {
                    SETPT();// IF NOW CODE=BEFORE CODE THEN SETPT
                    return;
                }
                a = c;
                //goto SSSKIP0;// NON TIE
            }

            //SSSKIP0:
            work.pg.beforeCode = a;// STORE KEY CODE & OCTAVE

            //Mem.stack.Push(Z80.HL);


            if (work.cd.keyOnCh != -1 && work.cd.keyOnCh != work.pg.pageNo)// || !work.pg.keyoffflg)//KUMA:既に他のページが発音中の場合は処理しない
            {
                SETPT();//KUMA:演奏位置の更新
                return;
            }
            work.cd.keyOnCh = work.pg.pageNo;
            if (work.cd.currentPageNo != work.pg.pageNo)
            {
                work.cd.currentPageNo = work.pg.pageNo;
                //復帰処理
                restoreNOISE();
                restoreNOISEW();
                restoreHRDENV();
                restoreENVPOD();
                //work.pg.lfoflg = false;
            }

            b = a;
            a &= 0b0000_1111;//  GET KEY CODE
            byte e = a;
            int hl = work.soundWork.SNUMB[work.soundWork.currentChip / 2][e];// GET FNUM2
            int de = work.pg.detune;// GET DETUNE DATA
            hl += de;//  DETUNE PLUS
            hl = (short)hl;
            work.pg.fnum = hl;// SAVE FOR LFO
            b >>= 4;//  OCTAVE=1?
            if (b != 0)
            {
                //SSSUB5:
                do
                {
                    hl >>= 1;
                    b--;
                } while (b != 0);// OCTAVE DATA ﾉ ｹｯﾃｲ
                //  1 ﾅﾗ SSSUB4 ﾍ
            }
            
            //KUMA:FNUMのセット
            //SSSUB4:
            e = (byte)hl;
            byte d = (byte)work.pg.channelNumber;
            PSGOUT(d, e);
            e = (byte)(hl >> 8);
            d++;
            PSGOUT(d, e);

            if (work.pg.keyoffflg)
            {
                goto SSSUBF;
            }
            SOFENV();
            goto SSSUB9;

        SSSUBF:         // KEYON ｻﾚﾀﾄｷ ﾉ ｼｮﾘ

            if (work.pg.hardEnveFlg)
            {
                //// ---   HARD ENV. KEY ON
                if (work.soundWork.KEY_FLAG != 0xff)
                {
                    PSGOUT((byte)work.pg.volReg, 0x10);
                }
                PSGOUT(0x0d, (byte)work.pg.hardEnvelopValue);
            }
            else
            {
                //// ---	SOFT ENV.KEYON     ---

                //SSSUBG:
                a = (byte)work.pg.volume;
                a &= 0b0000_1111;
                a |= 0b1001_0000;//  TO STATE 1 (ATTACK)
                work.pg.volume = a;

                a = (byte)work.pg.softEnvelopeParam[0];//  ENVE INIT
                work.pg.softEnvelopeCounter = a;//KUMA:ALがcounterの初期値として使用される
                work.pg.lfoContFlg = false;// RESET LFO CONTINE FLAG
                SOFEV7();

                //SSSUBH:
                c = (byte)work.pg.lfoPeak;
                c >>= 1;
                work.pg.lfoPeakWork = c;//  LFO PEAK LEVEL ｻｲ ｾｯﾃｲ
                work.pg.lfoDelayWork = work.pg.lfoDelay;//  LFO DELAY ﾉ ｻｲｾｯﾃｲ
            }
        SSSUB9:
            //Z80.HL = Mem.stack.Pop();

            // **   VOLUME OUT PROCESS**

            //
            //  ENTRY A: VOLUME DATA
            //
            SSSUB3(work.A_Reg);
        }

        // **	SOFT ENVEROPE PROCESS**

        public void SOFENV()
        {
            if ((work.pg.volume & 0x10) == 0)// CHECK ATTACK FLAG
            {
                goto SOFEV2; //KUMA:decay flagのチェックへゴー
            }

            byte a = (byte)work.pg.softEnvelopeCounter;  //KUMA:get counter
            byte d = (byte)work.pg.softEnvelopeParam[1];  //KUMA:get AR
            bool carry = ((a + d) > 0xff); //KUMA:counter + AR が255を超えたか？
            a += d;
            if (!carry)
            {
                goto SOFEV1;
            }
            a = 0xff; //KUMA:counterが上限を突破したので,counterを255に修正
        SOFEV1: //KUMA:counterとflagの更新
            work.pg.softEnvelopeCounter = a; //KUMA: counter = counter + AR(毎クロック,AR分だけcounterが増える)
            if ((a - 0xff) != 0)
            {
                SOFEV7(); //KUMA:counterが255に達していないならSOFEV7へ
                return;
            }
            a = (byte)work.pg.volume;//KUMA:current volume & flagsを取得
            a ^= 0b0011_0000;//KUMA:attack flag:off  decay flag:on をxorで実現(上手い)
            work.pg.volume = a;// TO STATE 2 (DECAY) //KUMA:current volume & flagsを更新
            SOFEV7();
            return;
        SOFEV2:
            if ((work.pg.volume & 0x20) == 0)//KUMA: Check decay flag
            {
                goto SOFEV4;//KUMA:sustain flagのチェックへ
            }
            a = (byte)work.pg.softEnvelopeCounter;// KUMA:get counter
            d = (byte)work.pg.softEnvelopeParam[2];// GET DECAY //KUMA:get DR
            byte e = (byte)work.pg.softEnvelopeParam[3];// GET SUSTAIN //KUMA:get SR
            carry = ((a - d) < 0); //KUMA:counter = counter - DR 結果、counterが0未満の場合はSOFEV8へ
            a -= d;
            if (carry)
            {
                goto SOFEV8;
            }
            if (a - e >= 0)//KUMA:counter-SR は0以上の場合はSOFEV3へ
            {
                goto SOFEV3;
            }
        SOFEV8:
            a = e;//KUMA: counter = SR
        SOFEV3:
            work.pg.softEnvelopeCounter = a;//KUMA:counter=counter-DR(毎クロック,DR分だけcounterが減る)
            if ((a - e) != 0)
            {
                SOFEV7();//KUMA: counterがSRに到達していないならSOFEV7へ
                return;
            }
            a = (byte)work.pg.volume;//KUMA:current volume & flagsを取得
            a ^= 0b0110_0000;//KUMA:dcay flag:off  sustain flag:on
            work.pg.volume = a;// TO STATE 3 (SUSTAIN) //KUMA:current volume & flagsを更新
            SOFEV7();
            return;
        SOFEV4:
            if ((work.pg.volume & 0x40) == 0)//KUMA: Check sustain flag
            {
                SOFEV9();//KUMA:release 処理へ
                return;
            }
            a = (byte)work.pg.softEnvelopeCounter;// KUMA:get counter
            d = (byte)work.pg.softEnvelopeParam[4];// GET SUSTAIN LEVEL// KUMA:get SL
            carry = ((a - d) < 0);//KUMA:counter = counter - SL 結果、counterが0以上の場合はSOFEV5へ
            a -= d;
            if (!carry)
            {
                goto SOFEV5;
            }
            a = 0;//KUMA: counter=0
        SOFEV5:
            work.pg.softEnvelopeCounter = a;//KUMA:counter=counter-SL(毎クロック,SL分だけcounterが減る)
            if (a != 0)
            {
                SOFEV7();
                return;
            }
            a = (byte)work.pg.volume;//KUMA:current volume & flagsを取得
            a &= 0b1000_1111;//KUMA:エンベロープで使用した進捗に関わるフラグをリセット
            work.pg.volume = a;// END OF ENVE //KUMA:KEYON中にSLにきて更にcounterが0になったらエンベロープ処理は終了する
            SOFEV7();
        }

        public void SOFEV9()
        {
            byte a = (byte)work.pg.softEnvelopeCounter;//KUMA:get counter
            byte d = (byte)work.pg.softEnvelopeParam[5];// GET REREASE//KUMA:get RR
            bool carry = ((a - d) < 0);//KUMA:RRでcounterを減算
            a -= d;
            if (!carry)
            {
                goto SOFEVA;
            }
            a = 0;
        SOFEVA:
            work.pg.softEnvelopeCounter = a;//KUMA:counterを更新
            SOFEV7();
        }

        // **	VOLUME CALCURATE	**

        public void SOFEV7()
        {
            byte e = (byte)work.pg.softEnvelopeCounter;//KUMA:get counter
            int hl = 0;
            byte a = (byte)work.pg.volume;// GET VOLUME
            a &= 0b0000_1111;
            a++;
            byte b = a;//繰り返す回数 VOLUME+1回
        //SOFEV6:
            do
            {
                hl += e;
                b--;
            } while (b != 0);
            a = (byte)(hl >> 8);//AにはVOLUME+1を最大値としたcounter/256の割合分の値が入る
            work.A_Reg = a;
            if (work.pg.keyoffflg)
            {
                return;
            }
            if (!work.pg.reverbFlg)
            {
                return;
            }
            a += (byte)work.pg.reverbVol;//.softEnvelopeParam[5];
            
            work.carry= ((a & 0x01) != 0);
            a >>= 1;
            work.A_Reg = a;
        }

        // **   SET POINTER   **

        public void SETPT()
        {
            work.pg.dataAddressWork = work.hl;//  SET NEXT SOUND DATA ADDRES
            return;
        }

        public void SSGEND()
        {
            work.pg.musicEnd = true;
            work.pg.dataAddressWork = work.hl;
            SKYOFF();
            work.pg.lfoflg = false;// RESET LFO FLAG
        }

        // **   SSG KEY OFF**

        public void SKYOFF()
        {
            work.pg.volume = 0;// ENVE FLAG RESET
            byte e = 0;
            byte d = (byte)work.pg.volReg;
            PSGOUT(d, e);
        }

        public void SSSUB3(byte a)
        {
            if (!work.pg.hardEnveFlg)
            {
                byte e = a;
                if (work.soundWork.READY == 0)
                {
                    e = 0;
                }
                if (work.soundWork.KEY_FLAG != 0xff)
                {
                    byte d = (byte)work.pg.volReg;
                    if (work.pg.pageNo == work.cd.currentPageNo) PSGOUT(d, e);
                }
            }
            work.pg.dataAddressWork = work.hl;

            //byte e = a;//added
            //if (work.soundWork.READY == 0)//added
            //{
            //    e = 0;//added
            //}
            ////SSSUB32:
            //byte d = (byte)work.pg.volReg;
            //PSGOUT(d,e);
            //SETPT();
        }

        public void HRDENV()
        {
            work.pg.backupHardEnv = (byte)work.pg.mData[work.hl++].dat;
            work.pg.hardEnveFlg = true;
            if (work.pg.pageNo != work.cd.currentPageNo) return;

            tHRDENV();
        }
        public void restoreHRDENV()
        {
            if (!work.pg.hardEnveFlg) return;
            tHRDENV();
        }
        public void tHRDENV()
        {
            byte e = work.pg.backupHardEnv;
            byte d = 0x0d;
            PSGOUT(d, e);
            work.pg.hardEnveFlg = true;
            work.pg.hardEnvelopValue = (byte)(e & 0xf);
            work.pg.volume = 16;
        }


        public void ENVPOD()
        {
            work.pg.backupHardEnvFine = (byte)work.pg.mData[work.hl++].dat;
            work.pg.backupHardEnvCoarse = (byte)work.pg.mData[work.hl++].dat;
            if (work.pg.pageNo != work.cd.currentPageNo) return;

            tENVPOD();
        }
        public void restoreENVPOD()
        {
            tENVPOD();
        }
        public void tENVPOD()
        {

            byte e = (byte)work.pg.backupHardEnvFine;
            byte d = 0x0b;
            PSGOUT(d, e);
            e = (byte)work.pg.backupHardEnvCoarse;
            d = 0x0c;
            PSGOUT(d, e);

        }

        private void SetKeyOnDelay()
        {
            work.pg.KeyOnDelayFlag = false;
            for (int i = 0; i < 4; i++)
            {
                work.pg.KDWork[i] = work.pg.KD[i] = (byte)work.pg.mData[work.hl++].dat;
                if (work.pg.KD[i] != 0) work.pg.KeyOnDelayFlag = true;
            }

            work.pg.keyOnSlot = 0x00;
            if (!work.pg.KeyOnDelayFlag) work.pg.keyOnSlot = 0xf0;
        }

        private void KeyOnDelaying()
        {
            if (work.soundWork.DRMF1 != 0) return;
            if (work.soundWork.PCMFLG != 0) return;
            if (!work.pg.KeyOnDelayFlag) return;

            byte newf = work.pg.keyOnSlot;
            for(int i = 0; i < 4; i++)
            {
                if (work.pg.KDWork[i] == 0) continue;
                work.pg.KDWork[i]--;
                if (work.pg.KDWork[i] == 0)
                {
                    newf |= (byte)(0x10 << i);
                }
            }

            if (newf != work.pg.keyOnSlot)
            {
                //Key On!!
                KEYON2();
                work.pg.keyOnSlot = newf;
            }
        }

        public void KEYON2()
        {
            if (work.soundWork.READY == 0) return;

            byte a = 0x04;
            if (work.soundWork.FMPORT == 0)
            {
                a = 0x00;
            }

            if (!work.pg.KeyOnDelayFlag)
            {
                a += work.pg.keyOnSlot;
            }
            else
            {
                work.pg.keyOnSlot = 0x00;
                if (work.pg.KDWork[0] == 0) work.pg.keyOnSlot += 0x10;
                if (work.pg.KDWork[1] == 0) work.pg.keyOnSlot += 0x20;
                if (work.pg.KDWork[2] == 0) work.pg.keyOnSlot += 0x40;
                if (work.pg.KDWork[3] == 0) work.pg.keyOnSlot += 0x80;
                a += work.pg.keyOnSlot;
            }

            //KEYON2:
            a += (byte)work.pg.channelNumber;
            PSGOUT(0x28, a);//KEY-ON

            if (work.pg.reverbFlg)
            {
                STVOL();
            }
        }

    }
}
