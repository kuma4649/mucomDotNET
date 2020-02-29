using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using mucomDotNET.Common;
using musicDriverInterface;

namespace mucomDotNET.Compiler
{
    public class Compiler : iCompiler
    {
        public string OutFileName { get; set; }
        private iEncoding enc = null;

        public enum EnmMUCOMFileType
        {
            unknown,
            MUB,
            MUC
        }

        public Compiler(iEncoding enc)
        {
            this.enc = enc;
        }

        public void Init()
        {
            muc88 = new Muc88(mucInfo, enc);
            msub = new Msub(mucInfo, enc);
            expand = new expand(mucInfo);
            smon = new smon(mucInfo);
            muc88.msub = msub;
            muc88.expand = expand;
            msub.muc88 = muc88;
            expand.msub = msub;
            expand.smon = smon;
            expand.muc88 = muc88;
        }

        public byte[] Start(string arg, Action<string> dispMessage)
        {
            MmlDatum[] ma = StartToMmlData(arg, dispMessage);
            if (ma == null || ma.Length < 1) return null;

            List<byte> ba = new List<byte>();
            foreach (MmlDatum d in dat)
            {
                ba.Add((byte)d.dat);
            }
            return ba.ToArray();
        }

        public MmlDatum[] StartToMmlData(string srcFn, Action<string> dispMessage)
        {
            workPath = Path.GetDirectoryName(srcFn);
            return StartToMmlData(srcFn, workPath, dispMessage);
        }

        public MmlDatum[] StartToMmlData(string srcPath, string wrkPath, Action<string> dispMessage)
        {
            try
            {
                Log.writeMethod = dispMessage;
                srcFn = srcPath;
                srcBuf = File.ReadAllBytes(srcFn);
                workPath = wrkPath;
                mucInfo = GetMUCInfo(srcBuf);
                fnVoicedat = string.IsNullOrEmpty(mucInfo.voice) ? "voice.dat" : System.IO.Path.Combine(workPath, mucInfo.voice);
                LoadFMVoice(fnVoicedat);
                fnPcm = string.IsNullOrEmpty(mucInfo.pcm) ? "mucompcm.bin" : System.IO.Path.Combine(workPath, mucInfo.pcm);
                LoadPCM(fnPcm);

                mucInfo.lines = StoreBasicSource(srcBuf);
                mucInfo.voiceData = voice;
                mucInfo.pcmData = pcmdata;
                mucInfo.basSrc = basSrc;
                mucInfo.fnSrc = srcFn;
                mucInfo.fnDst = Path.ChangeExtension(srcFn, ".mub");
                mucInfo.workPath = workPath;
                mucInfo.srcCPtr = 0;
                mucInfo.srcLinPtr = -1;

                //MUCOM88 初期化
                int ret = muc88.COMPIL();//vector 0xeea8

                //work.compilerInfo.bin = null;
                //work.compilerInfo.mucInfo = null;

                //コンパイルエラー発生時は0以外が返る
                if (ret != 0)
                {
                    int errLine = muc88.GetErrorLine();
                    work.compilerInfo.errorList.Add(new Tuple<int, int, string>(mucInfo.row, mucInfo.col, string.Format(msg.get("E0100"), errLine)));
                    Log.WriteLine(LogLevel.ERROR, string.Format(msg.get("E0100"), errLine));
                    return null;
                }

                ret = SaveMub();
                if (ret == 0)
                {
                    List<byte> ba = new List<byte>();
                    foreach (MmlDatum d in dat)
                    {
                        ba.Add((byte)d.dat);
                    }
                    //work.compilerInfo.bin = ba.ToArray();
                    //work.compilerInfo.mucInfo = mucInfo;

                    return dat.ToArray();
                }
            }
            catch (MucException me)
            {
                work.compilerInfo.errorList.Add(new Tuple<int, int, string>(-1, -1, me.Message));
                Log.WriteLine(LogLevel.ERROR, me.Message);
            }
            catch (Exception e)
            {
                work.compilerInfo.errorList.Add(new Tuple<int, int, string>(-1, -1, e.Message));
                Log.WriteLine(LogLevel.ERROR, string.Format(
                    msg.get("E0000")
                    , e.Message
                    , e.StackTrace));
            }

            return null;
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
                        mucInfo.comment = tag.Item2;
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
                    case "pcm":
                        mucInfo.pcm = tag.Item2;
                        break;
                }
            }

