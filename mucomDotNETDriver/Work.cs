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

        private int _status = 0;
        public int Status
        {
            get { lock (lockObj) { return _status; } }
            set { lock (lockObj) { _status = value; } }
        }

        public uint mDataAdr { get; internal set; }

        public OPNATimer timer = null;
        public ulong timeCounter = 0L;
        public byte[] fmVoice = null;
        public Tuple<string, ushort[]>[] pcmTables = null;
        public byte[] mData = null;
        public SoundWork soundWork = null;

        public Work()
        {
            Init();
        }

        internal void Init()
        {
            soundWork = new SoundWork();
            soundWork.Init();
        }
    }
}
