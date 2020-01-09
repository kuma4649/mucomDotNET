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

        public void MSTART(int musicNumber)
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;



            work.SystemInterrupt = false;
        }

        public void MSTOP()
        {
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;



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
            while (work.SystemInterrupt) ;
            work.SystemInterrupt = true;

            //opnaタイマー駆動
            work.timer.timer();
            work.timeCounter++;

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

    }
}
