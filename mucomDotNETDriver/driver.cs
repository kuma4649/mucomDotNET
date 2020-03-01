using mucomDotNET.Common;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mucomDotNET.Driver
{
    public class Driver :iDriver
    {
        private MUBHeader header = null;
        private List<Tuple<string, string>> tags = null;
        private byte[] pcm = null;
        private Action<ChipDatum> WriteOPNA;
        private Action<long,int> WaitSendOPNA;
        private string pathWork = "";
        private string fnMUB = "";
        private string fnVoicedat="";
        private string fnPcm="";

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

        public void Init(string fileName, Action<ChipDatum> opnaWrite, Action<long, int> opnaWaitSend, bool notSoundBoard2, bool isLoadADPCM, bool loadADPCMOnly)
        {
            byte[] srcBuf = File.ReadAllBytes(fileName);
            Init(fileName, opnaWrite, opnaWaitSend, notSoundBoard2, srcBuf, isLoadADPCM, loadADPCMOnly);
        }

        public void Init(string fileName, Action<ChipDatum> opnaWrite, Action<long, int> opnaWaitSend, bool notSoundBoard2, byte[] srcBuf, bool isLoadADPCM, bool loadADPCMOnly)
        {
            List<MmlDatum> bl = new List<MmlDatum>();
            foreach (byte b in srcBuf) bl.Add(new MmlDatum(b));
            Init(fileName, opnaWrite, opnaWaitSend, bl.ToArray(), new object[] { notSoundBoard2, isLoadADPCM, loadADPCMOnly });
        }

        public void Init(string fileName, Action<ChipDatum> chipWriteRegister, Action<long, int> chipWaitSend, MmlDatum[] srcBuf, object addtionalOption)
        {
            bool notSoundBoard2 = (bool)((object[])addtionalOption)[0];
            bool isLoadADPCM = (bool)((object[])addtionalOption)[1];
            bool loadADPCMOnly = (bool)((object[])addtionalOption)[2];

            pathWork = Path.GetDirectoryName(fileName);
            fnMUB = fileName;
            header = new MUBHeader(srcBuf, enc);
            work.mData = GetDATA();
            tags = GetTags();
            GetFileNameFromTag();
            work.fmVoice = GetFMVoiceFromFile();
            pcm = GetPCMFromSrcBuf();
            if (pcm == null) GetPCMDataFromFile();
            work.pcmTables = GetPCMTable();

            WriteOPNA = chipWriteRegister;
            WaitSendOPNA = chipWaitSend;

            //PCMを送信する
            if (pcm != null)
            {
                if (isLoadADPCM)
                {
                    ChipDatum[] pcmSendData = GetPCMSendData();

                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    foreach (ChipDatum dat in pcmSendData) { WriteRegister(dat); }
                    sw.Stop();

                    WaitSendOPNA(sw.ElapsedMilliseconds, pcmSendData.Length);
                }
            }

            if (loadADPCMOnly) return;

            music2 = new Music2(work, WriteRegister);
            music2.notSoundBoard2 = notSoundBoard2;
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
            return header.GetTags();
        }

        public byte[] GetPCMFromSrcBuf()
        {
            return header.GetPCM();
        }

        public Tuple<string, ushort[]>[] GetPCMTable()
        {
            if (pcm == null) return null;

            List<Tuple<string, ushort[]>> pcmtable = new List<Tuple<string, ushort[]>>();
            int inftable = 0x0000;
            int adr, whl, eadr;
            byte[] pcmname = new byte[17];
            int maxpcm = 32;

            for (int i = 0; i < maxpcm; i++)
            {
                adr = pcm[inftable + 28] | (pcm[inftable + 29] * 0x100);
                whl = pcm[inftable + 30] | (pcm[inftable + 31] * 0x100);
                eadr = adr + (whl >> 2);
                if (pcm[i * 32] != 0)
                {
                    ushort[] item2 = new ushort[4];
                    item2[0] = (ushort)adr;
                    item2[1] = (ushort)eadr;
                    item2[2] = (ushort)0;
                    item2[3] = (ushort)(pcm[inftable + 26] | (pcm[inftable + 27] * 0x100));
                    Array.Copy(pcm, i * 32, pcmname, 0, 16);
                    pcmname[16] = 0;
                    string item1 = enc.GetStringFromSjisArray(pcmname);//Encoding.GetEncoding("shift_jis").GetString(pcmname);

                    Tuple<string, ushort[]> pd = new Tuple<string, ushort[]>(item1, item2);
                    pcmtable.Add(pd);
                    //log.Write(string.Format("#PCM{0} ${1:x04} ${2:x04} {3}", i + 1, adr, eadr, Encoding.GetEncoding("shift_jis").GetString(pcmname)));
                }
                inftable += 32;
            }

            return pcmtable.ToArray();
        }

        public ChipDatum[] GetPCMSendData()
        {
            if (pcm == null) return null;

            int startAddress = 0;
            List<ChipDatum> dat = new List<ChipDatum>
            {
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
            int infosize = 0x400;
            for (int cnt = 0; cnt < pcm.Length - infosize; cnt++)
            {
                dat.Add(new ChipDatum(0x1, 0x08, pcm[infosize + cnt]));
                //log.Write(string.Format("#PCMDATA adr:{0:x04} dat:{1:x02}", (infosize + cnt) >> 2, pcmdata[infosize + cnt]));
            }
            dat.Add(new ChipDatum(0x1, 0x00, 0x00));
            dat.Add(new ChipDatum(0x1, 0x10, 0x80));

            return dat.ToArray();
        }



        //-------
        //rendering
        //-------

        public void StartRendering(int renderingFreq = 44100, int opnaMasterClock = 7987200)
        {
            lock (work.SystemInterrupt)
            {

                work.timeCounter = 0L;
                this.renderingFreq = renderingFreq <= 0 ? 44100 : renderingFreq;
                this.opnaMasterClock = opnaMasterClock <= 0 ? 7987200 : opnaMasterClock;
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

        public void WriteRegister(ChipDatum reg)
        {
            lock (lockObjWriteReg)
            {
                if (reg.port == 0) { work.timer.WriteReg((byte)reg.address, (byte)reg.data); }
                WriteOPNA?.Invoke(reg);
            }
        }


        //--------
        //Command
        //--------

        public void MusicSTART(int musicNumber)
        {
            Log.WriteLine(LogLevel.TRACE, "演奏開始");
            music2.MSTART(musicNumber);
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
                        fnVoicedat = tag.Item2;
                        break;
                    case "pcm":
                        fnPcm = tag.Item2;
                        break;
                }
            }

            return;
        }

        private byte[] GetFMVoiceFromFile()
        {
            try
            {
                fnVoicedat = string.IsNullOrEmpty(fnVoicedat) ? "voice.dat" : fnVoicedat;
                if (!File.Exists(Path.Combine(pathWork, fnVoicedat)))
                {
                    return null;
                }

                return File.ReadAllBytes(Path.Combine(pathWork, fnVoicedat));
            }
            catch
            {
                return null;
            }
        }

        private byte[] GetPCMDataFromFile()
        {
            try
            {
                fnPcm = string.IsNullOrEmpty(fnPcm) ? "mucompcm.bin" : fnPcm;
                if (!File.Exists(Path.Combine(pathWork, fnPcm)))
                {
                    return null;
                }

                return File.ReadAllBytes(Path.Combine(pathWork, fnPcm));
            }
            catch
            {
                return null;
            }
        }

        public int SetLoopCount(int loopCounter)
        {
            work.maxLoopCount = loopCounter;
            return 0;
        }
    }
}
