using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mucomDotNET.Common;
using musicDriverInterface;

namespace mucomDotNET.Driver
{
    public class MUBHeader
    {
        public uint magic = 0;
        public uint dataoffset = 0;
        public uint datasize = 0;
        public uint tagdata = 0;
        public uint tagsize = 0;
        public uint[] pcmdataPtr = new uint[] { 0, 0, 0, 0, 0, 0 };
        public uint[] pcmsize = new uint[] { 0, 0, 0, 0, 0, 0 };
        public uint jumpcount = 0;
        public uint jumpline = 0;
        public uint ext_flags = 0;
        public uint ext_system = 0;
        public uint ext_target = 0;
        public uint ext_channel_num = 0;
        public uint ext_fmvoice_num = 0;
        public uint ext_player = 0;
        public uint pad1 = 0;
        public byte[] ext_fmvoice = new byte[32];
        private MmlDatum[] srcBuf = null;
        private iEncoding enc = null;
        public MupbInfo mupb;
        private uint mupbDataPtr;
        public bool CarrierCorrection = true;
        public enmOPMClockMode OPMClockMode= enmOPMClockMode.normal;

        public enum enmOPMClockMode
        {
            normal,X68000
        }

        public MUBHeader(MmlDatum[] buf,iEncoding enc)
        {
            CarrierCorrection = true;
            OPMClockMode = enmOPMClockMode.normal;

            this.enc = enc;

            magic = Cmn.getLE32(buf, 0x0000);

            if (magic == 0x3843554d) //'MUC8'
            {
                dataoffset = Cmn.getLE32(buf, 0x0004);
                datasize = Cmn.getLE32(buf, 0x0008);
                tagdata = Cmn.getLE32(buf, 0x000c);
                tagsize = Cmn.getLE32(buf, 0x0010);
                pcmdataPtr[0] = Cmn.getLE32(buf, 0x0014);
                pcmsize[0] = Cmn.getLE32(buf, 0x0018);
                jumpcount = Cmn.getLE16(buf, 0x001c);
                jumpline = Cmn.getLE16(buf, 0x001e);
            }
            else if (magic == 0x3842554d) //'MUB8'
            {
                dataoffset = Cmn.getLE32(buf, 0x0004);
                datasize = Cmn.getLE32(buf, 0x0008);
                tagdata = Cmn.getLE32(buf, 0x000c);
                tagsize = Cmn.getLE32(buf, 0x0010);
                pcmdataPtr[0] = Cmn.getLE32(buf, 0x0014);
                pcmsize[0] = Cmn.getLE32(buf, 0x0018);
                jumpcount = Cmn.getLE16(buf, 0x001c);
                jumpline = Cmn.getLE16(buf, 0x001e);

                ext_flags = Cmn.getLE16(buf, 0x0020);
                ext_system = (uint)buf[0x0022].dat;
                ext_target = (uint)buf[0x0023].dat;
                ext_channel_num = Cmn.getLE16(buf, 0x0024);
                ext_fmvoice_num = Cmn.getLE16(buf, 0x0026);
                ext_player = Cmn.getLE32(buf, 0x0028);
                pad1 = Cmn.getLE32(buf, 0x002c);
                for (int i = 0; i < 32; i++)
                {
                    ext_fmvoice[i] = (byte)buf[0x0030 + i].dat;
                }
            }
            else if (magic == 0x6250756d)
            {
                mupb = new MupbInfo();
                mupb.version = Cmn.getLE32(buf, 0x0004);
                mupb.variableLengthCount = buf[0x0008].dat;
                mupb.useChipCount = buf[0x0009].dat;
                mupb.usePartCount = (int)Cmn.getLE16(buf,0x000a);
                mupb.usePageCount = (int)Cmn.getLE16(buf, 0x000c);
                mupb.useInstrumentSetCount = (int)Cmn.getLE16(buf, 0x000e);
                mupb.usePCMSetCount = (int)Cmn.getLE16(buf, 0x0010); 
                tagdata = Cmn.getLE32(buf, 0x0012);
                tagsize = Cmn.getLE32(buf, 0x0016);
                jumpcount = Cmn.getLE32(buf, 0x001a);
                jumpline = Cmn.getLE32(buf, 0x001e);

                uint ptr = 0x0022;

                //Chip Define division.
                mupb.chips = new MupbInfo.ChipDefine[mupb.useChipCount];
                for (int i = 0; i < mupb.useChipCount; i++)
                {
                    mupb.chips[i] = new MupbInfo.ChipDefine();
                    MupbInfo.ChipDefine cd = mupb.chips[i];
                    cd.indexNumber = Cmn.getLE16(buf, ptr); ptr += 2;
                    cd.identifyNumber = Cmn.getLE32(buf, ptr); ptr += 4;
                    cd.masterClock = Cmn.getLE32(buf, ptr); ptr += 4;
                    cd.option = Cmn.getLE32(buf, ptr); ptr += 4;
                    cd.heartBeat = Cmn.getLE32(buf, ptr); ptr += 4;
                    cd.heartBeat2 = Cmn.getLE32(buf, ptr); ptr += 4;
                    
                    cd.parts = new MupbInfo.ChipDefine.chipPart[buf[ptr].dat];ptr++;
                    for (int j = 0; j < cd.parts.Length; j++)
                    {
                        cd.parts[j] = new MupbInfo.ChipDefine.chipPart();
                    }
                    
                    cd.instrumentNumber = new uint[buf[ptr].dat]; ptr++;
                    for (int j = 0; j < cd.instrumentNumber.Length; j++)
                    {
                        cd.instrumentNumber[j] = Cmn.getLE16(buf, ptr); ptr += 2;
                    }

                    cd.pcmNumber = new uint[buf[ptr].dat]; ptr++;
                    for (int j = 0; j < cd.pcmNumber.Length; j++)
                    {
                        cd.pcmNumber[j] = Cmn.getLE16(buf, ptr); ptr += 2;
                    }
                }

                //Part division.
                mupb.parts = new MupbInfo.PartDefine[mupb.usePartCount];
                for (int i = 0; i < mupb.usePartCount; i++)
                {
                    mupb.parts[i] = new MupbInfo.PartDefine();
                    MupbInfo.PartDefine pd = mupb.parts[i];
                    pd.pageCount = buf[ptr].dat; ptr++;
                }

                //Page division.
                mupb.pages = new MupbInfo.PageDefine[mupb.usePageCount];
                for (int i = 0; i < mupb.usePageCount; i++)
                {
                    mupb.pages[i] = new MupbInfo.PageDefine();
                    MupbInfo.PageDefine pd = mupb.pages[i];
                    pd.length = Cmn.getLE32(buf, ptr); ptr += 4;
                    pd.loopPoint = (int)Cmn.getLE32(buf, ptr); ptr += 4;
                }

                //Instrument set division.
                mupb.instruments = new MupbInfo.InstrumentDefine[mupb.useInstrumentSetCount];
                for (int i = 0; i < mupb.useInstrumentSetCount; i++)
                {
                    mupb.instruments[i] = new MupbInfo.InstrumentDefine();
                    MupbInfo.InstrumentDefine id = mupb.instruments[i];
                    id.length = Cmn.getLE32(buf, ptr); ptr += 4;
                }

                //PCM set division.
                mupb.pcms = new MupbInfo.PCMDefine[mupb.usePCMSetCount];
                for (int i = 0; i < mupb.usePCMSetCount; i++)
                {
                    mupb.pcms[i] = new MupbInfo.PCMDefine();
                    MupbInfo.PCMDefine pd = mupb.pcms[i];
                    pd.length = Cmn.getLE32(buf, ptr); ptr += 4;
                }

                //データ部開始位置
                mupbDataPtr = ptr;

                //Chip毎のページ情報割り当てとページデータの取り込み
                int pp = 0;
                int pr = 0;
                for (int i = 0; i < mupb.useChipCount; i++)
                {
                    MupbInfo.ChipDefine cd = mupb.chips[i];
                    for (int j = 0; j < cd.parts.Length; j++)
                    {
                        cd.parts[j].pages = new MupbInfo.PageDefine[mupb.parts[pr++].pageCount];
                        for (int k = 0; k < cd.parts[j].pages.Length; k++)
                        {
                            cd.parts[j].pages[k] = mupb.pages[pp++];
                            cd.parts[j].pages[k].data = new MmlDatum[cd.parts[j].pages[k].length];
                            for (int l = 0; l < cd.parts[j].pages[k].length; l++)
                            {
                                cd.parts[j].pages[k].data[l] = buf[ptr++];
                            }
                        }
                    }
                }

                //instrumentデータの取り込み
                for (int i = 0; i < mupb.useInstrumentSetCount; i++)
                {
                    MupbInfo.InstrumentDefine id = mupb.instruments[i];
                    id.data = new byte[id.length];
                    for (int j = 0; j < id.length; j++)
                    {
                        id.data[j] = (byte)buf[ptr++].dat;
                    }
                }

                //pcmデータの取り込み
                for (int i = 0; i < mupb.usePCMSetCount; i++)
                {
                    MupbInfo.PCMDefine pd = mupb.pcms[i];
                    pd.data = new byte[pd.length];
                    for (int j = 0; j < pd.length; j++)
                    {
                        pd.data[j] = (byte)buf[ptr++].dat;
                    }
                }

            }
            else
            {
                throw new MubException("This data is not mucom88 data !");
            }
            srcBuf = buf;
        }

