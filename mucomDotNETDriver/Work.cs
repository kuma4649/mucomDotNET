using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{
    public class Work
    {
        public object lockObj = new object();
        private bool _SystemInterrupt = false;
        public bool SystemInterrupt
        {
            get { lock (lockObj) { return _SystemInterrupt; } }
            set { lock (lockObj) { _SystemInterrupt = value; } }
        }
        public OPNATimer timer = null;
        public ulong timeCounter = 0L;
        public byte[] fmVoice = null;
        public Tuple<string, ushort[]>[] pcmTables = null;
        public byte[] data = null;
        public SoundWork soundWork = null;

        internal void Init()
        {
            soundWork = new SoundWork();
            soundWork.Init();
        }
    }
}