            return mucInfo;
        }

        public CompilerInfo GetCompilerInfo()
        {
            return work.compilerInfo;
        }



        private string srcFn = "";
        private byte[] srcBuf = null;
        private string workPath = "";
        private MUCInfo mucInfo = new MUCInfo();
        private Muc88 muc88 = null;
        private Msub msub = null;
        private expand expand = null;
        private smon smon = null;
        private string fnVoicedat = "";
        private string fnPcm = "";
        private byte[] voice;
        private byte[] pcmdata;
        private readonly List<Tuple<int, string>> basSrc = new List<Tuple<int, string>>();
        private readonly List<MmlDatum> dat = new List<MmlDatum>();

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

        private List<Tuple<string, string>> GetTagsFromMUC(byte[] buf)
        {
            var text = enc.GetStringFromSjisArray(buf)  // Encoding.GetEncoding("shift_jis").GetString(buf)
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.IndexOf("#") == 0);
            tags.Clear();

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

        private void LoadFMVoice(string fn)
        {
            string mucPathVoice = fn;
            string mdpPathVoice = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), fn);
            string decideVoice = mucPathVoice;
            voice = null;

            if (!File.Exists(mucPathVoice))
            {
                if (!File.Exists(mdpPathVoice)) return;
                decideVoice = mdpPathVoice;
            }

            try
            {
                voice = File.ReadAllBytes(decideVoice);
            }
            catch
            {
                //失敗しても特に何もしない
            }
        }

        private void LoadPCM(string fn)
        {
            string mucPathPCM = fn;
            string mdpPathPCM = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), fn);
            string decidePCM = mucPathPCM;
            pcmdata = null;

            if (!File.Exists(mucPathPCM))
            {
                if (!File.Exists(mdpPathPCM)) return;
                decidePCM = mdpPathPCM;
            }

            try
            {
                pcmdata= File.ReadAllBytes(decidePCM);
            }
            catch
            {
                //失敗しても特に何もしない
            }
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

                int fmvoice = work.OTONUM;//.fmvoiceCnt;
                int pcmflag = 0;
                int maxcount = 0;
                int mubsize = 0;
                string strTcount = "";
                string strLcount = "";
                string strBcount = "";
                
                work.compilerInfo.totalCount = new List<int>();
                work.compilerInfo.loopCount = new List<int>();
                work.compilerInfo.bufferCount = new List<int>();

                for (int i = 0; i < Muc88.MAXCH; i++)
                {
                    if (work.lcnt[i] != 0) { work.lcnt[i] = work.tcnt[i] - (work.lcnt[i] - 1); }
                    if (work.tcnt[i] > maxcount) maxcount = work.tcnt[i];
                    strTcount += string.Format("{0}:{1} ", (char)('A' + i), work.tcnt[i]);
                    work.compilerInfo.totalCount.Add(work.tcnt[i]);
                    strLcount += string.Format("{0}:{1} ", (char)('A' + i), work.lcnt[i]);
                    work.compilerInfo.loopCount.Add(work.lcnt[i]);
                    strBcount += string.Format("{0}:{1:x04} ", (char)('A' + i), work.bufCount[i]);
                    work.compilerInfo.bufferCount.Add(work.bufCount[i]);
                    if (work.bufCount[i] > 0xffff)
                    {
                        throw new MucException(string.Format(Common.msg.get("E0700"), (char)('A' + i), work.bufCount[i]));
                    }
                }

                work.compilerInfo.jumpClock = work.JCLOCK;

                if (work.pcmFlag == 0) pcmflag = 2;
                msg = enc.GetStringFromSjisArray(textLineBuf, 31, 4);//Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 31, 4);
                int start = Convert.ToInt32(msg, 16);
                msg = enc.GetStringFromSjisArray(textLineBuf, 41, 4);//Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 41, 4);
                int length = mucInfo.bufDst.Count;
                mubsize = length;

                Log.WriteLine(LogLevel.INFO, "- mucom.NET -");
                Log.WriteLine(LogLevel.INFO, "[ Total count ]");
                Log.WriteLine(LogLevel.INFO,strTcount);
                Log.WriteLine(LogLevel.INFO, "[ Loop count  ]");
                Log.WriteLine(LogLevel.INFO, strLcount);
                Log.WriteLine(LogLevel.INFO, "[ Buffer count  ]");
                Log.WriteLine(LogLevel.INFO, strBcount);
                Log.WriteLine(LogLevel.INFO, "");
                Log.WriteLine(LogLevel.INFO, string.Format("Used FM voice : {0}", fmvoice));
                Log.WriteLine(LogLevel.INFO, string.Format("#Data Buffer  : ${0:x05} - ${1:x05} (${2:x05})", start, start + length - 1, length));
                Log.WriteLine(LogLevel.INFO, string.Format("#Max Count    : {0}", maxcount));
                Log.WriteLine(LogLevel.INFO, string.Format("#MML Lines    : {0}", mucInfo.lines));
                Log.WriteLine(LogLevel.INFO, string.Format("#Data         : {0}", mubsize));

                return SaveMusic(
                    Path.Combine(mucInfo.workPath
                    , mucInfo.fnDst)
                    //, (ushort)start
                    , length
                    , pcmflag);
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
        private int SaveMusic(string fname, int length, int option)
        {
            //		音楽データファイルを出力(コンパイルが必要)
            //		filename     = 出力される音楽データファイル
            //		option : 1   = #タグによるvoice設定を無視
            //		         2   = PCM埋め込みをスキップ
            //		(戻り値が0以外の場合はエラー)
            //

            if (string.IsNullOrEmpty(fname)) return -1;

            int footsize;
            footsize = 1;//かならず1以上

            int pcmsize = (pcmdata == null) ? 0 : pcmdata.Length;
            bool pcmuse = ((option & 2) == 0);
            pcmdata = (!pcmuse ? null : pcmdata);
            int pcmptr = (!pcmuse ? 0 : (32 + length + footsize));
            pcmsize = (!pcmuse ? 0 : pcmsize);
            if (pcmuse)
            {
                if (pcmdata == null || pcmsize == 0)
                {
                    pcmuse = false;
                    pcmdata = null;
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

            dat.Add(new MmlDatum(0));//jump line number(dummy)
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum(0));//ext_flags(?)
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum(1));//ext_system(?)

            dat.Add(new MmlDatum(2));//ext_target(?)

            dat.Add(new MmlDatum(11));//ext_channel_num
            dat.Add(new MmlDatum(0));

            dat.Add(new MmlDatum((byte)work.OTONUM));//ext_fmvoice_num
            dat.Add(new MmlDatum((byte)(work.OTONUM >> 8)));

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

            if (work.JCLOCK > 0)
            {
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump count [{0}].\r\n", work.JCLOCK));
            }

            for (int i = 0; i < length; i++) dat.Add(mucInfo.bufDst.Get(i));

            dat[dataOffset + 0] = new MmlDatum(0);//バイナリに含まれる曲データ数-1
            dat[dataOffset + 1] = new MmlDatum((byte)work.OTODAT);
            dat[dataOffset + 2] = new MmlDatum((byte)(work.OTODAT >> 8));
            dat[dataOffset + 3] = new MmlDatum((byte)work.ENDADR);
            dat[dataOffset + 4] = new MmlDatum((byte)(work.ENDADR >> 8));
            //dat[dataOffset + 5] = 0xff; //たぶん　テンポコマンド(タイマーB)設定時に更新される

            if (tags != null)
            {
                footsize = 0;

                foreach (Tuple<string, string> tag in tags)
                {
                    byte[] b = enc.GetSjisArrayFromString(string.Format("#{0} {1}\r\n", tag.Item1, tag.Item2));
                    //Encoding.GetEncoding("shift_jis").GetBytes(string.Format("#{0} {1}\r\n", tag.Item1, tag.Item2));
                    footsize += b.Length;
                    foreach (byte bd in b) dat.Add(new MmlDatum(bd));
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
                for (int i = 0; i < pcmsize; i++) dat.Add(new MmlDatum(pcmdata[i]));
                if (pcmsize > 0)
                {
                    pcmptr = 16 * 3 + 32 + length + footsize;
                    dat[20] = new MmlDatum((byte)pcmptr);//pcmdata size(32bit)
                    dat[21] = new MmlDatum((byte)(pcmptr >> 8));
                    dat[22] = new MmlDatum((byte)(pcmptr >> 16));
                    dat[23] = new MmlDatum((byte)(pcmptr >> 24));
                }
            }

            OutFileName = fname;
            Log.WriteLine(LogLevel.INFO, string.Format("#Saved [{0}].\r\n", fname));

            return 0;
        }

    }
}
