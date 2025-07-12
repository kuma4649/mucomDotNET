using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using mucomDotNET.Common;
using musicDriverInterface;

namespace mucomDotNET.Compiler
{
    public class Compiler : iCompiler
    {
        private byte[] srcBuf = null;
        private MUCInfo mucInfo = new MUCInfo();
        private work work = null;
        private Muc88 muc88 = null;
        private Msub msub = null;
        private expand expand = null;
        private smon smon = null;
        private byte[] voice;
        private byte[][] pcmdata=new byte[6][];
        private readonly List<Tuple<int, string>> basSrc = new List<Tuple<int, string>>();
        private readonly List<MmlDatum> dat = new List<MmlDatum>();

        public string OutFileName { get; set; }
        private Point skipPoint = Point.Empty;

        private iEncoding enc = null;
        private bool isIDE = false;

        public enum EnmMUCOMFileType
        {
            unknown,
            MUB,
            MUC
        }

        public Compiler(iEncoding enc = null)
        {
            this.enc = enc ?? myEncoding.Default;
        }

        public void Init()
        {
            //mucInfo = new MUCInfo();
            work = new work();
            muc88 = new Muc88(work, mucInfo, enc);
            msub = new Msub(work, mucInfo, enc);
            expand = new expand(work, mucInfo);
            smon = new smon(mucInfo);
            muc88.msub = msub;
            muc88.expand = expand;
            msub.muc88 = muc88;
            expand.msub = msub;
            expand.smon = smon;
            expand.muc88 = muc88;
        }

        public MmlDatum[] Compile(Stream sourceMML, Func<string, Stream> appendFileReaderCallback)
        {
            try
            {
                srcBuf = ReadAllBytes(sourceMML);
                mucInfo = GetMUCInfo(srcBuf);
                mucInfo.isIDE = isIDE;
                mucInfo.skipPoint = skipPoint;
                mucInfo.ErrSign = false;
                voice = null;
                for (int i = 0; i < 6; i++) pcmdata[i] = null;

                using (Stream vd = appendFileReaderCallback?.Invoke(string.IsNullOrEmpty(mucInfo.voice) ? "voice.dat" : mucInfo.voice))
                {
                    voice = ReadAllBytes(vd);
                }

                string[] pcmdefaultFilename = new string[]
                {
                    "mucompcm.bin",
                    "mucompcm_2nd.bin",
                    "mucompcm_3rd_B.bin",
                    "mucompcm_4th_B.bin",
                    "mucompcm_3rd_A.bin",
                    "mucompcm_4th_A.bin"
                };

                for (int i = 0; i < 6; i++)
                {
                    if (mucInfo.pcmAt[i].Count > 0)
                    {
                        pcmdata[i] = GetPackedPCM(i, mucInfo.pcmAt[i], appendFileReaderCallback);
                    }

                    if (pcmdata[i] == null)
                    {
                        using (Stream pd = appendFileReaderCallback?.Invoke(string.IsNullOrEmpty(mucInfo.pcm[i])
                        ? pcmdefaultFilename[i]
                        : mucInfo.pcm[i]))
                        {
                            pcmdata[i] = ReadAllBytes(pd);
                        }
                    }
                }

                mucInfo.lines = StoreBasicSource(srcBuf);
                mucInfo.voiceData = voice;
                mucInfo.pcmData = pcmdata[0];
                mucInfo.basSrc = basSrc;
                mucInfo.srcCPtr = 0;
                mucInfo.srcLinPtr = -1;
                //work.compilerInfo.jumpRow = -1;
                //work.compilerInfo.jumpCol = -1;
                if(!string.IsNullOrEmpty(mucInfo.artwork))
                {
                    string fn = mucInfo.artwork;
                    if (fn[0]=='"' && fn[fn.Length - 1] == '"')
                    {
                        fn = fn.Substring(1, fn.Length - 2);
                    }
                    using (Stream pd = appendFileReaderCallback?.Invoke(fn))
                    {
                        byte[] pic = ReadAllBytes(pd);
                        mucInfo.artwork= Convert.ToBase64String(pic);
                    }
                }

                //MUCOM88 初期化
                int ret = muc88.COMPIL();//vector 0xeea8

                //コンパイルエラー発生時は0以外が返る
                if (ret != 0)
                {
                    int errLine = muc88.GetErrorLine();
                    work.compilerInfo.errorList.Add(
                        new Tuple<int, int, string>(
                            mucInfo.row
                            , mucInfo.col
                            , string.Format(msg.get("E0100"), mucInfo.row, mucInfo.col)
                            ));
                    Log.WriteLine(LogLevel.ERROR, string.Format(msg.get("E0100"), mucInfo.row, mucInfo.col));
                    return null;
                }

                ret = SaveMub();
                if (ret == 0)
                {
                    return dat.ToArray();
                }
            }
            catch (MucException me)
            {
                if (work.compilerInfo == null) work.compilerInfo = new CompilerInfo();
                work.compilerInfo.errorList.Add(new Tuple<int, int, string>(-1, -1, me.Message));
                Log.WriteLine(LogLevel.ERROR, me.Message);
            }
            catch (Exception e)
            {
                if (work.compilerInfo == null) work.compilerInfo = new CompilerInfo();
                work.compilerInfo.errorList.Add(new Tuple<int, int, string>(-1, -1, e.Message));
                Log.WriteLine(LogLevel.ERROR, string.Format(
                    msg.get("E0000")
                    , e.Message
                    , e.StackTrace));
            }

            return null;
        }

        public bool Compile(Stream sourceMML, Stream destCompiledBin, Func<string, Stream> appendFileReaderCallback)
        {
            var dat = Compile(sourceMML, appendFileReaderCallback);
            if (dat == null)
            {
                return false;
            }
            foreach (MmlDatum md in dat)
            {
                if (md == null)
                {
                    destCompiledBin.WriteByte(0);
                }
                else
                {
                    destCompiledBin.WriteByte((byte)md.dat);
                }
            }
            return true;
        }