        public MmlDatum[] GetDATA()
        {
            try
            {
                if (dataoffset == 0) return null;
                if (srcBuf == null) return null;

                List<MmlDatum> lb = new List<MmlDatum>();
                for (int i = 0; i < datasize; i++)
                {
                    lb.Add(srcBuf[dataoffset + i]);
                }

                return lb.ToArray();
            }
            catch
            {
                return null;
            }
        }

        public List<Tuple<string, string>> GetTags()
        {
            try
            {
                if (tagdata == 0) return null;
                if (srcBuf == null) return null;

                List<byte> lb = new List<byte>();
                for (int i = 0; i < tagsize; i++)
                {
                    lb.Add((byte)srcBuf[tagdata + i].dat);
                }

                return GetTagsByteArray(lb.ToArray());
            }
            catch
            {
                return null;
            }
        }

        public byte[] GetPCM(int id)
        {
            try
            {
                if (pcmdataPtr[id] == 0) return null;
                if (srcBuf == null) return null;

                List<byte> lb = new List<byte>();
                for (int i = 0; i < pcmsize[id]; i++)
                {
                    lb.Add((byte)srcBuf[pcmdataPtr[id] + i].dat);
                }

                return lb.ToArray();
            }
            catch
            {
                return null;
            }
        }

        private List<Tuple<string, string>> GetTagsByteArray(byte[] buf)
        {
            var text = enc.GetStringFromSjisArray(buf) //Encoding.GetEncoding("shift_jis").GetString(buf)
                .Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.IndexOf("#") == 0);

