using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using mucomDotNET.Common;

namespace mucomDotNET.Compiler
{
    public class Compiler
    {
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

        public void Init()
        {
            muc88 = new Muc88(mucInfo);
            msub = new Msub(mucInfo);
            expand = new expand(mucInfo);
            smon = new smon(mucInfo);
            muc88.msub = msub;
            muc88.expand = expand;
            msub.muc88 = muc88;
            expand.msub = msub;
            expand.smon = smon;
            expand.muc88 = muc88;
        }

        public MubDat[] StartToMubDat(string arg)
        {
            try
            {
                srcFn = arg;
                srcBuf = File.ReadAllBytes(srcFn);
                workPath = Path.GetDirectoryName(srcFn);
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

                //コンパイルエラー発生時は0以外が返る
                if (ret != 0)
                {
                    int errLine = muc88.GetErrorLine();
                    Log.WriteLine(LogLevel.ERROR, string.Format(msg.get("E0100"), errLine));
                    return null;
                }

                ret = SaveMub();
                if (ret == 0) return dat.ToArray();
            }
            catch(MucException me)
            {
                Log.WriteLine(LogLevel.ERROR, me.Message);
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.ERROR, string.Format(
                    msg.get("E0000")
                    ,e.Message
                    ,e.StackTrace));
            }

            return null;
        }