        /// <summary>
		/// ストリームから一括でバイナリを読み込む
		/// </summary>
		private byte[] ReadAllBytes(Stream stream)
        {
            if (stream == null) return null;

            var buf = new byte[8192];
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var r = stream.Read(buf, 0, buf.Length);
                    if (r < 1)
                    {
                        break;
                    }
                    ms.Write(buf, 0, r);
                }
                return ms.ToArray();
            }
        }

        public MUCInfo GetMUCInfo(byte[] buf)
        {
            if (CheckFileType(buf) != EnmMUCOMFileType.MUC)
            {
                throw new NotImplementedException();
            }

            List<Tuple<string, string>> tags = GetTagsFromMUC(buf);
            mucInfo.Clear();
            foreach (Tuple<string, string> tag in tags)
            {
                switch (tag.Item1)
                {
                    case "title":
                        mucInfo.title = tag.Item2;
                        break;
                    case "composer":
                        mucInfo.composer = tag.Item2;
                        break;
                    case "author":
                        mucInfo.author = tag.Item2;
                        break;
                    case "comment":
                        if (string.IsNullOrEmpty(mucInfo.comment))
                            mucInfo.comment = tag.Item2;
                        else
                            mucInfo.comment += "\r\n" + tag.Item2;
                        break;
                    case "mucom88":
                        mucInfo.mucom88 = tag.Item2;
                        break;
                    case "date":
                        mucInfo.date = tag.Item2;
                        break;
                    case "voice":
                        mucInfo.voice = tag.Item2;
                        break;
                    case "artwork":
                        mucInfo.artwork = tag.Item2;
                        break;
                    case "pcm":
                        mucInfo.pcm[0] = tag.Item2;
                        break;
                    case "pcm_2nd":
                        mucInfo.pcm[1] = tag.Item2;
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "pcm_3rd_b":
                        mucInfo.pcm[2] = tag.Item2;
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "pcm_4th_b":
                        mucInfo.pcm[3] = tag.Item2;
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "pcm_3rd_a":
                        mucInfo.pcm[4] = tag.Item2;
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "pcm_4th_a":
                        mucInfo.pcm[5] = tag.Item2;
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "@pcm":
                        mucInfo.pcmAt[0].Add(tag.Item2);
                        break;
                    case "@pcm_2nd":
                        mucInfo.pcmAt[1].Add(tag.Item2);
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "@pcm_3rd_b":
                        mucInfo.pcmAt[2].Add(tag.Item2);
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "@pcm_4th_b":
                        mucInfo.pcmAt[3].Add(tag.Item2);
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "@pcm_3rd_a":
                        mucInfo.pcmAt[4].Add(tag.Item2);
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "@pcm_4th_a":
                        mucInfo.pcmAt[5].Add(tag.Item2);
                        mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        break;
                    case "driver":
                        mucInfo.driver = tag.Item2;
                        if (mucInfo.driver.ToLower() == "mucomdotnet")
                        {
                            mucInfo.DriverType = MUCInfo.enmDriverType.DotNet;
                        }
                        else if (mucInfo.driver.ToLower() == "mucom88e")
                        {
                            mucInfo.DriverType = MUCInfo.enmDriverType.E;
                        }
                        else if (mucInfo.driver.ToLower() == "mucom88em")
                        {
                            mucInfo.DriverType = MUCInfo.enmDriverType.em;
                        }
                        else
                            mucInfo.DriverType = MUCInfo.enmDriverType.normal;
                        break;
                    case "invert":
                        mucInfo.invert = tag.Item2;
                        break;
                    case "pcminvert":
                        mucInfo.pcminvert = tag.Item2;
                        break;
                    case "carriercorrection":
                        string val = tag.Item2.ToLower().Trim();

                        mucInfo.carriercorrection = false;
                        if (val == "yes" || val == "y" || val == "1" || val == "true" || val == "t")
                        {
                            mucInfo.carriercorrection = true;
                        }
                        break;
                    case "opmclockmode":
                        string ocmval = tag.Item2.ToLower().Trim();

                        mucInfo.opmclockmode = MUCInfo.enmOpmClockMode.normal;
                        if (ocmval == "x68000" || ocmval == "x68k" || ocmval == "x68" || ocmval == "x" || ocmval == "40000" || ocmval == "x680x0")
                        {
                            mucInfo.opmclockmode = MUCInfo.enmOpmClockMode.X68000;
                        }
                        break;
                    case "ssgextend":
                        string ssgextval = tag.Item2.ToLower().Trim();

                        mucInfo.SSGExtend = false;
                        if (ssgextval == "on" || ssgextval == "yes" || ssgextval == "y" || ssgextval == "1" || ssgextval == "true" || ssgextval == "t")
                        {
                            mucInfo.SSGExtend = true;
                        }
                        break;
                    case "opmpanreverse":
                        string opmpanval = tag.Item2.ToLower().Trim();

                        mucInfo.opmpanreverse = false;
                        if (opmpanval == "on" || opmpanval == "yes" || opmpanval == "y" || opmpanval == "1" || opmpanval == "true" || opmpanval == "t")
                        {
                            mucInfo.opmpanreverse = true;
                        }
                        break;
                    case "opna1rhythmmute":
                        string rhythmmute = tag.Item2.ToLower().Trim();
                        mucInfo.opna1rhythmmute = 0;
                        if (rhythmmute.IndexOf('b') > -1) mucInfo.opna1rhythmmute |= 1;
                        if (rhythmmute.IndexOf('s') > -1) mucInfo.opna1rhythmmute |= 2;
                        if (rhythmmute.IndexOf('c') > -1) mucInfo.opna1rhythmmute |= 4;
                        if (rhythmmute.IndexOf('h') > -1) mucInfo.opna1rhythmmute |= 8;
                        if (rhythmmute.IndexOf('t') > -1) mucInfo.opna1rhythmmute |= 16;
                        if (rhythmmute.IndexOf('r') > -1) mucInfo.opna1rhythmmute |= 32;
                        break;
                    case "opna2rhythmmute":
                        string rhythmmute2 = tag.Item2.ToLower().Trim();
                        mucInfo.opna2rhythmmute = 0;
                        if (rhythmmute2.IndexOf('b') > -1) mucInfo.opna2rhythmmute |= 1;
                        if (rhythmmute2.IndexOf('s') > -1) mucInfo.opna2rhythmmute |= 2;
                        if (rhythmmute2.IndexOf('c') > -1) mucInfo.opna2rhythmmute |= 4;
                        if (rhythmmute2.IndexOf('h') > -1) mucInfo.opna2rhythmmute |= 8;
                        if (rhythmmute2.IndexOf('t') > -1) mucInfo.opna2rhythmmute |= 16;
                        if (rhythmmute2.IndexOf('r') > -1) mucInfo.opna2rhythmmute |= 32;
                        break;
                    case "opnb1adpcmamute":
                        string adpcmamute1 = tag.Item2.ToLower().Trim();
                        mucInfo.opnb1adpcmamute = 0;
                        if (adpcmamute1.IndexOf('1') > -1) mucInfo.opnb1adpcmamute |= 1;
                        if (adpcmamute1.IndexOf('2') > -1) mucInfo.opnb1adpcmamute |= 2;
                        if (adpcmamute1.IndexOf('3') > -1) mucInfo.opnb1adpcmamute |= 4;
                        if (adpcmamute1.IndexOf('4') > -1) mucInfo.opnb1adpcmamute |= 8;
                        if (adpcmamute1.IndexOf('5') > -1) mucInfo.opnb1adpcmamute |= 16;
                        if (adpcmamute1.IndexOf('6') > -1) mucInfo.opnb1adpcmamute |= 32;
                        break;
                    case "opnb2adpcmamute":
                        string adpcmamute2 = tag.Item2.ToLower().Trim();
                        mucInfo.opnb2adpcmamute = 0;
                        if (adpcmamute2.IndexOf('1') > -1) mucInfo.opnb2adpcmamute |= 1;
                        if (adpcmamute2.IndexOf('2') > -1) mucInfo.opnb2adpcmamute |= 2;
                        if (adpcmamute2.IndexOf('3') > -1) mucInfo.opnb2adpcmamute |= 4;
                        if (adpcmamute2.IndexOf('4') > -1) mucInfo.opnb2adpcmamute |= 8;
                        if (adpcmamute2.IndexOf('5') > -1) mucInfo.opnb2adpcmamute |= 16;
                        if (adpcmamute2.IndexOf('6') > -1) mucInfo.opnb2adpcmamute |= 32;
                        break;
                }
            }

            if (mucInfo.SSGExtend && mucInfo.DriverType != MUCInfo.enmDriverType.DotNet) mucInfo.SSGExtend = false;

            return mucInfo;
        }

        public CompilerInfo GetCompilerInfo()
        {
            return work.compilerInfo;
        }



        private EnmMUCOMFileType CheckFileType(byte[] buf)
        {
            if (buf == null || buf.Length < 4)
            {
                return EnmMUCOMFileType.unknown;
            }

            if (buf[0] == 0x4d
                && buf[1] == 0x55
                && buf[2] == 0x43
                && buf[3] == 0x38)
            {
                return EnmMUCOMFileType.MUB;
            }

            return EnmMUCOMFileType.MUC;
        }

        private List<Tuple<string, string>> tags=new List<Tuple<string, string>>();

        /// <summary>
        /// mucからタグのリストを抽出する
        /// </summary>
        /// <param name="buf">muc(バイト配列、実態はsjisのテキスト)</param>
        /// <returns>
        /// tupleのリスト。
        /// item1がタグ名。トリム、小文字化済み。
        /// item2が値。トリム済み。
        /// </returns>
        private List<Tuple<string, string>> GetTagsFromMUC(byte[] buf)
        {
            var text = enc.GetStringFromSjisArray(buf)  // Encoding.GetEncoding("shift_jis").GetString(buf)
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.IndexOf("#") == 0);
            if (tags != null) tags.Clear();
            else tags = new List<Tuple<string, string>>();

            foreach (string v in text)
            {
                try
                {
                    int p = v.IndexOf(' ');
                    string tag = "";
                    string ele = "";
                    if (p >= 0)
                    {
                        tag = v.Substring(1, p).Trim().ToLower();
                        ele = v.Substring(p + 1).Trim();
                        Tuple<string, string> item = new Tuple<string, string>(tag, ele);
                        tags.Add(item);
                    }
                }
                catch { }
            }

            return tags;
        }

        private int StoreBasicSource(byte[] buf)
        {
            int line = 0;
            var text = enc.GetStringFromSjisArray(buf) //Encoding.GetEncoding("shift_jis").GetString(buf)
                .Split(new string[] { "\r\n" }, StringSplitOptions.None);

            basSrc.Clear();
            foreach (string txt in text)
            {
                Tuple<int, string> d = new Tuple<int, string>(line + 1, txt);
                basSrc.Add(d);

                line++;
            }

            return line;
        }

        private int SaveMub()
        {
            try
            {
                string msg;
                byte[] textLineBuf = new byte[80];
                for (int i = 0; i < work.title.Length; i++) textLineBuf[i] = (byte)work.title[i];

                //int fmvoice = work.OTONUM[0];//.fmvoiceCnt;
                int pcmflag = 0;
                int maxcount = 0;
                int mubsize = 0;
                string strTcount = "";
                string strLcount = "";
                string strBcount = "";
                
                work.compilerInfo.totalCount = new List<int>();
                work.compilerInfo.loopCount = new List<int>();
                work.compilerInfo.bufferCount = new List<int>();

                bool isExtendFormat = mucInfo.isExtendFormat;// false;
                int bufferLength = 0;
                for (int i = 0; i < (isExtendFormat ? work.MAXChips : 1); i++)
                {
                    string Tc = "";
                    string Lc = "";
                    string Bc = "";
                    for (int j = 0; j < work.MAXCH; j++)
                    {
                        if (!isExtendFormat)
                        {
                            work.compilerInfo.formatType = "mub";
                            if (work.lcnt[i][j][0] != 0) { work.lcnt[i][j][0] = work.tcnt[i][j][0] - (work.lcnt[i][j][0] - 1); }
                            if (work.tcnt[i][j][0] > maxcount) maxcount = work.tcnt[i][j][0];
                            strTcount += string.Format("{0}:{1:d05} ", work.GetTrackCharacterFromChipValue(i, j), work.tcnt[i][j][0]);
                            work.compilerInfo.totalCount.Add(work.tcnt[i][j][0]);
                            strLcount += string.Format("{0}:{1:d05} ", work.GetTrackCharacterFromChipValue(i, j), work.lcnt[i][j][0]);
                            work.compilerInfo.loopCount.Add(work.lcnt[i][j][0]);
                            strBcount += string.Format("{0}:${1:x04} ", work.GetTrackCharacterFromChipValue(i, j), work.bufCount[i][j][0]);
                            work.compilerInfo.bufferCount.Add(work.bufCount[i][j][0]);
                            if (work.bufCount[i][j][0] > 0xffff)
                            {
                                throw new MucException(string.Format(Common.msg.get("E0700"), work.GetTrackCharacterFromChipValue(i, j), work.bufCount[i]));
                            }
                        }
                        else
                        {
                            work.compilerInfo.formatType = "mupb";
                            int n = 0;
                            for (int pg = 0; pg < 10; pg++)
                            {
                                if (work.lcnt[i][j][pg] != 0) { work.lcnt[i][j][pg] = work.tcnt[i][j][pg] - (work.lcnt[i][j][pg] - 1); }
                                if (work.tcnt[i][j][pg] > maxcount) maxcount = work.tcnt[i][j][pg];
                                work.compilerInfo.totalCount.Add(work.tcnt[i][j][pg]);
                                work.compilerInfo.loopCount.Add(work.lcnt[i][j][pg]);
                                if (work.bufCount[i][j][pg] > 1)
                                {
                                    Tc += string.Format("{0}{1}:{2:d05} ", work.GetTrackCharacterFromChipValue(i, j), pg, work.tcnt[i][j][pg]);
                                    Lc += string.Format("{0}{1}:{2:d05} ", work.GetTrackCharacterFromChipValue(i, j), pg, work.lcnt[i][j][pg]);
                                    Bc += string.Format("{0}{1}:${2:x04} ", work.GetTrackCharacterFromChipValue(i, j), pg, work.bufCount[i][j][pg]);
                                    bufferLength = work.bufCount[i][j][pg];
                                    n++;
                                    if (n == 10)
                                    {
                                        n = 0;
                                        Tc += "\r\n";
                                        Lc += "\r\n";
                                        Bc += "\r\n";
                                    }
                                    //if (j != 0) usePageFunction = true;
                                }
                            }
                            for (int pg = 0; pg < 10; pg++)
                            {
                                work.compilerInfo.bufferCount.Add(work.bufCount[i][j][pg]);
                                if (work.bufCount[i][j][pg] > 0xffff)
                                {
                                    throw new MucException(string.Format(Common.msg.get("E0700")
                                        , work.GetTrackCharacterFromChipValue(i, j).ToString() + pg.ToString()
                                        , work.bufCount[i][j]));
                                }
                            }
                        }
                    }

                    if (!isExtendFormat)
                    {
                        if (strTcount.Length > 2) strTcount += "\r\n";
                        if (strLcount.Length > 2) strLcount += "\r\n";
                        if (strBcount.Length > 2) strBcount += "\r\n";
                    }
                    else
                    {
                        strTcount += Tc;
                        if (Tc.Length > 2) strTcount += "\r\n";
                        strLcount += Lc;
                        if (Lc.Length > 2) strLcount += "\r\n";
                        strBcount += Bc;
                        if (Bc.Length > 2) strBcount += "\r\n";
                    }

                }

                work.compilerInfo.jumpClock = work.JCLOCK;
                work.compilerInfo.jumpChannel = work.JCHCOM;

                if (work.pcmFlag == 0) pcmflag = 2;
                msg = enc.GetStringFromSjisArray(textLineBuf, 31, 4);//Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 31, 4);
                int start = Convert.ToInt32(msg, 16);
                msg = enc.GetStringFromSjisArray(textLineBuf, 41, 4);//Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 41, 4);
                int length = mucInfo.bufDst.Count;
                if (isExtendFormat) length = bufferLength;
                mubsize = length;

                Log.WriteLine(LogLevel.INFO, "- mucom.NET -");
                Log.WriteLine(LogLevel.INFO, "[ Total count ]\r\n" + strTcount);
                Log.WriteLine(LogLevel.INFO, "[ Loop count  ]\r\n"+strLcount);
                if (isExtendFormat) 
                    Log.WriteLine(LogLevel.INFO, "[ Buffer count  ]\r\n" + strBcount);
                Log.WriteLine(LogLevel.INFO, "");
                Log.WriteLine(LogLevel.INFO, string.Format("#mucom type    : {0}", mucInfo.DriverType));
                Log.WriteLine(LogLevel.INFO, string.Format("#MUB Format    : {0}", isExtendFormat ? "Extend" : "Normal"));
                Log.WriteLine(LogLevel.INFO, "#Used FM voice : ");
                Log.WriteLine(LogLevel.INFO, string.Format("#      @ count : {0} ", work.usedFMVoiceNumber.Count));//, work.OTONUM[0], work.OTONUM[1], work.OTONUM[2], work.OTONUM[3], work.OTONUM[4]);
                List<int> usedFMVoiceNumberList=work.usedFMVoiceNumber.ToList();
                usedFMVoiceNumberList.Sort();
                Log.WriteLine(LogLevel.INFO, string.Format("#      @ list  : {0} ", String.Join(" ", usedFMVoiceNumberList)));
                Log.WriteLine(LogLevel.INFO, string.Format("#Data Buffer   : ${0:x05} - ${1:x05} (${2:x05})", start, start + length - 1, length));
                Log.WriteLine(LogLevel.INFO, string.Format("#Max Count     : {0}", maxcount));
                Log.WriteLine(LogLevel.INFO, string.Format("#MML Lines     : {0}", mucInfo.lines));
                Log.WriteLine(LogLevel.INFO, string.Format("#Data          : {0}", mubsize));

                return SaveMusic(length, pcmflag, isExtendFormat);
            }
            catch (MucException me)
            {
                work.compilerInfo.errorList.Add(new Tuple<int, int, string>(-1, -1, me.Message));
                Log.WriteLine(LogLevel.ERROR, me.Message);
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        //private int SaveMusic(string fname, ushort start, ushort length, int option)
        private int SaveMusic(int length, int option,bool isExtendFormat)
        {
            //		音楽データファイルを出力(コンパイルが必要)
            //		option : 1   = #タグによるvoice設定を無視
            //		         2   = PCM埋め込みをスキップ
            //		(戻り値が0以外の場合はエラー)
            //      usePageFunc : ページ機能を使用しているか
            //

            if (isExtendFormat)
            {
                return SaveMusicExtendFormat(length, option);
            }

            int footsize;
            footsize = 1;//かならず1以上

            int pcmsize = (pcmdata[0] == null) ? 0 : pcmdata[0].Length;
            bool pcmuse = ((option & 2) == 0);
            pcmdata[0] = (!pcmuse ? null : pcmdata[0]);
            int pcmptr = (!pcmuse ? 0 : (32 + length + footsize));
            pcmsize = (!pcmuse ? 0 : pcmsize);
            if (pcmuse)
            {
                if (pcmdata[0] == null || pcmsize == 0)
                {
                    pcmuse = false;
                    pcmdata[0] = null;
                    pcmptr = 0;
                    pcmsize = 0;
                }
            }

            int dataOffset = 0x50;
            int dataSize = length;
            int tagOffset = length + 0x50;

            dat.Clear();

            dat.Add(new MmlDatum(0x4d));// M
            dat.Add(new MmlDatum(0x55));// U
            dat.Add(new MmlDatum(0x42));// B
            dat.Add(new MmlDatum(0x38));// 8

            dat.Add(new MmlDatum((byte)dataOffset));
            dat.Add(new MmlDatum((byte)(dataOffset >> 8)));
            dat.Add(new MmlDatum((byte)(dataOffset >> 16)));
            dat.Add(new MmlDatum((byte)(dataOffset >> 24)));

            dat.Add(new MmlDatum((byte)dataSize));
            dat.Add(new MmlDatum((byte)(dataSize >> 8)));
            dat.Add(new MmlDatum((byte)(dataSize >> 16)));
            dat.Add(new MmlDatum((byte)(dataSize >> 24)));

            dat.Add(new MmlDatum((byte)tagOffset));
            dat.Add(new MmlDatum((byte)(tagOffset >> 8)));
            dat.Add(new MmlDatum((byte)(tagOffset >> 16)));
            dat.Add(new MmlDatum((byte)(tagOffset >> 24)));

            dat.Add(new MmlDatum(0));//tagdata size(dummy)
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum((byte)pcmptr));//pcmdata ptr(32bit)
            dat.Add(new MmlDatum((byte)(pcmptr >> 8)));
            dat.Add(new MmlDatum((byte)(pcmptr >> 16)));
            dat.Add(new MmlDatum((byte)(pcmptr >> 24)));

            dat.Add(new MmlDatum((byte)pcmsize));//pcmdata size(32bit)
            dat.Add(new MmlDatum((byte)(pcmsize >> 8)));
            dat.Add(new MmlDatum((byte)(pcmsize >> 16)));
            dat.Add(new MmlDatum((byte)(pcmsize >> 24)));

            dat.Add(new MmlDatum((byte)work.JCLOCK));// JCLOCKの値(Jコマンドのタグ位置)
            dat.Add(new MmlDatum((byte)(work.JCLOCK >> 8)));

            dat.Add(new MmlDatum((byte)work.JPLINE));//jump line number
            dat.Add(new MmlDatum((byte)(work.JPLINE >> 8)));

            dat.Add(new MmlDatum(0));//ext_flags(?)
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum(1));//ext_system(?)

            dat.Add(new MmlDatum(2));//ext_target(?)

            dat.Add(new MmlDatum(11));//ext_channel_num
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum((byte)work.OTONUM[0]));//ext_fmvoice_num
            dat.Add(new MmlDatum((byte)(work.OTONUM[0] >> 8)));

            dat.Add(new MmlDatum(0));//ext_player(?)
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum(0));//pad1
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));
            dat.Add(new MmlDatum(0));

            for (int i = 0; i < 32; i++)
            {
                dat.Add(new MmlDatum((byte)mucInfo.bufDefVoice.Get(i)));
            }

            work.compilerInfo.jumpRow = -1;
            work.compilerInfo.jumpCol = -1;
            if (work.JPLINE >= 0)
            {
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump count [{0}]. channelNumber[{1}]", work.JCLOCK, work.JCHCOM[0]));
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump line [row:{0} col:{1}].", work.JPLINE, work.JPCOL));
                work.compilerInfo.jumpRow = work.JPLINE;
                work.compilerInfo.jumpCol = work.JPCOL;
            }

            for (int i = 0; i < length; i++) dat.Add(mucInfo.bufDst.Get(i));

            dat[dataOffset + 0] = new MmlDatum(0);//バイナリに含まれる曲データ数-1
            dat[dataOffset + 1] = new MmlDatum((byte)work.OTODAT);
            dat[dataOffset + 2] = new MmlDatum((byte)(work.OTODAT >> 8));
            dat[dataOffset + 3] = new MmlDatum((byte)work.ENDADR);
            dat[dataOffset + 4] = new MmlDatum((byte)(work.ENDADR >> 8));
            if (dat[dataOffset + 5] == null)
            {
                dat[dataOffset + 5] = new MmlDatum(0);//テンポコマンド(タイマーB)を未設定時nullのままになってしまうので、とりあえず値をセット
            }

            footsize = 0;

            bool useDriverTAG = false;
            if (tags != null)
            {
                foreach (Tuple<string, string> tag in tags)
                {
                    if (tag.Item1 == "driver") useDriverTAG = true;
                }
            }

            //データサイズが64k超えていたらdotnet確定
            if (work.ENDADR - work.MU_NUM > 0xffff)
            {
                if(mucInfo.DriverType!= MUCInfo.enmDriverType.DotNet)
                {
                    //TBD
                    return 1;
                }
            }

            if(!useDriverTAG && mucInfo.DriverType== MUCInfo.enmDriverType.DotNet)
            {
                if (tags == null) tags = new List<Tuple<string, string>>();
                tags.Add(new Tuple<string, string>("driver", MUCInfo.DotNET));
            }

            if (tags != null)
            {
                foreach (Tuple<string, string> tag in tags)
                {
                    if (tag.Item1!=null && tag.Item1.Length>0 && tag.Item1[0] == '*') continue;
                    if (string.IsNullOrEmpty(tag.Item1) && !string.IsNullOrEmpty(tag.Item2) && tag.Item2.Trim()[0] == '*') continue;
                    byte[] b = enc.GetSjisArrayFromString(string.Format("#{0} {1}\r\n", tag.Item1, tag.Item2));
                    footsize += b.Length;
                    foreach (byte bd in b) dat.Add(new MmlDatum(bd));
                }
            }

            if (footsize > 0)
            {
                dat.Add(new MmlDatum(0));
                dat.Add(new MmlDatum(0));
                dat.Add(new MmlDatum(0));
                dat.Add(new MmlDatum(0));
                footsize += 4;

                dat[16] = new MmlDatum((byte)footsize);//tagdata size(32bit)
                dat[17] = new MmlDatum((byte)(footsize >> 8));
                dat[18] = new MmlDatum((byte)(footsize >> 16));
                dat[19] = new MmlDatum((byte)(footsize >> 24));
            }
            else
            {
                tags = null;
            }

            if (tags == null)
            {
                //クリア
                for (int i = 0; i < 8; i++)
                {
                    dat[12 + i] = new MmlDatum(0);
                }
            }

            if (pcmuse)
            {
                for (int i = 0; i < pcmsize; i++) dat.Add(new MmlDatum(pcmdata[0][i]));
                if (pcmsize > 0)
                {
                    pcmptr = 16 * 3 + 32 + length + footsize;
                    dat[20] = new MmlDatum((byte)pcmptr);//pcmdata size(32bit)
                    dat[21] = new MmlDatum((byte)(pcmptr >> 8));
                    dat[22] = new MmlDatum((byte)(pcmptr >> 16));
                    dat[23] = new MmlDatum((byte)(pcmptr >> 24));
                }
            }

            return 0;
        }

        private int SaveMusicExtendFormat(int length, int option)
        {

            dat.Clear();

            //固定長ヘッダー情報　作成

            dat.Add(new MmlDatum(0x6d));// m
            dat.Add(new MmlDatum(0x75));// u
            dat.Add(new MmlDatum(0x50));// P
            dat.Add(new MmlDatum(0x62));// b

            dat.Add(new MmlDatum(0x30));// 0
            dat.Add(new MmlDatum(0x31));// 1
            dat.Add(new MmlDatum(0x30));// 0
            dat.Add(new MmlDatum(0x30));// 0

            dat.Add(new MmlDatum(0x05));// 可変長ヘッダー情報の数。
            dat.Add(new MmlDatum(work.MAXChips));// 使用する音源の数(0～)

            int n = work.MAXCH * work.MAXChips;// 使用するパートの総数(0～)
            dat.Add(new MmlDatum((byte)n));
            dat.Add(new MmlDatum((byte)(n >> 8)));

            n = 0;
            for (int i = 0; i < work.MAXChips; i++)
            {
                for (int j = 0; j < work.MAXCH; j++)
                {
                    for (int k = 0; k < work.MAXPG; k++)
                    {
                        //if (work.bufCount[i][j][k] > 1)
                            n++;
                    }
                }
            }

            dat.Add(new MmlDatum((byte)n));// 使用するページの総数(0～)
            dat.Add(new MmlDatum((byte)(n>>8)));

            int instSets = 0;
            for (int i = 0; i < work.MAXChips; i++) instSets += work.OTONUM[i];
            if (mucInfo.ssgVoice.Count > 0) instSets = 2;
            else instSets = instSets > 0 ? 1 : 0;

            dat.Add(new MmlDatum(instSets));// 使用するInstrumentセットの総数(0～)
            dat.Add(new MmlDatum(0x00));

            bool pcmuse = ((option & 2) == 0);
            int[] pcmsize = new int[6];
            int m = 0;
            for (int k = 0; k < 6; k++)
            {
                pcmsize[k] = (pcmdata[k] == null) ? 0 : pcmdata[k].Length;
                pcmdata[k] = (!pcmuse ? null : pcmdata[k]);
                pcmsize[k] = (!pcmuse ? 0 : pcmsize[k]);

                if (pcmdata[k] == null || pcmsize[k] == 0)
                {
                    pcmdata[k] = null;
                    pcmsize[k] = 0;
                    m++;
                }
            }
            if (m == 6) pcmuse = false;

            dat.Add(new MmlDatum(pcmuse ? 6 : 0));// 使用するPCMセットの総数(0～)
            dat.Add(new MmlDatum(0x00));

            dat.Add(new MmlDatum(0x00));// 曲情報への絶対アドレス
            dat.Add(new MmlDatum(0x00));// 
            dat.Add(new MmlDatum(0x00));// 
            dat.Add(new MmlDatum(0x00));// 

            dat.Add(new MmlDatum(0x00));// 曲情報のサイズ
            dat.Add(new MmlDatum(0x00));// 
            dat.Add(new MmlDatum(0x00));// 
            dat.Add(new MmlDatum(0x00));// 

            dat.Add(new MmlDatum((byte)work.JCLOCK));// JCLOCKの値(Jコマンドのタグ位置)
            dat.Add(new MmlDatum((byte)(work.JCLOCK >> 8)));
            dat.Add(new MmlDatum((byte)(work.JCLOCK >> 16)));
            dat.Add(new MmlDatum((byte)(work.JCLOCK >> 24)));

            dat.Add(new MmlDatum((byte)work.JPLINE));//jump line number
            dat.Add(new MmlDatum((byte)(work.JPLINE >> 8)));
            dat.Add(new MmlDatum((byte)(work.JPLINE >> 16)));
            dat.Add(new MmlDatum((byte)(work.JPLINE >> 24)));

            work.compilerInfo.jumpRow = -1;
            work.compilerInfo.jumpCol = -1;
            if (work.JPLINE >= 0)
            {
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump count [{0}]. channelNumber[{1}]", work.JCLOCK, work.JCHCOM[0]));
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump line [row:{0} col:{1}].", work.JPLINE, work.JPCOL));
                work.compilerInfo.jumpRow = work.JPLINE;
                work.compilerInfo.jumpCol = work.JPCOL;
            }


            //可変長ヘッダー情報


            //Chip Define division.

            int pcmI = 0;
            for (int chipI = 0; chipI < work.MAXChips; chipI++)
            {
                dat.Add(new MmlDatum((byte)(chipI >> 0)));// Chip Index
                dat.Add(new MmlDatum((byte)(chipI >> 8)));// 

                int opmIdentifyNumber = 0x0000_0030;
                int opnaIdentifyNumber = 0x0000_0048;
                int opnbIdentifyNumber = 0x0000_004c;
                int opmMasterClock = 3579545;
                int opnaMasterClock = 7987200;
                int opnbMasterClock = 8000000;

                if (chipI < 2)
                {
                    dat.Add(new MmlDatum((byte)(opnaIdentifyNumber >> 0)));// Chip Identify number
                    dat.Add(new MmlDatum((byte)(opnaIdentifyNumber >> 8)));// 
                    dat.Add(new MmlDatum((byte)(opnaIdentifyNumber >> 16)));// 
                    dat.Add(new MmlDatum((byte)(opnaIdentifyNumber >> 24)));// 

                    dat.Add(new MmlDatum((byte)opnaMasterClock));// Chip Clock
                    dat.Add(new MmlDatum((byte)(opnaMasterClock >> 8)));
                    dat.Add(new MmlDatum((byte)(opnaMasterClock >> 16)));
                    dat.Add(new MmlDatum((byte)(opnaMasterClock >> 24)));
                }
                else if(chipI<4)
                {
                    dat.Add(new MmlDatum((byte)(opnbIdentifyNumber >> 0)));// Chip Identify number
                    dat.Add(new MmlDatum((byte)(opnbIdentifyNumber >> 8)));// 
                    dat.Add(new MmlDatum((byte)(opnbIdentifyNumber >> 16)));// 
                    dat.Add(new MmlDatum((byte)(opnbIdentifyNumber >> 24)));// 

                    dat.Add(new MmlDatum((byte)opnbMasterClock));// Chip Clock
                    dat.Add(new MmlDatum((byte)(opnbMasterClock >> 8)));
                    dat.Add(new MmlDatum((byte)(opnbMasterClock >> 16)));
                    dat.Add(new MmlDatum((byte)(opnbMasterClock >> 24)));
                }
                else
                {
                    dat.Add(new MmlDatum((byte)(opmIdentifyNumber >> 0)));// Chip Identify number
                    dat.Add(new MmlDatum((byte)(opmIdentifyNumber >> 8)));// 
                    dat.Add(new MmlDatum((byte)(opmIdentifyNumber >> 16)));// 
                    dat.Add(new MmlDatum((byte)(opmIdentifyNumber >> 24)));// 

                    dat.Add(new MmlDatum((byte)opmMasterClock));// Chip Clock
                    dat.Add(new MmlDatum((byte)(opmMasterClock >> 8)));
                    dat.Add(new MmlDatum((byte)(opmMasterClock >> 16)));
                    dat.Add(new MmlDatum((byte)(opmMasterClock >> 24)));
                }

                dat.Add(new MmlDatum(0x00));// Chip Option
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 

                dat.Add(new MmlDatum(0x01));// Heart Beat (1:OPNA Timer)
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 

                dat.Add(new MmlDatum(0x00));// Heart Beat2 (0:Unuse)
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 
                dat.Add(new MmlDatum(0x00));// 

                dat.Add(new MmlDatum(work.MAXCH));//part count 

                n = work.OTONUM[chipI] > 0 ? 1 : 0;
                dat.Add(new MmlDatum(n));// 使用するInstrumentセットの総数(0～)

                for (int i = 0; i < n; i++)
                {
                    dat.Add(new MmlDatum(0x00));// この音源Chipで使用するInstrumentセットの番号。上記パラメータの個数だけ繰り返す。
                    dat.Add(new MmlDatum(0x00));
                }

                n = pcmuse ? (chipI < 2 ? 1 : (chipI < 4 ? 2 : 0)) : 0;
                dat.Add(new MmlDatum(n));// この音源Chipで使用するPCMセットの個数
                for (int i = 0; i < n; i++)
                {
                    dat.Add(new MmlDatum((byte)pcmI));// この音源Chipで使用するPCMセットの番号。上記パラメータの個数だけ繰り返す。
                    dat.Add(new MmlDatum((byte)(pcmI >> 8)));
                    pcmI++;
                }
            }

            //Part division.

            for (int i = 0; i < work.MAXChips; i++)
            {
                for (int j = 0; j < work.MAXCH; j++)
                {
                    n = 0;
                    for (int pg = 0; pg < work.MAXPG; pg++) 
                        //if (work.bufCount[i][j][pg] > 1) 
                            n++;
                    dat.Add(new MmlDatum(n));//ページの数(0～)
                }
            }

            //Page division.

            for (int i = 0; i < work.MAXChips;i++)
                for (int j = 0; j < work.MAXCH;j++)
                    for (int pg = 0; pg < work.MAXPG; pg++)
                    {
                        //if (work.bufCount[i][j][pg] < 2) continue;

                        n = work.bufCount[i][j][pg];
                        dat.Add(new MmlDatum((byte)n));// ページの大きさ(0～)
                        dat.Add(new MmlDatum((byte)(n >> 8)));
                        dat.Add(new MmlDatum((byte)(n >> 16)));
                        dat.Add(new MmlDatum((byte)(n >> 24)));
                        n = work.loopPoint[i][j][pg];
                        dat.Add(new MmlDatum((byte)n));// ページのループポイント(0～)
                        dat.Add(new MmlDatum((byte)(n >> 8)));
                        dat.Add(new MmlDatum((byte)(n >> 16)));
                        dat.Add(new MmlDatum((byte)(n >> 24)));
                    }


            //Instrument set division.

            // 使用するInstrumentセットの総数(0～)
            if (instSets > 0)//FM の音色を使用する場合は1(但しSSG波形を使用している場合は、FMを使用していなくとも定義する)
            {
                dat.Add(new MmlDatum((byte)mucInfo.bufUseVoice.Count));
                dat.Add(new MmlDatum((byte)(mucInfo.bufUseVoice.Count >> 8)));
                dat.Add(new MmlDatum((byte)(mucInfo.bufUseVoice.Count >> 16)));
                dat.Add(new MmlDatum((byte)(mucInfo.bufUseVoice.Count >> 24)));
            }
            if (instSets == 2)//SSG の波形を使用する場合は2
            {
                int ssgVoiceSize = mucInfo.ssgVoice.Count * 65;//65 : 64(dataSize) + 1(音色番号)
                dat.Add(new MmlDatum((byte)ssgVoiceSize));
                dat.Add(new MmlDatum((byte)(ssgVoiceSize >> 8)));
                dat.Add(new MmlDatum((byte)(ssgVoiceSize >> 16)));
                dat.Add(new MmlDatum((byte)(ssgVoiceSize >> 24)));
            }


            //PCM set division.

            if (pcmuse)
            {
                for (int i = 0; i < pcmI; i++)
                {
                    dat.Add(new MmlDatum((byte)pcmsize[i]));
                    dat.Add(new MmlDatum((byte)(pcmsize[i] >> 8)));
                    dat.Add(new MmlDatum((byte)(pcmsize[i] >> 16)));
                    dat.Add(new MmlDatum((byte)(pcmsize[i] >> 24)));
                }
            }



            //ページデータ出力

            for (int i = 0; i < work.MAXChips; i++)
                for (int j = 0; j < work.MAXCH; j++)
                    for (int pg = 0; pg < work.MAXPG; pg++)
                    {
                        //if (work.bufCount[i][j][pg] < 2) continue;
                        for (int p = 0; p < work.bufCount[i][j][pg]; p++)
                        {
                            dat.Add(mucInfo.bufPage[i][j][pg].Get(p));
                        }
                    }



            //Instrumentデータ出力

            if (instSets > 0)
            {
                for (int i = 0; i < mucInfo.bufUseVoice.Count; i++)
                {
                    dat.Add(mucInfo.bufUseVoice.Get(i));
                }
            }
            if (instSets == 2)
            {
                foreach(int key in mucInfo.ssgVoice.Keys)
                {
                    dat.Add(new MmlDatum((byte)key));
                    foreach(byte d in mucInfo.ssgVoice[key])
                    {
                        dat.Add(new MmlDatum((byte)d));
                    }
                }
            }



            //PCMデータ出力

            if (pcmuse)
            {
                for (int i = 0; i < pcmI; i++)
                    for (int j = 0; j < pcmsize[i]; j++) dat.Add(new MmlDatum(pcmdata[i][j]));
            }



            //曲情報出力

            int infoAdr = dat.Count;
            dat[0x12] = new MmlDatum((byte)infoAdr);
            dat[0x13] = new MmlDatum((byte)(infoAdr >> 8));
            dat[0x14] = new MmlDatum((byte)(infoAdr >> 16));
            dat[0x15] = new MmlDatum((byte)(infoAdr >> 24));

            bool useDriverTAG = false;
            if (tags != null)
            {
                foreach (Tuple<string, string> tag in tags)
                {
                    if (tag.Item1 == "driver") useDriverTAG = true;
                }
            }

            if (!useDriverTAG && mucInfo.DriverType == MUCInfo.enmDriverType.DotNet)
            {
                if (tags == null) tags = new List<Tuple<string, string>>();
                tags.Add(new Tuple<string, string>("driver", MUCInfo.DotNET));
            }

            if (tags != null)
            {
                int tagsize = 0;
                foreach (Tuple<string, string> tag in tags)
                {
                    if (tag.Item1 != null && tag.Item1.Length > 0 && tag.Item1[0] == '*') continue;
                    byte[] b;
                    if (tag.Item1 == "artwork")
                    {
                        b = enc.GetSjisArrayFromString(string.Format("#{0} {1}\r\n", tag.Item1, mucInfo.artwork));
                    }
                    else
                    {
                        b = enc.GetSjisArrayFromString(string.Format("#{0} {1}\r\n", tag.Item1, tag.Item2));
                    }
                    tagsize += b.Length;
                    foreach (byte bd in b) dat.Add(new MmlDatum(bd));
                }

                dat[0x16] = new MmlDatum((byte)tagsize);
                dat[0x17] = new MmlDatum((byte)(tagsize >> 8));
                dat[0x18] = new MmlDatum((byte)(tagsize >> 16));
                dat[0x19] = new MmlDatum((byte)(tagsize >> 24));

            }



            return 0;
        }

        public void SetCompileSwitch(params object[] param)
        {
            this.isIDE = false;
            this.skipPoint = Point.Empty;

            if (param == null) return;

            foreach (object prm in param)
            {
                if (!(prm is string)) continue;

                //IDEフラグオン
                if ((string)prm == "IDE")
                {
                    this.isIDE = true;
                }

                //スキップ再生指定
                if (((string)prm).IndexOf("SkipPoint=") == 0)
                {
                    try
                    {
                        string[] p = ((string)prm).Split('=')[1].Split(':');
                        //if (p.Length != 2) continue;
                        //if (p[0].Length < 2 || p[1].Length < 2) continue;
                        //if (p[0][0] != 'R' || p[1][0] != 'C') continue;
                        int r = int.Parse(p[0].Substring(1));
                        int c = int.Parse(p[1].Substring(1));
                        this.skipPoint = new Point(c, r);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

        }

        public GD3Tag GetGD3TagInfo(byte[] srcBuf)
        {
            List<Tuple<string, string>> tags = GetTagsFromMUC(srcBuf);

            GD3Tag gt = new GD3Tag();

            foreach (Tuple<string, string> tag in tags)
            {
                switch (tag.Item1)
                {
                    case "title":
                        addItemAry(gt, enmTag.Title, tag.Item2);
                        addItemAry(gt, enmTag.TitleJ, tag.Item2);
                        break;
                    case "composer":
                        addItemAry(gt, enmTag.Composer, tag.Item2);
                        addItemAry(gt, enmTag.ComposerJ, tag.Item2);
                        break;
                    case "author":
                        addItemAry(gt, enmTag.Artist, tag.Item2);
                        addItemAry(gt, enmTag.ArtistJ, tag.Item2);
                        break;
                    case "comment":
                        addItemAry(gt, enmTag.Note, tag.Item2);
                        break;
                    case "mucom88":
                        addItemAry(gt, enmTag.RequestDriverVersion, tag.Item2);
                        break;
                    case "date":
                        addItemAry(gt, enmTag.ReleaseDate, tag.Item2);
                        break;
                    case "driver":
                        addItemAry(gt, enmTag.DriverName, tag.Item2);
                        break;
                }
            }

            return gt;
        }

        private static void addItemAry(GD3Tag gt,enmTag tag, string item)
        {
            if (!gt.dicItem.ContainsKey(tag))
                gt.dicItem.Add(tag, new string[] { item });
            else
            {
                string[] dmy = gt.dicItem[tag];
                Array.Resize(ref dmy, dmy.Length + 1);
                dmy[dmy.Length - 1] = item;
                gt.dicItem[tag] = dmy;
            }
        }

        private byte[] GetPackedPCM(int i, List<string> list, Func<string, Stream> appendFileReaderCallback)
        {
            PCMTool.AdpcmMaker adpcmMaker = new PCMTool.AdpcmMaker(i, list, appendFileReaderCallback);
            return adpcmMaker.Make();
        }

    }
}