            List<Tuple<string, string>> tags = new List<Tuple<string, string>>();
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

            SetDriverOptionFromTags(tags);

            return tags;
        }

        public void SetDriverOptionFromTags(List<Tuple<string, string>> tags)
        {
            if (tags == null) return;
            if (tags.Count < 1) return;

            foreach (var tag in tags)
            {
                if (tag == null) continue;
                if (string.IsNullOrEmpty(tag.Item1)) continue;

                if (tag.Item1.ToLower().Trim() == "carriercorrection")
                {
                    if (!string.IsNullOrEmpty(tag.Item2))
                    {
                        string val = tag.Item2.ToLower().Trim();

                        CarrierCorrection = false;
                        if (val == "yes" || val == "y" || val == "1" || val == "true" || val == "t")
                        {
                            CarrierCorrection = true;
                        }
                    }
                }
                else if (tag.Item1.ToLower().Trim() == "opmclockmode")
                {
                    if (!string.IsNullOrEmpty(tag.Item2))
                    {
                        string val = tag.Item2.ToLower().Trim();

                        OPMClockMode = enmOPMClockMode.normal;
                        if (val == "x68000" || val == "x68k" || val == "x68" || val == "x" || val == "40000" || val == "x680x0")
                        {
                            OPMClockMode = enmOPMClockMode.X68000;
                        }
                    }
                }

            }
        }

    }

}