        public byte[] Start(string arg)
        {
            MubDat[] ma = StartToMubDat(arg);
            if (ma == null || ma.Length < 1) return null;

            List<byte> ba = new List<byte>();
            foreach(MubDat d in ma)
            {
                ba.Add(d.dat);
            }

            return ba.ToArray();
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
        public enum EnmMUCOMFileType
        {
            unknown,
            MUB,
            MUC
        }

        private readonly List<Tuple<string, string>> tags=new List<Tuple<string, string>>();

        private List<Tuple<string, string>> GetTagsFromMUC(byte[] buf)
        {
            //Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var text = Encoding.GetEncoding("shift_jis").GetString(buf)
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
            var text = Encoding.GetEncoding("shift_jis").GetString(buf)
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
                for (int i = 0; i < Muc88.MAXCH; i++)
                {
                    if (work.lcnt[i] != 0) { work.lcnt[i] = work.tcnt[i] - (work.lcnt[i] - 1); }
                    if (work.tcnt[i] > maxcount) maxcount = work.tcnt[i];
                    strTcount += string.Format("{0}:{1} ", (char)('A' + i), work.tcnt[i]);
                    strLcount += string.Format("{0}:{1} ", (char)('A' + i), work.lcnt[i]);
                    strBcount += string.Format("{0}:{1:x04} ", (char)('A' + i), work.bufCount[i]);
                    if (work.bufCount[i] > 0xffff)
                    {
                        throw new MucException(string.Format(Common.msg.get("E0700"), (char)('A' + i), work.bufCount[i]));
                    }
                }

                if (work.pcmFlag == 0) pcmflag = 2;
                msg = Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 31, 4);
                int start = Convert.ToInt32(msg, 16);
                msg = Encoding.GetEncoding("Shift_JIS").GetString(textLineBuf, 41, 4);
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
                Log.WriteLine(LogLevel.ERROR, me.Message);
                return -1;
            }
            catch
            {
                return -1;
            }
        }

        private readonly List<MubDat> dat = new List<MubDat>();

        public string OutFileName { get; set; }

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

            dat.Add(new MubDat(0x4d));// M
            dat.Add(new MubDat(0x55));// U
            dat.Add(new MubDat(0x42));// B
            dat.Add(new MubDat(0x38));// 8

            dat.Add(new MubDat((byte)dataOffset));
            dat.Add(new MubDat((byte)(dataOffset >> 8)));
            dat.Add(new MubDat((byte)(dataOffset >> 16)));
            dat.Add(new MubDat((byte)(dataOffset >> 24)));

            dat.Add(new MubDat((byte)dataSize));
            dat.Add(new MubDat((byte)(dataSize >> 8)));
            dat.Add(new MubDat((byte)(dataSize >> 16)));
            dat.Add(new MubDat((byte)(dataSize >> 24)));

            dat.Add(new MubDat((byte)tagOffset));
            dat.Add(new MubDat((byte)(tagOffset >> 8)));
            dat.Add(new MubDat((byte)(tagOffset >> 16)));
            dat.Add(new MubDat((byte)(tagOffset >> 24)));

            dat.Add(new MubDat(0));//tagdata size(dummy)
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));

            dat.Add(new MubDat((byte)pcmptr));//pcmdata ptr(32bit)
            dat.Add(new MubDat((byte)(pcmptr >> 8)));
            dat.Add(new MubDat((byte)(pcmptr >> 16)));
            dat.Add(new MubDat((byte)(pcmptr >> 24)));

            dat.Add(new MubDat((byte)pcmsize));//pcmdata size(32bit)
            dat.Add(new MubDat((byte)(pcmsize >> 8)));
            dat.Add(new MubDat((byte)(pcmsize >> 16)));
            dat.Add(new MubDat((byte)(pcmsize >> 24)));

            dat.Add(new MubDat((byte)work.JCLOCK));// JCLOCKの値(Jコマンドのタグ位置)
            dat.Add(new MubDat((byte)(work.JCLOCK >> 8)));

            dat.Add(new MubDat(0));//jump line number(dummy)
            dat.Add(new MubDat(0));

            dat.Add(new MubDat(0));//ext_flags(?)
            dat.Add(new MubDat(0));

            dat.Add(new MubDat(1));//ext_system(?)

            dat.Add(new MubDat(2));//ext_target(?)

            dat.Add(new MubDat(11));//ext_channel_num
            dat.Add(new MubDat(0));

            dat.Add(new MubDat((byte)work.OTONUM));//ext_fmvoice_num
            dat.Add(new MubDat((byte)(work.OTONUM >> 8)));

            dat.Add(new MubDat(0));//ext_player(?)
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));

            dat.Add(new MubDat(0));//pad1
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));
            dat.Add(new MubDat(0));

            for (int i = 0; i < 32; i++)
            {
                dat.Add(new MubDat((byte)mucInfo.bufDefVoice.Get(i)));
            }

            if (work.JCLOCK > 0)
            {
                Log.WriteLine(LogLevel.INFO, string.Format("#Jump count [{0}].\r\n", work.JCLOCK));
            }

            for (int i = 0; i < length; i++) dat.Add(mucInfo.bufDst.Get(i));

            dat[dataOffset + 0] = new MubDat(0);//バイナリに含まれる曲データ数-1
            dat[dataOffset + 1] = new MubDat((byte)work.OTODAT);
            dat[dataOffset + 2] = new MubDat((byte)(work.OTODAT >> 8));
            dat[dataOffset + 3] = new MubDat((byte)work.ENDADR);
            dat[dataOffset + 4] = new MubDat((byte)(work.ENDADR >> 8));
            //dat[dataOffset + 5] = 0xff; //たぶん　テンポコマンド(タイマーB)設定時に更新される

            if (tags != null)
            {
                footsize = 0;

                foreach (Tuple<string, string> tag in tags)
                {
                    byte[] b = Encoding.GetEncoding("shift_jis").GetBytes(string.Format("#{0} {1}\r\n", tag.Item1, tag.Item2));
                    footsize += b.Length;
                    foreach (byte bd in b) dat.Add(new MubDat(bd));
                }

                if (footsize > 0)
                {
                    dat.Add(new MubDat(0));
                    dat.Add(new MubDat(0));
                    dat.Add(new MubDat(0));
                    dat.Add(new MubDat(0));
                    footsize += 4;

                    dat[16] = new MubDat((byte)footsize);//tagdata size(32bit)
                    dat[17] = new MubDat((byte)(footsize >> 8));
                    dat[18] = new MubDat((byte)(footsize >> 16));
                    dat[19] = new MubDat((byte)(footsize >> 24));
                }
            }

            if (pcmuse)
            {
                for (int i = 0; i < pcmsize; i++) dat.Add(new MubDat(pcmdata[i]));
                if (pcmsize > 0)
                {
                    pcmptr = 16 * 3 + 32 + length + footsize;
                    dat[20] = new MubDat((byte)pcmptr);//pcmdata size(32bit)
                    dat[21] = new MubDat((byte)(pcmptr >> 8));
                    dat[22] = new MubDat((byte)(pcmptr >> 16));
                    dat[23] = new MubDat((byte)(pcmptr >> 24));
                }
            }

            OutFileName = fname;
            Log.WriteLine(LogLevel.INFO, string.Format("#Saved [{0}].\r\n", fname));

            return 0;
        }

    }
}
