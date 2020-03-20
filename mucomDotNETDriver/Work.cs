using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mucomDotNET.Common;
using musicDriverInterface;

namespace mucomDotNET.Driver
{
    public class Work
    {
        public object lockObj = new object();
        public object SystemInterrupt = new object();

        private int _status = 0;
        public int Status
        {
            get { lock (lockObj) { return _status; } }
            set { lock (lockObj) { _status = value; } }
        }

        public uint mDataAdr { get; internal set; }
        public int idx { get; internal set; }
        public CHDAT cd { get; internal set; }
        public bool carry { get; internal set; }
        public uint hl { get; internal set; }
        public byte A_Reg { get; internal set; }
        public int weight { get; internal set; }
        public object crntMmlDatum { get; internal set; }
        public int maxLoopCount { get; internal set; } = -1;

        public OPNATimer timer = null;
        public ulong timeCounter = 0L;
        public byte[] fmVoice = null;
        public Tuple<string, ushort[]>[] pcmTables = null;
        public MmlDatum[] mData = null;
        public SoundWork soundWork = null;
        public byte[] fmVoiceAtMusData = null;
        public bool isDotNET = false;

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
