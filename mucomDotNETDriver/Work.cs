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
        public bool resetPlaySync = false;

        private int _status = 0;

        public MUBHeader header { get; internal set; }

        public int Status
        {
            get { lock (lockObj) { return _status; } }
            set { lock (lockObj) { _status = value; } }
        }

        public uint mDataAdr { get; internal set; }
        //public int idx { get; internal set; }

        /// <summary>
        /// カレントのチャンネル
        /// </summary>
        public CHDAT cd { get; internal set; }

        /// <summary>
        /// カレントのページ
        /// </summary>
        public PGDAT pg { get; internal set; }

        public bool carry { get; internal set; }
        public uint hl { get; internal set; }
        public byte A_Reg { get; internal set; }
        public int weight { get; internal set; }
        public object crntMmlDatum { get; internal set; }
        public int maxLoopCount { get; internal set; } = -1;
        public int nowLoopCounter { get; internal set; } = -1;
        public int[] rhythmORKeyOff { get; internal set; } = new int[4];
        public int[] rhythmOR { get; internal set; } = new int[4];
        public bool abnormalEnd { get; internal set; } = false;
        public int currentTimer { get; internal set; }
        public Dictionary<int, byte[]> ssgVoiceAtMusData { get; internal set; }

        public OPNATimer timerOPNA1 = null;
        public OPNATimer timerOPNA2 = null;
        public OPNATimer timerOPNB1 = null;
        public OPNATimer timerOPNB2 = null;
        public OPMTimer timerOPM = null;

        public ulong timeCounter = 0L;
        public byte[][] fmVoice = new byte[4][] { null, null, null, null };
        public Tuple<string, ushort[]>[][] pcmTables = new Tuple<string, ushort[]>[6][] { null, null, null, null, null, null };
        public MmlDatum[] mData = null;
        public SoundWork soundWork = null;
        public byte[] fmVoiceAtMusData = null;
        public bool isDotNET = false;
        public bool SSGExtend = false;

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
