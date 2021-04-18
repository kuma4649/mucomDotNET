using mucomDotNET.Common;
using mucomDotNET.Driver;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace Wav
{
    class Program
    {
        private static readonly int SamplingRate = 55467;//44100;
        private static readonly int samplingBuffer = 1024;
        private static short[] frames = new short[samplingBuffer * 4];
        private static MDSound.MDSound mds = null;
        private static short[] emuRenderBuf = new short[2];
        private static iDriver drv = null;
        private static readonly uint opnaMasterClock = 7987200;
        private static readonly uint opnbMasterClock = 8000000;
        private static WaveWriter ww = null;
        private static int loop = 2;

        static int Main(string[] args)
        {
            Log.writeLine += WriteLine;
#if DEBUG
            //Log.writeLine += WriteLineF;
            Log.level = LogLevel.INFO;//.TRACE;
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

            try
            {
                ww = new WaveWriter(SamplingRate);
                ww.Open(
                    Path.Combine(
                        Path.GetDirectoryName(args[fnIndex])
                        , Path.GetFileNameWithoutExtension(args[fnIndex]) + ".wav")
                    );

                List<MDSound.MDSound.Chip> lstChips = new List<MDSound.MDSound.Chip>();
                MDSound.MDSound.Chip chip = null;

                MDSound.ym2608 ym2608 = new MDSound.ym2608();
                for (int i = 0; i < 2; i++)
                {
                    chip = new MDSound.MDSound.Chip
                    {
                        type = MDSound.MDSound.enmInstrumentType.YM2608,
                        ID = (byte)i,
                        Instrument = ym2608,
                        Update = ym2608.Update,
                        Start = ym2608.Start,
                        Stop = ym2608.Stop,
                        Reset = ym2608.Reset,
                        SamplingRate = (uint)SamplingRate,
                        Clock = opnaMasterClock,
                        Volume = 0,
                        Option = new object[] { GetApplicationFolder() }
                    };
                    lstChips.Add(chip);
                }
                MDSound.ym2610 ym2610 = new MDSound.ym2610();
                for (int i = 0; i < 2; i++)
                {
                    chip = new MDSound.MDSound.Chip
                    {
                        type = MDSound.MDSound.enmInstrumentType.YM2610,
                        ID = (byte)i,
                        Instrument = ym2610,
                        Update = ym2610.Update,
                        Start = ym2610.Start,
                        Stop = ym2610.Stop,
                        Reset = ym2610.Reset,
                        SamplingRate = (uint)SamplingRate,
                        Clock = opnbMasterClock,
                        Volume = 0,
                        Option = new object[] { GetApplicationFolder() }
                    };
                    lstChips.Add(chip);
                }
                mds = new MDSound.MDSound((uint)SamplingRate, (uint)samplingBuffer
                    , lstChips.ToArray());


#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
                List<ChipAction> lca = new List<ChipAction>();
                mucomChipAction ca;
                ca = new mucomChipAction(OPNAWriteP, null, OPNAWaitSend); lca.Add(ca);
                ca = new mucomChipAction(OPNAWriteS, null, null); lca.Add(ca);
                ca = new mucomChipAction(OPNBWriteP, OPNBWriteAdpcmP, null); lca.Add(ca);
                ca = new mucomChipAction(OPNBWriteS, OPNBWriteAdpcmS, null); lca.Add(ca);

                List<MmlDatum> bl = new List<MmlDatum>();
                byte[] srcBuf = File.ReadAllBytes(args[fnIndex]);
                foreach (byte b in srcBuf) bl.Add(new MmlDatum(b));

                drv = new Driver();
                ((Driver)drv).Init(
                    lca
                    , bl.ToArray()
                    , null
                    , new object[] {
                        false
                        , true
                        , false
                    }
                    );

                drv.SetLoopCount(loop);

                List<Tuple<string, string>> tags = ((Driver)drv).GetTags();
                if (tags != null)
                {
                    foreach (Tuple<string, string> tag in tags)
                    {
                        if (tag.Item1 == "") continue;
                        Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
                    }
                }

                drv.StartRendering((int)SamplingRate,new Tuple<string, int>[]{
                new Tuple<string, int>("YM2608",(int)opnaMasterClock)
                });

                drv.MusicSTART(0);

                while (true)
                {

                    EmuCallback(frames, 0, samplingBuffer);
                    //ステータスが0(終了)又は0未満(エラー)の場合はループを抜けて終了
                    if (drv.GetStatus() <= 0)
                    {
                        break;
                    }

                    //Log.writeLine(LogLevel.TRACE, string.Format("{0}  {1}",frames[0],frames[1]));
                    ww.Write(frames, 0, samplingBuffer);
                }

                drv.MusicSTOP();
                drv.StopRendering();
            }
            catch
            {
            }
            finally
            {
                if (ww != null)
                {
                    ww.Close();
                }
            }

            return 0;
        }

        static void WriteLine(LogLevel level, string msg)
        {
            Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        private static int AnalyzeOption(string[] args)
        {
            int i = 0;
            loop = 2;

            while (args != null 
                && args.Length > 0 
                && args[i].Length > 0 
                && args[i] != null 
                && args[i][0] == '-')
            {
                string op = args[i].Substring(1).ToUpper();
                if (op.Length > 2 && op.Substring(0, 2) == "L=")
                {
                    if (!int.TryParse(op.Substring(2), out loop))
                    {
                        loop = 2;
                    }
                }

                i++;
            }

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
            mds.WriteYM2608(0, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNAWaitSend(long elapsed, int size)
        {
            return;
        }

        private static int EmuCallback(short[] buffer, int offset, int count)
        {
            try
            {
                long bufCnt = count / 2;

                for (int i = 0; i < bufCnt; i++)
                {
                    mds.Update(emuRenderBuf, 0, 2, drv.Rendering);

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

        private static void OPNAWriteP(ChipDatum dat)
        {
            OPNAWrite(0, dat);
        }
        private static void OPNAWriteS(ChipDatum dat)
        {
            OPNAWrite(1, dat);
        }
        private static void OPNBWriteP(ChipDatum dat)
        {
            OPNBWrite(0, dat);
        }
        private static void OPNBWriteS(ChipDatum dat)
        {
            OPNBWrite(1, dat);
        }
        private static void OPNBWriteAdpcmP(byte[] pcmData, int s, int e)
        {
            if (s == 0) OPNBWrite_AdpcmA(0, pcmData);
            else OPNBWrite_AdpcmB(0, pcmData);
        }
        private static void OPNBWriteAdpcmS(byte[] pcmData, int s, int e)
        {
            if (s == 0) OPNBWrite_AdpcmA(1, pcmData);
            else OPNBWrite_AdpcmB(1, pcmData);
        }


        private static void OPNAWrite(int chipId, ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! OPNA i{0} r{1} c{2}"
                        , chipId
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }

            Log.WriteLine(LogLevel.TRACE, string.Format("Out ChipA:{0} Port:{1} Adr:[{2:x02}] val[{3:x02}]", chipId, dat.port, (int)dat.address, (int)dat.data));

            mds.WriteYM2608((byte)chipId, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNBWrite(int chipId, ChipDatum dat)
        {
            if (dat != null && dat.addtionalData != null)
            {
                MmlDatum md = (MmlDatum)dat.addtionalData;
                if (md.linePos != null)
                {
                    Log.WriteLine(LogLevel.TRACE, string.Format("! OPNB i{0} r{1} c{2}"
                        , chipId
                        , md.linePos.row
                        , md.linePos.col
                        ));
                }
            }

            Log.WriteLine(LogLevel.TRACE, string.Format("Out ChipB:{0} Port:{1} Adr:[{2:x02}] val[{3:x02}]", chipId, dat.port, (int)dat.address, (int)dat.data));

            mds.WriteYM2610((byte)chipId, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNBWrite_AdpcmA(int chipId, byte[] pcmData)
        {
            mds.WriteYM2610_SetAdpcmA((byte)chipId, pcmData);

        }

        private static void OPNBWrite_AdpcmB(int chipId, byte[] pcmData)
        {
            mds.WriteYM2610_SetAdpcmB((byte)chipId, pcmData);

        }


    }
}
