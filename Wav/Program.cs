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
                    SamplingRate = (uint)SamplingRate,
                    Clock = opnaMasterClock,
                    Volume = 0,
                    Option = new object[] { GetApplicationFolder() }
                };
                mds = new MDSound.MDSound((uint)SamplingRate, (uint)samplingBuffer, new MDSound.MDSound.Chip[] { chip });


#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
                drv = new Driver();
                ((Driver)drv).Init(
                    args[fnIndex]
                    , OPNAWrite
                    , OPNAWaitSend
                    , false
                    , true
                    , false
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

                drv.StartRendering((int)SamplingRate, (int)opnaMasterClock);

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

    }
}
