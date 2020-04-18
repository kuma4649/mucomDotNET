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
        // **	FM CONTROL COMMAND(s)   **
        public Action[] FMCOM = null;
        public Action[] FMCOM2 = null;
        public Action[] LFOTBL = null;
        // **   PSG COMMAND TABLE**
        public Action[] PSGCOM = null;

        private Work work;
        private Action<ChipDatum> WriteOPNARegister = null;

        private byte[] autoPantable = new byte[] { 2, 3, 1, 3 };

        private void WriteOPNASimultaneousOutput(ChipDatum dat) {
            var d1 = new ChipDatum(dat.port & 0x01, dat.address, dat.data, dat.time, dat.addtionalData);
            WriteOPNARegister(d1);

            var d2 = new ChipDatum(0x02 + (dat.port & 0x01), dat.address, dat.data, dat.time, dat.addtionalData);
            WriteOPNARegister(d2);
        }

        /// <summary>
        /// トラック拡張時のフラグ
        /// </summary>
        public bool trackExtend = false;

        /// <summary>
        /// ドライバ再生時の最大チャンネル数
        /// </summary>
        public int MaxDriverChannel = 11;

        public Music2(Work work, Action<ChipDatum> WriteOPNARegister, bool extend) : this(work, WriteOPNARegister) {
            trackExtend = extend;
            MaxDriverChannel = trackExtend ? 22 : 11;
        }
        public Music2(Work work, Action<ChipDatum> WriteOPNARegister)
        {
            this.work = work;
            this.WriteOPNARegister = WriteOPNARegister;
            initMusic2();
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
                for (int i = 0; i < work.soundWork.CHDAT.Length; i++) work.soundWork.CHDAT[i].muteFlg = true;

                while (count > 0)
                {
                    PL_SND();
                    count--;
                }

                for (int i = 0; i < work.soundWork.CHDAT.Length; i++) work.soundWork.CHDAT[i].muteFlg = false;
                //work.SystemInterrupt = false;
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
                ,SetKeyOnDelay // 0xFF 0xF6
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


        private void AKYOFF()
        {
            for (int e = 0; e < 7; e++)
            {
                ChipDatum dat = new ChipDatum(0, 0x28, (byte)e);
                WriteOPNASimultaneousOutput(dat);
            }
        }

        // **	SSG ALL SOUND OFF	**

        private void SSGOFF()
        {
            for (int b = 0; b < 3; b++)
            {
                ChipDatum dat = new ChipDatum(0, (byte)(0x8 + b), 0x0);
                WriteOPNASimultaneousOutput(dat);
            }
        }

        // **   VOLUME OR FADEOUT etc RESET**

        public void WORKINIT() {
            work.soundWork.C2NUM = 0;
            work.soundWork.CHNUM = 0;
            work.soundWork.PVMODE = 0;

            work.soundWork.KEY_FLAG = 0;
            work.soundWork.RANDUM = (ushort)System.DateTime.Now.Ticks;

            int num = work.soundWork.MUSNUM;
            work.mDataAdr = work.soundWork.MU_TOP;


            for (int i = 0; i < num; i++) {
                work.mDataAdr += 1 + (uint)MaxDriverChannel * 4;
                work.mDataAdr = work.soundWork.MU_TOP + Cmn.getLE16(work.mData, (uint)work.mDataAdr);
            }

            work.soundWork.TIMER_B = (work.mData[work.mDataAdr] != null) ? ((byte)work.mData[work.mDataAdr].dat) : (byte)200;
            work.soundWork.TB_TOP = ++work.mDataAdr;

            InitWork(0);

            if (trackExtend) {
                work.soundWork.C2NUM = 0;
                work.soundWork.CHNUM = 0;
                InitWork(11);
            }

            work.fmVoiceAtMusData = GetVoiceDataAtMusData();

            work.mData = null;
        }

        private int InitWork(int ofs) {
            int ch = 0;
            for (ch = 0; ch < 6; ch++) {
                FMINIT(ofs + ch);
                //ch++;//オリジナルは　ix+=WKLENG だが、配列化しているので。
            }

            work.soundWork.CHNUM = 0;
            ch = 6;//DRAMDAT
            FMINIT(ofs + ch);

            work.soundWork.CHNUM = 0;
            //ix = 7;//CHADAT
            for (ch = 7; ch < 7 + 4; ch++) {
                FMINIT(ofs + ch);
                //オリジナルは　ix+=WKLENG だが、配列化しているので。
            }

            return ch;
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

        private void FMINIT(int ch)
        {
            work.soundWork.CHDAT[ch] = new CHDAT();
            work.soundWork.CHDAT[ch].lengthCounter = 1;
            work.soundWork.CHDAT[ch].volume = 0;
            work.soundWork.CHDAT[ch].musicEnd = false;

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
            work.soundWork.CHDAT[ch].mData = bf.ToArray();

            if (nCPtr < stPtr)
            {
                work.weight += 0x1_0000;
            }

            //work.soundWork.CHDAT[ch].dataAddressWork
            //    = (uint)(work.soundWork.MU_TOP + stPtr + work.weight);//ix 2,3
            //work.soundWork.CHDAT[ch].dataTopAddress
            //    = (uint)(lpPtr != 0 ? (work.soundWork.MU_TOP + lpPtr + work.weight) : 0);//ix 4,5
            work.soundWork.CHDAT[ch].dataAddressWork
                = (uint)0;//ix 2,3
            work.soundWork.CHDAT[ch].dataTopAddress
                = (int)(lpPtr != -1 ? (lpPtr - stPtr) : -1);//ix 4,5


            work.soundWork.C2NUM++;
            work.soundWork.TB_TOP += 4;
            if (work.soundWork.CHNUM > 2)
            {
                // ---   FOR SSG   ---
                work.soundWork.CHDAT[ch].volReg = work.soundWork.CHNUM + 5;//ix 7
                work.soundWork.CHDAT[ch].channelNumber = (work.soundWork.CHNUM - 3) * 2;//ix 8
                work.soundWork.CHNUM++;
                return;
            }
            work.soundWork.CHDAT[ch].channelNumber = work.soundWork.CHNUM;//ix 8
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

            ChipDatum dat = new ChipDatum(0, 0x29, 0x83);// CH 4-6 ENABLE
            WriteOPNASimultaneousOutput(dat);

            for (int b = 0; b < 6; b++)
            {
                dat = new ChipDatum(0, (byte)b, 0x00);// CH 4-6 ENABLE
                WriteOPNASimultaneousOutput(dat);
            }

            dat = new ChipDatum(0, 7, 0b0011_1000);
            WriteOPNASimultaneousOutput(dat);

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
                WriteOPNASimultaneousOutput(dat);
            }

            for (int b = 0; b < 6; b++)
            {
                dat = new ChipDatum(0, (byte)(0x18 + b), 0xc0);//rhythm
                WriteOPNASimultaneousOutput(dat);
            }

            work.soundWork.FMPORT = 4;
            for (int b = 0; b < 3; b++)
            {
                dat = new ChipDatum(1, (byte)(0xb4 + b), 0xc0);//fm 4-6
                WriteOPNASimultaneousOutput(dat);
            }

            work.soundWork.FMPORT = 0;
            dat = new ChipDatum(0, 0x22, 0x00);//lfo freq control
            WriteOPNASimultaneousOutput(dat);
            dat = new ChipDatum(0, 0x12, 0x00);//rhythm test data
            WriteOPNASimultaneousOutput(dat);


            for (int b = 0; b < 7; b++)
            {
                work.soundWork.PALDAT[b] = 0xc0;
            }

            work.soundWork.PCMLR = 3;
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
            WriteOPNARegister(dat);

            dat = new ChipDatum(0, 0x27, 0x78);
            WriteOPNARegister(dat);//  Timer-B OFF

            dat = new ChipDatum(0, 0x27, 0x7a);
            WriteOPNARegister(dat);//  Timer-B ON

            //割り込みレベルリセット不要
            //Z80.A = 5;
            //PC88.OUT(0xe4, Z80.A);
        }


        // **	MUSIC MAIN	**

        public void PL_SND()
        {

            ChipDatum dat = new ChipDatum(0, 0x27, work.soundWork.PLSET1_VAL);//  TIMER-OFF DATA
            WriteOPNASimultaneousOutput(dat);
            dat = new ChipDatum(0, 0x27, work.soundWork.PLSET2_VAL);//  TIMER-ON DATA
            WriteOPNARegister(dat);

            DRIVE();
            //FDOUT();

            int n = 0;
            for(int i = 0; i < MaxDriverChannel; i++)
            {
                if (work.soundWork.CHDAT[i].musicEnd) n++;
            }
            if (n == MaxDriverChannel) work.Status = 0;
        }

        // **	CALL FM		**

        //private long loopC = 0;

        public void DRIVE() {
            int n = 0;

            work.soundWork.PORTOFS = 0;
            n = DriveOffset(0, n);

            if (trackExtend) {
                work.soundWork.PORTOFS = 2;
                n = DriveOffset(11, n);
            }

            if (work.maxLoopCount == -1) n = 0;
            if (n == MaxDriverChannel)
                MSTOP();
        }

        private int DriveOffset(int ofs, int n) {

            work.soundWork.FMPORT = 0;

            //Log.WriteLine(LogLevel.TRACE, string.Format("----- -----{0}", loopC++));
            Log.WriteLine(LogLevel.TRACE, "----- FM 1");
            FMENT(ofs + 0);
            if ((work.soundWork.CHDAT[ofs + 0].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 0].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 0].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- FM 2");
            FMENT(ofs + 1);
            if ((work.soundWork.CHDAT[ofs + 1].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 1].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 1].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- FM 3");
            FMENT(ofs + 2);
            if ((work.soundWork.CHDAT[ofs + 2].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 2].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 2].loopCounter >= work.maxLoopCount)
                n++;


            work.soundWork.SSGF1 = 0xff;
            Log.WriteLine(LogLevel.TRACE, "----- SSG1");
            SSGENT(ofs + 3);
            if ((work.soundWork.CHDAT[ofs + 3].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 3].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 3].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- SSG2");
            SSGENT(ofs + 4);
            if ((work.soundWork.CHDAT[ofs + 4].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 4].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 4].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- SSG3");
            SSGENT(ofs + 5);
            if ((work.soundWork.CHDAT[ofs + 5].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 5].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 5].loopCounter >= work.maxLoopCount)
                n++;

            work.soundWork.SSGF1 = 0;


            if (notSoundBoard2) return n;

            work.soundWork.DRMF1 = 1;
            Log.WriteLine(LogLevel.TRACE, "----- Ryhthm");
            FMENT(ofs + 6);
            if ((work.soundWork.CHDAT[ofs + 6].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 6].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 6].loopCounter >= work.maxLoopCount)
                n++;

            work.soundWork.DRMF1 = 0;

            work.soundWork.FMPORT = 4;
            Log.WriteLine(LogLevel.TRACE, "----- FM 4");
            FMENT(ofs + 7);
            if ((work.soundWork.CHDAT[ofs + 7].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 7].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 7].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- FM 5");
            FMENT(ofs + 8);
            if ((work.soundWork.CHDAT[ofs + 8].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 8].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 8].loopCounter >= work.maxLoopCount)
                n++;

            Log.WriteLine(LogLevel.TRACE, "----- FM 6");
            FMENT(ofs + 9);
            if ((work.soundWork.CHDAT[ofs + 9].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 9].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 9].loopCounter >= work.maxLoopCount)
                n++;


            work.soundWork.PCMFLG = 0xff;
            Log.WriteLine(LogLevel.TRACE, "----- ADPCM");
            FMENT(ofs + 10);
            if ((work.soundWork.CHDAT[ofs + 10].dataTopAddress == -1 && work.soundWork.CHDAT[ofs + 10].loopEndFlg)
                || work.soundWork.CHDAT[ofs + 10].loopCounter >= work.maxLoopCount)
                n++;

            work.soundWork.PCMFLG = 0;
            return n;
        }

        public void FMENT(int ix)
        {
            work.idx = ix;
            work.cd = work.soundWork.CHDAT[work.idx];

            PANNING();//AMD98
            KeyOnDelaying();

            if (work.soundWork.CHDAT[ix].muteFlg) //KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0x00;
            }
            FMSUB();
            PLLFO();
            if (work.soundWork.CHDAT[ix].muteFlg)//KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0xff;
            }
        }


        public void SSGENT(int ix)
        {
            if (work.soundWork.CHDAT[ix].muteFlg) //KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0x00;
            }
            work.idx = ix;
            SSGSUB();
            PLLFO();
            if (work.soundWork.CHDAT[ix].muteFlg)//KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0xff;
            }
        }


        //**	FM ｵﾝｹﾞﾝ ﾆ ﾀｲｽﾙ ｴﾝｿｳ ﾙｰﾁﾝ	**

        public void FMSUB()
        {
            //work.carry = false;
            work.cd.lengthCounter--;
            work.cd.lengthCounter = (byte)work.cd.lengthCounter;
            if (work.cd.lengthCounter == 0)
            {
                FMSUB1();
                return;
            }

            if ((byte)work.cd.lengthCounter > (byte)work.cd.quantize)
            {
                //if(!work.carry)
                return;
            }

            //FMSUB0

            if (work.cd.mData[work.cd.dataAddressWork].dat == 0xfd) return;// COUNT OVER ?

            //    BIT	5,(IX+33)
            if (work.cd.reverbFlg)//KUMA: 0x20(0b0010_0000)(bit5) = REVERVE FLAG  
            {
                FS2();
                return;
            }
            KEYOFF();
        }

        public void FS2()
        {
            STV2((byte)((byte)(work.cd.volume + work.cd.reverbVol) >> 1));
            work.cd.keyoffflg = true;
        }

        public void STV2(byte c)
        {
            byte e = work.soundWork.FMVDAT[c];// GET VOLUME DATA
            byte d = (byte)(0x40 + work.cd.channelNumber);// GET PORT No.

            if (work.cd.algo >= 8) return;//KUMA: オリジナルはチェック無し

            c = work.soundWork.CRYDAT[work.cd.algo];

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

            ChipDatum dat = new ChipDatum(work.soundWork.PORTOFS + port, d, e, 0, work.crntMmlDatum);
            WriteOPNARegister(dat);
        }

        public void DummyOUT()
        {
            ChipDatum dat = new ChipDatum(-1,-1,-1, 0, work.crntMmlDatum);
            WriteOPNARegister(dat);
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
                // --	ﾘｽﾞﾑ ｵﾝｹﾞﾝ ﾉ ｷｰｵﾌ	--
                PSGOUT(0x10, (byte)((work.soundWork.RHYTHM & 0b0011_1111) | 0x80));// GET RETHM PARAMETER
                return;
            }

            work.cd.KDWork[0] = 0;
            work.cd.KDWork[1] = 0;
            work.cd.KDWork[2] = 0;
            work.cd.KDWork[3] = 0;
            PSGOUT(0x28, (byte)(work.soundWork.FMPORT + work.cd.channelNumber));//  KEY-OFF

        }

        public void PCMEND()
        {
            PCMOUT(0x0b, 0x00);
            PCMOUT(0x01, 0x00);
            PCMOUT(0x00, 0x21);
        }

        // ***	ADPCM OUT	***

        public void PCMOUT(byte d, byte e)
        {
            ChipDatum dat = new ChipDatum(work.soundWork.PORTOFS + 1, d, e, 0, work.crntMmlDatum);
            WriteOPNARegister(dat);
        }

        // **	SET NEW SOUND**

        public void FMSUB1()
        {
            work.cd.keyoffflg = true;
            if (work.cd.mData[work.cd.dataAddressWork].dat != 0x0FD)// COUNT OVER?
            {
                FMSUBC(work.cd.dataAddressWork);
                return;
            }

            work.cd.keyoffflg = false;            // RES KEYOFF FLAG
            FMSUBC(work.cd.dataAddressWork + 1);
        }

        public void FMSUBC(uint hl)
        {

            byte a;
            bool nrFlg = false;
            do
            {
                Log.WriteLine(LogLevel.TRACE, string.Format("{0:x}", hl + 0xc200));
                a = (byte)work.cd.mData[hl].dat;
                //* 00H as end
                while (a == 0)// ﾃﾞｰﾀ ｼｭｳﾘｮｳ ｦ ｼﾗﾍﾞﾙ
                {
                    work.cd.loopEndFlg = true;

                    if (work.cd.dataTopAddress == -1 || nrFlg)
                    {
                        FMEND(hl);//* DATA TOP ADRESS ｶﾞ 0000H ﾃﾞ BGM
                        return; // ﾉ ｼｭｳﾘｮｳ ｦ ｹｯﾃｲ ｿﾚ ｲｶﾞｲﾊ ｸﾘｶｴｼ
                    }
                    hl = (uint)work.cd.dataTopAddress;
                    a = (byte)work.cd.mData[hl].dat;// GET FLAG & LENGTH
                    work.cd.loopCounter++;
                    nrFlg = true;
                }

                //演奏情報退避
                work.crntMmlDatum = work.cd.mData[hl];

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
            work.cd.lengthCounter = a & 0x7f;// SET WAIT COUNTER


            if ((a & 0x80) != 0) //BIT7(ｷｭｳﾌ ﾌﾗｸﾞ)
            {
                work.crntMmlDatum = work.cd.mData[hl - 1];
                // **	SET F-NUMBER**
                work.cd.dataAddressWork = hl;// SET NEXT SOUND DATA ADD

                if (work.cd.reverbMode)
                {
                    KEYOFF();
                    return;
                }
                if (work.cd.reverbFlg)
                {
                    FS2();
                    return;
                }
                KEYOFF();
                return;
            }

            // ｵﾝﾌﾟ ﾅﾗ FMSUB5 ﾍ
            if (work.cd.keyoffflg)
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

            if (work.cd.channelNumber == 2)//CH=3?
            {
                EXMODE(hl);
                return;
            }

            FMSUB4(hl);
        }

        // **	ｴﾝｿｳ ｵﾜﾘ	**

        public void FMEND(uint hl)
        {
            work.cd.musicEnd = true;
            work.cd.dataAddressWork = hl;

            if (work.soundWork.PCMFLG != 0)
            {
                PCMEND();
                return;
            }
            KEYOFF();
        }

        public void FMSUB4(uint hl)
        {
            byte a, b;
            work.carry = false;

            a = (byte)work.cd.mData[hl].dat;// A=BLOCK(OCTAVE-1 ) & KEY CODE DATA
            work.cd.dataAddressWork = hl + 1;// SET NEXT SOUND DATA ADD
            if  (!work.cd.keyoffflg // CHECK KEYOFF FLAG
                && work.cd.beforeCode == a)// GET BEFORE CODE DATA
            {
                work.carry = true;
                return;
            }

            work.cd.beforeCode = a;

            if (work.soundWork.PCMFLG != 0)
            {
                //PCMGFQ:
                hl = (uint)(work.soundWork.PCMNMB[a & 0b0000_1111] + work.cd.detune);
                a >>= 4;
                b = a;
                //ASUB7:
                while (b != 0)
                {
                    hl >>= 1;
                    b--;
                }
                //ASUB72:
                work.soundWork.DELT_N = hl;
                if (!work.cd.keyoffflg)
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
                hl = work.soundWork.FNUMB[a & 0xf];// GET KEY CODE(C, C+, D...B)
                hl |= (ushort)((a & 0x70) << 7);// GET BLOCK DATA
                                                // A4-A6 ﾎﾟｰﾄ ｼｭﾂﾘｮｸﾖｳ ﾆ ｱﾜｾﾙ
                                                // GET FNUM2
                                                // A= KEY CODE & FNUM HI

                hl = (uint)(hl + work.cd.detune);// GET DETUNE DATA
                                                 // DETUNE PLUS

                if (!work.cd.tlLfoflg)
                {
                    work.cd.fnum = (int)hl;// FOR LFO
                                           // FOR LFO
                    work.soundWork.FNUM = hl;
                }
                if (work.cd.keyoffflg)
                {
                    LFORST();
                }
                LFORST2();
                //FMSUB8:
                FMSUB6(hl, work.soundWork.FMSUB8_VAL);//戻り値がcarry
                return;
            }

            //DRMFQ:
            if (!work.cd.keyoffflg)
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
            d += (byte)work.cd.channelNumber;
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
            work.cd.lfoDelayWork = work.cd.lfoDelay;// LFO DELAY ﾉ ｻｲｾｯﾃｲ
            work.cd.lfoContFlg = false;            // RESET LFO CONTINE FLAG
        }

        public void LFORST2()
        {
            work.cd.lfoPeakWork = work.cd.lfoPeak >> 1;// LFO PEAK LEVEL ｻｲ ｾｯﾃｲ
            work.cd.lfoDeltaWork = work.cd.lfoDelta;// ﾍﾝｶﾘｮｳ ｻｲｾｯﾃｲ
            if (!work.cd.tlLfoflg)
            {
                return;
            }
            work.cd.fnum = work.cd.TLlfo;
            work.cd.bfnum2 = 0;
        }

        // ***	ADPCM PLAY	***

        // IN:(STTADR)<=ｻｲｾｲ ｽﾀｰﾄ ｱﾄﾞﾚｽ
        //	   (ENDADR)  <=ｻｲｾｲ ｴﾝﾄﾞ ｱﾄﾞﾚｽ
        //	   (DELT_N)<=ｻｲｾｲ ﾚｰﾄ

        public void PLAY()
        {
            if (work.soundWork.READY == 0) return;

            PCMOUT(0x0b, 0x00);
            PCMOUT(0x01, 0x00);
            PCMOUT(0x00, 0x21);
            PCMOUT(0x10, 0x08);
            PCMOUT(0x10, 0x80);// INIT
            PCMOUT(0x02, (byte)work.soundWork.STTADR);// START ADR
            PCMOUT(0x03, (byte)(work.soundWork.STTADR >> 8));
            PCMOUT(0x04, (byte)work.soundWork.ENDADR);// END ADR
            PCMOUT(0x05, (byte)(work.soundWork.ENDADR >> 8));
            PCMOUT(0x09, (byte)work.soundWork.DELT_N);// ｻｲｾｲ ﾚｰﾄ ｶｲ
            PCMOUT(0x0a, (byte)(work.soundWork.DELT_N >> 8));// ｻｲｾｲ ﾚｰﾄ ｼﾞｮｳｲ
            PCMOUT(0x00, 0xa0);

            byte e = (byte)(work.soundWork.TOTALV * 4 + work.cd.volume);
            if (e >= 250)
            {
                e = 0;
            }
            //PL1:
            if (work.soundWork.PVMODE != 0)
            {
                e += (byte)work.cd.volReg;
            }
            //PL2:
            PCMOUT(0xb, e);// VOLUME

            e = (byte)((work.soundWork.PCMLR & 3) << 6);
            PCMOUT(0x01, e);// 1 bit TYPE, L&R OUT

            // ｼﾝｺﾞｳﾀﾞｽ
            work.soundWork.P_OUT = work.soundWork.PCMNUM;
        }

        // **   ﾘｽﾞﾑ ｵﾝｹﾞﾝ ﾉ ｷｰｵﾝ   **

        public void DKEYON()
        {
            if (work.soundWork.READY == 0) return;
            PSGOUT(0x10, (byte)(work.soundWork.RHYTHM & 0b0011_1111));// KEY ON
        }

        // **	KEY-ON ROUTINE   **

        public void KEYON()
        {
            if (work.soundWork.READY == 0) return;

            byte a = 0x04;
            if (work.soundWork.FMPORT == 0)
            {
                a = 0x00;
            }

            if (!work.cd.KeyOnDelayFlag)
            {
                a += work.cd.keyOnSlot;
            }
            else
            {
                work.cd.keyOnSlot = 0x00;
                if (work.cd.KD[0] == 0) work.cd.keyOnSlot += 0x10;
                if (work.cd.KD[1] == 0) work.cd.keyOnSlot += 0x20;
                if (work.cd.KD[2] == 0) work.cd.keyOnSlot += 0x40;
                if (work.cd.KD[3] == 0) work.cd.keyOnSlot += 0x80;
                a += work.cd.keyOnSlot;

                work.cd.KDWork[0] = work.cd.KD[0];
                work.cd.KDWork[1] = work.cd.KD[1];
                work.cd.KDWork[2] = work.cd.KD[2];
                work.cd.KDWork[3] = work.cd.KD[3];
            }

            //KEYON2:
            a += (byte)work.cd.channelNumber;
            PSGOUT(0x28, a);//KEY-ON

            if (work.cd.reverbFlg)
            {
                STVOL();
            }
        }

        // **	ﾎﾞﾘｭｰﾑ**

        public void STVOL()
        {
            //STV1
            byte c = (byte)(work.soundWork.TOTALV + work.cd.volume);// INPUT VOLUME
            if (c >= 20)
            {
                c = 0;
            }
            //STV12:
            STV2(c);
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

            work.cd.instrumentNumber = work.cd.mData[work.hl++].dat;

            STENV();
            STVOL();
        }

        public void OTODRM()
        {
            DummyOUT();
            work.soundWork.RHYTHM = work.cd.mData[work.hl++].dat;// SET RETHM PARA
        }

        public void OTOPCM()
        {
            DummyOUT();
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.soundWork.PCMNUM = a;
            a--;
            work.cd.instrumentNumber = a;

            if (work.pcmTables != null && work.pcmTables.Length > a)
            {
                work.soundWork.STTADR = work.pcmTables[a].Item2[0];//start address
                work.soundWork.ENDADR = work.pcmTables[a].Item2[1];//end address
            }

            if (work.soundWork.PVMODE == 0) return;

            work.cd.volume = (byte)work.pcmTables[a].Item2[3];
        }

        // **	ｵﾝｼｮｸ ｾｯﾄ ｻﾌﾞﾙｰﾁﾝ(FM)  **

        public void STENV()
        {
            KEYOFF();
            byte a = (byte)(0x80 + work.cd.channelNumber);
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
            int hl = work.cd.instrumentNumber * 25;// HL=*25
            //hl += work.mData[work.soundWork.OTODAT].dat + work.mData[work.soundWork.OTODAT + 1].dat * 0x100 + 1;// HL ﾊ ｵﾝｼｮｸﾃﾞｰﾀ ｶｸﾉｳ ｱﾄﾞﾚｽ
            //hl += work.soundWork.MUSNUM;
            hl++;//音色数を格納している為いっこずらす

            //STENV1:
            byte d = 0x30;// START=PORT 30H
            d += (byte)work.cd.channelNumber;// PLUS CHANNEL No.
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
            work.cd.algo = e & 0x07;// STORE ALGORIZM
            // GET ALGO SET ADDRES
            d = (byte)(0xb0 + work.cd.channelNumber);// CH PLUS
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

            work.cd.volume = work.cd.mData[work.hl++].dat;
            STVOL();
        }

        public void PCMVOL()
        {
            byte e = (byte)work.cd.mData[work.hl++].dat;
            if (work.soundWork.PVMODE != 0)
            {
                work.cd.volReg = e;
                return;
            }
            work.cd.volume = e;
        }

        public void VOLDRM()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.cd.volume = a;
            DVOLSET();
            //VOLDR1:
            byte b = 6;
            int de = 0;// work.soundWork.DRMVOL;
                       //VOLDR2:
            do
            {
                a = (byte)(work.soundWork.DRMVOL[de] & 0b1100_0000);
                a |= (byte)work.cd.mData[work.hl++].dat;
                work.soundWork.DRMVOL[de++] = a;
                PSGOUT((byte)(0x18 - b + 6), a);
                b--;
            } while (b != 0);
        }

        // --   SET TOTAL RHYTHM VOL	--

        public void DVOLSET()
        {
            byte d = 0x11;
            byte a = (byte)work.cd.volume;
            a &= 0b0011_1111;
            a = (byte)(work.soundWork.TOTALV * 5 + a);
            if (a >= 64)
            {
                a = 0;
            }
            //DV2:
            PSGOUT(d, a);
        }

        // **	ﾃﾞﾁｭｰﾝ ｾｯﾄ	**

        public void FRQ_DF()
        {
            DummyOUT();
            work.cd.beforeCode = 0;// DETUNE ﾉ ﾊﾞｱｲﾊ BEFORE CODE ｦ CLEAR
            int de = work.cd.mData[work.hl].dat + work.cd.mData[work.hl + 1].dat * 0x100;
            work.hl += 2;
            byte a = (byte)work.cd.mData[work.hl++].dat;
            if (a != 0)
            {
                de += work.cd.detune;
            }
            //FD2:
            work.cd.detune = de;
            if (work.soundWork.PCMFLG == 0)
            {
                return;
            }

            ushort hl = (ushort)work.soundWork.DELT_N;
            hl += (ushort)de;
            PCMOUT(0x09, (byte)hl);
            PCMOUT(0x0a, (byte)(hl >> 8));
        }

        // **	SET Q COMMAND**

        public void SETQ()
        {
            work.cd.quantize = work.cd.mData[work.hl++].dat;
        }

        // **	SOFT LFO SET(RESET) **

        public void LFOON()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;// GET SUB COMMAND
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
            work.cd.lfoflg = true;// SET LFO FLAG
        }

        public void SETDEL()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.cd.lfoDelay = a;
            work.cd.lfoDelayWork = a;
        }

        public void SETCO()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.cd.lfoCounter = a;
            work.cd.lfoCounterWork = a;
        }

        public void SETVCT()
        {
            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = (byte)work.cd.mData[work.hl++].dat;

            work.cd.lfoDelta = e + d * 0x100;
            work.cd.lfoDeltaWork = e + d * 0x100;
        }

        public void SETPEK()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;

            work.cd.lfoPeak = a;//SET PEAK LEVEL
            a >>= 1;
            work.cd.lfoPeakWork = a;
        }

        public void LFOOFF()
        {
            work.cd.lfoflg = false;// RESET LFO
        }

        public void LFOON2()
        {
            work.cd.lfoflg = true;// LFOON
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

            byte a = (byte)work.cd.mData[work.hl++].dat;
            if (a == 0)
            {
                work.cd.tlLfoflg = false;
                return;
            }

        //TLL2:
            work.cd.TLlfoSlot = a;
            work.cd.tlLfoflg = true;
            a = (byte)work.cd.mData[work.hl++].dat;
            work.cd.fnum = a;
            work.cd.bfnum2 = 0;
            work.cd.TLlfo = a;
        }

        public void SSGTremolo()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            if (a == 0)
            {
                work.cd.SSGTremoloFlg = false;
                work.cd.SSGTremoloVol = 0;
                return;
            }

            work.cd.SSGTremoloFlg = true;
            work.cd.SSGTremoloVol = 0;
        }

        // **	ﾘﾋﾟｰﾄ ｽﾀｰﾄ ｾｯﾄ**

        public void REPSTF()
        {
            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = (byte)work.cd.mData[work.hl++].dat;//DE as REWRITE ADR OFFSET +1

            int hl = (int)work.hl;
            hl -= 2;
            hl += e + d * 0x100;
            byte a = (byte)work.cd.mData[hl--].dat;
            work.cd.mData[hl].dat = a;
        }

        // **	ﾘﾋﾟｰﾄ ｴﾝﾄﾞ ｾｯﾄ(FM) **

        public void REPENF()
        {
            byte a = (byte)(work.cd.mData[work.hl].dat - (byte)1);// DEC REPEAT Co.
            work.cd.mData[work.hl].dat--;

            if (a == 0)
            {
                //REPENF2();
                work.cd.mData[work.hl].dat = work.cd.mData[work.hl + 1].dat;
                work.hl += 4;
                return;
            }

            work.hl += 2;

            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = (byte)work.cd.mData[work.hl--].dat;

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
                    byte l = (byte)work.cd.mData[work.hl++].dat;
                    byte m = (byte)work.cd.mData[work.hl++].dat;
                    work.soundWork.DETDAT[bc] = (ushort)(l + m * 0x100);
                }
            }
            else
            {
                for (int bc = 0; bc < 4; bc++)
                {
                    byte a = (byte)work.cd.mData[work.hl++].dat;
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
                work.soundWork.PCMLR = work.cd.mData[work.hl++].dat;
                return;
            }
            //STER2:
            byte a = (byte)work.cd.mData[work.hl++].dat;
            byte c = (byte)(((a >> 2) & 0x3f) | (a << 6));
            byte d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber] = d;
            a = (byte)(0x0B4 + work.cd.channelNumber);
            PSGOUT(a, d);
            return;
        STE2:
            byte dat = (byte)work.cd.mData[work.hl++].dat;
            c = dat;
            dat &= 0b0000_1111;
            a = work.soundWork.DRMVOL[dat];
            a = (byte)(((c << 2) & 0b1100_0000) | (a & 0b0001_1111));
            work.soundWork.DRMVOL[dat] = a;
            PSGOUT((byte)(dat + 0x18), a);
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

            a = (byte)work.cd.mData[work.hl++].dat;
            if (a >= 4)
            {
                goto STE012;
            }

            DummyOUT();

            //既存処理
            c = (byte)(((a >> 2) & 0x3f) | (a << 6));//右ローテート2回(左6回のほうがC#的にはシンプル)
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber] = d;
            a = (byte)(0x0B4 + work.cd.channelNumber);
            PSGOUT(a, d);
            work.cd.panEnable = 0;//パーン禁止
            return;

        STE012:
            work.cd.panEnable |= 1;//パーン許可
            work.cd.panMode = a;
            work.cd.panCounterWork = (byte)work.cd.mData[work.hl].dat;
            work.cd.panCounter = (byte)work.cd.mData[work.hl].dat;
            work.hl++;
            switch (a)
            {
                case 4:
                    work.cd.panValue = a = 2; //LEFT index
                    break;
                case 5:
                    work.cd.panValue = a = 0; //RIGHT index
                    break;
                default:
                    work.cd.panValue = a = 1; //CENTER index
                    break;
            }

            a = (byte)autoPantable[a];

            c = (byte)(a << 6);
            d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber];
            d = (byte)((d & 0b0011_1111) | c);
            work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber] = d;
            a = (byte)(0x0B4 + work.cd.channelNumber);
            PSGOUT(a, d);

        }

        public void STEREO_AMD98_RHYTHM()
        {
            DummyOUT();

            // bit0～3 rythmType RTHCSB
            // bit4～7 パン(1:右, 2:左, 3:中央 4:右オート 5:左オート 6:ランダム)を指定する。
            byte a = (byte)(work.cd.mData[work.hl].dat >> 4);
            byte b = (byte)(work.cd.mData[work.hl].dat & 0xf);
            work.hl++;
            byte c;
            if (b >= 6) return;

            if (a < 4)
            {
                //既存処理
                c = work.soundWork.DRMVOL[b];
                a = (byte)(((a << 6) & 0b1100_0000) | (c & 0b0001_1111));
                work.soundWork.DRMVOL[b] = a;
                PSGOUT((byte)(b + 0x18), a);
                work.soundWork.DrmPanEnable[b] = 0;//パーン禁止
                return;
            }

            work.soundWork.DrmPanEnable[b] |= 1;//パーン許可
            work.soundWork.DrmPanMode[b] = a;
            work.soundWork.DrmPanCounterWork[b] = (byte)work.cd.mData[work.hl].dat;
            work.soundWork.DrmPanCounter[b] = (byte)work.cd.mData[work.hl].dat;
            work.hl++;

            switch (a)
            {
                case 4:
                    work.soundWork.DrmPanValue[b] = a = 2;
                    break;
                case 5:
                    work.soundWork.DrmPanValue[b] = a = 0;
                    break;
                default:
                    work.soundWork.DrmPanValue[b] = a = 1;
                    break;
            }

            a = (byte)autoPantable[a];
            c = work.soundWork.DRMVOL[b];
            a = (byte)(((a << 6) & 0b1100_0000) | (c & 0b0001_1111));
            work.soundWork.DRMVOL[b] = a;
            PSGOUT((byte)(b + 0x18), a);
        }

        public void STEREO_AMD98_ADPCM()
        {
            DummyOUT();

            byte a = (byte)work.cd.mData[work.hl++].dat;

            if (a < 4)
            {
                //既存処理
                work.soundWork.PCMLR = a;
                work.cd.panEnable = 0;//パーン禁止
                return;
            }

            work.cd.panEnable |= 1;//パーン許可
            work.cd.panMode = a;
            work.cd.panCounterWork = (byte)work.cd.mData[work.hl].dat;
            work.cd.panCounter = (byte)work.cd.mData[work.hl].dat;
            work.hl++;

            switch (a)
            {
                case 4:
                    work.cd.panValue = a = 2;
                    break;
                case 5:
                    work.cd.panValue = a = 0;
                    break;
                default:
                    work.cd.panValue = a = 1;
                    break;
            }

            a = (byte)autoPantable[a];
            work.soundWork.PCMLR = a;
            PCMOUT(0x01, (byte)(a << 6));
        }

        public void PANNING()
        {
            if (work.soundWork.DRMF1 != 0)
            {
                PANNING_RHYTHM();
                return;
            }

            if ((work.cd.panEnable & 1) == 0) return;
            if ((--work.cd.panCounterWork) != 0) return;

            work.cd.panCounterWork = work.cd.panCounter;//; カウンター再設定

            if (work.cd.panMode == 4 || work.cd.panMode == 5)
            {
                //LEFT / RIGHT
                byte ah = work.cd.panValue;
                ah++;
                if (ah == autoPantable.Length)
                {
                    ah = 0;
                }
                work.cd.panValue = ah;//ah : 0～
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
                work.cd.panValue = (byte)(ax >> 8);//ah : 1～3
            }

            byte a, c, d;
            a = (work.cd.panMode == 4 || work.cd.panMode == 5) ? autoPantable[work.cd.panValue] : work.cd.panValue;

            List<object> args = new List<object>();
            args.Add((int)a);

            LinePos lp = new LinePos("", -1, -1, -1
                , work.soundWork.SSGF1 == 0 ? "FM" : "ADPCM"
                , "YM2608", 0, 0, work.cd.channelNumber);
            work.crntMmlDatum = new MmlDatum(enmMMLType.Pan, args, lp, 0);
            DummyOUT();

            if (work.soundWork.PCMFLG == 0)
            {
                c = (byte)(((a >> 2) & 0x3f) | (a << 6));//右ローテート2回(左6回のほうがC#的にはシンプル)
                d = work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber];
                d = (byte)((d & 0b0011_1111) | c);
                work.soundWork.PALDAT[work.soundWork.FMPORT + work.cd.channelNumber] = d;
                a = (byte)(0x0B4 + work.cd.channelNumber);
                PSGOUT(a, d);
                return;
            }

            work.soundWork.PCMLR = a;
            c = (byte)((a << 6) & 0xc0);
            PCMOUT(0x01, c);
        }

        public void PANNING_RHYTHM()
        {
            for (int n = 0; n < 6; n++)
            {
                if ((work.soundWork.DrmPanEnable[n] & 1) == 0) continue;
                if ((--work.soundWork.DrmPanCounterWork[n]) != 0) continue;

                work.soundWork.DrmPanCounterWork[n] = work.soundWork.DrmPanCounter[n];//; カウンター再設定

                if (work.soundWork.DrmPanMode[n] == 4|| work.soundWork.DrmPanMode[n] == 5)
                {
                    //LEFT / RIGHT
                    byte ah = work.soundWork.DrmPanValue[n];
                    ah++;
                    if (ah == autoPantable.Length)
                    {
                        ah = 0;
                    }
                    work.soundWork.DrmPanValue[n] = ah;//ah : 0～
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
                    work.soundWork.DrmPanValue[n] = (byte)(ax >> 8);//ah : 1～3
                }

                byte a = (work.soundWork.DrmPanMode[n] == 4 || work.soundWork.DrmPanMode[n] == 5) 
                    ? autoPantable[work.soundWork.DrmPanValue[n]] 
                    : work.soundWork.DrmPanValue[n];

                byte c = work.soundWork.DRMVOL[n];
                a = (byte)(((work.soundWork.DrmPanValue[n] << 6) & 0b1100_0000) | (c & 0b0001_1111));
                work.soundWork.DRMVOL[n] = a;
                PSGOUT((byte)(n + 0x18), a);

            }
        }

        // **	ﾌﾗｸﾞｾｯﾄ**

        public void FLGSET()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.soundWork.FLGADR = a;
        }

        // **   WRITE REG   **

        public void W_REG()
        {
            byte d = (byte)work.cd.mData[work.hl++].dat;
            byte e = (byte)work.cd.mData[work.hl++].dat;
            PSGOUT(d, e);
        }

        // **	VOLUME UP & DOWN**

        public void VOLUPF()
        {
            work.cd.volume = work.cd.mData[work.hl++].dat + work.cd.volume;

            if (work.soundWork.PCMFLG != 0)
            {
                return;
            }

            if (work.soundWork.DRMF1 != 0)
            {
                DVOLSET();
                return;
            }

            STVOL();
        }

        // **	HARD LFO SET**

        public void HLFOON()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;// FREQ CONT
            a |= 0b0000_1000;
            PSGOUT(0x22, a);

            byte c = (byte)work.cd.mData[work.hl++].dat;// PMS
            c = (byte)(c | (work.cd.mData[work.hl++].dat << 4));// AMS+PMS
            int de = work.soundWork.FMPORT + work.cd.channelNumber; // PALDAT;
            a = (byte)((work.soundWork.PALDAT[de] & 0b1100_0000) | c);
            work.soundWork.PALDAT[de] = a;
            PSGOUT((byte)(0xb4 + work.cd.channelNumber), a);
        }

        public void TIE()
        {
            work.cd.keyoffflg = false;
        }

        // **	ﾘﾋﾟｰﾄ ｽｷｯﾌﾟ	**

        public void RSKIP()
        {
            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = (byte)work.cd.mData[work.hl++].dat;

            uint hl = work.hl;
            hl -= 2;
            hl += (uint)(e + d * 0x100);

            byte a = (byte)work.cd.mData[hl].dat;
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
            byte a = (byte)work.cd.mData[work.hl++].dat;
            a &= 0xf;// A=COMMAND No.(0-F)

            FMCOM2[a]();
        }

        public void NTMEAN() { }

        // **	PCM VMODE CHANGE**

        public void PVMCHG()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.soundWork.PVMODE = a;
        }

        // **	ﾘﾊﾞｰﾌﾞ**

        public void REVERVE()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            work.cd.reverbVol = a;
            //RV1:
            work.cd.reverbFlg = true;
        }

        public void REVSW()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            if (a != 0)
            {
                //goto RV1;
                work.cd.reverbFlg = true;
                return;
            }
            work.cd.reverbFlg = false;
            STVOL();
        }

        public void REVMOD()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;
            if (a != 0)
            {
                work.cd.reverbMode = true;
                return;
            }
            //RM2:
            work.cd.reverbMode = false;
        }




        // **   PSG ｵﾝｼｮｸｾｯﾄ   **

        public void OTOSSG()
        {
            DummyOUT();

            byte a = (byte)work.cd.mData[work.hl++].dat;

            //OTOCAL
            int ptr = 0;// SSGDAT;
            ptr = a * 6;
            //ENVPST();
            for (int i = 0; i < 6; i++)
            {
                work.cd.softEnvelopeParam[i] = work.soundWork.SSGDAT[ptr + i];
            }
            work.cd.volume = work.cd.volume | 0b1001_0000;

        }

        public void OTOSET()
        {
            byte a = (byte)work.cd.mData[work.hl++].dat;

            //OTOCAL
            int ptr = 0;// SSGDAT;
            ptr = a * 6;

            for (int i = 0; i < 6; i++)
            {
                work.soundWork.SSGDAT[ptr + i] = (byte)work.cd.mData[work.hl++].dat;
            }

        }

        // **   ｴﾝﾍﾞﾛｰﾌﾟ ﾊﾟﾗﾒｰﾀ ｾｯﾄ**

        public void ENVPST()
        {
            for (int i = 0; i < 6; i++)
            {
                work.cd.softEnvelopeParam[i] = work.cd.mData[work.hl++].dat;
            }
            work.cd.volume = work.cd.volume | 0b1001_0000;// ｴﾝﾍﾞﾌﾗｸﾞ ｱﾀｯｸﾌﾗｸﾞ ｾｯﾄ

        }

        // **	PSG VOLUME	**

        public void PSGVOL()
        {
            DummyOUT();
            work.cd.hardEnveFlg = false;
            byte e = (byte)(work.cd.volume & 0b1111_0000);
            byte c = (byte)work.cd.mData[work.hl].dat;
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
            work.cd.volume = a;
        }

        // **   MIX PORT CONTROL**

        public void NOISE()
        {
            byte c = (byte)work.cd.mData[work.hl++].dat;
            byte b = (byte)work.cd.channelNumber;
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
            byte e = (byte)work.cd.mData[work.hl++].dat;
            PSGOUT(6, e);
            work.soundWork.PREGBF[4] = e;
        }

        // **	SSG VOLUME UP & DOWN**

        public void VOLUPS()
        {
            byte d = (byte)work.cd.mData[work.hl++].dat;
            if (!work.cd.hardEnveFlg)
            {
                byte a = (byte)work.cd.volume;
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
                work.cd.volume = a;
            }
        }

        // **	LFO ﾙｰﾁﾝ	**

        public void PLLFO()
        {
            // ---	FOR FM & SSG LFO	---
            if (!work.cd.lfoflg)
            {
                return;
            }
            uint hl = work.cd.dataAddressWork;
            hl--;
            byte a = (byte)work.cd.mData[hl].dat;
            if (a == 0xf0)
            {
                return;//  ｲｾﾞﾝ ﾉ ﾃﾞｰﾀ ｶﾞ '&' ﾅﾗ RET
            }
            if (!work.cd.lfoContFlg)
            {
                // **	LFO INITIARIZE   **
                LFORST();
                LFORST2();
                work.cd.lfoCounterWork = work.cd.lfoCounter;
                work.cd.lfoContFlg = true;// SET CONTINUE FLAG
            }
            //CTLFO:
            if (work.cd.lfoDelayWork == 0)//delayが完了していたら次の処理へ
            {
                CTLFO1();
                return;
            }
            work.cd.lfoDelayWork--;//delayのカウントダウン
        }

        public void CTLFO1()
        {
            work.cd.lfoCounterWork--;// ｶｳﾝﾀ
            if (work.cd.lfoCounterWork != 0)
            {
                return;
            }
            work.cd.lfoCounterWork = work.cd.lfoCounter;//ｶｳﾝﾀ ｻｲ ｾｯﾃｲ
            if (work.cd.lfoPeakWork == 0)//  GET PEAK LEVEL COUNTER(P.L.C)
            {
                work.cd.lfoDeltaWork = -work.cd.lfoDeltaWork;// WAVE ﾊﾝﾃﾝ
                work.cd.lfoPeakWork = work.cd.lfoPeak;//  P.L.C ｻｲ ｾｯﾃｲ
            }
            //PLLFO1:
            work.cd.lfoPeakWork--;// P.L.C.-1
            int hl = work.cd.lfoDeltaWork;
            PLS2(hl);
        }

        public void PLS2(int hl)
        {
            if (work.soundWork.PCMFLG == 0)
            {
                PLSKI2(hl);
                return;
            }

            hl += (int)work.soundWork.DELT_N;
            work.soundWork.DELT_N = (uint)hl;

            PCMOUT(0x09, (byte)hl);
            PCMOUT(0x0a, (byte)(hl >> 8));
        }

        public void PLSKI2(int hl)
        {
            if (work.soundWork.SSGF1 != 0 && work.cd.SSGTremoloFlg)
            {
                work.cd.SSGTremoloVol += hl;
                //Console.WriteLine(work.cd.SSGTremoloVol);
                return;
            }

            if (work.soundWork.SSGF1 == 0)
            {
                //KUMA:FMの時はリミットチェック処理

                int num = work.cd.fnum & 0x7ff;
                int blk = work.cd.fnum >> 11;
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

                int de = work.cd.fnum;// GET FNUM1
                // GET B/FNUM2
                hl += de;//  HL= NEW F-NUMBER
                hl = (ushort)hl;
            }

            work.cd.fnum = hl;// SET NEW F-NUM1
                              // SET NEW F-NUM2

            if (work.soundWork.SSGF1 == 0)
            {
                LFOP5(hl);
                return;
            }

            // ---	FOR SSG LFO	---
            byte a = (byte)work.cd.beforeCode;// GET KEY CODE&OCTAVE
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
            byte d = (byte)work.cd.channelNumber;
            PSGOUT(d, e);
            d++;
            e = (byte)(hl >> 8);
            PSGOUT(d, e);
        }

        // ---	FOR FM LFO	---

        public void LFOP5(int hl)
        {
            if (work.cd.tlLfoflg)
            {
                LFOP6(hl);
                return;
            }

            if ((work.cd.channelNumber & 0x02) == 0)//  CH=3?
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
            d += (byte)work.cd.channelNumber;
            byte e = (byte)(hl >> 8);
            PSGOUT(d, e);

            d -= 4;
            e = (byte)hl;// F-NUMBER1 DATA
            PSGOUT(d, e);
        }

        public void LFOP6(int hl)
        {
            byte c = work.cd.TLlfoSlot;//.soundWork.LFOP6_VAL;
            
            byte d = 0x40;
            d += (byte)work.cd.channelNumber;
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
            work.cd = work.soundWork.CHDAT[work.idx];
            work.hl = work.cd.dataAddressWork;

            work.cd.lengthCounter = (byte)(work.cd.lengthCounter - 1);
            if (work.cd.lengthCounter == 0)
            {
                SSSUB7();
                return;
            }

            if (work.cd.lengthCounter != work.cd.quantize )
            {
                SSSUB0();
                return;
            }

            if (work.cd.mData[work.cd.dataAddressWork].dat == 0xfd)//COUNT OVER?
            {
                goto SSUB0;
            }
            SSSUBA();// TO REREASE
            return;//    RET
        SSUB0:
            work.cd.keyoffflg = false;//SET TIE FLAG(たぶんキーオフをリセット)
            SSSUB0();
        }

        public void SSSUB0()
        {
            if ((work.cd.volume & 0x80) == 0)// ENVELOPE CHECK
            {
                return;
            }

            SOFENV();

            if (work.cd.SSGTremoloFlg)
            {
                work.A_Reg = (byte)Math.Max(Math.Min((work.A_Reg + work.cd.SSGTremoloVol / 16), 15), 0);
            }
            

            byte e = work.A_Reg;
            if (work.soundWork.READY == 0)
            {
                e = 0;
            }
            if (work.soundWork.KEY_FLAG != 0xff)
            {
                byte d = (byte)work.cd.volReg;
                PSGOUT(d, e);
            }
            return;
        }

        public void SSSUB7()
        {
            work.hl = work.cd.dataAddressWork;
            if (work.cd.mData[work.hl].dat == 0xfd)//COUNT OVER?
            {
                //SSUB1:
                work.cd.keyoffflg = false;//SET TIE FLAG(たぶんキーオフをリセット)
                work.hl++;
                SSSUBB();
                return;
            }
            //SSSUBE:
            work.cd.keyoffflg = true;
            SSSUBB();
        }

        // **	KEY OFF ｼﾞ ﾉ RR ｼｮﾘ	**

        public void SSSUBA()
        {
            // --	HARD ENV.KEY OFF   --
            if (work.cd.hardEnveFlg)
            {
                PSGOUT((byte)work.cd.volReg, 0);//SSG KEY OFF
            }

            // --	SOFT ENV.KEY OFF   --

            if (work.cd.reverbFlg)
            {
                work.cd.keyoffflg = false;
                SSSUB0();
                return;
            }

            //SSUBAC:
            if ((work.cd.volume & 0x80) == 0)
            {
                SSSUB3(0);// ﾘﾘｰｽ ｼﾞｬﾅｹﾚﾊﾞ SSSUB3
                return;
            }
            work.cd.volume &= 0b1000_1111;// STATE 4 (ﾘﾘｰｽ)
            SOFEV9();
            SSSUB3(work.A_Reg);
        }

        public void SSSUBB()
        {
            work.crntMmlDatum = work.cd.mData[work.hl];

            byte a;
            do
            {
                if (work.cd.mData[work.hl].dat == 0)// CHECK END MARK
                {
                    work.cd.loopEndFlg = true;
                    // HL=DATA TOP ADD
                    if (work.cd.dataTopAddress == -1)
                    {
                        SSGEND();
                        return;
                    }
                    work.hl = (uint)work.cd.dataTopAddress;
                    work.cd.loopCounter++;
                }

                //演奏情報退避
                work.crntMmlDatum = work.cd.mData[work.hl];

                //SSSUB1:
                //SSSUB2:
                a = (byte)work.cd.mData[work.hl++].dat;// INPUT FLAG &LENGTH
                if (a < 0xf0) break;
                //COMMAND OF PSG?
                //SSSUB8();
                a &= 0xf;// A=COMMAND No.(0-F)
                PSGCOM[a]();

            } while (true);

            bool carry = ((a & 0x80) != 0);
            a &= 0x7f;// CY=REST FLAG

            work.cd.lengthCounter = a;//  SET WAIT COUNTER
                                      //  ｷｭｳﾌ ﾅﾗ SSSUBA
            if (carry)
            {
                work.crntMmlDatum = work.cd.mData[work.hl-1];
                SSSUBA();
                SETPT();
                return;
            }

            // **	SET FINE TUNE & COARSE TUNE	**

            //SSSUB6:
            a = (byte)work.cd.mData[work.hl++].dat;// LOAD OCT & KEY CODE
            byte b, c;
            if (!work.cd.keyoffflg)
            {
                c = a;
                b = (byte)work.cd.beforeCode;
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
            work.cd.beforeCode = a;// STORE KEY CODE & OCTAVE

            //Mem.stack.Push(Z80.HL);

            b = a;
            a &= 0b0000_1111;//  GET KEY CODE
            byte e = a;
            int hl = work.soundWork.SNUMB[e];// GET FNUM2
            int de = work.cd.detune;// GET DETUNE DATA
            hl += de;//  DETUNE PLUS
            hl = (short)hl;
            work.cd.fnum = hl;// SAVE FOR LFO
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
            //SSSUB4:
            e = (byte)hl;
            byte d = (byte)work.cd.channelNumber;
            PSGOUT(d, e);
            e = (byte)(hl >> 8);
            d++;
            PSGOUT(d, e);
            if (work.cd.keyoffflg)
            {
                goto SSSUBF;
            }
            SOFENV();
            goto SSSUB9;

        SSSUBF:         // KEYON ｻﾚﾀﾄｷ ﾉ ｼｮﾘ

            if (work.cd.hardEnveFlg)
            {
                //// ---   HARD ENV. KEY ON
                if (work.soundWork.KEY_FLAG != 0xff)
                {
                    PSGOUT((byte)work.cd.volReg, 0x10);
                }
                PSGOUT(0x0d, (byte)work.cd.hardEnvelopValue);
            }
            else
            {
                //// ---	SOFT ENV.KEYON     ---

                //SSSUBG:
                a = (byte)work.cd.volume;
                a &= 0b0000_1111;
                a |= 0b1001_0000;//  TO STATE 1 (ATTACK)
                work.cd.volume = a;

                a = (byte)work.cd.softEnvelopeParam[0];//  ENVE INIT
                work.cd.softEnvelopeCounter = a;//KUMA:ALがcounterの初期値として使用される
                work.cd.lfoContFlg = false;// RESET LFO CONTINE FLAG
                SOFEV7();

                //SSSUBH:
                c = (byte)work.cd.lfoPeak;
                c >>= 1;
                work.cd.lfoPeakWork = c;//  LFO PEAK LEVEL ｻｲ ｾｯﾃｲ
                work.cd.lfoDelayWork = work.cd.lfoDelay;//  LFO DELAY ﾉ ｻｲｾｯﾃｲ
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
            if ((work.cd.volume & 0x10) == 0)// CHECK ATTACK FLAG
            {
                goto SOFEV2; //KUMA:decay flagのチェックへゴー
            }

            byte a = (byte)work.cd.softEnvelopeCounter;  //KUMA:get counter
            byte d = (byte)work.cd.softEnvelopeParam[1];  //KUMA:get AR
            bool carry = ((a + d) > 0xff); //KUMA:counter + AR が255を超えたか？
            a += d;
            if (!carry)
            {
                goto SOFEV1;
            }
            a = 0xff; //KUMA:counterが上限を突破したので,counterを255に修正
        SOFEV1: //KUMA:counterとflagの更新
            work.cd.softEnvelopeCounter = a; //KUMA: counter = counter + AR(毎クロック,AR分だけcounterが増える)
            if ((a - 0xff) != 0)
            {
                SOFEV7(); //KUMA:counterが255に達していないならSOFEV7へ
                return;
            }
            a = (byte)work.cd.volume;//KUMA:current volume & flagsを取得
            a ^= 0b0011_0000;//KUMA:attack flag:off  decay flag:on をxorで実現(上手い)
            work.cd.volume = a;// TO STATE 2 (DECAY) //KUMA:current volume & flagsを更新
            SOFEV7();
            return;
        SOFEV2:
            if ((work.cd.volume & 0x20) == 0)//KUMA: Check decay flag
            {
                goto SOFEV4;//KUMA:sustain flagのチェックへ
            }
            a = (byte)work.cd.softEnvelopeCounter;// KUMA:get counter
            d = (byte)work.cd.softEnvelopeParam[2];// GET DECAY //KUMA:get DR
            byte e = (byte)work.cd.softEnvelopeParam[3];// GET SUSTAIN //KUMA:get SR
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
            work.cd.softEnvelopeCounter = a;//KUMA:counter=counter-DR(毎クロック,DR分だけcounterが減る)
            if ((a - e) != 0)
            {
                SOFEV7();//KUMA: counterがSRに到達していないならSOFEV7へ
                return;
            }
            a = (byte)work.cd.volume;//KUMA:current volume & flagsを取得
            a ^= 0b0110_0000;//KUMA:dcay flag:off  sustain flag:on
            work.cd.volume = a;// TO STATE 3 (SUSTAIN) //KUMA:current volume & flagsを更新
            SOFEV7();
            return;
        SOFEV4:
            if ((work.cd.volume & 0x40) == 0)//KUMA: Check sustain flag
            {
                SOFEV9();//KUMA:release 処理へ
                return;
            }
            a = (byte)work.cd.softEnvelopeCounter;// KUMA:get counter
            d = (byte)work.cd.softEnvelopeParam[4];// GET SUSTAIN LEVEL// KUMA:get SL
            carry = ((a - d) < 0);//KUMA:counter = counter - SL 結果、counterが0以上の場合はSOFEV5へ
            a -= d;
            if (!carry)
            {
                goto SOFEV5;
            }
            a = 0;//KUMA: counter=0
        SOFEV5:
            work.cd.softEnvelopeCounter = a;//KUMA:counter=counter-SL(毎クロック,SL分だけcounterが減る)
            if (a != 0)
            {
                SOFEV7();
                return;
            }
            a = (byte)work.cd.volume;//KUMA:current volume & flagsを取得
            a &= 0b1000_1111;//KUMA:エンベロープで使用した進捗に関わるフラグをリセット
            work.cd.volume = a;// END OF ENVE //KUMA:KEYON中にSLにきて更にcounterが0になったらエンベロープ処理は終了する
            SOFEV7();
        }

        public void SOFEV9()
        {
            byte a = (byte)work.cd.softEnvelopeCounter;//KUMA:get counter
            byte d = (byte)work.cd.softEnvelopeParam[5];// GET REREASE//KUMA:get RR
            bool carry = ((a - d) < 0);//KUMA:RRでcounterを減算
            a -= d;
            if (!carry)
            {
                goto SOFEVA;
            }
            a = 0;
        SOFEVA:
            work.cd.softEnvelopeCounter = a;//KUMA:counterを更新
            SOFEV7();
        }

        // **	VOLUME CALCURATE	**

        public void SOFEV7()
        {
            byte e = (byte)work.cd.softEnvelopeCounter;//KUMA:get counter
            int hl = 0;
            byte a = (byte)work.cd.volume;// GET VOLUME
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
            if (work.cd.keyoffflg)
            {
                return;
            }
            if (!work.cd.reverbFlg)
            {
                return;
            }
            a += (byte)work.cd.reverbVol;//.softEnvelopeParam[5];
            
            work.carry= ((a & 0x01) != 0);
            a >>= 1;
            work.A_Reg = a;
        }

        // **   SET POINTER   **

        public void SETPT()
        {
            work.cd.dataAddressWork = work.hl;//  SET NEXT SOUND DATA ADDRES
            return;
        }

        public void SSGEND()
        {
            work.cd.musicEnd = true;
            work.cd.dataAddressWork = work.hl;
            SKYOFF();
            work.cd.lfoflg = false;// RESET LFO FLAG
        }

        // **   SSG KEY OFF**

        public void SKYOFF()
        {
            work.cd.volume = 0;// ENVE FLAG RESET
            byte e = 0;
            byte d = (byte)work.cd.volReg;
            PSGOUT(d, e);
        }

        public void SSSUB3(byte a)
        {
            if (!work.cd.hardEnveFlg)
            {
                byte e = a;
                if (work.soundWork.READY == 0)
                {
                    e = 0;
                }
                if (work.soundWork.KEY_FLAG != 0xff)
                {
                    byte d = (byte)work.cd.volReg;
                    PSGOUT(d, e);
                }
            }
            work.cd.dataAddressWork = work.hl;

            //byte e = a;//added
            //if (work.soundWork.READY == 0)//added
            //{
            //    e = 0;//added
            //}
            ////SSSUB32:
            //byte d = (byte)work.cd.volReg;
            //PSGOUT(d,e);
            //SETPT();
        }

        public void HRDENV()
        {

            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = 0x0d;
            PSGOUT(d, e);
            work.cd.hardEnveFlg = true;
            work.cd.hardEnvelopValue = (byte)(e & 0xf);
            work.cd.volume = 16;

        }

        public void ENVPOD()
        {

            byte e = (byte)work.cd.mData[work.hl++].dat;
            byte d = 0x0b;
            PSGOUT(d, e);

            e = (byte)work.cd.mData[work.hl++].dat;
            d = 0x0c;
            PSGOUT(d, e);

        }

        private void SetKeyOnDelay()
        {
            work.cd.KeyOnDelayFlag = false;
            for (int i = 0; i < 4; i++)
            {
                work.cd.KDWork[i] = work.cd.KD[i] = (byte)work.cd.mData[work.hl++].dat;
                if (work.cd.KD[i] != 0) work.cd.KeyOnDelayFlag = true;
            }

            work.cd.keyOnSlot = 0x00;
            if (!work.cd.KeyOnDelayFlag) work.cd.keyOnSlot = 0xf0;
        }

        private void KeyOnDelaying()
        {
            if (work.soundWork.DRMF1 != 0) return;
            if (work.soundWork.PCMFLG != 0) return;
            if (!work.cd.KeyOnDelayFlag) return;

            byte newf = work.cd.keyOnSlot;
            for(int i = 0; i < 4; i++)
            {
                if (work.cd.KDWork[i] == 0) continue;
                work.cd.KDWork[i]--;
                if (work.cd.KDWork[i] == 0)
                {
                    newf |= (byte)(0x10 << i);
                }
            }

            if (newf != work.cd.keyOnSlot)
            {
                //Key On!!
                KEYON2();
                work.cd.keyOnSlot = newf;
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

            if (!work.cd.KeyOnDelayFlag)
            {
                a += work.cd.keyOnSlot;
            }
            else
            {
                work.cd.keyOnSlot = 0x00;
                if (work.cd.KDWork[0] == 0) work.cd.keyOnSlot += 0x10;
                if (work.cd.KDWork[1] == 0) work.cd.keyOnSlot += 0x20;
                if (work.cd.KDWork[2] == 0) work.cd.keyOnSlot += 0x40;
                if (work.cd.KDWork[3] == 0) work.cd.keyOnSlot += 0x80;
                a += work.cd.keyOnSlot;
            }

            //KEYON2:
            a += (byte)work.cd.channelNumber;
            PSGOUT(0x28, a);//KEY-ON

            if (work.cd.reverbFlg)
            {
                STVOL();
            }
        }

    }
}
