using mucomDotNET.Common;
using mucomDotNET.Driver;
using musicDriverInterface;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vgm
{
    class Program
    {
        private static readonly int SamplingRate = 44100;//vgm format freq
        private static readonly uint opnaMasterClock = 7987200;

        private static iDriver drv = null;
        private static VgmWriter vw = null;
        private static int loop = 2;
        private static List<Tuple<string, string>> tags = null;

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
                vw = new VgmWriter();
                vw.Open(
                    Path.Combine(
                        Path.GetDirectoryName(args[fnIndex])
                        , Path.GetFileNameWithoutExtension(args[fnIndex]) + ".vgm")
                    );

#if NETCOREAPP
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif

                drv = new Driver();
                ((Driver)drv).Init(
                    args[fnIndex]
                    , OPNAWrite
                    , OPNAWaitSend
                    , false
                    , false
                    , false
                    );

                drv.SetLoopCount(loop);

                tags = ((Driver)drv).GetTags();
                if (tags != null)
                {
                    foreach (Tuple<string, string> tag in tags)
                    {
                        if (tag.Item1 == "") continue;
                        Log.WriteLine(LogLevel.INFO, string.Format("{0,-16} : {1}", tag.Item1, tag.Item2));
                    }
                }

                byte[] pcmdata = drv.GetPCMFromSrcBuf();
                if (pcmdata != null && pcmdata.Length > 0) vw.WriteAdpcm(pcmdata);

                drv.StartRendering((int)SamplingRate, (int)opnaMasterClock);

                drv.MusicSTART(0);

                while (true)
                {

                    drv.Rendering();
                    vw.IncrementWaitCOunter();

                    //ステータスが0(終了)又は0未満(エラー)の場合はループを抜けて終了
                    if (drv.GetStatus() <= 0)
                    {
                        break;
                    }

                }

                drv.MusicSTOP();
                drv.StopRendering();
            }
            catch
            {
            }
            finally
            {
                if (vw != null)
                {
                    vw.Close(tags);
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

            vw.WriteYM2608(0, (byte)dat.port, (byte)dat.address, (byte)dat.data);
        }

        private static void OPNAWaitSend(long elapsed, int size)
        {
            return;
        }

    }
}
