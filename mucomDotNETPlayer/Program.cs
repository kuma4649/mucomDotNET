using mucomDotNET.Common;
using mucomDotNET.Driver;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace mucomDotNET.Player
{
    class Program
    {
        private static DirectSoundOut output = null;
        public delegate int naudioCallBack(short[] buffer, int offset, int sampleCount);
        private static naudioCallBack callBack = null;

        private static readonly uint SamplingRate = 55467;//44100;
        private static readonly uint samplingBuffer = 1024;
        private static short[] frames = new short[samplingBuffer * 4];
        private static MDSound.MDSound mds = null;
        private static short[] emuRenderBuf = new short[2];
        private static Driver.Driver drv = null;
        private static readonly uint opnaMasterClock = 7987200;

        static int Main(string[] args)
        {
            Log.writeLine += WriteLine;
#if DEBUG
            //Log.writeLine += WriteLineF;
            Log.level = LogLevel.INFO;//.TRACE;
#else
            Log.level = LogLevel.INFO;
#endif

            if (args == null || args.Length != 1)
            {
                Log.WriteLine(LogLevel.ERROR, "引数(.mubファイル)１個欲しいよぉ");
                return -1;
            }
            if (!File.Exists(args[0]))
            {
                Log.WriteLine(LogLevel.ERROR, "ファイルが見つかりません");
                return -1;
            }

            try
            {

                SineWaveProvider16 waveProvider;
                waveProvider = new SineWaveProvider16();
                waveProvider.SetWaveFormat((int)SamplingRate, 2);
                callBack = EmuCallback;
                output = new DirectSoundOut(1000);
                output.Init(waveProvider);

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
                    Option = null
                };
                mds = new MDSound.MDSound(SamplingRate, samplingBuffer, new MDSound.MDSound.Chip[] { chip });



                drv = new Driver.Driver();
                drv.Init(args[0], OPNAWrite, false);

                List<Tuple<string, string>> tags = drv.GetTags();
                foreach (Tuple<string, string> tag in tags)
                {
                    if (tag.Item1 == "") continue;
                    Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
                }

                drv.StartRendering((int)SamplingRate, (int)opnaMasterClock);

                output.Play();
                drv.MSTART(0);

                Log.WriteLine(LogLevel.INFO, "終了する場合は何かキーを押してください");

                while (true)
                {
                    System.Threading.Thread.Sleep(1);
                    if (Console.KeyAvailable)
                    {
                        break;
                    }
                    //ステータスが0(終了)又は0未満(エラー)の場合はループを抜けて終了
                    if (drv.Status() <= 0)
                    {
                        break;
                    }
                }

                drv.MSTOP();
                drv.StopRendering();
            }
            catch
            {

            }
            finally
            {
                if (output != null)
                {
                    output.Stop();
                    output.Dispose();
                    output = null;
                }
            }

            return 0;
        }

        private static long traceLine = 0;
        private static void WriteLineF(LogLevel level, string msg)
        {
            traceLine++;
            //if (traceLine < 48434) return;
            File.AppendAllText(@"C:\Users\kuma\Desktop\new.log", string.Format("[{0,-7}] {1}" + Environment.NewLine, level, msg));
        }

        static void WriteLine(LogLevel level, string msg)
        {
            System.Console.WriteLine("[{0,-7}] {1}", level, msg);
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

                    //Console.WriteLine(frames[i * 2 + 0]);
                }
            }
            catch//(Exception ex)
            {
                //Log.WriteLine(LogLevel.FATAL, string.Format("{0} {1}", ex.Message, ex.StackTrace));
            }

            return count;
        }

        private static void OneFrame()
        {
            drv.Rendering();
        }

        private static void OPNAWrite(Driver.OPNAData dat)
        {
            //Log.WriteLine(LogLevel.TRACE, string.Format("FM P{2} Out:Adr[{0:x02}] val[{1:x02}]", (int)dat.address, (int)dat.data,dat.port));
            mds.WriteYM2608(0, dat.port, dat.address, dat.data);
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