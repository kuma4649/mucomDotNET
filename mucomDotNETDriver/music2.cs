using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{
    public class Music2
    {
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
    }
}
