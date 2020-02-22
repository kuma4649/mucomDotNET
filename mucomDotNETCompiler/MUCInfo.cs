using System;
using System.Collections.Generic;
using System.Text;
using mucomDotNET.Common;
using mucomDotNET.Interface;

namespace mucomDotNET.Compiler
{
    public class MUCInfo_
    {
        public string title { get; internal set; }
        public string composer { get; internal set; }
        public string author { get; internal set; }
        public string comment { get; internal set; }
        public string mucom88 { get; internal set; }
        public string date { get; internal set; }
        public string voice { get; internal set; }
        public string pcm { get; internal set; }
        public int lines { get; internal set; }
        /// <summary>
        /// mml中で定義した音色データ
        /// </summary>
        public byte[] mmlVoiceData { get; internal set; }
        /// <summary>
        /// ファイルから読み込んだプリセットの音色データ
        /// </summary>
        public byte[] voiceData { get; internal set; }
        public byte[] pcmData { get; internal set; }
        public List<Tuple<int, string>> basSrc { get; internal set; }
        public string fnSrc { get; internal set; }
        public string workPath { get; internal set; }
        public string fnDst { get; internal set; }
        public AutoExtendList<MubDat> bufDst { get; internal set; }
        public int srcLinPtr { get; internal set; }
        public int srcCPtr { get; internal set; }
        public Tuple<int, string> lin { get; internal set; }
        public bool Carry { get; internal set; }
        public bool ErrSign { get; internal set; }
        public AutoExtendList<int> bufMac { get; internal set; }
        public AutoExtendList<int> bufMacStack { get; internal set; }
        public AutoExtendList<byte> bufLoopStack { get; internal set; }

        /// <summary>
        /// mml全体で実際に使用した音色番号
        /// 関連項目:
        /// orig:DEFVOICE
        /// </summary>
        public AutoExtendList<int> bufDefVoice { get; internal set; }

        public int useOtoAdr { get; internal set; }
        public AutoExtendList<int> bufTitle { get; internal set; }
        public AutoExtendList<byte> mmlVoiceDataWork { get; internal set; }

        public int row { get; set; }
        public int col { get; set; }
        public int VM { get; internal set; }

        internal void Clear()
        {
            title = "";
            composer = "";
            author = "";
            comment = "";
            mucom88 = "";
            date = "";
            voice = "";
            pcm = "";
            lines = 0;
            voiceData = null;
            pcmData = null;
            basSrc = new List<Tuple<int, string>>();
            fnSrc = "";
            workPath = "";
            fnDst = "";
            bufDst = new AutoExtendList<MubDat>();
            srcLinPtr = 0;
            srcCPtr = 0;
            bufMac = new AutoExtendList<int>();
            bufMacStack = new AutoExtendList<int>();
            bufLoopStack = new AutoExtendList<byte>();
            bufDefVoice = new AutoExtendList<int>();
            bufTitle = new AutoExtendList<int>();
            mmlVoiceDataWork = new AutoExtendList<byte>();
        }
    }
}
