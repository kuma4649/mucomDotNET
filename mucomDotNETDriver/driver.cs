using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace mucomDotNET.Driver
{
    public class Driver :iDriver
    {
        private MUBHeader header = null;
        private List<Tuple<string, string>> tags = null;
        private byte[][] pcm = new byte[6][];
        private string[] pcmType = new string[6];
        private int[] pcmStartPos = new int[6];
        private Action<ChipDatum> WriteOPNAP;
        private Action<ChipDatum> WriteOPNAS;
        private Action<ChipDatum> WriteOPNBP;
        private Action<ChipDatum> WriteOPNBS;
        private Action<byte[], int, int> WriteOPNBAdpcmAP;
        private Action<byte[], int, int> WriteOPNBAdpcmBP;
        private Action<byte[], int, int> WriteOPNBAdpcmAS;
        private Action<byte[], int, int> WriteOPNBAdpcmBS;
        private Action<long,int> WaitSendOPNA;
        private string[] fnVoicedat = { "", "", "", "" };
        private string[] fnPcm = { "", "", "", "", "", "" };

        private int renderingFreq = 44100;
        private int opnaMasterClock = 7987200;
        private Work work = new Work();
        private Music2 music2 = null;
        private object lockObjWriteReg = new object();

        public enum Command
        {
            MusicSTART
            , MusicSTOP
            , FaDeOut
            , EFfeCt
            , RETurnWork
        }
        private iEncoding enc = null;

        public Driver(iEncoding enc = null)
        {
            this.enc = enc ?? myEncoding.Default;
        }

        public void Init(List<ChipAction> chipsAction, MmlDatum[] srcBuf, Func<string, Stream> appendFileReaderCallback, object addtionalOption)
        {
            List<Action<ChipDatum>> lstChipWrite = new List<Action<ChipDatum>>();
            List<Action<byte[], int, int>> lstChipWriteAdpcm = new List<Action<byte[], int, int>>();
            List<Action<long, int>> lstChipWaitSend = new List<Action<long, int>>();

            foreach (ChipAction ca in chipsAction)
            {
                lstChipWrite.Add(ca.WriteRegister);
                lstChipWriteAdpcm.Add(ca.WritePCMData);
                lstChipWaitSend.Add(ca.WaitSend);
            }
            InitT(lstChipWrite, lstChipWriteAdpcm, lstChipWaitSend, srcBuf, addtionalOption, appendFileReaderCallback);
        }

        private void Init(
            string fileName,
            List<Action<ChipDatum>> lstChipWrite,
            List<Action<byte[],int,int>> lstChipWriteAdpcm,
            List<Action<long, int>> opnaWaitSend,
            bool notSoundBoard2, bool isLoadADPCM, bool loadADPCMOnly, Func<string, Stream> appendFileReaderCallback = null)
        {
            if (Path.GetExtension(fileName).ToLower() != ".xml")
            {
                byte[] srcBuf = File.ReadAllBytes(fileName);
                if (srcBuf == null || srcBuf.Length < 1) return;
                Init(lstChipWrite, lstChipWriteAdpcm, opnaWaitSend, notSoundBoard2, srcBuf, isLoadADPCM, loadADPCMOnly, appendFileReaderCallback ?? CreateAppendFileReaderCallback(Path.GetDirectoryName(fileName)));
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MmlDatum[]), typeof(MmlDatum[]).GetNestedTypes());
                using (StreamReader sr = new StreamReader(fileName, new UTF8Encoding(false)))
                {
                    MmlDatum[] s = (MmlDatum[])serializer.Deserialize(sr);
                    InitT(lstChipWrite, lstChipWriteAdpcm, opnaWaitSend, s, new object[] { notSoundBoard2, isLoadADPCM, loadADPCMOnly }, appendFileReaderCallback);
                }

            }
        }

        private void Init(string fileName, List<Action<ChipDatum>> lstChipWrite, List<Action<byte[],int,int>> lstChipWriteAdpcm, List<Action<long, int>> opnaWaitSend, bool notSoundBoard2, byte[] srcBuf, bool isLoadADPCM, bool loadADPCMOnly)
        {
            if (srcBuf == null || srcBuf.Length < 1) return;
            Init(lstChipWrite, lstChipWriteAdpcm, opnaWaitSend, notSoundBoard2, srcBuf, isLoadADPCM, loadADPCMOnly, CreateAppendFileReaderCallback(Path.GetDirectoryName(fileName)));
        }

        private void Init(List<Action<ChipDatum>> lstChipWrite, List<Action<byte[],int,int>> lstChipWriteAdpcm, List<Action<long, int>> opnaWaitSend, bool notSoundBoard2, byte[] srcBuf, bool isLoadADPCM, bool loadADPCMOnly, Func<string, Stream> appendFileReaderCallback)
        {
            if (srcBuf == null || srcBuf.Length < 1) return;
            List<MmlDatum> bl = new List<MmlDatum>();
            foreach (byte b in srcBuf) bl.Add(new MmlDatum(b));
            InitT(lstChipWrite,lstChipWriteAdpcm, opnaWaitSend, bl.ToArray(), new object[] { notSoundBoard2, isLoadADPCM, loadADPCMOnly }, appendFileReaderCallback);
        }

        private void Init(string fileName, List<Action<ChipDatum>> lstChipWrite, List<Action<byte[],int,int>> lstChipWriteAdpcm, List<Action<long, int>> chipWaitSend, MmlDatum[] srcBuf, object addtionalOption)
        {
            if (srcBuf == null || srcBuf.Length < 1) return;
            InitT(lstChipWrite, lstChipWriteAdpcm, chipWaitSend, srcBuf, addtionalOption, CreateAppendFileReaderCallback(Path.GetDirectoryName(fileName)));
        }

        private void InitT(List<Action<ChipDatum>> lstChipWrite, List<Action<byte[],int,int>> lstChipWriteAdpcm,List< Action<long, int>> chipWaitSend, MmlDatum[] srcBuf, object addtionalOption, Func<string, Stream> appendFileReaderCallback)
        {
            if (srcBuf == null || srcBuf.Length < 1) return;

            bool notSoundBoard2 = (bool)((object[])addtionalOption)[0];
            bool isLoadADPCM = (bool)((object[])addtionalOption)[1];
            bool loadADPCMOnly = (bool)((object[])addtionalOption)[2];
            string filename = (string)((object[])addtionalOption)[3];
            appendFileReaderCallback = appendFileReaderCallback ?? CreateAppendFileReaderCallback(Path.GetDirectoryName(filename));

            work = new Work();
            header = new MUBHeader(srcBuf, enc);
            work.mData = GetDATA();
            work.header = header;
            tags = GetTags();
            GetFileNameFromTag();
            for (int i = 0; i < 4; i++)
            {
                work.fmVoice[i] = GetFMVoiceFromFile(i, appendFileReaderCallback);
                pcm[i] = GetPCMFromSrcBuf(i) ?? GetPCMDataFromFile(i, appendFileReaderCallback);
                work.pcmTables[i] = GetPCMTable(i);
            }
            for (int i = 4; i < 6; i++)
            {
                pcm[i] = GetPCMFromSrcBuf(i) ?? GetPCMDataFromFile(i, appendFileReaderCallback);
                work.pcmTables[i] = GetPCMTable(i);
            }

            if (pcm[2] != null && pcmType[2] == "") { TransformOPNAPCMtoOPNBPCM(2); pcmStartPos[2] = 0; }
            if (pcm[3] != null && pcmType[3] == "") { TransformOPNAPCMtoOPNBPCM(3); pcmStartPos[3] = 0; }
            if (pcm[4] != null && pcmType[4] == "") { TransformOPNAPCMtoOPNBPCM(4); pcmStartPos[4] = 0; }
            if (pcm[5] != null && pcmType[5] == "") { TransformOPNAPCMtoOPNBPCM(5); pcmStartPos[5] = 0; }

            work.isDotNET = IsDotNETFromTAG();

            WriteOPNAP = lstChipWrite[0];
            WriteOPNAS = lstChipWrite[1];
            WriteOPNBP = lstChipWrite[2];
            WriteOPNBS = lstChipWrite[3];
            WriteOPNBAdpcmAP = lstChipWriteAdpcm[2];
            WriteOPNBAdpcmBP = lstChipWriteAdpcm[2];
            WriteOPNBAdpcmAS = lstChipWriteAdpcm[3];
            WriteOPNBAdpcmBS = lstChipWriteAdpcm[3];
            WaitSendOPNA = chipWaitSend[0];

            //PCMを送信する
            if (pcm != null)
            {
                if (isLoadADPCM)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (pcm[i] == null) continue;
                        ChipDatum[] pcmSendData = GetPCMSendData(0, i, 0);

                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        if (i == 0) foreach (ChipDatum dat in pcmSendData) WriteOPNAPRegister(dat);
                        if (i == 1) foreach (ChipDatum dat in pcmSendData) WriteOPNASRegister(dat);
                        sw.Stop();

                        WaitSendOPNA(sw.ElapsedMilliseconds, pcmSendData.Length);
                    }

                    List<byte> buf = new List<byte>();
                    if (pcm[2] != null)
                    {
                        buf.Clear();
                        for (int i = pcmStartPos[2]; i < pcm[2].Length; i++) buf.Add(pcm[2][i]);
                        WriteOPNBPAdpcmB(buf.ToArray());
                    }
                    if (pcm[3] != null)
                    {
                        buf.Clear();
                        for (int i = pcmStartPos[3]; i < pcm[3].Length; i++) buf.Add(pcm[3][i]);
                        WriteOPNBSAdpcmB(pcm[3]);
                    }
                    if (pcm[4] != null)
                    {
                        buf.Clear();
                        for (int i = pcmStartPos[4]; i < pcm[4].Length; i++) buf.Add(pcm[4][i]);
                        WriteOPNBPAdpcmA(pcm[4]);
                    }
                    if (pcm[5] != null)
                    {
                        buf.Clear();
                        for (int i = pcmStartPos[5]; i < pcm[5].Length; i++) buf.Add(pcm[5][i]);
                        WriteOPNBSAdpcmA(pcm[5]);
                    }

                }
            }

            if (loadADPCMOnly) return;

            music2 = new Music2(work, WriteOPNAPRegister, WriteOPNASRegister, WriteOPNBPRegister, WriteOPNBSRegister);
            music2.notSoundBoard2 = notSoundBoard2;
        }

        private void TransformOPNAPCMtoOPNBPCM(int v)
        {
            List<List<byte>> pcmData = new List<List<byte>>();
            List<byte> dest = new List<byte>(0);
            //for (int i = 0; i < 0x400; i++) dest.Add(0);
            for (int i = 0; i < work.pcmTables[v].Length; i++)
            {
                pcmData.Add(new List<byte>());
                List<byte> one = pcmData[i];
                for (int ptr = (work.pcmTables[v][i].Item2[0] << 2); ptr < (work.pcmTables[v][i].Item2[1] << 2) + 16; ptr++)
                {
                    one.Add(pcm[v][ptr + 0x400]);//0x400 ヘッダのサイズ
                }
            }

            int tblPtr = 0;
            for (int i = 0; i < work.pcmTables[v].Length; i++)
            {
                for (int j = 0; j < pcmData[i].Count; j++) dest.Add(pcmData[i][j]);
                for (int j = 0; j < 256 - (pcmData[i].Count % 256); j++) dest.Add(0x00);

                ushort stAdr = (ushort)(tblPtr >> 8);
                int length = pcmData[i].Count + 256 - (pcmData[i].Count % 256);
                tblPtr += length != 0 ? (length - 0x100) : 0;
                ushort edAdr = (ushort)(tblPtr >> 8);
                tblPtr += length != 0 ? 0x100 : 0;
                //ushort stAdr = (ushort)(tblPtr >> 9);
                //int length = pcmData[i].Count + 512 - (pcmData[i].Count % 512);
                //tblPtr += length != 0 ? (length - 0x200) : 0;
                //ushort edAdr = (ushort)(tblPtr >> 9);
                //tblPtr += length != 0 ? 0x200 : 0;
                work.pcmTables[v][i] = new Tuple<string, ushort[]>(work.pcmTables[v][i].Item1, new ushort[] { stAdr, edAdr, 0, work.pcmTables[v][i].Item2[3] });

            }
            pcm[v] = dest.ToArray();
        }

        private bool IsDotNETFromTAG()
        {
            if (tags == null) return false;
            foreach (Tuple<string, string> tag in tags)
            {
                if (tag.Item1== "driver")
                {
                    if(tag.Item2.ToLower()== "mucomdotnet")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private Func<string, Stream> CreateAppendFileReaderCallback(string dir)
        {
            return fname =>
            {
                if (!string.IsNullOrEmpty(dir))
                {
                    var path = Path.Combine(dir, fname);
                    if (File.Exists(path))
                    {
                        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                if (File.Exists(fname))
                {
                    return new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                return null;
            };
        }


        //-------
        //data Information
        //-------

        public MmlDatum[] GetDATA()
        {
            return header.GetDATA();
        }

        public List<Tuple<string, string>> GetTags()
        {
            if (header == null)
            {
                throw new MubException("Header information not found.");
            }
            return header.GetTags();
        }

        public byte[] GetPCMFromSrcBuf(int id)
        {
            return header.GetPCM(id);
        }

        public Tuple<string, ushort[]>[] GetPCMTable(int id)
        {
            if (pcm == null) return null;
            if (pcm[id] == null) return null;

            List<Tuple<string, ushort[]>> pcmtable = new List<Tuple<string, ushort[]>>();
            int inftable = 0x0000;
            int adr, whl, eadr;
            byte[] pcmname = new byte[17];
            int maxpcm = 32;

            string fcc="";
            if (pcm[id].Length > 4) fcc = ((char)pcm[id][0]).ToString() + ((char)pcm[id][1]).ToString() + ((char)pcm[id][2]).ToString() + ((char)pcm[id][3]).ToString();
            pcmType[id] = fcc;
            switch(fcc)
            {
                case "mda "://OPNA ADPCM
                case "mdbb"://OPNB ADPCM-B
                case "mdba"://OPNB ADPCM-A
                    int cnt = pcm[id][4] + (pcm[id][5] << 8) + 1;
                    int ptr = 6;
                    for (int i = 0; i < cnt; i++)
                    {
                        List<byte> b = new List<byte>();
                        while (pcm[id][ptr] != 0x0) b.Add(pcm[id][ptr++]);
                        string item1 = enc.GetStringFromSjisArray(b.ToArray());
                        ptr++;
                        ptr++;
                        ushort[] item2 = new ushort[4];
                        item2[0] = (ushort)(pcm[id][ptr + 2] | (pcm[id][ptr + 3] * 0x100));
                        item2[1] = (ushort)(pcm[id][ptr + 4] | (pcm[id][ptr + 5] * 0x100));
                        item2[2] = (ushort)0;
                        item2[3] = (ushort)(pcm[id][ptr + 0] | (pcm[id][ptr + 1] * 0x100));
                        Tuple<string, ushort[]> pd = new Tuple<string, ushort[]>(item1, item2);
                        pcmtable.Add(pd);
                        ptr += 6;
                    }
                    pcmStartPos[id] = ptr;
                    break;
                default://mucom88
                    pcmType[id] = "";
                    for (int i = 0; i < maxpcm; i++)
                    {
                        adr = pcm[id][inftable + 28] | (pcm[id][inftable + 29] * 0x100);
                        whl = pcm[id][inftable + 30] | (pcm[id][inftable + 31] * 0x100);
                        eadr = adr + (whl >> 2);
                        if (pcm[id][i * 32] != 0)
                        {
                            ushort[] item2 = new ushort[4];
                            item2[0] = (ushort)adr;
                            item2[1] = (ushort)eadr;
                            item2[2] = (ushort)0;
                            item2[3] = (ushort)(pcm[id][inftable + 26] | (pcm[id][inftable + 27] * 0x100));
                            Array.Copy(pcm[id], i * 32, pcmname, 0, 16);
                            pcmname[16] = 0;
                            string item1 = enc.GetStringFromSjisArray(pcmname);//Encoding.GetEncoding("shift_jis").GetString(pcmname);

                            Tuple<string, ushort[]> pd = new Tuple<string, ushort[]>(item1, item2);
                            pcmtable.Add(pd);
                            //log.Write(string.Format("#PCM{0} ${1:x04} ${2:x04} {3}", i + 1, adr, eadr, Encoding.GetEncoding("shift_jis").GetString(pcmname)));
                        }
                        inftable += 32;
                    }
                    pcmStartPos[id] = 0x400;
                    break;
            }

            return pcmtable.ToArray();
        }

        public ChipDatum[] GetPCMSendData(int c,int id,int tp)
        {
            if (pcm == null) return null;
            if (pcm[id] == null) return null;
            if (c != 0) return null;
            if (tp != 0) return null;

            int startAddress = 0;
            List<ChipDatum> dat = new List<ChipDatum>
            {
                new ChipDatum(0, 0x29, 0x83),// CH 4-6 ENABLE
                new ChipDatum(0x1, 0x00, 0x20),
                new ChipDatum(0x1, 0x00, 0x21),
                new ChipDatum(0x1, 0x00, 0x00),

                new ChipDatum(0x1, 0x10, 0x00),
                new ChipDatum(0x1, 0x10, 0x80),

                new ChipDatum(0x1, 0x00, 0x61),
                new ChipDatum(0x1, 0x00, 0x68),
                new ChipDatum(0x1, 0x01, 0x00),
                new ChipDatum(0x1, 0x02, (byte)((startAddress >> 2) & 0xff)),
                new ChipDatum(0x1, 0x03, (byte)((startAddress >> 10) & 0xff)),
                new ChipDatum(0x1, 0x04, 0xff),
                new ChipDatum(0x1, 0x05, 0xff),
                new ChipDatum(0x1, 0x0c, 0xff),
                new ChipDatum(0x1, 0x0d, 0xff)
            };

            // データ転送
            int infosize = pcmStartPos[id];
            for (int cnt = 0; cnt < pcm[id].Length - infosize; cnt++)
            {
                dat.Add(new ChipDatum(0x1, 0x08, pcm[id][infosize + cnt]));
                //log.Write(string.Format("#PCMDATA adr:{0:x04} dat:{1:x02}", (infosize + cnt) >> 2, pcmdata[infosize + cnt]));
            }
            dat.Add(new ChipDatum(0x1, 0x00, 0x00));
            dat.Add(new ChipDatum(0x1, 0x10, 0x80));

            return dat.ToArray();
        }



        //-------
        //rendering
        //-------

        public void StartRendering(int renderingFreq, Tuple<string, int>[] chipsMasterClock)
        {
            lock (work.SystemInterrupt)
            {

                work.timeCounter = 0L;
                this.renderingFreq = renderingFreq <= 0 ? 44100 : renderingFreq;
                this.opnaMasterClock = 7987200;
                if (chipsMasterClock != null && chipsMasterClock.Length > 0)
                {
                    this.opnaMasterClock = chipsMasterClock[0].Item2 <= 0 ? 7987200 : chipsMasterClock[0].Item2;
                }
                work.timer = new OPNATimer(renderingFreq, opnaMasterClock);
                Log.WriteLine(LogLevel.TRACE, "Start rendering.");

            }
        }

        public void StopRendering()
        {
            lock (work.SystemInterrupt)
            {
                if (work.Status > 0) work.Status = 0;
                Log.WriteLine(LogLevel.TRACE, "Stop rendering.");

            }
        }

        public void Rendering()
        {
            if (work.Status < 0) return;

            try
            {
                music2.Rendering();
            }
            catch
            {
                work.Status = -1;
                throw;
            }
        }

        public void WriteOPNAPRegister(ChipDatum reg)
        {
            lock (lockObjWriteReg)
            {
                if (reg.port == 0) { work.timer?.WriteReg((byte)reg.address, (byte)reg.data); }
                WriteOPNAP?.Invoke(reg);
            }
        }

        public void WriteOPNASRegister(ChipDatum reg)
        {
            lock (lockObjWriteReg)
            {
                if (reg.port == 0) { work.timer?.WriteReg((byte)reg.address, (byte)reg.data); }
                WriteOPNAS?.Invoke(reg);
            }
        }

        public void WriteOPNBPRegister(ChipDatum reg)
        {
            lock (lockObjWriteReg)
            {
                if (reg.port == 0) { work.timer?.WriteReg((byte)reg.address, (byte)reg.data); }
                WriteOPNBP?.Invoke(reg);
            }
        }

        public void WriteOPNBSRegister(ChipDatum reg)
        {
            lock (lockObjWriteReg)
            {
                if (reg.port == 0) { work.timer?.WriteReg((byte)reg.address, (byte)reg.data); }
                WriteOPNBS?.Invoke(reg);
            }
        }

        public void WriteOPNBPAdpcmA(byte[] pcmdata)
        {
            if (pcmdata == null) return;
            lock (lockObjWriteReg)
            {
                WriteOPNBAdpcmAP?.Invoke(pcmdata, 0, 0);
            }
        }
        public void WriteOPNBPAdpcmB(byte[] pcmdata)
        {
            if (pcmdata == null) return;
            lock (lockObjWriteReg)
            {
                WriteOPNBAdpcmBP?.Invoke(pcmdata, 1, 0);
            }
        }

        public void WriteOPNBSAdpcmA(byte[] pcmdata)
        {
            if (pcmdata == null) return;
            lock (lockObjWriteReg)
            {
                WriteOPNBAdpcmAS?.Invoke(pcmdata, 0, 0);
            }
        }
        public void WriteOPNBSAdpcmB(byte[] pcmdata)
        {
            if (pcmdata == null) return;
            lock (lockObjWriteReg)
            {
                WriteOPNBAdpcmBS?.Invoke(pcmdata, 1, 0);
            }
        }

        //--------
        //Command
        //--------

        public void MusicSTART(int musicNumber)
        {
            Log.WriteLine(LogLevel.TRACE, "演奏開始");
            music2.MSTART(musicNumber);
            music2.SkipCount((int)header.jumpcount);
        }

        public void MusicSTOP()
        {
            Log.WriteLine(LogLevel.TRACE, "演奏停止");
            music2.MSTOP();
        }

        public void FadeOut()
        {
            Log.WriteLine(LogLevel.TRACE, "フェードアウト");
            music2.FDO();
        }

        public object GetWork()
        {
            Log.WriteLine(LogLevel.TRACE, "ワークエリア取得");
            return music2.RETW();
        }

        public void ShotEffect()
        {
            Log.WriteLine(LogLevel.TRACE, "効果音");
            music2.EFC();
        }

        public int GetStatus()
        {
            return work.Status;
        }





        private void GetFileNameFromTag()
        {
            if (tags == null) return;
            foreach (Tuple<string, string> tag in tags)
            {
                switch (tag.Item1)
                {
                    case "voice":
                        fnVoicedat[0] = tag.Item2;
                        break;
                    case "pcm":
                        fnPcm[0] = tag.Item2;
                        break;
                    case "pcmOPNA_P":
                        fnPcm[0] = tag.Item2;
                        break;
                    case "pcmOPNA_S":
                        fnPcm[1] = tag.Item2;
                        break;
                    case "pcmOPNB_B_P":
                        fnPcm[2] = tag.Item2;
                        break;
                    case "pcmOPNB_B_S":
                        fnPcm[3] = tag.Item2;
                        break;
                    case "pcmOPNB_A_P":
                        fnPcm[4] = tag.Item2;
                        break;
                    case "pcmOPNB_A_S":
                        fnPcm[5] = tag.Item2;
                        break;
                }
            }

            return;
        }

        private byte[] GetFMVoiceFromFile(int id,Func<string, Stream> appendFileReaderCallback)
        {
            try
            {
                fnVoicedat[id] = string.IsNullOrEmpty(fnVoicedat[id]) ? "voice.dat" : fnVoicedat[id];

                using (Stream vd = appendFileReaderCallback?.Invoke(fnVoicedat[id]))
                {
                    return ReadAllBytes(vd);
                }
            }
            catch
            {
                return null;
            }
        }

        private string[] defaultPCMFileName = new string[]
        {
            "mucompcm.bin",
            "mucompcm_2nd.bin",
            "mucompcm_3rd_B.bin",
            "mucompcm_4th_B.bin",
            "mucompcm_3rd_A.bin",
            "mucompcm_4th_A.bin"
        };

        private byte[] GetPCMDataFromFile(int id,Func<string, Stream> appendFileReaderCallback)
        {
            try
            {
                fnPcm[id] = string.IsNullOrEmpty(fnPcm[id]) ? defaultPCMFileName[id] : fnPcm[id]; 

                using (Stream pd = appendFileReaderCallback?.Invoke(fnPcm[id]))
                {
                    return ReadAllBytes(pd);
                }
            }
            catch
            {
                return null;
            }
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

        public int SetLoopCount(int loopCounter)
        {
            work.maxLoopCount = loopCounter;
            return 0;
        }

        public GD3Tag GetGD3TagInfo(byte[] srcBuf)
        {
            uint tagdata = Cmn.getLE32(srcBuf, 0x000c);
            uint tagsize = Cmn.getLE32(srcBuf, 0x0010);

            if (tagdata == 0) return null;
            if (srcBuf == null) return null;

            List<byte> lb = new List<byte>();
            for (int i = 0; i < tagsize; i++)
            {
                lb.Add(srcBuf[tagdata + i]);
            }

            List<Tuple<string, string>> tags = GetTagsByteArray(lb.ToArray());
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

            return tags;
        }

        private void addItemAry(GD3Tag gt, enmTag tag, string item)
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

        public int GetNowLoopCounter()
        {
            try
            {
                return work.nowLoopCounter;
            }
            catch
            {
                return -1;
            }
        }

        public void SetDriverSwitch(params object[] param)
        {
            //throw new NotImplementedException();
        }

        public void WriteRegister(ChipDatum reg)
        {
            throw new NotImplementedException();
        }

        public byte[] GetPCMFromSrcBuf()
        {
            throw new NotImplementedException();
        }

        public Tuple<string, ushort[]>[] GetPCMTable()
        {
            throw new NotImplementedException();
        }

        public ChipDatum[] GetPCMSendData()
        {
            throw new NotImplementedException();
        }

    }
}
