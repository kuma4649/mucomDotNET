using mucomDotNET.Common;
using mucomDotNET.Driver;
using SdlDotNet.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace mucomDotNET.Player
{
    class Program
    {
        private static AudioStream sdl;
        private static readonly AudioCallback sdlCb = new AudioCallback(EmuCallback);
        private static IntPtr sdlCbPtr;
        private static GCHandle sdlCbHandle;
        private static readonly uint SamplingRate = 44100;
        private static readonly uint samplingBuffer = 1024;
        private static readonly short[] frames = new short[samplingBuffer * 4];
        private static MDSound.MDSound mds = null;
        private static readonly short[] emuRenderBuf = new short[2];
        private static Driver.Driver drv = null;
        private static readonly uint opnaMasterClock = 7987200;

        static int Main(string[] args)
        {
            Log.writeLine = WriteLine;
#if DEBUG
            Log.level = LogLevel.TRACE;
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



            //SDLとMDSoundのセットアップ
            sdlCbHandle = GCHandle.Alloc(sdlCb);
            sdlCbPtr = Marshal.GetFunctionPointerForDelegate(sdlCb);
            sdl = new AudioStream((int)SamplingRate, AudioFormat.Signed16Little, SoundChannel.Stereo, (short)samplingBuffer, sdlCb, null)
            {
                Paused = true
            };
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
            drv.Init(args[0], OPNAWrite);

            List<Tuple<string, string>> tags = drv.GetTags();
            foreach (Tuple<string, string> tag in tags)
            {
                if (tag.Item1 == "") continue;
                Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
            }

            drv.StartRendering((int)SamplingRate, (int)opnaMasterClock);
            sdl.Paused = false;
            drv.MSTART(0);

            Log.WriteLine(LogLevel.INFO, "終了する場合は何かキーを押してください");
            Console.ReadKey();

            drv.MSTOP();
            sdl.Paused = true;
            drv.StopRendering();



            return 0;
        }

        static void WriteLine(LogLevel level, string msg)
        {
            System.Console.WriteLine("[{0,-7}] {1}", level, msg);
        }

        private static void EmuCallback(IntPtr userData, IntPtr stream, int len)
        {
            long bufCnt = len / 4;

            for (int i = 0; i < bufCnt; i++)
            {
                mds.Update(emuRenderBuf, 0, 2, OneFrame);

                frames[i * 2 + 0] = emuRenderBuf[0];
                frames[i * 2 + 1] = emuRenderBuf[1];
                //Console.Write("Adr[{0:x8}] : Wait[{1:d8}] : [{2:d8}]/[{3:d8}]\r\n", vgmAdr, vgmWait, buf[0], buf[1]);
            }

            Marshal.Copy(frames, 0, stream, len / 2);

        }

        private static void OneFrame()
        {
            drv.Rendering();
        }

        private static void OPNAWrite(Driver.OPNAData dat)
        {
            mds.WriteYM2608(0, dat.port, dat.address, dat.data);
        }
    }
}