using musicDriverInterface;

namespace mucomDotNET.Driver
{
    public class MupbInfo
    {
        public MupbInfo()
        {
        }

        public uint version { get; set; }
        public int variableLengthCount { get; set; }
        public int useChipCount { get; set; }
        public int usePartCount { get; set; }
        public int usePageCount { get; set; }
        public int useInstrumentSetCount { get; set; }
        public int usePCMSetCount { get; set; }
        public ChipDefine[] chips { get; set; }
        public uint tagDataOffset { get; set; }
        public uint tagDataSize { get; set; }
        public uint JCLOCK { get; set; }
        public uint JPLINE { get; set; }
        public PartDefine[] parts { get; set; }
        public PageDefine[] pages { get; internal set; }
        public InstrumentDefine[] instruments { get; set; }
        public PCMDefine[] pcms { get; set; }

        public class ChipDefine
        {
            public uint indexNumber { get; set; }
            public uint identifyNumber { get; set; }
            public uint masterClock { get; set; }
            public uint option { get; set; }
            public uint heartBeat { get; set; }
            public uint heartBeat2 { get; set; }
            public uint[] instrumentNumber { get; set; }
            public uint[] pcmNumber { get; set; }

            public chipPart[] parts { get; set; }

            public class chipPart
            {
                public PageDefine[] pages { get; set; }

            }

        }

        public class PartDefine
        {
            public int pageCount { get;  set; }
        }

        public class PageDefine
        {
            public uint length { get; set; }
            public int loopPoint { get; set; }
            public MmlDatum[] data { get; set; }
        }

        public class InstrumentDefine
        {
            public uint length { get;  set; }
            public MmlDatum[] data { get; set; }
        }

        public class PCMDefine
        {
            public uint length { get;  set; }
            public MmlDatum[] data { get; set; }
        }
    }
}