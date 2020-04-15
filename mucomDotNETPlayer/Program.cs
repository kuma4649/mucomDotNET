using mucomDotNET.Common;
using musicDriverInterface;
using NAudio.Wave;
using Nc86ctl;
using NScci;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace mucomDotNET.Player
{
    class Program
    {
        private static DirectSoundOut audioOutput = null;
        public delegate int naudioCallBack(short[] buffer, int offset, int sampleCount);
        private static naudioCallBack callBack = null;
        private static Thread trdMain=null;
        private static Stopwatch sw = null;
        private static double swFreq = 0;
        public static bool trdClosed = false;
        private static object lockObj = new object();
        private static bool _trdStopped = true;
        public static bool trdStopped
        {
            get
            {
                lock (lockObj)
                {
                    return _trdStopped;
                }
            }
            set
            {
                lock (lockObj)
                {
                    _trdStopped = value;
                }
            }
        }

        private static readonly uint SamplingRate = 55467;//44100;
        private static readonly uint samplingBuffer = 1024;
        private static short[] frames = new short[samplingBuffer * 4];
        private static MDSound.MDSound mds = null;
        private static short[] emuRenderBuf = new short[2];
        private static musicDriverInterface.iDriver drv = null;
        private static readonly uint opnaMasterClock = 7987200;
        private static int device = 0;
        private static int loop = 0;

        private static bool loadADPCMOnly = false;
        private static bool isLoadADPCM = true;

        private static NScci.NScci nScci;
        private static Nc86ctl.Nc86ctl nc86ctl;
        private static RSoundChip rsc;

        static int Main(string[] args)
        {
            Log.writeLine += WriteLine;
#if DEBUG
            //Log.writeLine += WriteLineF;
            Log.level = LogLevel.INFO;// TRACE;
#else
            Log.level = LogLevel.INFO;
#endif
            int fnIndex = AnalyzeOption(args);

            if (args == null || args.Length != fnIndex + 1)
            {
                Log.WriteLine(LogLevel.ERROR, "引数(.mubファイル)１個欲しいよぉ");
                return -1;
            }
            if (!File.Exists(args[fnIndex]))
            {
                Log.WriteLine(LogLevel.ERROR, "ファイルが見つかりません");
                return -1;
            }

            rsc = CheckDevice();

            try
            {

                SineWaveProvider16 waveProvider;
                int latency = 1000;

                switch (device)
                {
                    case 0:
                        waveProvider = new SineWaveProvider16();
                        waveProvider.SetWaveFormat((int)SamplingRate, 2);
                        callBack = EmuCallback;
                        audioOutput = new DirectSoundOut(latency);
                        audioOutput.Init(waveProvider);
                        break;
                    case 1:case 2:
                        trdMain = new Thread(new ThreadStart(RealCallback));
                        trdMain.Priority = ThreadPriority.Highest;
                        trdMain.IsBackground = true;
                        trdMain.Name = "trdVgmReal";
                        sw = Stopwatch.StartNew();
                        swFreq = Stopwatch.Frequency;
                        break;
                }

                MDSound.ym2608 ym2608 = new MDSound.ym2608();
                MDSound.MDSound.Chip chip = new MDSound.MDSound.Chip
                {
                    type = MDSound.MDSound.enmInstrumentType.YM2608,
                    ID = 0,
                    Instrument = ym2608,
                    Update = ym2608.Update,
                    Start = ym2608.Start,
                    Stop = ym2608.Stop,
                    Reset = ym2608.Reset,
                    SamplingRate = SamplingRate,
                    Clock = opnaMasterClock,
                    Volume = 0,
                    Option = new object[] { GetApplicationFolder() }
                };

                // ふたつめだよ 2nd instance here
                MDSound.ym2608 ym2608_2 = new MDSound.ym2608();
                MDSound.MDSound.Chip chip2 = new MDSound.MDSound.Chip {
                    type = MDSound.MDSound.enmInstrumentType.YM2608,
                    ID = 0,
                    Instrument = ym2608_2,
                    Update = ym2608_2.Update,
                    Start = ym2608_2.Start,
                    Stop = ym2608_2.Stop,
                    Reset = ym2608_2.Reset,
                    SamplingRate = SamplingRate,
                    Clock = opnaMasterClock,
                    Volume = 0,
                    Option = new object[] { GetApplicationFolder() }
                };

                mds = new MDSound.MDSound(SamplingRate, samplingBuffer, new MDSound.MDSound.Chip[] { chip, chip2 });

#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
                drv = new Driver.Driver();
                ((Driver.Driver)drv).Init(
                    args[fnIndex]
                    , OPNAWrite
                    , OPNAWaitSend
                    , false
                    , isLoadADPCM
                    , loadADPCMOnly
                    );

                List<Tuple<string, string>> tags = drv.GetTags();
                if (tags != null)
                {
                    foreach (Tuple<string, string> tag in tags)
                    {
                        if (tag.Item1 == "") continue;
                        Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
                    }
                }
                
                if (loadADPCMOnly) return 0;

                drv.StartRendering((int)SamplingRate, (int)opnaMasterClock);

                switch(device)
                {
                    case 0:
                        audioOutput.Play();
                        break;
                    case 1: case 2:
                        trdMain.Start();
                        break;
                }

                drv.MusicSTART(0);

                Log.WriteLine(LogLevel.INFO, "終了する場合は何かキーを押してください");

                while (true)
                {
                    System.Threading.Thread.Sleep(1);
                    if (Console.KeyAvailable)
                    {
                        break;
                    }
                    //ステータスが0(終了)又は0未満(エラー)の場合はループを抜けて終了
                    if (drv.GetStatus() <= 0)
                    {
                        if (drv.GetStatus() == 0)
                        {
                            System.Threading.Thread.Sleep((int)(latency * 2.0));//実際の音声が発音しきるまでlatency*2の分だけ待つ
                        }
                        break;
                    }
                }

                drv.MusicSTOP();
                drv.StopRendering();
            }
            catch(Exception ex)
            {
                Log.WriteLine(LogLevel.FATAL, "演奏失敗");
                Log.WriteLine(LogLevel.FATAL, string.Format("message:{0}", ex.Message));
                Log.WriteLine(LogLevel.FATAL, string.Format("stackTrace:{0}", ex.StackTrace));
            }
            finally
            {
                if (audioOutput != null)
                {
                    audioOutput.Stop();
                    while (audioOutput.PlaybackState == PlaybackState.Playing) { Thread.Sleep(1); }
                    audioOutput.Dispose();
                    audioOutput = null;
                }
                if(trdMain!=null)
                {
                    trdClosed = true;
                    while (!trdStopped) { Thread.Sleep(1); }
                }
                if (nc86ctl != null)
                {
                    nc86ctl.deinitialize();
                    nc86ctl = null;
                }
                if (nScci != null)
                {
                    nScci.Dispose();
                    nScci = null;
                }
            }

            return 0;
        }

        private static void OPNAWaitSend(long elapsed, int size)
        {
            switch (device)
            {
                case 0://EMU
                    return;
                case 1://GIMIC

                    //サイズと経過時間から、追加でウエイトする。
                    int m = Math.Max((int)(size / 20 - elapsed), 0);//20 閾値(magic number)
                    Thread.Sleep(m);

                    //ポートも一応見る
                    int n = nc86ctl.getNumberOfChip();
                    for (int i = 0; i < n; i++)
                    {
                        NIRealChip rc = nc86ctl.getChipInterface(i);
                        if (rc != null)
                        {
                            while ((rc.@in(0x0) & 0x83) != 0)
                                Thread.Sleep(0);
                            while ((rc.@in(0x100) & 0xbf) != 0)
                                Thread.Sleep(0);
                        }
                    }

                    break;
                case 2://SCCI
                    nScci.NSoundInterfaceManager_.sendData();
                    while (!nScci.NSoundInterfaceManager_.isBufferEmpty()) 
                    {
                        Thread.Sleep(0);
                    }
                    break;
            }
        }

        private static RSoundChip CheckDevice()
        {
            SChipType ct = null;
            int iCount = 0;

            switch (device)
            {
                case 1://GIMIC存在チェック
                    nc86ctl = new Nc86ctl.Nc86ctl();
                    nc86ctl.initialize();
                    iCount = nc86ctl.getNumberOfChip();
                    if (iCount == 0)
                    {
                        nc86ctl.deinitialize();
                        nc86ctl = null;
                        Log.WriteLine(LogLevel.ERROR, "Not found G.I.M.I.C.");
                        device = 0;
                        break;
                    }
                    for (int i = 0; i < iCount; i++)
                    {
                        NIRealChip rc = nc86ctl.getChipInterface(i);
                        NIGimic2 gm = rc.QueryInterface();
                        ChipType cct = gm.getModuleType();
                        int o = -1;
                        if (cct == ChipType.CHIP_YM2608 || cct == ChipType.CHIP_YMF288 || cct == ChipType.CHIP_YM2203)
                        {
                            ct = new SChipType();
                            ct.SoundLocation = -1;
                            ct.BusID = i;
                            string seri = gm.getModuleInfo().Serial;
                            if (!int.TryParse(seri, out o))
                            {
                                o = -1;
                                ct = null;
                                continue;
                            }
                            ct.SoundChip = o;
                            ct.ChipName = gm.getModuleInfo().Devname;
                            ct.InterfaceName = gm.getMBInfo().Devname;
                            break;
                        }
                    }
                    RC86ctlSoundChip rsc = null;
                    if (ct == null)
                    {
                        nc86ctl.deinitialize();
                        nc86ctl = null;
                        Log.WriteLine(LogLevel.ERROR, "Not found G.I.M.I.C.(OPNA module)");
                        device = 0;
                    }
                    else
                    {
                        rsc = new RC86ctlSoundChip(-1, ct.BusID, ct.SoundChip);
                        rsc.c86ctl = nc86ctl;
                        rsc.init();

                        rsc.SetMasterClock(7987200);//SoundBoardII
                        rsc.setSSGVolume(63);//PC-8801
                    }
                    return rsc;
                case 2://SCCI存在チェック
                    nScci = new NScci.NScci();
                    iCount = nScci.NSoundInterfaceManager_.getInterfaceCount();
                    if (iCount == 0)
                    {
                        nScci.Dispose();
                        nScci = null;
                        Log.WriteLine(LogLevel.ERROR, "Not found SCCI.");
                        device = 0;
                        break;
                    }
                    for (int i = 0; i < iCount; i++)
                    {
                        NSoundInterface iIntfc = nScci.NSoundInterfaceManager_.getInterface(i);
                        NSCCI_INTERFACE_INFO iInfo = nScci.NSoundInterfaceManager_.getInterfaceInfo(i);
                        int sCount = iIntfc.getSoundChipCount();
                        for (int s = 0; s < sCount; s++)
                        {
                            NSoundChip sc = iIntfc.getSoundChip(s);
                            int t = sc.getSoundChipType();
                            if (t == 1)
                            {
                                ct = new SChipType();
                                ct.SoundLocation = 0;
                                ct.BusID = i;
                                ct.SoundChip = s;
                                ct.ChipName = sc.getSoundChipInfo().cSoundChipName;
                                ct.InterfaceName = iInfo.cInterfaceName;
                                goto scciExit;
                            }
                        }
                    }
                scciExit:;
                    RScciSoundChip rssc = null;
                    if (ct == null)
                    {
                        nScci.Dispose();
                        nScci = null;
                        Log.WriteLine(LogLevel.ERROR, "Not found SCCI(OPNA module).");
                        device = 0;
                    }
                    else
                    {
                        rssc = new RScciSoundChip(0, ct.BusID, ct.SoundChip);
                        rssc.scci = nScci;
                        rssc.init();
                    }
                    return rssc;
            }

            return null;
        }

        private static int AnalyzeOption(string[] args)
        {
            int i = 0;

            device = 0;
            loop = 0;
            isLoadADPCM = true;

            while (args[i] != null && args[i].Length > 0 && args[i][0] == '-')
            {
                string op = args[i].Substring(1).ToUpper();
                if (op == "D=EMU")
                {
                    device = 0;
                }
                if (op == "D=GIMIC")
                {
                    device = 1;
                }
                if (op == "D=SCCI")
                {
                    device = 2;
                }
                if (op == "D=WAVE")
                {
                    device = 3;
                }

                if (op.Length>2 && op.Substring(0, 2) == "L=")
                {
                    if (!int.TryParse(op.Substring(2), out loop))
                    {
                        loop = 0;
                    }
                }

                if (op.Length > 10 && op.Substring(0, 10) == "LOADADPCM=")
                {
                    if (op.Substring(10) == "ONLY")
                    {
                        loadADPCMOnly = true;
                        isLoadADPCM = true;
                    }
                    else
                    {
                        loadADPCMOnly = false;
                        if (!bool.TryParse(op.Substring(10), out isLoadADPCM))
                        {
                            isLoadADPCM = true;
                        }
                    }
                }

                i++;
            }

            if (device == 3 && loop == 0) loop = 1;

            return i;
        }

        public static string GetApplicationFolder()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(path))
            {
                path += path[path.Length - 1] == '\\' ? "" : "\\";
            }
            return path;
        }


        //private static long traceLine = 0;
        private static void WriteLineF(LogLevel level, string msg)
        {
            //traceLine++;
            //if (traceLine < 48434) return;
            //File.AppendAllText(@"C:\Users\kuma\Desktop\new.log", string.Format("[{0,-7}] {1}" + Environment.NewLine, level, msg));
        }

        static void WriteLine(LogLevel level, string msg)
        {
            Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        private static int EmuCallback(short[] buffer, int offset, int count)
        {
            try
            {
                long bufCnt = count / 2;

                for (int i = 0; i < bufCnt; i++)
                {
                    mds.Update(emuRenderBuf, 0, 2, OneFrame);

                    buffer[offset + i * 2 + 0] = emuRenderBuf[0];
                    buffer[offset + i * 2 + 1] = emuRenderBuf[1];

                }
            }
            catch//(Exception ex)
            {
                //Log.WriteLine(LogLevel.FATAL, string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }

            return count;
        }

        private static void RealCallback()
        {
            double o = sw.ElapsedTicks / swFreq;
            double step = 1 / (double)SamplingRate;

            trdStopped = false;
            try
            {
                while (!trdClosed)
                {
                    Thread.Sleep(0);

                    double el1 = sw.ElapsedTicks / swFreq;
                    if (el1 - o < step) continue;
                    if (el1 - o >= step * SamplingRate / 100.0)//閾値10ms
                    {
                        do
                        {
                            o += step;
                        } while (el1 - o >= step);
                    }
                    else
                    {
                        o += step;
                    }

                    OneFrame();

                }
            }
            catch
            {
            }
            trdStopped = true;
        }

        private static void OneFrame()
        {
            drv.Rendering();
        }

        private static void OPNAWrite(ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! r{0} c{1}"
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }
            //Log.WriteLine(LogLevel.TRACE, string.Format("FM P{2} Out:Adr[{0:x02}] val[{1:x02}]", (int)dat.address, (int)dat.data,dat.port));
            switch(device)
            {
                case 0:
                    var portno = (byte)dat.port;
                    var chipId = (byte)0;
                    var chipIndex = 0;
                    if ((portno & 0x02) != 0) {
                        chipIndex = 1;
                    }
                    portno &= 0x01;
                    mds.WriteYM2608(chipIndex, chipId, portno, (byte)dat.address, (byte)dat.data);
                    break;
                case 1:
                case 2:
                    rsc.setRegister(dat.port * 0x100 + dat.address, dat.data);
                    break;
            }
        }


        public class SineWaveProvider16 : WaveProvider16
        {

            public SineWaveProvider16()
            {
            }

            public override int Read(short[] buffer, int offset, int sampleCount)
            {

                return callBack(buffer, offset, sampleCount);

            }

        }
    }

}