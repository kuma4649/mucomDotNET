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
        public long[] loopCounter = null;
        // **	FM CONTROL COMMAND(s)   **
        public Action[] FMCOM = null;
        public Action[] FMCOM2 = null;
        public Action[] LFOTBL = null;
        // **   PSG COMMAND TABLE**
        public Action[] PSGCOM = null;

        private Work work;
        private Action<OPNAData> WriteOPNARegister = null;

        public Music2(Work work, Action<OPNAData> WriteOPNARegister)
        {
            this.work = work;
            this.WriteOPNARegister = WriteOPNARegister;
        }

        internal bool notSoundBoard2;

        public void MSTART(int musicNumber)
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;

            work.soundWork.MUSNUM = musicNumber;
            AKYOFF();
            SSGOFF();
            WORKINIT();

            CHK();//added
            INT57();
            ENBL();
            TO_NML();

            work.Status = 1;

            work.SystemInterrupt = false;
        }

        public void MSTOP()
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;

            AKYOFF();
            SSGOFF();

            work.Status = 0;

            work.SystemInterrupt = false;
        }

        public void FDO()
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;



            work.SystemInterrupt = false;
        }

        public object RETW()
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;



            work.SystemInterrupt = false;
            return null;
        }

        public void EFC()
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;



            work.SystemInterrupt = false;
        }

        public void Rendering()
        {
            if (work.Status == 0) return;

            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;

            work.timer.timer();
            work.timeCounter++;
            if ((work.timer.StatReg & 3) != 0)
            {
                PL_SND();
            }

            work.SystemInterrupt = false;
        }


        public void initMusic2()
        {
            SetFMCOMTable();
            SetLFOTBL();
            SetPSGCOM();
            SetSoundWork();

            loopCounter = new long[MAXCH];
            for (int i = 0; i < loopCounter.Length; i++) loopCounter[i] = -1;// ulong.MaxValue;
        }

        public void SetFMCOMTable()
        {
            FMCOM = new Action[] {
            //OTOPST // 0xF0 - ｵﾝｼｮｸ ｾｯﾄ    '@'
            //,VOLPST// 0xF1 - VOLUME SET   'v'
            //,FRQ_DF// 0xF2 - DETUNE(ｼｭｳﾊｽｳ ｽﾞﾗｼ) 'D'
            //,SETQ  // 0xF3 - SET COMMAND 'q'
            //,LFOON // 0xF4 - LFO SET
            //,REPSTF// 0xF5 - REPEAT START SET  '['
            //,REPENF// 0xF6 - REPEAT END SET    ']'
            //,MDSET // 0xF7 - FMｵﾝｹﾞﾝ ﾓｰﾄﾞｾｯﾄ
            //,STEREO// 0xF8 - STEREO MODE
            //,FLGSET// 0xF9 - FLAG SET
            //,W_REG // 0xFA - COMMAND OF   'y'
            //,VOLUPF// 0xFB - VOLUME UP    ')'
            //,HLFOON// 0xFC - HARD LFO
            //,TIE   // (CANT USE)
            //,RSKIP // 0xFE - REPEAT JUMP'/'
            //,SECPRC// 0xFF - to second com
            };

            FMCOM2 = new Action[] {
            // PVMCHG // 0xFF 0xF0 - PCM VOLUME MODE
            //,HRDENV	// 0xFF 0xF1 - HARD ENVE SET 's'
            //,ENVPOD // 0xFF 0xF2 - HARD ENVE PERIOD
            //,REVERVE// 0xFF 0xF3 - ﾘﾊﾞｰﾌﾞ
            //,REVMOD	// 0xFF 0xF4 - ﾘﾊﾞｰﾌﾞﾓｰﾄﾞ
            //,REVSW	// 0xFF 0xF5 - ﾘﾊﾞｰﾌﾞ ｽｲｯﾁ
            //,NTMEAN
            //,NTMEAN
            };
        }

        public void SetLFOTBL()
        {
            LFOTBL = new Action[]{
            // LFOOFF
            //, LFOON2
            //, SETDEL
            //, SETCO
            //, SETVC2
            //, SETPEK
            //, TLLFO
            };
        }

        public void SetPSGCOM()
        {
            PSGCOM = new Action[] {
                // OTOSSG// 0xF0 - ｵﾝｼｮｸ ｾｯﾄ         '@'
                //,PSGVOL// 0xF1 - VOLUME SET
                //,FRQ_DF// 0xF2 - DETUNE
                //,SETQ  // 0xF3 - COMMAND OF        'q'
                //,LFOON // 0xF4 - LFO
                //,REPSTF// 0xF5 - REPEAT START SET  '['
                //,REPENF// 0xF6 - REPEAT END SET    ']'
                //,NOISE // 0xF7 - MIX PORT          'P'
                //,NOISEW// 0xF8 - NOIZE PARAMATER   'w'
                //,FLGSET// 0xF9 - FLAG SET
                //,ENVPST// 0xFA - SOFT ENVELOPE     'E'
                //,VOLUPS// 0xFB - VOLUME UP    ')'
                //,NTMEAN// 0xFC -
                //,TIE   // 0x
                //,RSKIP // 0x
                //,SECPRC// 0xFF - to sec com
            };
        }

        public void SetSoundWork()
        {
            work.Init();
        }


        private void AKYOFF()
        {
            for(int e = 0; e < 7; e++)
            {
                OPNAData dat = new OPNAData(0, (byte)e, 0x28);
                WriteOPNARegister(dat);
            }
        }

        // **	SSG ALL SOUND OFF	**

        private void SSGOFF()
        {
            for (int b = 0; b < 3; b++)
            {
                OPNAData dat = new OPNAData(0, (byte)(0x8 + b), 0x0);
                WriteOPNARegister(dat);
            }
        }

        // **   VOLUME OR FADEOUT etc RESET**

        public void WORKINIT()
        {
            work.soundWork.C2NUM = 0;
            work.soundWork.CHNUM = 0;
            work.soundWork.PVMODE = 0;

            int num = work.soundWork.MUSNUM;
            work.mDataAdr = work.soundWork.MU_TOP;

            for (int i = 0; i < num; i++)
            {
                work.mDataAdr += 1 + MAXCH * 4;
                work.mDataAdr = work.soundWork.MU_TOP + Common.getLE16(work.mData, (uint)work.mDataAdr);
            }

            work.soundWork.TIMER_B = work.mData[work.mDataAdr];
            work.soundWork.TB_TOP = ++work.mDataAdr;

            int ch = 0;// (CH1DATのこと)
            for (ch = 0; ch < 6; ch++)
            {
                FMINIT(ch);
                ch++;//オリジナルは　ix+=WKLENG だが、配列化しているので。
            }

            work.soundWork.CHNUM = 0;
            ch = 6;//DRAMDAT
            FMINIT(ch);

            work.soundWork.CHNUM = 0;
            //ix = 7;//CHADAT
            for (ch = 7; ch < 7 + 4; ch++)
            {
                FMINIT(ch);
                ch++;//オリジナルは　ix+=WKLENG だが、配列化しているので。
            }
        }

        private void FMINIT(int ch)
        {
            work.soundWork.CHDAT[ch] = new CHDAT();
            work.soundWork.CHDAT[ch].lengthCounter = 1;
            work.soundWork.CHDAT[ch].volume = 0;

            // ---	POINTER ﾉ ｻｲｾｯﾃｲ	---

            work.soundWork.CHDAT[ch].dataAddressWork = work.soundWork.MU_TOP + Common.getLE16(work.mData, work.soundWork.TB_TOP);//ix 2,3
            work.soundWork.CHDAT[ch].dataTopAddress = work.soundWork.MU_TOP + Common.getLE16(work.mData, work.soundWork.TB_TOP + 2);//ix 4,5

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

            OPNAData dat = new OPNAData(0, 0x29, 0x83);// CH 4-6 ENABLE
            WriteOPNARegister(dat);

            for(int b = 0; b < 6; b++)
            {
                dat = new OPNAData(0, (byte)b, 0x00);// CH 4-6 ENABLE
                WriteOPNARegister(dat);
            }

            dat = new OPNAData(0, 7, 0b0011_1000);

            // PSGﾊﾞｯﾌｧ ｲﾆｼｬﾗｲｽﾞ
            for(int i = 0; i < work.soundWork.INITPM.Length; i++)
            {
                work.soundWork.PREGBF[i] = work.soundWork.INITPM[i];
            }

        }

        private void TO_NML()
        {
            work.soundWork.PLSET1_VAL = 0x38;
            work.soundWork.PLSET2_VAL = 0x3a;

            OPNAData dat = new OPNAData(0, 0x27, 0x3a);
            WriteOPNARegister(dat);
        }

        // **	ALL MONORAL / H.LFO OFF	***

        private void MONO()
        {
            OPNAData dat;
            for (int b = 0; b < 3; b++)
            {
                dat = new OPNAData(0, (byte)(0xb4 + b), 0xc0);//fm 1-3
                WriteOPNARegister(dat);
            }

            for (int b = 0; b < 6; b++)
            {
                dat = new OPNAData(0, (byte)(0x18 + b), 0xc0);//rhythm
                WriteOPNARegister(dat);
            }

            for (int b = 0; b < 3; b++)
            {
                dat = new OPNAData(1, (byte)(0xb4 + b), 0xc0);//fm 4-6
                WriteOPNARegister(dat);
            }

            dat = new OPNAData(0, 0x22, 0x00);//lfo freq control
            WriteOPNARegister(dat);
            dat = new OPNAData(0, 0x12, 0x00);//rhythm test data
            WriteOPNARegister(dat);


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
            OPNAData dat = new OPNAData(0, 0x26, e);
            WriteOPNARegister(dat);

            dat = new OPNAData(0, 0x27, 0x78);
            WriteOPNARegister(dat);//  Timer-B OFF

            dat = new OPNAData(0, 0x27, 0x7a);
            WriteOPNARegister(dat);//  Timer-B ON

            //割り込みレベルリセット不要
            //Z80.A = 5;
            //PC88.OUT(0xe4, Z80.A);
        }









        // **	MUSIC MAIN	**

        public void PL_SND()
        {

            OPNAData dat = new OPNAData(0, 0x27, work.soundWork.PLSET1_VAL);//  TIMER-OFF DATA
            WriteOPNARegister(dat);
            dat = new OPNAData(0, 0x27, work.soundWork.PLSET2_VAL);//  TIMER-ON DATA
            WriteOPNARegister(dat);

            DRIVE();
            //FDOUT();
        }

        // **	CALL FM		**

        public void DRIVE()
        {
            work.soundWork.FMPORT = 0;
              
            FMENT(0);
            FMENT(1);
            FMENT(2);

            work.soundWork.SSGF1 = 0xff;
            SSGENT(3);
            SSGENT(4);
            SSGENT(5);
            work.soundWork.SSGF1 = 0;


            if (notSoundBoard2) return;

            work.soundWork.DRMF1 = 1;
            FMENT(6);
            work.soundWork.DRMF1 = 0;

            work.soundWork.FMPORT = 1;
            FMENT(7);
            FMENT(8);
            FMENT(9);

            work.soundWork.PCMFLG = 0xff;
            FMENT(10);
            work.soundWork.PCMFLG = 0;

        }

        public void FMENT(int ix)
        {
            if (work.soundWork.CHDAT[ix].muteFlg) //KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0x00;
            }
            //FMSUB();
            //PLLFO();
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
            //SSGSUB();
            //PLLFO();
            if (work.soundWork.CHDAT[ix].muteFlg)//KUMA: 0x08(bit3)=MUTE FLAG
            {
                work.soundWork.READY = 0xff;
            }
        }








    }
}
