using System;
using System.Collections.Generic;
using System.Drawing;
using musicDriverInterface;

namespace mucomDotNET.Common
{
    public class MUCInfo
    {
        public string title { get; set; }
        public string composer { get; set; }
        public string author { get; set; }
        public string comment { get; set; }
        public string mucom88 { get; set; }
        public string date { get; set; }
        public string voice { get; set; }
        public string[] pcm { get; set; } = new string[6];
        public List<string>[] pcmAt { get; set; } = new List<string>[6] {
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>(),
            new List<string>()
        };
        public string driver { get; set; }

        public string invert { get; set; }
        public string pcminvert { get; set; }

        public const string DotNET = "mucomDotNET";


        public int lines { get; set; }
        /// <summary>
        /// mml中で定義した音色データ
        /// </summary>
        public byte[] mmlVoiceData { get; set; }
        /// <summary>
        /// ファイルから読み込んだプリセットの音色データ
        /// </summary>
        public byte[] voiceData { get; set; }
        public byte[] pcmData { get; set; }
        public List<Tuple<int, string>> basSrc { get; set; }

        public object document = null;

        private string _fnSrc = null;
        public string fnSrc {
            get
            {
                return _fnSrc;
            }
            set
            {
                _fnSrc = value;
                _fnSrcOnlyFile = System.IO.Path.GetFileName(value);
            } 
        }
        private string _fnSrcOnlyFile = null;
        public string fnSrcOnlyFile {
            get
            {
                return _fnSrcOnlyFile;
            }
        }
        public string workPath { get; set; }
        public string fnDst { get; set; }

        //KUMA:作業向けメモリ
        public AutoExtendList<MmlDatum> bufDst { get; set; }

        //KUMA:ページ毎のメモリ
        public AutoExtendList<MmlDatum>[][][] bufPage { get; set; }

        //KUMA:音色用のメモリ(ページ機能使用時のみ)
        public AutoExtendList<MmlDatum> bufUseVoice { get; set; }

        public int srcLinPtr { get; set; }
        public int srcCPtr { get; set; }
        public Tuple<int, string> lin { get; set; }
        public bool Carry { get; set; }
        public bool ErrSign { get; set; }
        public AutoExtendList<int> bufMac { get; set; }
        public AutoExtendList<int> bufMacStack { get; set; }
        public AutoExtendList<byte> bufLoopStack { get; set; }

        /// <summary>
        /// mml全体で実際に使用した音色番号
        /// 関連項目:
        /// orig:DEFVOICE
        /// </summary>
        public AutoExtendList<int> bufDefVoice { get; set; }

        public int useOtoAdr { get; set; }
        public AutoExtendList<int> bufTitle { get; set; }
        public AutoExtendList<byte> mmlVoiceDataWork { get; set; }

        public int row { get; set; }
        public int col { get; set; }
        public int VM { get; set; }
        //public bool needNormalMucom { get; set; } = false;

        public enum enmDriverType
        {
            normal,
            E,
            em,
            DotNet
        }

        private enmDriverType _DriverType = enmDriverType.normal;
        //public bool needEMucom = false;

        public enmDriverType DriverType
        {
            get
            {
                return _DriverType;
            }
            set

            {
                //if (_DriverType == enmDriverType.normal && value == enmDriverType.DotNet && needNormalMucom)
                //{
                //    throw new MucException(msg.get("E0001"), row, col);
                //}

                //if (_DriverType == enmDriverType.E && needEMucom) 
                //    return;

                _DriverType = value;
            }
        }//mucomDotNET独自機能を使用したか否か

        public bool isIDE { get; set; } = false;
        public Point skipPoint { get; set; } = Point.Empty;
        public int skipChannel { get; set; } = -1;
        public bool isExtendFormat { get; set; } = false;
        public bool carriercorrection { get; set; } = false;
        public enmOpmClockMode opmclockmode { get; set; } = enmOpmClockMode.normal;

        public enum enmOpmClockMode
        {
            normal,X68000
        }
        public bool SSGExtend { get; set; } = false;

        public void Clear()
        {
            title = "";
            composer = "";
            author = "";
            comment = "";
            mucom88 = "";
            date = "";
            voice = "";
            for (int i = 0; i < 6; i++)
            {
                pcm[i] = "";
                pcmAt[i].Clear();
            }
            driver = "";
            invert = "";
            pcminvert = "";
            lines = 0;
            voiceData = null;
            pcmData = null;
            basSrc = new List<Tuple<int, string>>();
            fnSrc = "";
            workPath = "";
            fnDst = "";

            //バッファの作成
            bufPage = new AutoExtendList<MmlDatum>[5][][];
            for (int i = 0; i < 5; i++)
            {
                bufPage[i] = new AutoExtendList<MmlDatum>[11][];
                for (int j = 0; j < 11; j++)
                {
                    bufPage[i][j] = new AutoExtendList<MmlDatum>[10];
                    for (int pg = 0; pg < 10; pg++)
                    {
                        bufPage[i][j][pg] = new AutoExtendList<MmlDatum>();
                    };
                };
            }
            bufDst = bufPage[0][0][0];

            srcLinPtr = 0;
            srcCPtr = 0;
            bufMac = new AutoExtendList<int>();
            bufMacStack = new AutoExtendList<int>();
            bufLoopStack = new AutoExtendList<byte>();
            bufDefVoice = new AutoExtendList<int>();
            bufTitle = new AutoExtendList<int>();
            mmlVoiceDataWork = new AutoExtendList<byte>();

            DriverType = enmDriverType.DotNet;//.normal;
            //needNormalMucom = false;
            isIDE = false;
            isExtendFormat = false;
            carriercorrection = false;
            opmclockmode = enmOpmClockMode.normal;
            SSGExtend = false;
        }

    }
}